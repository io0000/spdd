using spdd.items;
using spdd.sprites;
using spdd.messages;
using spdd.effects;
using spdd.effects.particles;

namespace spdd.actors.mobs.npcs
{
    public class ImpShopkeeper : Shopkeeper
    {
        public ImpShopkeeper()
        {
            spriteClass = typeof(ImpSprite);
        }

        private bool seenBefore;

        public override bool Act()
        {
            if (!seenBefore && Dungeon.level.heroFOV[pos])
            {
                Yell(Messages.Get(this, "greetings", Dungeon.hero.Name()));
                seenBefore = true;
            }

            return base.Act();
        }

        public override void Flee()
        {
            foreach (var heap in Dungeon.level.heaps.Values)
            {
                if (heap.type == Heap.Type.FOR_SALE)
                {
                    CellEmitter.Get(heap.pos).Burst(ElmoParticle.Factory, 4);
                    heap.Destroy();
                }
            }

            Destroy();

            sprite.Emitter().Burst(Speck.Factory(Speck.WOOL), 15);
            sprite.KillAndErase();
        }
    }
}