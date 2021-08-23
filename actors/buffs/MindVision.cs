using System;
using spdd.ui;
using spdd.scenes;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class MindVision : FlavourBuff
    {
        public const float DURATION = 20.0f;
        public const int DISTANCE = 2;

        public MindVision()
        {
            type = BuffType.POSITIVE;
            announced = true;
        }

        public override int Icon()
        {
            return BuffIndicator.MIND_VISION;
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