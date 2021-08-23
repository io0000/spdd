using watabou.utils;
using spdd.sprites;

namespace spdd.items.quest
{
    // Embers - À×°ÉºÒ
    public class Embers : Item
    {
        public Embers()
        {
            image = ItemSpriteSheet.EMBER;

            unique = true;
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return new ItemSprite.Glowing(new Color(0x66, 0x00, 0x00, 0xFF), 3f);
        }
    }
}