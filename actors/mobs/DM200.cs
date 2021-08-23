using System;
using watabou.utils;
using spdd.sprites;
using spdd.items;
using spdd.mechanics;
using spdd.actors.blobs;
using spdd.scenes;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class DM200 : Mob
    {
        public DM200()
        {
            spriteClass = typeof(DM200Sprite);

            HP = HT = 70;
            defenseSkill = 8;

            EXP = 9;
            maxLvl = 17;

            loot = Rnd.OneOf(Generator.Category.WEAPON, Generator.Category.ARMOR);
            lootChance = 0.125f; //initially, see rollToDropLoot

            properties.Add(Property.INORGANIC);
            properties.Add(Property.LARGE);

            HUNTING = new HuntingDM200(this);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(10, 25);
        }

        public override int AttackSkill(Character target)
        {
            return 20;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 8);
        }

        public override void RollToDropLoot()
        {
            //each drop makes future drops 1/2 as likely
            // so loot chance looks like: 1/8, 1/16, 1/32, 1/64, etc.
            lootChance *= (float)Math.Pow(1 / 2f, Dungeon.LimitedDrops.DM200_EQUIP.count);
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.DM200_EQUIP.count;
            //uses probability tables for dwarf city
            if (loot is Generator.Category && (Generator.Category)loot == Generator.Category.WEAPON)
            {
                return Generator.RandomWeapon(4);
            }
            else
            {
                return Generator.RandomArmor(4);
            }
        }

        private int ventCooldown = 0;

        private const string VENT_COOLDOWN = "vent_cooldown";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(VENT_COOLDOWN, ventCooldown);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            ventCooldown = bundle.GetInt(VENT_COOLDOWN);
        }

        public override bool Act()
        {
            //ensures toxic gas acts at the appropriate time when added
            //TODO we have this check in 2 places now, can we just ensure that blobs spend an extra turn when added?
            GameScene.Add(Blob.Seed(pos, 0, typeof(ToxicGas)));
            --ventCooldown;
            return base.Act();
        }

        public virtual void OnZapComplete()
        {
            Zap();
            Next();
        }

        private void Zap()
        {
            Spend(TICK);
            ventCooldown = 30;

            Ballistic trajectory = new Ballistic(pos, enemy.pos, Ballistic.STOP_TARGET);

            foreach (int i in trajectory.SubPath(0, trajectory.dist))
            {
                GameScene.Add(Blob.Seed(i, 20, typeof(ToxicGas)));
            }

            GLog.Warning(Messages.Get(this, "vent"));
            GameScene.Add(Blob.Seed(trajectory.collisionPos, 100, typeof(ToxicGas)));
        }

        class HuntingDM200 : Mob.Hunting
        {
            public HuntingDM200(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                var dm200 = (DM200)mob;
                bool callBase = false;

                bool result = dm200.DM200HuntingAct(enemyInFOV, justAlerted, ref callBase);

                if (callBase)
                    return base.Act(enemyInFOV, justAlerted);
                else
                    return result;
            }
        }

        public bool DM200HuntingAct(bool enemyInFOV, bool justAlerted, ref bool callBase)
        {
            if (!enemyInFOV || CanAttack(enemy))
            {
                callBase = true;
                return true;
            }
            else
            {
                enemySeen = true;
                target = enemy.pos;

                int oldPos = pos;

                if (ventCooldown <= 0 && Rnd.Int(100 / Distance(enemy)) == 0)
                {
                    if (sprite != null && (sprite.visible || enemy.sprite.visible))
                    {
                        sprite.Zap(enemy.pos);
                        return false;
                    }
                    else
                    {
                        Zap();
                        return true;
                    }
                }
                else if (GetCloser(target))
                {
                    Spend(1 / Speed());
                    return MoveSprite(oldPos, pos);
                }
                else if (ventCooldown <= 0)
                {
                    if (sprite != null && (sprite.visible || enemy.sprite.visible))
                    {
                        sprite.Zap(enemy.pos);
                        return false;
                    }
                    else
                    {
                        Zap();
                        return true;
                    }
                }
                else
                {
                    Spend(TICK);
                    return true;
                }
            }
        }
    }
}