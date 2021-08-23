using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.items;
using spdd.scenes;
using spdd.levels.features;
using spdd.effects;

namespace spdd.actors.mobs
{
    public class Ghoul : Mob
    {
        public Ghoul()
        {
            spriteClass = typeof(GhoulSprite);

            HP = HT = 45;
            defenseSkill = 20;

            EXP = 5;
            maxLvl = 20;

            SLEEPING = new SleepingGhoul(this);
            WANDERING = new WanderingGhoul(this);
            state = SLEEPING;

            loot = typeof(Gold);
            lootChance = 0.2f;

            properties.Add(Property.UNDEAD);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(16, 22);
        }

        public override int AttackSkill(Character target)
        {
            return 24;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 4);
        }

        public override float SpawningWeight()
        {
            return 0.5f;
        }

        private int timesDowned;
        protected int partnerID = -1;

        private const string PARTNER_ID = "partner_id";
        private const string TIMES_DOWNED = "times_downed";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(PARTNER_ID, partnerID);
            bundle.Put(TIMES_DOWNED, timesDowned);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            partnerID = bundle.GetInt(PARTNER_ID);
            timesDowned = bundle.GetInt(TIMES_DOWNED);
        }

        public override bool Act()
        {
            //create a child
            if (partnerID == -1)
            {
                var candidates = new List<int>();

                int[] neighbors = { pos + 1, pos - 1, pos + Dungeon.level.Width(), pos - Dungeon.level.Width() };
                foreach (int n in neighbors)
                {
                    if (Dungeon.level.passable[n] && Actor.FindChar(n) == null)
                    {
                        candidates.Add(n);
                    }
                }

                if (candidates.Count > 0)
                {
                    Ghoul child = new Ghoul();
                    child.partnerID = this.Id();
                    this.partnerID = child.Id();
                    if (state != SLEEPING)
                    {
                        child.state = child.WANDERING;
                    }

                    child.pos = Rnd.Element(candidates);

                    Dungeon.level.OccupyCell(child);

                    GameScene.Add(child);
                    if (sprite.visible)
                    {
                        Actor.AddDelayed(new Pushing(child, pos, child.pos), -1);
                    }
                }

            }
            return base.Act();
        }

        private bool beingLifeLinked;

        public override void Die(object cause)
        {
            if (Utils.CheckObjectType(cause, typeof(Chasm)) == false &&
                Utils.CheckObjectType(cause, typeof(GhoulLifeLink)) == false &&
                !Dungeon.level.pit[pos])
            {
                Ghoul nearby = GhoulLifeLink.SearchForHost(this);
                if (nearby != null)
                {
                    beingLifeLinked = true;
                    Actor.Remove(this);
                    Dungeon.level.mobs.Remove(this);
                    ++timesDowned;
                    Buff.Append<GhoulLifeLink>(nearby).Set(timesDowned * 5, this);
                    ((GhoulSprite)sprite).Crumple();
                    beingLifeLinked = false;
                    return;
                }
            }

            base.Die(cause);
        }

        protected override void OnRemove()
        {
            if (beingLifeLinked)
            {
                foreach (var buff in Buffs())
                {
                    //corruption and king damager are preserved when removed via life link
                    if (!(buff is Corruption) && !(buff is DwarfKing.KingDamager))
                    {
                        buff.Detach();
                    }
                }
            }
            else
            {
                base.OnRemove();
            }
        }

        private class SleepingGhoul : Mob.Sleeping
        {
            public SleepingGhoul(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                Ghoul ghoul = (Ghoul)mob;
                Ghoul partner = (Ghoul)Actor.FindById(ghoul.partnerID);
                if (partner != null && partner.state != partner.SLEEPING)
                {
                    ghoul.state = ghoul.WANDERING;
                    ghoul.target = partner.pos;
                    return true;
                }
                else
                {
                    return base.Act(enemyInFOV, justAlerted);
                }
            }
        }

        private class WanderingGhoul : Mob.Wandering
        {
            public WanderingGhoul(Mob mob)
                : base(mob)
            { }

            protected override bool ContinueWandering()
            {
                Ghoul ghoul = (Ghoul)mob;

                ghoul.enemySeen = false;

                Ghoul partner = (Ghoul)Actor.FindById(ghoul.partnerID);
                if (partner != null && (partner.state != partner.WANDERING || Dungeon.level.Distance(ghoul.pos, partner.target) > 1))
                {
                    ghoul.target = partner.pos;
                    int oldPos = ghoul.pos;
                    if (ghoul.GetCloser(ghoul.target))
                    {
                        ghoul.Spend(1 / ghoul.Speed());
                        return ghoul.MoveSprite(oldPos, ghoul.pos);
                    }
                    else
                    {
                        ghoul.Spend(TICK);
                        return true;
                    }
                }
                else
                {
                    return base.ContinueWandering();
                }
            }
        }

        [SPDStatic]
        public class GhoulLifeLink : Buff
        {
            private Ghoul ghoul;
            private int turnsToRevive;

            public override bool Act()
            {
                ghoul.sprite.visible = Dungeon.level.heroFOV[ghoul.pos];

                if (target.fieldOfView == null)
                {
                    target.fieldOfView = new bool[Dungeon.level.Length()];
                    Dungeon.level.UpdateFieldOfView(target, target.fieldOfView);
                }

                if (!target.fieldOfView[ghoul.pos] && Dungeon.level.Distance(ghoul.pos, target.pos) >= 4)
                {
                    Detach();
                    return true;
                }

                if (Dungeon.level.pit[ghoul.pos])
                {
                    base.Detach();
                    ghoul.Die(this);
                    return true;
                }

                --turnsToRevive;
                if (turnsToRevive <= 0)
                {
                    ghoul.HP = (int)Math.Round(ghoul.HT / 10f, MidpointRounding.AwayFromZero);
                    if (Actor.FindChar(ghoul.pos) != null)
                    {
                        List<int> candidates = new List<int>();
                        foreach (int n in PathFinder.NEIGHBORS8)
                        {
                            int cell = ghoul.pos + n;
                            if (Dungeon.level.passable[cell] && Actor.FindChar(cell) == null)
                            {
                                candidates.Add(cell);
                            }
                        }
                        if (candidates.Count > 0)
                        {
                            int newPos = Rnd.Element(candidates);
                            Actor.AddDelayed(new Pushing(ghoul, ghoul.pos, newPos), -1);
                            ghoul.pos = newPos;
                        }
                        else
                        {
                            Spend(TICK);
                            return true;
                        }
                    }
                    Actor.Add(ghoul);
                    ghoul.Spend(-ghoul.Cooldown());
                    Dungeon.level.mobs.Add(ghoul);
                    Dungeon.level.OccupyCell(ghoul);
                    ghoul.sprite.Idle();
                    base.Detach();
                    return true;
                }

                Spend(TICK);
                return true;
            }

            public void Set(int turns, Ghoul ghoul)
            {
                this.ghoul = ghoul;
                turnsToRevive = turns;
            }

            public override void Fx(bool on)
            {
                if (on && ghoul != null && ghoul.sprite == null)
                {
                    GameScene.AddSprite(ghoul);
                    ((GhoulSprite)ghoul.sprite).Crumple();
                }
            }

            public override void Detach()
            {
                base.Detach();
                Ghoul newHost = SearchForHost(ghoul);
                if (newHost != null)
                {
                    AttachTo(newHost);
                    Spend(-Cooldown());
                }
                else
                {
                    ghoul.Die(this);
                }
            }

            private const string GHOUL = "ghoul";
            private const string LEFT = "left";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(GHOUL, ghoul);
                bundle.Put(LEFT, turnsToRevive);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                ghoul = (Ghoul)bundle.Get(GHOUL);
                turnsToRevive = bundle.GetInt(LEFT);
            }

            public static Ghoul SearchForHost(Ghoul dieing)
            {
                foreach (var ch in Actor.Chars())
                {
                    if (ch != dieing && ch is Ghoul && ch.alignment == dieing.alignment)
                    {
                        if (ch.fieldOfView == null)
                        {
                            ch.fieldOfView = new bool[Dungeon.level.Length()];
                            Dungeon.level.UpdateFieldOfView(ch, ch.fieldOfView);
                        }
                        if (ch.fieldOfView[dieing.pos] || Dungeon.level.Distance(ch.pos, dieing.pos) < 4)
                            return (Ghoul)ch;

                    }
                }
                return null;
            }
        }
    }
}