using watabou.noosa.audio;
using spdd.sprites;
using spdd.effects;
using spdd.actors.buffs;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfMysticalEnergy : ExoticScroll
    {
        public ScrollOfMysticalEnergy()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_MYSTENRG;
        }

        public override void DoRead()
        {
            //append buff
            Buff.Affect<ArtifactRecharge>(curUser).Set(30);

            Sample.Instance.Play(Assets.Sounds.READ);
            Sample.Instance.Play(Assets.Sounds.CHARGEUP);

            SpellSprite.Show(curUser, SpellSprite.CHARGE);
            SetKnown();
            ScrollOfRecharging.Charge(curUser);

            ReadAnimation();
        }
    }
}