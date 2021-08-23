using System;
using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Levitation : FlavourBuff
    {
        public Levitation()
        {
            type = BuffType.POSITIVE;
        }

        public const float DURATION = 20.0f;

        public override bool AttachTo(Character target)
        {
            if (base.AttachTo(target))
            {
                target.flying = true;
                Buff.Detach<Roots>(target);
                return true;
            }
            
            return false;
        }

        public override void Detach()
        {
            target.flying = false;
            base.Detach();
            Dungeon.level.OccupyCell(target);
        }

        public override int Icon()
        {
            return BuffIndicator.LEVITATION;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override void Fx(bool on)
        {
            if (on) 
                target.sprite.Add(CharSprite.State.LEVITATING);
            else 
                target.sprite.Remove(CharSprite.State.LEVITATING);
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