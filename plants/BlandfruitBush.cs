using watabou.utils;
using spdd.actors;
using spdd.items.food;

namespace spdd.plants
{
    public class BlandfruitBush : Plant
    {
        public BlandfruitBush()
        {
            image = 12;
        }

        public override void Activate(Character ch)
        {
            Dungeon.level.Drop(new Blandfruit(), pos).sprite.Drop();
        }

        //seed is never dropped
        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                plantClass = typeof(BlandfruitBush);
            }
        }
    }
}