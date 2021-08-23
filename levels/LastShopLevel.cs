using System.Collections.Generic;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.items;
using spdd.levels.builders;
using spdd.levels.painters;
using spdd.levels.rooms;
using spdd.levels.rooms.standard;
using spdd.messages;

namespace spdd.levels
{
    public class LastShopLevel : RegularLevel
    {
        public LastShopLevel()
        {
            color1 = new Color(0x4b, 0x66, 0x36, 0xff);
            color2 = new Color(0xf2, 0xf2, 0xf2, 0xff);
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_CITY;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_CITY;
        }

        public override bool Build()
        {
            feeling = Feeling.CHASM;
            if (base.Build())
            {
                for (int i = 0; i < Length(); ++i)
                {
                    if (map[i] == Terrain.SECRET_DOOR)
                        map[i] = Terrain.DOOR;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override List<Room> InitRooms()
        {
            List<Room> rooms = new List<Room>();

            rooms.Add(roomEntrance = new EntranceRoom());
            rooms.Add(new ImpShopRoom());
            rooms.Add(roomExit = new ExitRoom());

            return rooms;
        }

        protected override Builder Builder()
        {
            return (new LineBuilder())
                .SetPathVariance(0f)
                .SetPathLength(1f, new float[] { 1 })
                .SetTunnelLength(new float[] { 0, 0, 1 }, new float[] { 1 });
        }

        protected override Painter Painter()
        {
            return (new CityPainter())
                .SetWater(0.10f, 4)
                .SetGrass(0.10f, 3);
        }

        public override Mob CreateMob()
        {
            return null;
        }

        public override void CreateMobs()
        {
        }

        public override Actor AddRespawner()
        {
            return null;
        }

        public override void CreateItems()
        {
            Item item = Bones.Get();
            if (item != null)
            {
                int pos;
                do
                {
                    pos = PointToCell(roomEntrance.Random());
                }
                while (pos == entrance);

                Drop(item, pos).SetHauntedIfCursed().type = Heap.Type.REMAINS;
            }
        }

        public override int RandomRespawnCell(Character ch)
        {
            int cell;
            do
            {
                cell = PointToCell(roomEntrance.Random());
            }
            while (!passable[cell] ||
                     (Character.HasProp(ch, Character.Property.LARGE) && !openSpace[cell]) ||
                     Actor.FindChar(cell) != null);

            return cell;
        }

        public override string TileName(int tile)
        {
            switch (tile)
            {
                case Terrain.WATER:
                    return Messages.Get(typeof(CityLevel), "water_name");
                case Terrain.HIGH_GRASS:
                    return Messages.Get(typeof(CityLevel), "high_grass_name");
                default:
                    return base.TileName(tile);
            }
        }

        public override string TileDesc(int tile)
        {
            switch (tile)
            {
                case Terrain.ENTRANCE:
                    return Messages.Get(typeof(CityLevel), "entrance_desc");
                case Terrain.EXIT:
                    return Messages.Get(typeof(CityLevel), "exit_desc");
                case Terrain.WALL_DECO:
                case Terrain.EMPTY_DECO:
                    return Messages.Get(typeof(CityLevel), "deco_desc");
                case Terrain.EMPTY_SP:
                    return Messages.Get(typeof(CityLevel), "sp_desc");
                case Terrain.STATUE:
                case Terrain.STATUE_SP:
                    return Messages.Get(typeof(CityLevel), "statue_desc");
                case Terrain.BOOKSHELF:
                    return Messages.Get(typeof(CityLevel), "bookshelf_desc");
                default:
                    return base.TileDesc(tile);
            }
        }

        public override Group AddVisuals()
        {
            base.AddVisuals();
            CityLevel.AddCityVisuals(this, visuals);
            return visuals;
        }
    }
}