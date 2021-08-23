using watabou.noosa.particles;
using spdd.scenes;
using spdd.tiles;

namespace spdd.effects
{
    public class CellEmitter
    {
        public static Emitter Floor(int cell)
        {
            var p = DungeonTilemap.TileToWorld(cell);

            var emitter = GameScene.FloorEmitter();
            emitter.Pos(p.x, p.y, DungeonTilemap.SIZE, DungeonTilemap.SIZE);

            return emitter;
        }

        public static Emitter Get(int cell)
        {
            var p = DungeonTilemap.TileToWorld(cell);

            var emitter = GameScene.GetEmitter();
            emitter.Pos(p.x, p.y, DungeonTilemap.SIZE, DungeonTilemap.SIZE);

            return emitter;
        }

        public static Emitter Center(int cell)
        {
            var p = DungeonTilemap.TileToWorld(cell);

            var emitter = GameScene.GetEmitter();
            emitter.Pos(p.x + DungeonTilemap.SIZE / 2, p.y + DungeonTilemap.SIZE / 2);

            return emitter;
        }

        public static Emitter Bottom(int cell)
        {
            var p = DungeonTilemap.TileToWorld(cell);

            var emitter = GameScene.GetEmitter();
            emitter.Pos(p.x, p.y + DungeonTilemap.SIZE, DungeonTilemap.SIZE, 0);

            return emitter;
        }
    }
}