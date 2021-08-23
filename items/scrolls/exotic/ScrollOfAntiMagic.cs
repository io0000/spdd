using watabou.utils;
using spdd.sprites;
using spdd.effects;
using spdd.actors.buffs;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfAntiMagic : ExoticScroll
    {
        public ScrollOfAntiMagic()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_ANTIMAGIC;
        }

        public override void DoRead()
        {
            Buff.Affect<MagicImmune>(curUser, MagicImmune.DURATION);
            new Flare(5, 32).Color(new Color(0xFF, 0x00, 0x00, 0xFF), true).Show(curUser.sprite, 2f);

            SetKnown();

            ReadAnimation();
        }
    }
}