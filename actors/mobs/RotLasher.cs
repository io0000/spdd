using System;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.sprites;
using spdd.items;

namespace spdd.actors.mobs
{
    public class RotLasher : Mob
    {
        public RotLasher()
        {
            InitInstance();

            spriteClass = typeof(RotLasherSprite);

            HP = HT = 40;
            defenseSkill = 0;

            EXP = 1;

            loot = Generator.Category.SEED;
            lootChance = 1f;

            state = WANDERING = new RotLasherWaiting(this);

            properties.Add(Property.IMMOVABLE);
            properties.Add(Property.MINIBOSS);
        }

        public override bool Act()
        {
            if (enemy == null || !Dungeon.level.Adjacent(pos, enemy.pos))
            {
                HP = Math.Min(HT, HP + 3);
            }
            return base.Act();
        }

        public override void Damage(int dmg, object src)
        {
            if (src is Burning)
            {
                Destroy();
                sprite.Die();
            }
            else
            {
                base.Damage(dmg, src);
            }
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);
            Buff.Affect<Cripple>(enemy, 2f);
            return base.AttackProc(enemy, damage);
        }

        public override bool Reset()
        {
            return true;
        }

        public override bool GetCloser(int target)
        {
            return true;
        }

        public override bool GetFurther(int target)
        {
            return true;
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(8, 15);
        }

        public override int AttackSkill(Character target)
        {
            return 15;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 8);
        }

        private void InitInstance()
        {
            immunities.Add(typeof(ToxicGas));
        }

        class RotLasherWaiting : Mob.Wandering
        {
            public RotLasherWaiting(Mob mob)
                : base(mob)
            { }
        }
    }
}