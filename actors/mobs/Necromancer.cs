using System;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.effects;
using spdd.scenes;
using spdd.items;
using spdd.items.scrolls;
using spdd.items.potions;
using watabou.noosa.particles;
using watabou.utils;
using watabou.noosa.audio;

namespace spdd.actors.mobs
{
    public class Necromancer : Mob
    {
        public Necromancer()
        {
            spriteClass = typeof(NecromancerSprite);

            HP = HT = 35;
            defenseSkill = 13;

            EXP = 7;
            maxLvl = 5;

            loot = new PotionOfHealing();
            lootChance = 0.2f; //see createloot

            properties.Add(Property.UNDEAD);

            HUNTING = new NecromancerHunting(this);
        }

        public bool Summoning = false;
        private Emitter summoningEmitter = null;
        private int summoningPos = -1;

        private bool firstSummon = true;

        private NecroSkeleton mySkeleton;
        private int storedSkeletonID = -1;

        public override bool Act()
        {
            if (Summoning && state != HUNTING)
            {
                Summoning = false;
                UpdateSpriteState();
            }
            return base.Act();
        }

        public override void UpdateSpriteState()
        {
            base.UpdateSpriteState();

            if (Summoning && summoningEmitter == null)
            {
                summoningEmitter = CellEmitter.Get(summoningPos);
                summoningEmitter.Pour(Speck.Factory(Speck.RATTLE), 0.2f);
                sprite.Zap(summoningPos);
            }
            else if (!Summoning && summoningEmitter != null)
            {
                summoningEmitter.on = false;
                summoningEmitter = null;
            }
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 5);
        }

        public override void RollToDropLoot()
        {
            lootChance *= ((6f - Dungeon.LimitedDrops.NECRO_HP.count) / 6f);
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.NECRO_HP.count;
            return base.CreateLoot();
        }

        public override void Die(object cause)
        {
            if (storedSkeletonID != -1)
            {
                Actor ch = Actor.FindById(storedSkeletonID);
                storedSkeletonID = -1;
                if (ch is NecroSkeleton)
                {
                    mySkeleton = (NecroSkeleton)ch;
                }
            }

            if (mySkeleton != null && mySkeleton.IsAlive())
            {
                mySkeleton.Die(null);
            }

            if (summoningEmitter != null)
            {
                summoningEmitter.on = false;
                summoningEmitter = null;
            }

            base.Die(cause);
        }

        private const string SUMMONING = "summoning";
        private const string FIRST_SUMMON = "first_summon";
        private const string SUMMONING_POS = "summoning_pos";
        private const string MY_SKELETON = "my_skeleton";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(SUMMONING, Summoning);
            bundle.Put(FIRST_SUMMON, firstSummon);
            if (Summoning)
            {
                bundle.Put(SUMMONING_POS, summoningPos);
            }
            if (mySkeleton != null)
            {
                bundle.Put(MY_SKELETON, mySkeleton.Id());
            }
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            Summoning = bundle.GetBoolean(SUMMONING);
            if (bundle.Contains(FIRST_SUMMON))
                firstSummon = bundle.GetBoolean(FIRST_SUMMON);

            if (Summoning)
            {
                summoningPos = bundle.GetInt(SUMMONING_POS);
            }
            if (bundle.Contains(MY_SKELETON))
            {
                storedSkeletonID = bundle.GetInt(MY_SKELETON);
            }
        }

        public void OnZapComplete()
        {
            if (mySkeleton == null || mySkeleton.sprite == null || !mySkeleton.IsAlive())
            {
                return;
            }

            //heal skeleton first
            if (mySkeleton.HP < mySkeleton.HT)
            {
                if (sprite.visible || mySkeleton.sprite.visible)
                {
                    sprite.parent.Add(new Beam.HealthRay(sprite.Center(), mySkeleton.sprite.Center()));
                }

                mySkeleton.HP = Math.Min(mySkeleton.HP + 5, mySkeleton.HT);
                mySkeleton.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);

                //otherwise give it adrenaline
            }
            else if (mySkeleton.FindBuff<Adrenaline>() == null)
            {
                if (sprite.visible || mySkeleton.sprite.visible)
                    sprite.parent.Add(new Beam.HealthRay(sprite.Center(), mySkeleton.sprite.Center()));

                Buff.Affect<Adrenaline>(mySkeleton, 3f);
            }

            Next();
        }

        private class NecromancerHunting : Mob.Hunting
        {
            public NecromancerHunting(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                Necromancer necromancer = (Necromancer)mob;
                bool callBase = false;

                bool result = necromancer.NecromancerHuntingAct(enemyInFOV, justAlerted, ref callBase);
                if (callBase)
                    return base.Act(enemyInFOV, justAlerted);

                return result;
            }
        }

        public bool NecromancerHuntingAct(bool enemyInFOV, bool justAlerted, ref bool callBase)
        {
            enemySeen = enemyInFOV;

            if (storedSkeletonID != -1)
            {
                Actor ch = Actor.FindById(storedSkeletonID);
                storedSkeletonID = -1;
                if (ch is NecroSkeleton)
                    mySkeleton = (NecroSkeleton)ch;
            }

            if (Summoning)
            {

                //push anything on summoning spot away, to the furthest valid cell
                if (Actor.FindChar(summoningPos) != null)
                {
                    int pushPos = pos;
                    foreach (int c in PathFinder.NEIGHBORS8)
                    {
                        if (Actor.FindChar(summoningPos + c) == null
                                && Dungeon.level.passable[summoningPos + c]
                                && Dungeon.level.TrueDistance(pos, summoningPos + c) > Dungeon.level.TrueDistance(pos, pushPos))
                        {
                            pushPos = summoningPos + c;
                        }
                    }

                    //push enemy, or wait a turn if there is no valid pushing position
                    if (pushPos != pos)
                    {
                        var ch = Actor.FindChar(summoningPos);
                        Actor.AddDelayed(new Pushing(ch, ch.pos, pushPos), -1);

                        ch.pos = pushPos;
                        Dungeon.level.OccupyCell(ch);

                    }
                    else
                    {
                        Spend(TICK);
                        return true;
                    }
                }

                Summoning = firstSummon = false;

                mySkeleton = new NecroSkeleton();
                mySkeleton.pos = summoningPos;
                GameScene.Add(mySkeleton);
                Dungeon.level.OccupyCell(mySkeleton);
                Sample.Instance.Play(Assets.Sounds.BONES);
                summoningEmitter.Burst(Speck.Factory(Speck.RATTLE), 5);
                sprite.Idle();

                if (FindBuff<Corruption>() != null)
                {
                    Buff.Affect<Corruption>(mySkeleton);
                }

                Spend(TICK);
                return true;
            }

            if (mySkeleton != null &&
                    (!mySkeleton.IsAlive()
                    || !Dungeon.level.mobs.Contains(mySkeleton)
                    || mySkeleton.alignment != alignment))
            {
                mySkeleton = null;
            }

            //if enemy is seen, and enemy is within range, and we haven no skeleton, summon a skeleton!
            if (enemySeen && Dungeon.level.Distance(pos, enemy.pos) <= 4 && mySkeleton == null)
            {
                summoningPos = -1;
                foreach (int c in PathFinder.NEIGHBORS8)
                {
                    if (Actor.FindChar(enemy.pos + c) == null
                            && Dungeon.level.passable[enemy.pos + c]
                            && fieldOfView[enemy.pos + c]
                            && Dungeon.level.TrueDistance(pos, enemy.pos + c) < Dungeon.level.TrueDistance(pos, summoningPos))
                    {
                        summoningPos = enemy.pos + c;
                    }
                }

                if (summoningPos != -1)
                {
                    Summoning = true;
                    summoningEmitter = CellEmitter.Get(summoningPos);
                    summoningEmitter.Pour(Speck.Factory(Speck.RATTLE), 0.2f);

                    sprite.Zap(summoningPos);

                    Spend(firstSummon ? TICK : 2 * TICK);
                }
                else
                {
                    //wait for a turn
                    Spend(TICK);
                }

                return true;
                //otherwise, if enemy is seen, and we have a skeleton...
            }
            else if (enemySeen && mySkeleton != null)
            {
                target = enemy.pos;
                Spend(TICK);

                if (!fieldOfView[mySkeleton.pos])
                {

                    //if the skeleton is not next to the _enemy
                    //teleport them to the closest spot next to the _enemy that can be seen
                    if (!Dungeon.level.Adjacent(mySkeleton.pos, enemy.pos))
                    {
                        int telePos = -1;
                        foreach (int c in PathFinder.NEIGHBORS8)
                        {
                            if (Actor.FindChar(enemy.pos + c) == null
                                    && Dungeon.level.passable[enemy.pos + c]
                                    && fieldOfView[enemy.pos + c]
                                    && Dungeon.level.TrueDistance(pos, enemy.pos + c) < Dungeon.level.TrueDistance(pos, telePos))
                            {
                                telePos = enemy.pos + c;
                            }
                        }

                        if (telePos != -1)
                        {
                            ScrollOfTeleportation.Appear(mySkeleton, telePos);
                            mySkeleton.TeleportSpend();

                            if (sprite != null && sprite.visible)
                            {
                                sprite.Zap(telePos);
                                return false;
                            }
                            else
                            {
                                OnZapComplete();
                            }
                        }
                    }

                    return true;

                }
                else
                {
                    //zap skeleton
                    if (mySkeleton.HP < mySkeleton.HT || mySkeleton.FindBuff<Adrenaline>() == null)
                    {
                        if (sprite != null && sprite.visible)
                        {
                            sprite.Zap(mySkeleton.pos);
                            return false;
                        }
                        else
                        {
                            OnZapComplete();
                        }
                    }

                }

                return true;
                //otherwise, default to regular hunting behaviour
            }
            else
            {
                //return base.Act(enemyInFOV, justAlerted);
                callBase = true;
                return true;
            }
        }

        [SPDStatic]
        public class NecroSkeleton : Skeleton
        {
            public NecroSkeleton()
            {
                state = WANDERING;

                spriteClass = typeof(NecroSkeletonSprite);

                //no loot or exp
                maxLvl = -5;

                //20/25 health to start
                HP = 20;
            }

            public override float SpawningWeight()
            {
                return 0;
            }

            public void TeleportSpend()
            {
                Spend(TICK);
            }

            public class NecroSkeletonSprite : SkeletonSprite
            {
                public NecroSkeletonSprite()
                {
                    Brightness(0.75f);
                }

                public override void ResetColor()
                {
                    base.ResetColor();
                    Brightness(0.75f);
                }
            }
        }
    }
}