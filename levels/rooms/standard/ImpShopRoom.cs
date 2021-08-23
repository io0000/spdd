using watabou.utils;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.levels.painters;
using spdd.levels.rooms.special;
using spdd.scenes;

namespace spdd.levels.rooms.standard
{
    //shops probably shouldn't extend special room, because of cases like this.
    public class ImpShopRoom : ShopRoom
    {
        private bool impSpawned = false;

        //force a certain size here to guarantee enough room for 48 items, and the same center space
        public override int MinWidth()
        {
            return 9;
        }
        public override int MinHeight()
        {
            return 9;
        }
        public override int MaxWidth()
        {
            return 9;
        }
        public override int MaxHeight()
        {
            return 9;
        }

        public override int MaxConnections(int direction)
        {
            return 2;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);
            Painter.Fill(level, this, 3, Terrain.WATER);

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }

            if (Imp.Quest.IsCompleted())
                SpawnShop(level);
            else
                impSpawned = false;
        }

        protected override void PlaceShopkeeper(Level level)
        {
            int pos = level.PointToCell(Center());

            foreach (Point p in GetPoints())
            {
                if (level.map[level.PointToCell(p)] == Terrain.PEDESTAL)
                {
                    pos = level.PointToCell(p);
                    break;
                }
            }

            Mob shopkeeper = new ImpShopkeeper();
            shopkeeper.pos = pos;
            if (ShatteredPixelDungeonDash.Scene() is GameScene)
            {
                GameScene.Add(shopkeeper);
            }
            else
            {
                level.mobs.Add(shopkeeper);
            }
        }

        //fix for connections not being bundled normally
        public override Door Entrance()
        {
            return connected.Count == 0 ? new Door(left, top + 2) : base.Entrance();
        }

        public void SpawnShop(Level level)
        {
            impSpawned = true;
            PlaceShopkeeper(level);
            PlaceItems(level);
        }

        public bool ShopSpawned()
        {
            return impSpawned;
        }

        private const string IMP = "imp_spawned";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(IMP, impSpawned);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            impSpawned = bundle.GetBoolean(IMP);
        }

        public override void OnLevelLoad(Level level)
        {
            base.OnLevelLoad(level);

            if (Imp.Quest.IsCompleted() && !impSpawned)
            {
                SpawnShop(level);
            }
        }
    }
}