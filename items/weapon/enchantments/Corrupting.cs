using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.messages;
using spdd.sprites;


namespace spdd.items.weapon.enchantments
{
    public class Corrupting : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x44, 0x00, 0x66, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            int level = Math.Max(0, weapon.BuffedLvl());

            // lvl 0 - 20%
            // lvl 1 ~ 23%
            // lvl 2 ~ 26%
            if (damage >= defender.HP &&
                Rnd.Int(level + 25) >= 20 &&
                !defender.IsImmune(typeof(Corruption)) &&
                defender.FindBuff<Corruption>() == null &&
                defender is Mob &&
                defender.IsAlive())
            {
                Mob enemy = (Mob)defender;
                Hero hero = (attacker is Hero) ? (Hero)attacker : Dungeon.hero;

                enemy.HP = enemy.HT;
                foreach (Buff buff in enemy.Buffs())
                {
                    if (buff.type == Buff.BuffType.NEGATIVE && !(buff is SoulMark))
                    {
                        buff.Detach();
                    }
                    else if (buff is PinCushion)
                    {
                        buff.Detach();
                    }
                }
                if (enemy.alignment == Character.Alignment.ENEMY)
                {
                    enemy.RollToDropLoot();
                }

                Buff.Affect<Corruption>(enemy);

                ++Statistics.enemiesSlain;
                BadgesExtensions.ValidateMonstersSlain();
                Statistics.qualifiedForNoKilling = false;
                if (enemy.EXP > 0 && hero.lvl <= enemy.maxLvl)
                {
                    hero.sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(enemy, "exp", enemy.EXP));
                    hero.EarnExp(enemy.EXP, enemy.GetType());
                }
                else
                {
                    hero.EarnExp(0, enemy.GetType());
                }

                return 0;
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BLACK;
        }
    }
}