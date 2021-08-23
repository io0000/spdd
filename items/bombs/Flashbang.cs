using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.scenes;

namespace spdd.items.bombs
{
    public class Flashbang : Bomb
    {
        public Flashbang()
        {
            image = ItemSpriteSheet.FLASHBANG;
        }

        public override void Explode(int cell)
        {
            base.Explode(cell);

            //Level l = Dungeon.Level;
            foreach (var ch in Actor.Chars())
            {
                if (ch.fieldOfView != null && ch.fieldOfView[cell])
                {
                    int power = 16 - 4 * Dungeon.level.Distance(ch.pos, cell);
                    if (power > 0)
                    {
                        Buff.Prolong<Blindness>(ch, power);
                        Buff.Prolong<Cripple>(ch, power);
                    }

                    if (ch == Dungeon.hero)
                        GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                }
            }
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 40);
        }
    }
}