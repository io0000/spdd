using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.sprites;

namespace spdd.items.armor.curses
{
    public class Corrosion : Armor.Glyph
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(10) == 0)
            {
                int pos = defender.pos;
                foreach (int i in PathFinder.NEIGHBORS9)
                {
                    Splash.At(pos + i, new Color(0x00, 0x00, 0x00, 0xFF), 5);
                    if (Actor.FindChar(pos + i) != null)
                        Buff.Affect<Ooze>(Actor.FindChar(pos + i)).Set(Ooze.DURATION);
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