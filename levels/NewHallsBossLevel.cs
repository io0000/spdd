using System;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.levels.painters;
using spdd.messages;
using spdd.scenes;
using spdd.tiles;

namespace spdd.levels
{
    public class NewHallsBossLevel : Level
    {
        public NewHallsBossLevel()
        {
            color1 = new Color(0x80, 0x15, 0x00, 0xFF);
            color2 = new Color(0xa6, 0x85, 0x21, 0xFF);

            viewDistance = Math.Min(4, viewDistance);
        }

        private const int WIDTH = 32;
        private const int HEIGHT = 32;

        private const int ROOM_LEFT = WIDTH / 2 - 4;
        private const int ROOM_RIGHT = WIDTH / 2 + 4;
        private const int ROOM_TOP = 8;
        private const int ROOM_BOTTOM = ROOM_TOP + 8;

        public override string TilesTex()
        {
            return Assets.Environment.TILES_HALLS;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_HALLS;
        }

        public override bool Build()
        {
            SetSize(WIDTH, HEIGHT);

            for (int i = 0; i < 5; ++i)
            {
                int top;
                int bottom;

                if (i == 0 || i == 4)
                {
                    top = Rnd.IntRange(ROOM_TOP - 1, ROOM_TOP + 3);
                    bottom = Rnd.IntRange(ROOM_BOTTOM + 2, ROOM_BOTTOM + 6);
                }
                else if (i == 1 || i == 3)
                {
                    top = Rnd.IntRange(ROOM_TOP - 5, ROOM_TOP - 1);
                    bottom = Rnd.IntRange(ROOM_BOTTOM + 6, ROOM_BOTTOM + 10);
                }
                else
                {
                    top = Rnd.IntRange(ROOM_TOP - 6, ROOM_TOP - 3);
                    bottom = Rnd.IntRange(ROOM_BOTTOM + 8, ROOM_BOTTOM + 12);
                }

                Painter.Fill(this, 4 + i * 5, top, 5, bottom - top + 1, Terrain.EMPTY);

                if (i == 2)
                {
                    entrance = (6 + i * 5) + (bottom - 1) * Width();
                }
            }

            bool[] patch = Patch.Generate(width, height, 0.20f, 0, true);
            for (int i = 0; i < Length(); ++i)
            {
                if (map[i] == Terrain.EMPTY && patch[i])
                {
                    map[i] = Terrain.STATUE;
                }
            }

            map[entrance] = Terrain.ENTRANCE;

            Painter.Fill(this, ROOM_LEFT - 1, ROOM_TOP - 1, 11, 11, Terrain.EMPTY);

            patch = Patch.Generate(width, height, 0.30f, 3, true);
            for (int i = 0; i < Length(); ++i)
            {
                if ((map[i] == Terrain.EMPTY || map[i] == Terrain.STATUE) && patch[i])
                {
                    map[i] = Terrain.WATER;
                }
            }

            for (int i = 0; i < Length(); ++i)
            {
                if (map[i] == Terrain.EMPTY && Rnd.Int(4) == 0)
                {
                    map[i] = Terrain.EMPTY_DECO;
                }
            }

            Painter.Fill(this, ROOM_LEFT, ROOM_TOP, 9, 9, Terrain.EMPTY_SP);

            Painter.Fill(this, ROOM_LEFT, ROOM_TOP, 9, 2, Terrain.WALL_DECO);
            Painter.Fill(this, ROOM_LEFT, ROOM_BOTTOM - 1, 2, 2, Terrain.WALL_DECO);
            Painter.Fill(this, ROOM_RIGHT - 1, ROOM_BOTTOM - 1, 2, 2, Terrain.WALL_DECO);

            Painter.Fill(this, ROOM_LEFT + 3, ROOM_TOP + 2, 3, 4, Terrain.EMPTY);

            exit = width / 2 + ((ROOM_TOP + 1) * width);

            CustomTilemap vis = new CenterPieceVisuals();
            vis.Pos(ROOM_LEFT, ROOM_TOP + 1);
            customTiles.Add(vis);

            vis = new CenterPieceWalls();
            vis.Pos(ROOM_LEFT, ROOM_TOP);
            customWalls.Add(vis);

            //basic version of building flag maps for the pathfinder test
            for (int i = 0; i < length; ++i)
            {
                passable[i] = (Terrain.flags[map[i]] & Terrain.PASSABLE) != 0;
            }

            //ensures a path to the exit exists
            return (PathFinder.GetStep(entrance, exit, passable) != -1);
        }

        public override void CreateMobs()
        { }

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
                    pos = RandomRespawnCell(null);
                }
                while (pos == entrance);

                Drop(item, pos).SetHauntedIfCursed().type = Heap.Type.REMAINS;
            }
        }

        public override int RandomRespawnCell(Character ch)
        {
            int pos = entrance;
            int cell;
            do
            {
                cell = pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
            }
            while (!passable[cell] ||
                     (Character.HasProp(ch, Character.Property.LARGE) && !openSpace[cell]) ||
                     Actor.FindChar(cell) != null);

            return cell;
        }

        public override void OccupyCell(Character ch)
        {
            base.OccupyCell(ch);

            if (map[entrance] == Terrain.ENTRANCE &&
                map[exit] != Terrain.EXIT &&
                ch == Dungeon.hero &&
                Dungeon.level.Distance(ch.pos, entrance) >= 2)
            {
                Seal();
            }
        }

        public override void Seal()
        {
            base.Seal();
            Set(entrance, Terrain.EMPTY_SP);
            GameScene.UpdateMap(entrance);
            CellEmitter.Get(entrance).Start(FlameParticle.Factory, 0.1f, 10);

            Dungeon.Observe();

            YogDzewa boss = new YogDzewa();
            boss.pos = exit + width * 3;
            GameScene.Add(boss);
        }

        public override void Unseal()
        {
            base.Unseal();
            Set(entrance, Terrain.ENTRANCE);
            GameScene.UpdateMap(entrance);

            Set(exit, Terrain.EXIT);
            GameScene.UpdateMap(exit);

            CellEmitter.Get(exit - 1).Burst(ShadowParticle.Up, 25);
            CellEmitter.Get(exit).Burst(ShadowParticle.Up, 100);
            CellEmitter.Get(exit + 1).Burst(ShadowParticle.Up, 25);
            foreach (CustomTilemap t in customTiles)
            {
                if (t is CenterPieceVisuals)
                {
                    ((CenterPieceVisuals)t).UpdateState();
                }
            }
            foreach (CustomTilemap t in customWalls)
            {
                if (t is CenterPieceWalls)
                {
                    ((CenterPieceWalls)t).UpdateState();
                }
            }

            Dungeon.Observe();
        }

        public override string TileName(int tile)
        {
            switch (tile)
            {
                case Terrain.WATER:
                    return Messages.Get(typeof(HallsLevel), "water_name");
                case Terrain.GRASS:
                    return Messages.Get(typeof(HallsLevel), "grass_name");
                case Terrain.HIGH_GRASS:
                    return Messages.Get(typeof(HallsLevel), "high_grass_name");
                case Terrain.STATUE:
                case Terrain.STATUE_SP:
                    return Messages.Get(typeof(HallsLevel), "statue_name");
                default:
                    return base.TileName(tile);
            }
        }

        public override string TileDesc(int tile)
        {
            switch (tile)
            {
                case Terrain.WATER:
                    return Messages.Get(typeof(HallsLevel), "water_desc");
                case Terrain.STATUE:
                case Terrain.STATUE_SP:
                    return Messages.Get(typeof(HallsLevel), "statue_desc");
                case Terrain.BOOKSHELF:
                    return Messages.Get(typeof(HallsLevel), "bookshelf_desc");
                default:
                    return base.TileDesc(tile);
            }
        }

        public override Group AddVisuals()
        {
            base.AddVisuals();
            HallsLevel.AddHallsVisuals(this, visuals);
            return visuals;
        }

        [SPDStatic]
        public class CenterPieceVisuals : CustomTilemap
        {
            public CenterPieceVisuals()
            {
                texture = Assets.Environment.HALLS_SP;

                tileW = 9;
                tileH = 8;
            }

            private static int[] map = new int[]{
                 8,  9, 10, 11, 11, 11, 12, 13, 14,
                16, 17, 18, 27, 19, 27, 20, 21, 22,
                24, 25, 26, 19, 19, 19, 28, 29, 30,
                24, 25, 26, 19, 19, 19, 28, 29, 30,
                24, 25, 26, 19, 19, 19, 28, 29, 30,
                24, 25, 34, 35, 35, 35, 34, 29, 30,
                40, 41, 36, 36, 36, 36, 36, 40, 41,
                48, 49, 36, 36, 36, 36, 36, 48, 49
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                UpdateState();
                return v;
            }

            public void UpdateState()
            {
                if (vis != null)
                {
                    int[] data = (int[])map.Clone();
                    if (Dungeon.level.map[Dungeon.level.exit] == Terrain.EXIT)
                    {
                        data[4] = 19;
                        data[12] = data[14] = 31;
                    }
                    vis.Map(data, tileW);
                }
            }
        }

        [SPDStatic]
        public class CenterPieceWalls : CustomTilemap
        {
            public CenterPieceWalls()
            {
                texture = Assets.Environment.HALLS_SP;

                tileW = 9;
                tileH = 9;
            }

            private static int[] map = new int[]{
                -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1,
                32, 33, -1, -1, -1, -1, -1, 32, 33,
                40, 41, -1, -1, -1, -1, -1, 40, 41
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                UpdateState();
                return v;
            }

            public void UpdateState()
            {
                if (vis != null)
                {
                    int[] data = (int[])map.Clone();
                    if (Dungeon.level.map[Dungeon.level.exit] == Terrain.EXIT)
                    {
                        data[3] = 1;
                        data[4] = 0;
                        data[5] = 2;
                        data[13] = 23;
                    }
                    vis.Map(data, tileW);
                }
            }
        }
    }
}
