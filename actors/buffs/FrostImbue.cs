using System;
using spdd.ui;
using spdd.effects.particles;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class FrostImbue : FlavourBuff
    {
        public FrostImbue()
        {
            type = BuffType.POSITIVE;
            announced = true;

            immunities.Add(typeof(Frost));
            immunities.Add(typeof(Chill));
        }

        public const float DURATION = 50f;

        public void Proc(Character enemy)
        {
            Buff.Affect<Chill>(enemy, 2.0f);
            enemy.sprite.Emitter().Burst(SnowParticle.Factory, 2);
        }

        public override int Icon()
        {
            return BuffIndicator.FROST;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
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