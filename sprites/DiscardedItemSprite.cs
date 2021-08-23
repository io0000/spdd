using watabou.noosa;
using spdd.tiles;

namespace spdd.sprites
{
    public class DiscardedItemSprite : ItemSprite
    {
        public override void Drop()
        {
            scale.Set(1);
            am = 1;

            if (emitter != null) 
                emitter.KillAndErase();
            
            origin.Set(width / 2, height - DungeonTilemap.SIZE / 2);
            angularSpeed = 720;
        }

        public override void Update()
        {
            base.Update();

            scale.Set(scale.x -= Game.elapsed);
            y += 12 * Game.elapsed;
            if ((am -= Game.elapsed) <= 0)
                Remove();
        }
    }
}