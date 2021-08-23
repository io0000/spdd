using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.sprites;

namespace spdd.items.armor.curses
{
    public class Metabolism : Armor.Glyph
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(6) == 0 && defender is Hero)
            {
                //assumes using up 10% of starving, and healing of 1 hp per 10 turns;
                int healing = Math.Min((int)Hunger.STARVING / 100, defender.HT - defender.HP);

                if (healing > 0)
                {
                    Hunger hunger = Buff.Affect<Hunger>(defender);

                    if (!hunger.IsStarving())
                    {
                        hunger.ReduceHunger(healing * -10);

                        defender.HP += healing;
                        defender.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);
                        defender.sprite.ShowStatus(CharSprite.POSITIVE, healing.ToString());
                    }
                }
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BLACK;
        }

        public override bool Curse()
        {
            return true;
        }
    }
}