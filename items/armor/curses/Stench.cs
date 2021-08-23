using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items.armor.curses
{
    public class Stench : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0x00));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(8) == 0)
            {
                GameScene.Add(Blob.Seed(defender.pos, 250, typeof(ToxicGas)));
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