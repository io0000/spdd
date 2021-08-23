using System;
using spdd.ui;
using spdd.effects;
using spdd.effects.particles;
using spdd.messages;

namespace spdd.actors.buffs
{
    //pre-0.7.0, otherwise unused
    public class EarthImbue : FlavourBuff
    {
        public EarthImbue()
        {
            type = BuffType.POSITIVE;
            announced = true;
        }

        public const float DURATION = 30f;

        public void Proc(Character enemy)
        {
            Buff.Affect<Cripple>(enemy, 2);
            CellEmitter.Bottom(enemy.pos).Start(EarthParticle.Factory, 0.05f, 8);
        }

        public override int Icon()
        {
            return BuffIndicator.ROOTS;
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