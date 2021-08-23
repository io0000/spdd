using System.Collections.Generic;
using watabou.utils;
using watabou.noosa;
using watabou.noosa.audio;
using spdd.mechanics;
using spdd.effects;
using spdd.items.scrolls;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.scenes;
using spdd.utils;
using spdd.ui;
using spdd.plants;
using spdd.actors;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class LloydsBeacon : Artifact
    {
        public const float TIME_TO_USE = 1;

        public const string AC_ZAP = "ZAP";
        public const string AC_SET = "SET";
        public const string AC_RETURN = "RETURN";

        public int returnDepth = -1;
        public int returnPos;

        public LloydsBeacon()
        {
            image = ItemSpriteSheet.ARTIFACT_BEACON;

            levelCap = 3;

            charge = 0;
            chargeCap = 3 + GetLevel();

            defaultAction = AC_ZAP;
            usesTargeting = true;

            zapper = new Zapper(this);
        }

        private const string DEPTH = "depth";
        private const string POS = "pos";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(DEPTH, returnDepth);
            if (returnDepth != -1)
            {
                bundle.Put(POS, returnPos);
            }
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            returnDepth = bundle.GetInt(DEPTH);
            returnPos = bundle.GetInt(POS);
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(AC_ZAP);
            actions.Add(AC_SET);
            if (returnDepth != -1)
                actions.Add(AC_RETURN);

            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action == AC_SET || action == AC_RETURN)
            {
                if (Dungeon.BossLevel())
                {
                    hero.Spend(LloydsBeacon.TIME_TO_USE);
                    GLog.Warning(Messages.Get(this, "preventing"));
                    return;
                }

                for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
                {
                    var ch = Actor.FindChar(hero.pos + PathFinder.NEIGHBORS8[i]);
                    if (ch != null && ch.alignment == Character.Alignment.ENEMY)
                    {
                        GLog.Warning(Messages.Get(this, "creatures"));
                        return;
                    }
                }
            }

            if (action == AC_ZAP)
            {
                curUser = hero;
                int chargesToUse = Dungeon.depth > 20 ? 2 : 1;

                if (!IsEquipped(hero))
                {
                    GLog.Information(Messages.Get(typeof(Artifact), "need_to_equip"));
                    QuickSlotButton.Cancel();
                }
                else if (charge < chargesToUse)
                {
                    GLog.Information(Messages.Get(this, "no_charge"));
                    QuickSlotButton.Cancel();
                }
                else
                {
                    GameScene.SelectCell(zapper);
                }
            }
            else if (action == AC_SET)
            {
                returnDepth = Dungeon.depth;
                returnPos = hero.pos;

                hero.Spend(LloydsBeacon.TIME_TO_USE);
                hero.Busy();

                hero.sprite.Operate(hero.pos);
                Sample.Instance.Play(Assets.Sounds.BEACON);

                GLog.Information(Messages.Get(this, "return"));
            }
            else if (action == AC_RETURN)
            {
                if (returnDepth == Dungeon.depth)
                {
                    ScrollOfTeleportation.Appear(hero, returnPos);
                    foreach (var m in Dungeon.level.mobs)
                    {
                        if (m.pos == hero.pos)
                        {
                            //displace mob
                            foreach (int i in PathFinder.NEIGHBORS8)
                            {
                                if (Actor.FindChar(m.pos + i) == null &&
                                    Dungeon.level.passable[m.pos + i])
                                {
                                    m.pos += i;
                                    m.sprite.Point(m.sprite.WorldToCamera(m.pos));
                                    break;
                                }
                            }
                        }
                    }
                    Dungeon.level.OccupyCell(hero);
                    Dungeon.Observe();
                    GameScene.UpdateFog();
                }
                else
                {
                    Buff buff = Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>();
                    if (buff != null)
                        buff.Detach();
                    buff = Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>();
                    if (buff != null)
                        buff.Detach();

                    InterlevelScene.mode = InterlevelScene.Mode.RETURN;
                    InterlevelScene.returnDepth = returnDepth;
                    InterlevelScene.returnPos = returnPos;
                    Game.SwitchScene(typeof(InterlevelScene));
                }
            }
        }

        protected Zapper zapper;

        public class Zapper : CellSelector.IListener
        {
            LloydsBeacon beacon;

            public Zapper(LloydsBeacon beacon)
            {
                this.beacon = beacon;
            }

            public void OnSelect(int? t)
            {
                if (t == null)
                    return;

                int target = t.Value;
                var curUser = Item.curUser;

                Invisibility.Dispel();
                beacon.charge -= Dungeon.depth > 20 ? 2 : 1;
                Item.UpdateQuickslot();

                if (Actor.FindChar(target) == curUser)
                {
                    ScrollOfTeleportation.TeleportHero(curUser);
                    curUser.SpendAndNext(1f);
                }
                else
                {
                    Ballistic bolt = new Ballistic(curUser.pos, target, Ballistic.MAGIC_BOLT);
                    var ch = Actor.FindChar(bolt.collisionPos);

                    if (ch == curUser)
                    {
                        ScrollOfTeleportation.TeleportHero(curUser);
                        curUser.SpendAndNext(1f);
                    }
                    else
                    {
                        Sample.Instance.Play(Assets.Sounds.ZAP);
                        curUser.sprite.Zap(bolt.collisionPos);
                        curUser.Busy();

                        var callback = new ActionCallback();
                        callback.action = () =>
                        {
                            if (ch != null)
                            {
                                int count = 10;
                                int pos;
                                do
                                {
                                    pos = Dungeon.level.RandomRespawnCell(ch);
                                    if (count-- <= 0)
                                        break;
                                } while (pos == -1);

                                if (pos == -1 || Dungeon.BossLevel())
                                {
                                    GLog.Warning(Messages.Get(typeof(ScrollOfTeleportation), "no_tele"));
                                }
                                else if (ch.Properties().Contains(Character.Property.IMMOVABLE))
                                {
                                    GLog.Warning(Messages.Get(typeof(LloydsBeacon), "tele_fail"));
                                }
                                else
                                {
                                    ch.pos = pos;
                                    if (ch is Mob && ((Mob)ch).state == ((Mob)ch).HUNTING)
                                    {
                                        ((Mob)ch).state = ((Mob)ch).WANDERING;
                                    }
                                    ch.sprite.Place(ch.pos);
                                    ch.sprite.visible = Dungeon.level.heroFOV[pos];
                                }
                            }
                            curUser.SpendAndNext(1f);
                        };

                        MagicMissile.BoltFromChar(curUser.sprite.parent,
                                MagicMissile.BEACON,
                                curUser.sprite,
                                bolt.collisionPos,
                                callback);
                    }
                }
            }

            public string Prompt()
            {
                return Messages.Get(typeof(LloydsBeacon), "prompt");
            }
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new BeaconRecharge(this);
        }

        public override void Charge(Hero target)
        {
            if (charge < chargeCap)
            {
                partialCharge += 0.25f;
                if (partialCharge >= 1)
                {
                    --partialCharge;
                    ++charge;
                    UpdateQuickslot();
                }
            }
        }

        public override Item Upgrade()
        {
            if (GetLevel() == levelCap)
                return this;
            ++chargeCap;
            GLog.Positive(Messages.Get(this, "levelup"));
            return base.Upgrade();
        }

        public override string Desc()
        {
            string desc = base.Desc();
            if (returnDepth != -1)
            {
                desc += "\n\n" + Messages.Get(this, "desc_set", returnDepth);
            }
            return desc;
        }

        private static ItemSprite.Glowing WHITE = new ItemSprite.Glowing(new Color(0xFF, 0xFF, 0xFF, 0xFF));

        public override ItemSprite.Glowing Glowing()
        {
            return returnDepth != -1 ? WHITE : null;
        }

        public bool BeaconRechargeAct(BeaconRecharge buff)
        {
            var target = buff.target;

            var lockedFloor = target.FindBuff<LockedFloor>();
            if (charge < chargeCap && !cursed && (lockedFloor == null || lockedFloor.RegenOn()))
            {
                partialCharge += 1 / (100f - (chargeCap - charge) * 10f);

                if (partialCharge >= 1)
                {
                    --partialCharge;
                    ++charge;

                    if (charge == chargeCap)
                        partialCharge = 0;
                }
            }

            UpdateQuickslot();
            buff.Spend(Actor.TICK);
            return true;
        }

        public class BeaconRecharge : ArtifactBuff
        {
            public BeaconRecharge(Artifact artifact)
                : base(artifact)
            { }

            public override bool Act()
            {
                var beacon = (LloydsBeacon)artifact;

                return beacon.BeaconRechargeAct(this);
            }
        }
    }
}