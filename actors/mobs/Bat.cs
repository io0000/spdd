using System;
using watabou.utils;
using spdd.effects;
using spdd.items;
using spdd.items.potions;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Bat : Mob
    {
        public Bat()
        {
            spriteClass = typeof(BatSprite);

            HP = HT = 30;
            defenseSkill = 15;
            baseSpeed = 2f;

            EXP = 7;
            maxLvl = 15;

            flying = true;

            loot = new PotionOfHealing();
            lootChance = 0.1667f; //by default, see rollToDropLoot()
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(5, 15);
        }

        public override int AttackSkill(Character target)
        {
            return 16;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 4);
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);

            var reg = Math.Min(damage - 4, HT - HP);

            if (reg > 0)
            {
                HP += reg;
                sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);
            }
            return damage;
        }

        public override void RollToDropLoot()
        {
            lootChance *= ((7f - Dungeon.LimitedDrops.BAT_HP.count) / 7f);
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.BAT_HP.count;
            return base.CreateLoot();
        }
    }
}