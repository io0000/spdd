using watabou.noosa.audio;
using spdd.sprites;
using spdd.effects;
using spdd.actors.buffs;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfForesight : ExoticScroll
    {
        public ScrollOfForesight()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_FORESIGHT;
        }

        public override void DoRead()
        {
            SpellSprite.Show(curUser, SpellSprite.MAP);
            Sample.Instance.Play(Assets.Sounds.READ);

            Buff.Affect<Foresight>(curUser, Foresight.DURATION);

            SetKnown();

            ReadAnimation();
        }
    }
}