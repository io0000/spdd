using watabou.noosa;
using watabou.noosa.tweeners;
using watabou.utils;
using spdd.levels;
using spdd.levels.traps;
using spdd.plants;

namespace spdd.tiles
{
    //TODO add in a proper set of vfx for plants growing/withering, grass burning, discovering traps
    public class TerrainFeaturesTilemap : DungeonTilemap
    {
        private static TerrainFeaturesTilemap instance;

        private SparseArray<Plant> plants;
        private SparseArray<Trap> traps;

        public TerrainFeaturesTilemap(SparseArray<Plant> plants, SparseArray<Trap> traps)
            : base(Assets.Environment.TERRAIN_FEATURES)
        {
            this.plants = plants;
            this.traps = traps;

            Map(Dungeon.level.map, Dungeon.level.Width());

            instance = this;
        }

        protected override int GetTileVisual(int pos, int tile, bool flat)
        {
            if (traps[pos] != null)
            {
                Trap trap = traps[pos];
                if (!trap.visible)
                {
                    return -1;
                }
                else
                {
                    return (trap.active ? trap.color : Trap.BLACK) + (trap.shape * 16);
                }
            }

            if (plants[pos] != null)
            {
                return plants[pos].image + 7 * 16;
            }

            int stage = (Dungeon.depth - 1) / 5;
            if (Dungeon.depth == 21 && Dungeon.level is LastShopLevel)
            {
                --stage;
            }
            if (tile == Terrain.HIGH_GRASS)
            {
                return 9 + 16 * stage + (DungeonTileSheet.tileVariance[pos] >= 50 ? 1 : 0);
            }
            else if (tile == Terrain.FURROWED_GRASS)
            {
                //TODO
                return 11 + 16 * stage + (DungeonTileSheet.tileVariance[pos] >= 50 ? 1 : 0);
            }
            else if (tile == Terrain.GRASS)
            {
                return 13 + 16 * stage + (DungeonTileSheet.tileVariance[pos] >= 50 ? 1 : 0);
            }
            else if (tile == Terrain.EMBERS)
            {
                return 9 * (16 * 5) + (DungeonTileSheet.tileVariance[pos] >= 50 ? 1 : 0);
            }

            return -1;
        }

        public static Image Tile(int pos, int tile)
        {
            RectF uv = instance.tileset.Get(instance.GetTileVisual(pos, tile, true));
            if (uv == null)
            {
                return null;
            }

            Image img = new Image(instance.texture);
            img.Frame(uv);
            return img;
        }

        public void GrowPlant(int pos)
        {
            Image plant = Tile(pos, map[pos]);
            if (plant == null)
            {
                return;
            }

            plant.origin.Set(8, 12);
            plant.scale.Set(0);
            plant.Point(DungeonTilemap.TileToWorld(pos));

            parent.Add(plant);

            //new ScaleTweener( plant, new PointF(1, 1), 0.2f )
            var tweener = new GrowPlantScaleTweener(plant, new PointF(1, 1), 0.2f);
            tweener.tilemap = this;
            tweener.plant = plant;
            tweener.pos = pos;

            parent.Add(tweener);
        }

        private class GrowPlantScaleTweener : ScaleTweener
        {
            internal TerrainFeaturesTilemap tilemap;
            internal Image plant;
            internal int pos;

            public GrowPlantScaleTweener(Visual visual, PointF scale, float time)
                : base(visual, scale, time)
            { }

            public override void OnComplete()
            {
                plant.KillAndErase();
                KillAndErase();
                tilemap.UpdateMapCell(pos);
            }
        }
    }
}