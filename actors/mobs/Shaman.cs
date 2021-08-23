using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.items;
using spdd.mechanics;
using spdd.sprites;
using spdd.utils;
using spdd.actors.buffs;
using spdd.messages;

namespace spdd.actors.mobs
{
    //TODO stats on these might be a bit weak
    public abstract class Shaman : Mob
    {
        public Shaman()
        {
            HP = HT = 35;
            defenseSkill = 15;

            EXP = 8;
            maxLvl = 16;

            loot = Generator.Category.WAND;
            lootChance = 0.03f; //initially, see rollToDropLoot
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(5, 10);
        }

        public override int AttackSkill(Character target)
        {
            return 18;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 6);
        }

        protected override bool CanAttack(Character enemy)
        {
            return new Ballistic(pos, enemy.pos, Ballistic.MAGIC_BOLT).collisionPos == enemy.pos;
        }

        public override void RollToDropLoot()
        {
            //each drop makes future drops 1/3 as likely
            // so loot chance looks like: 1/33, 1/100, 1/300, 1/900, etc.
            lootChance *= (float)Math.Pow(1 / 3f, Dungeon.LimitedDrops.SHAMAN_WAND.count);
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.SHAMAN_WAND.count;
            return base.CreateLoot();
        }

        protected override bool DoAttack(Character enemy)
        {
            if (Dungeon.level.Adjacent(pos, enemy.pos))
            {
                return base.DoAttack(enemy);
            }
            else
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
        }

        //used so resistances can differentiate between melee and magical attacks
        public class EarthenBolt
        { }

        private void Zap()
        {
            Spend(1f);

            if (Hit(this, enemy, true))
            {
                if (enemy == Dungeon.hero && Rnd.Int(2) == 0)
                {
                    Debuff(enemy);
                    Sample.Instance.Play(Assets.Sounds.DEBUFF);
                }

                int dmg = Rnd.NormalIntRange(6, 15);
                enemy.Damage(dmg, new EarthenBolt());

                if (!enemy.IsAlive() && enemy == Dungeon.hero)
                {
                    Dungeon.Fail(GetType());
                    GLog.Negative(Messages.Get(this, "bolt_kill"));
                }
            }
            else
            {
                enemy.sprite.ShowStatus(CharSprite.NEUTRAL, enemy.DefenseVerb());
            }
        }

        protected abstract void Debuff(Character enemy);

        public void OnZapComplete()
        {
            Zap();
            Next();
        }

        public override string Description()
        {
            return base.Description() + "\n\n" + Messages.Get(this, "spell_desc");
        }

        [SPDStatic]
        public class RedShaman : Shaman
        {
            public RedShaman()
            {
                spriteClass = typeof(ShamanSprite.Red);
            }

            protected override void Debuff(Character enemy)
            {
                Buff.Prolong<Weakness>(enemy, Weakness.DURATION);
            }
        }

        [SPDStatic]
        public class BlueShaman : Shaman
        {
            public BlueShaman()
            {
                spriteClass = typeof(ShamanSprite.Blue);
            }

            protected override void Debuff(Character enemy)
            {
                Buff.Prolong<Vulnerable>(enemy, Vulnerable.DURATION);
            }
        }

        [SPDStatic]
        public class PurpleShaman : Shaman
        {
            public PurpleShaman()
            {
                spriteClass = typeof(ShamanSprite.Purple);
            }

            protected override void Debuff(Character enemy)
            {
                Buff.Prolong<Hex>(enemy, Hex.DURATION);
            }
        }

        public static Type Random()
        {
            float roll = Rnd.Float();
            if (roll < 0.4f)
            {
                return typeof(RedShaman);
            }
            else if (roll < 0.8f)
            {
                return typeof(BlueShaman);
            }
            else
            {
                return typeof(PurpleShaman);
            }
        }
    }
}