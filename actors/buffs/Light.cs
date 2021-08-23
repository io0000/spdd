using System;
using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Light : FlavourBuff
    {
        public Light()
        {
            type = BuffType.POSITIVE;
        }

        public const float DURATION = 250.0f;
        public const int DISTANCE = 6;

        public override bool AttachTo(Character target)
        {
            if (!base.AttachTo(target))
                return false;

            if (Dungeon.level != null)
            {
                target.viewDistance = Math.Max(Dungeon.level.viewDistance, DISTANCE);
                Dungeon.Observe();
            }

            return true;
        }

        public override void Detach()
        {
            target.viewDistance = Dungeon.level.viewDistance;
            Dungeon.Observe();
            base.Detach();
        }

        public void Weaken(int amount)
        {
            Spend(-amount);
        }

        public override int Icon()
        {
            return BuffIndicator.LIGHT;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override void Fx(bool on)
        {
            if (on)
                target.sprite.Add(CharSprite.State.ILLUMINATED);
            else
                target.sprite.Remove(CharSprite.State.ILLUMINATED);
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