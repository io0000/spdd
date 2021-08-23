using spdd.sprites;

namespace spdd.items.quest
{
    public class MetalShard : Item
    {
        public MetalShard()
        {
            image = ItemSpriteSheet.SHARD;
            stackable = true;
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override int Value()
        {
            return quantity * 100;
        }
    }
}