using System;
using watabou.noosa;
using spdd.ui;
using spdd.scenes;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class MagicalSight : FlavourBuff
    {
        public const float DURATION = 50.0f;

        //public int distance = 8;

        public MagicalSight()
        {
            type = BuffType.POSITIVE;
            announced = true;
        }

        public override int Icon()
        {
            return BuffIndicator.MIND_VISION;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(1f, 1.67f, 1f);
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override void Detach()
        {
            base.Detach();
            Dungeon.Observe();
            GameScene.UpdateFog();
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns());
        }
    }
}