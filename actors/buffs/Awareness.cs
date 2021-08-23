using spdd.scenes;

namespace spdd.actors.buffs
{
    public class Awareness : FlavourBuff
    {
        public Awareness()
        {
            type = BuffType.POSITIVE;
        }
        
        public const float DURATION = 2.0f;

        public override void Detach()
        {
            base.Detach();
            Dungeon.Observe();
            GameScene.UpdateFog();
        }
    }
}