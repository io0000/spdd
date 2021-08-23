using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.utils;
using spdd.ui;
using spdd.items.scrolls;
using spdd.items.rings;
using spdd.scenes;
using spdd.levels;
using spdd.effects;
using spdd.mechanics;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class TalismanOfForesight : Artifact
    {
        public TalismanOfForesight()
        {
            image = ItemSpriteSheet.ARTIFACT_TALISMAN;

            exp = 0;
            levelCap = 10;

            charge = 0;
            partialCharge = 0;
            chargeCap = 100;

            defaultAction = AC_SCRY;

            scry = new Scry(this);
        }

        public const string AC_SCRY = "SCRY";

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            if (IsEquipped(hero) && !cursed)
                actions.Add(AC_SCRY);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_SCRY))
            {
                if (!IsEquipped(hero))
                    GLog.Information(Messages.Get(typeof(Artifact), "need_to_equip"));
                else if (charge < 5)
                    GLog.Information(Messages.Get(this, "low_charge"));
                else
                    GameScene.SelectCell(scry);
            }
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new Foresight(this);
        }

        public override void Charge(Hero target)
        {
            if (charge < chargeCap)
            {
                charge += 2;
                if (charge >= chargeCap)
                {
                    charge = chargeCap;
                    partialCharge = 0;
                    //GLog.Positive(Messages.Get(typeof(Foresight), "full_charge"));
                    GLog.Positive(Messages.Get(typeof(TalismanOfForesight), "full_charge"));
                }
            }
        }

        public override string Desc()
        {
            string desc = base.Desc();

            if (IsEquipped(Dungeon.hero))
            {
                if (!cursed)
                {
                    desc += "\n\n" + Messages.Get(this, "desc_worn");
                }
                else
                {
                    desc += "\n\n" + Messages.Get(this, "desc_cursed");
                }
            }

            return desc;
        }

        private float MaxDist()
        {
            return Math.Min(5 + 2 * GetLevel(), (charge - 3) / 1.08f);
        }

        private Scry scry;

        public class Scry : CellSelector.IListener
        {
            TalismanOfForesight tof;

            public Scry(TalismanOfForesight tof)
            {
                this.tof = tof;
            }

            public void OnSelect(int? target)
            {
                if (target == null)
                    return;

                tof.OnSelect(target.Value);
            }

            public string Prompt()
            {
                return Messages.Get(typeof(TalismanOfForesight), "prompt");
            }
        }

        public void OnSelect(int target)
        {
            if (target != curUser.pos)
            {
                //enforces at least 2 tiles of distance
                if (Dungeon.level.Adjacent(target, curUser.pos))
                {
                    target += (target - curUser.pos);
                }

                float dist = Dungeon.level.TrueDistance(curUser.pos, target);

                if (dist >= 3 && dist > MaxDist())
                {
                    Ballistic trajectory = new Ballistic(curUser.pos, target, Ballistic.STOP_TARGET);
                    int i = 0;
                    while (i < trajectory.path.Count &&
                        Dungeon.level.TrueDistance(curUser.pos, trajectory.path[i]) <= MaxDist())
                    {
                        target = trajectory.path[i];
                        ++i;
                    }
                    dist = Dungeon.level.TrueDistance(curUser.pos, target);
                }

                //starts at 200 degrees, loses 8% per tile of distance
                float angle = (float)Math.Round(200 * (float)Math.Pow(0.92, dist), MidpointRounding.AwayFromZero);
                var cone = new ConeAOE(new Ballistic(curUser.pos, target, Ballistic.STOP_TARGET), angle);

                int earnedExp = 0;
                bool noticed = false;
                foreach (int cell in cone.cells)
                {
                    GameScene.EffectOverFog(new CheckedCell(cell, curUser.pos));
                    if (Dungeon.level.discoverable[cell] && !(Dungeon.level.mapped[cell] || Dungeon.level.visited[cell]))
                    {
                        Dungeon.level.mapped[cell] = true;
                        ++earnedExp;
                    }

                    if (Dungeon.level.secret[cell])
                    {
                        int oldValue = Dungeon.level.map[cell];
                        GameScene.DiscoverTile(cell, oldValue);
                        Dungeon.level.Discover(cell);
                        ScrollOfMagicMapping.Discover(cell);
                        noticed = true;

                        if (oldValue == Terrain.SECRET_TRAP)
                        {
                            earnedExp += 10;
                        }
                        else if (oldValue == Terrain.SECRET_DOOR)
                        {
                            earnedExp += 100;
                        }
                    }

                    var ch = Actor.FindChar(cell);
                    if (ch != null && ch.alignment != Character.Alignment.NEUTRAL && ch.alignment != curUser.alignment)
                    {
                        Buff.Append<CharAwareness>(curUser, 5 + 2 * base.GetLevel()).charID = ch.Id();

                        if (!curUser.fieldOfView[ch.pos])
                        {
                            earnedExp += 10;
                        }
                    }

                    Heap h = Dungeon.level.heaps[cell];
                    if (h != null)
                    {
                        Buff.Append<HeapAwareness>(curUser, 5 + 2 * GetLevel()).pos = h.pos;

                        if (!h.seen)
                        {
                            earnedExp += 10;
                        }
                    }
                }

                exp += earnedExp;
                if (exp >= 100 + 50 * GetLevel() && GetLevel() < levelCap)
                {
                    exp -= 100 + 50 * GetLevel();
                    Upgrade();
                    GLog.Positive(Messages.Get(typeof(TalismanOfForesight), "levelup"));
                }
                UpdateQuickslot();

                //5 charge at 2 tiles, up to 30 charge at 25 tiles
                charge -= (int)(3 + dist * 1.08f);
                partialCharge -= (dist * 1.08f) % 1f;
                if (partialCharge < 0 && charge > 0)
                {
                    ++partialCharge;
                    --charge;
                }
                UpdateQuickslot();
                Dungeon.Observe();
                Dungeon.hero.CheckVisibleMobs();
                GameScene.UpdateFog();

                curUser.sprite.Zap(target);
                curUser.Spend(Actor.TICK);
                Sample.Instance.Play(Assets.Sounds.SCAN);
                if (noticed)
                    Sample.Instance.Play(Assets.Sounds.SECRET);
            }
        }

        private const string WARN = "warn";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(WARN, warn);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            warn = bundle.GetBoolean(WARN);
        }

        private bool warn;

        public class Foresight : ArtifactBuff
        {
            public Foresight(Artifact artifact)
                : base(artifact)
            { }

            public override bool Act()
            {
                var tof = (TalismanOfForesight)artifact;
                return tof.ForesightAct(this);
            }

            public void Charge(int boost)
            {
                var tof = (TalismanOfForesight)artifact;

                tof.charge = Math.Min((tof.charge + boost), tof.chargeCap);
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc");
            }

            public override int Icon()
            {
                var tof = (TalismanOfForesight)artifact;

                if (tof.warn)
                    return BuffIndicator.FORESIGHT;
                else
                    return BuffIndicator.NONE;
            }
        }

        public bool ForesightAct(Foresight foresight)
        {
            var target = foresight.target;
            foresight.Spend(Actor.TICK);

            bool smthFound = false;

            int distance = 3;

            int cx = target.pos % Dungeon.level.Width();
            int cy = target.pos / Dungeon.level.Width();
            int ax = cx - distance;
            if (ax < 0)
                ax = 0;

            int bx = cx + distance;
            if (bx >= Dungeon.level.Width())
                bx = Dungeon.level.Width() - 1;

            int ay = cy - distance;
            if (ay < 0)
                ay = 0;

            int by = cy + distance;
            if (by >= Dungeon.level.Height())
                by = Dungeon.level.Height() - 1;

            for (int y = ay; y <= by; ++y)
            {
                for (int x = ax, p = ax + y * Dungeon.level.Width(); x <= bx; ++x, ++p)
                {
                    if (Dungeon.level.heroFOV[p] &&
                        Dungeon.level.secret[p] &&
                        Dungeon.level.map[p] != Terrain.SECRET_DOOR)
                    {
                        if (Dungeon.level.traps[p] != null && Dungeon.level.traps[p].canBeSearched)
                            smthFound = true;
                    }
                }
            }

            if (smthFound && !cursed)
            {
                if (!warn)
                {
                    GLog.Warning(Messages.Get(typeof(Foresight), "uneasy"));
                    if (target is Hero)
                        ((Hero)target).Interrupt();

                    warn = true;
                }
            }
            else
            {
                warn = false;
            }

            var lockedFloor = target.FindBuff<LockedFloor>();
            if (charge < chargeCap && !cursed && (lockedFloor == null || lockedFloor.RegenOn()))
            {
                //fully charges in 2000 turns at +0, scaling to 1000 turns at +10.
                float chargeGain = (0.05f + (GetLevel() * 0.005f));
                chargeGain *= RingOfEnergy.ArtifactChargeMultiplier(target);
                partialCharge += chargeGain;

                if (partialCharge > 1 && charge < chargeCap)
                {
                    --partialCharge;
                    ++charge;
                    UpdateQuickslot();
                }
                else if (charge >= chargeCap)
                {
                    partialCharge = 0;
                    GLog.Positive(Messages.Get(typeof(TalismanOfForesight), "full_charge"));
                }
            }
            return true;
        }

        [SPDStatic]
        public class CharAwareness : FlavourBuff
        {
            public int charID;
            public int depth = Dungeon.depth;

            private const string ID = "id";

            public override void Detach()
            {
                base.Detach();
                Dungeon.Observe();
                GameScene.UpdateFog();
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                charID = bundle.GetInt(ID);
            }

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(ID, charID);
            }
        }

        [SPDStatic]
        public class HeapAwareness : FlavourBuff
        {
            public int pos;
            public int depth = Dungeon.depth;

            private const string POS = "pos";
            private const string DEPTH = "depth";

            public override void Detach()
            {
                base.Detach();
                Dungeon.Observe();
                GameScene.UpdateFog();
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                pos = bundle.GetInt(POS);
                depth = bundle.GetInt(DEPTH);
            }

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(POS, pos);
                bundle.Put(DEPTH, depth);
            }
        }
    }
}