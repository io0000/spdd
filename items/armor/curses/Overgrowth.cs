using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.effects;
using spdd.effects.particles;
using spdd.plants;
using spdd.sprites;

namespace spdd.items.armor.curses
{
    public class Overgrowth : Armor.Glyph
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(20) == 0)
            {
                Plant.Seed s;
                do
                {
                    s = (Plant.Seed)Generator.RandomUsingDefaults(Generator.Category.SEED);
                }
                while (s is Starflower.Seed);

                Plant p = s.Couch(defender.pos, null);

                //momentarily revoke warden benefits, otherwise this curse would be incredibly powerful
                if (defender is Hero && ((Hero)defender).subClass == HeroSubClass.WARDEN)
                {
                    ((Hero)defender).subClass = HeroSubClass.NONE;
                    p.Activate(defender);
                    ((Hero)defender).subClass = HeroSubClass.WARDEN;
                }
                else
                {
                    p.Activate(defender);
                }

                CellEmitter.Get(defender.pos).Burst(LeafParticle.LevelSpecific, 10);
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