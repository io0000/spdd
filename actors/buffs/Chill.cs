using System;
using System.Globalization;
using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Chill : FlavourBuff
    {
        public const float DURATION = 10f;

        public Chill()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override bool AttachTo(Character target)
        {
            //can't chill what's frozen!
            if (target.FindBuff<Frost>() != null)
                return false;

            if (base.AttachTo(target))
            {
                Buff.Detach<Burning>(target);
                return true;
            }
            else
            {
                return false;
            }
        }

        //reduces speed by 10% for every turn remaining, capping at 50%
        public float SpeedFactor()
        {
            return Math.Max(0.5f, 1 - Cooldown() * 0.1f);
        }

        public override int Icon()
        {
            return BuffIndicator.FROST;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override void Fx(bool on)
        {
            if (on) 
                target.sprite.Add(CharSprite.State.CHILLED);
            else 
                target.sprite.Remove(CharSprite.State.CHILLED);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            float value = (1.0f - SpeedFactor()) * 100.0f;
            string str = value.ToString("0.00", CultureInfo.InvariantCulture);

            return Messages.Get(this, "desc", DispTurns(), str);
        }
    }
}