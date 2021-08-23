using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfRecharging : Scroll
    {
        public ScrollOfRecharging()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_RECHARGE;
        }

        public override void DoRead()
        {
            Buff.Affect<Recharging>(curUser, Recharging.DURATION);
            Charge(curUser);

            Sample.Instance.Play(Assets.Sounds.READ);
            Sample.Instance.Play(Assets.Sounds.CHARGEUP);

            GLog.Information(Messages.Get(this, "surge"));
            SpellSprite.Show(curUser, SpellSprite.CHARGE);
            SetKnown();

            ReadAnimation();
        }

        public static void Charge(Character user)
        {
            user.sprite.CenterEmitter().Burst(EnergyParticle.Factory, 15);
        }

        public override int Value()
        {
            return IsKnown() ? 30 * quantity : base.Value();
        }
    }
}