using System;
using watabou.noosa;
using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class SoulMark : FlavourBuff
    {
        public const float DURATION = 10f;

        public SoulMark()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override int Icon()
        {
            return BuffIndicator.CORRUPT;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(0.5f, 0.5f, 0.5f);
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override void Fx(bool on)
        {
            if (on)
                target.sprite.Add(CharSprite.State.MARKED);
            else
                target.sprite.Remove(CharSprite.State.MARKED);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns());
        }
    }
}