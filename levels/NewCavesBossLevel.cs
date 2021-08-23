using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.mobs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.levels.painters;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.tiles;
using spdd.utils;

namespace spdd.levels
{
    public class NewCavesBossLevel : Level
    {
        public NewCavesBossLevel()
        {
            color1 = new Color(0x53, 0x4f, 0x3e, 0xff);
            color2 = new Color(0xb9, 0xd6, 0x61, 0xff);
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_CAVES;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_HALLS;
        }

        private const int WIDTH = 33;
        private const int HEIGHT = 42;

        public static Rect mainArena = new Rect(5, 14, 28, 37);
        public static Rect gate = new Rect(14, 13, 19, 14);
        public static int[] pylonPositions = new int[] { 4 + 13 * WIDTH, 28 + 13 * WIDTH, 4 + 37 * WIDTH, 28 + 37 * WIDTH };

        private ArenaVisuals customArenaVisuals;

        public override bool Build()
        {
            SetSize(WIDTH, HEIGHT);

            //These signs are visually overridden with custom tile visuals
            Painter.Fill(this, gate, Terrain.SIGN);

            //set up main boss arena
            Painter.FillEllipse(this, mainArena, Terrain.EMPTY);

            bool[] patch = Patch.Generate(width, height - 14, 0.15f, 2, true);
            for (int i = 14 * Width(); i < Length(); ++i)
            {
                if (map[i] == Terrain.EMPTY)
                {
                    if (patch[i - 14 * Width()])
                    {
                        map[i] = Terrain.WATER;
                    }
                    else if (Rnd.Int(8) == 0)
                    {
                        map[i] = Terrain.INACTIVE_TRAP;
                    }
                }
            }

            BuildEntrance();
            BuildCorners();

            new CavesPainter().Paint(this, null);

            //setup exit area above main boss arena
            Painter.Fill(this, 0, 3, Width(), 4, Terrain.CHASM);
            Painter.Fill(this, 6, 7, 21, 1, Terrain.CHASM);
            Painter.Fill(this, 10, 8, 13, 1, Terrain.CHASM);
            Painter.Fill(this, 12, 9, 9, 1, Terrain.CHASM);
            Painter.Fill(this, 13, 10, 7, 1, Terrain.CHASM);
            Painter.Fill(this, 14, 3, 5, 10, Terrain.EMPTY);

            //fill in special floor, statues, and exits
            Painter.Fill(this, 15, 2, 3, 3, Terrain.EMPTY_SP);
            Painter.Fill(this, 15, 5, 3, 1, Terrain.STATUE);
            Painter.Fill(this, 15, 7, 3, 1, Terrain.STATUE);
            Painter.Fill(this, 15, 9, 3, 1, Terrain.STATUE);
            Painter.Fill(this, 16, 5, 1, 6, Terrain.EMPTY_SP);
            Painter.Fill(this, 15, 0, 3, 3, Terrain.EXIT);

            exit = 16 + 2 * Width();

            CustomTilemap customVisuals = new CityEntrance();
            customVisuals.SetRect(0, 0, Width(), 11);
            customTiles.Add(customVisuals);

            customVisuals = new EntranceOverhang();
            customVisuals.SetRect(0, 0, Width(), 11);
            customWalls.Add(customVisuals);

            customVisuals = customArenaVisuals = new ArenaVisuals();
            customVisuals.SetRect(0, 12, Width(), 27);
            customTiles.Add(customVisuals);

            return true;
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            foreach (CustomTilemap c in customTiles)
            {
                if (c is ArenaVisuals)
                {
                    customArenaVisuals = (ArenaVisuals)c;
                }
            }

            //pre-0.8.1 saves that may not have had pylons added
            int gatePos = PointToCell(new Point(gate.left, gate.top));
            if (!locked && solid[gatePos])
            {
                foreach (int i in pylonPositions)
                {
                    if (FindMob(i) == null)
                    {
                        Pylon pylon = new Pylon();
                        pylon.pos = i;
                        mobs.Add(pylon);
                    }
                }
            }
        }

        public override void CreateMobs()
        {
            foreach (int i in pylonPositions)
            {
                Pylon pylon = new Pylon();
                pylon.pos = i;
                mobs.Add(pylon);
            }
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
                    pos = RandomRespawnCell(null);
                }
                while (pos == entrance);

                Drop(item, pos).SetHauntedIfCursed().type = Heap.Type.REMAINS;
            }
        }

        public override int RandomRespawnCell(Character ch)
        {
            //this check is mainly here for DM-300, to prevent an infinite loop
            if (Character.HasProp(ch, Character.Property.LARGE) && map[entrance] != Terrain.ENTRANCE)
            {
                return -1;
            }
            int cell;
            do
            {
                cell = entrance + PathFinder.NEIGHBORS8[Rnd.Int(8)];
            }
            while (!passable[cell] ||
                     (Character.HasProp(ch, Character.Property.LARGE) && !openSpace[cell]) ||
                     Actor.FindChar(cell) != null);

            return cell;
        }


        public override bool SetCellToWater(bool includeTraps, int cell)
        {
            foreach (int i in pylonPositions)
            {
                if (Dungeon.level.Distance(cell, i) <= 1)
                {
                    return false;
                }
            }

            return base.SetCellToWater(includeTraps, cell);
        }

        public override void OccupyCell(Character ch)
        {
            base.OccupyCell(ch);

            //seal the level when the hero moves near to a pylon, the level isn't already sealed, and the gate hasn't been destroyed
            int gatePos = PointToCell(new Point(gate.left, gate.top));
            if (ch == Dungeon.hero && !locked && solid[gatePos])
            {
                foreach (int pos in pylonPositions)
                {
                    if (Dungeon.level.Distance(ch.pos, pos) <= 3)
                    {
                        Seal();
                        break;
                    }
                }
            }
        }

        public override void Seal()
        {
            base.Seal();

            Set(entrance, Terrain.WALL);

            Heap heap = Dungeon.level.heaps[entrance];
            if (heap != null)
            {
                int n;
                do
                {
                    n = entrance + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                }
                while (!Dungeon.level.passable[n]);

                Dungeon.level.Drop(heap.PickUp(), n).sprite.Drop(entrance);
            }

            Character ch = Actor.FindChar(entrance);
            if (ch != null)
            {
                int n;
                do
                {
                    n = entrance + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                }
                while (!Dungeon.level.passable[n]);

                ch.pos = n;
                ch.sprite.Place(n);
            }

            GameScene.UpdateMap(entrance);
            Dungeon.Observe();

            CellEmitter.Get(entrance).Start(Speck.Factory(Speck.ROCK), 0.07f, 10);
            Camera.main.Shake(3, 0.7f);
            Sample.Instance.Play(Assets.Sounds.ROCKS);

            NewDM300 boss = new NewDM300();
            boss.state = boss.WANDERING;
            do
            {
                boss.pos = PointToCell(Rnd.Element(mainArena.GetPoints()));
            }
            while (!openSpace[boss.pos] || map[boss.pos] == Terrain.EMPTY_SP);

            GameScene.Add(boss);
        }

        public override void Unseal()
        {
            base.Unseal();

            GetBlob(typeof(PylonEnergy)).FullyClear();

            Set(entrance, Terrain.ENTRANCE);
            int i = 14 + 13 * Width();
            for (int j = 0; j < 5; ++j)
            {
                Set(i + j, Terrain.EMPTY);
                if (Dungeon.level.heroFOV[i + j])
                {
                    CellEmitter.Get(i + j).Burst(BlastParticle.Factory, 10);
                }
            }
            GameScene.UpdateMap();

            customArenaVisuals.UpdateState();

            Dungeon.Observe();
        }

        public void ActivatePylon()
        {
            List<Pylon> pylons = new List<Pylon>();
            foreach (Mob m in mobs)
            {
                if (m is Pylon && m.alignment == Character.Alignment.NEUTRAL)
                {
                    pylons.Add((Pylon)m);
                }
            }

            if (pylons.Count == 1)
            {
                pylons[0].Activate();
            }
            else if (pylons.Count > 0)
            {
                Pylon closest = null;
                foreach (Pylon p in pylons)
                {
                    if (closest == null || TrueDistance(p.pos, Dungeon.hero.pos) < TrueDistance(closest.pos, Dungeon.hero.pos))
                    {
                        closest = p;
                    }
                }
                pylons.Remove(closest);
                Rnd.Element(pylons).Activate();
            }

            for (int i = (mainArena.top - 1) * width; i < length; ++i)
            {
                if (map[i] == Terrain.INACTIVE_TRAP || map[i] == Terrain.WATER || map[i] == Terrain.SIGN)
                {
                    GameScene.Add(Blob.Seed(i, 1, typeof(PylonEnergy)));
                }
            }
        }

        public void EliminatePylon()
        {
            customArenaVisuals.UpdateState();
            int pylonsRemaining = 0;
            foreach (Mob m in mobs)
            {
                if (m is NewDM300)
                {
                    ((NewDM300)m).LoseSupercharge();
                    PylonEnergy.energySourceSprite = m.sprite;
                }
                else if (m is Pylon)
                {
                    ++pylonsRemaining;
                }
            }

            if (pylonsRemaining > 2)
                GetBlob(typeof(PylonEnergy)).FullyClear();
        }

        public override string TileName(int tile)
        {
            switch (tile)
            {
                case Terrain.GRASS:
                    return Messages.Get(typeof(CavesLevel), "grass_name");
                case Terrain.HIGH_GRASS:
                    return Messages.Get(typeof(CavesLevel), "high_grass_name");
                case Terrain.WATER:
                    return Messages.Get(typeof(CavesLevel), "water_name");
                case Terrain.STATUE:
                    //city statues are used
                    return Messages.Get(typeof(CityLevel), "statue_name");
                default:
                    return base.TileName(tile);
            }
        }

        public override string TileDesc(int tile)
        {
            switch (tile)
            {
                case Terrain.WATER:
                    return base.TileDesc(tile) + "\n\n" + Messages.Get(typeof(NewCavesBossLevel), "water_desc");
                case Terrain.ENTRANCE:
                    return Messages.Get(typeof(CavesLevel), "entrance_desc");
                case Terrain.EXIT:
                    //city exit is used
                    return Messages.Get(typeof(CityLevel), "exit_desc");
                case Terrain.HIGH_GRASS:
                    return Messages.Get(typeof(CavesLevel), "high_grass_desc");
                case Terrain.WALL_DECO:
                    return Messages.Get(typeof(CavesLevel), "wall_deco_desc");
                case Terrain.BOOKSHELF:
                    return Messages.Get(typeof(CavesLevel), "bookshelf_desc");
                //city statues are used
                case Terrain.STATUE:
                    return Messages.Get(typeof(CityLevel), "statue_desc");
                default:
                    return base.TileDesc(tile);
            }
        }

        public override Group AddVisuals()
        {
            base.AddVisuals();
            CavesLevel.AddCavesVisuals(this, visuals);
            return visuals;
        }

        /**
         * semi-randomized setup for entrance and corners
         */

        private const short n = -1; //used when a tile shouldn't be changed
        private const short W = Terrain.WALL;
        private const short e = Terrain.EMPTY;
        private const short s = Terrain.EMPTY_SP;

        private static short[] entrance1 = {
            n, n, n, n, n, n, n, n,
            n, n, n, n, n, n, n, n,
            n, n, n, n, W, e, W, W,
            n, n, n, W, W, e, W, W,
            n, n, W, W, e, e, e, e,
            n, n, e, e, e, W, W, e,
            n, n, W, W, e, W, e, e,
            n, n, W, W, e, e, e, e
        };

        private static short[] entrance2 = {
            n, n, n, n, n, n, n, n,
            n, n, n, n, n, n, n, n,
            n, n, n, n, n, e, e, e,
            n, n, n, W, e, W, W, e,
            n, n, n, e, e, e, e, e,
            n, n, e, W, e, W, W, e,
            n, n, e, W, e, W, e, e,
            n, n, e, e, e, e, e, e
        };

        private static short[] entrance3 = {
            n, n, n, n, n, n, n, n,
            n, n, n, n, n, n, n, n,
            n, n, n, n, n, n, n, n,
            n, n, n, W, W, e, W, W,
            n, n, n, W, W, e, W, W,
            n, n, n, e, e, e, e, e,
            n, n, n, W, W, e, W, e,
            n, n, n, W, W, e, e, e
        };

        private static short[] entrance4 = {
            n, n, n, n, n, n, n, n,
            n, n, n, n, n, n, n, e,
            n, n, n, n, n, n, W, e,
            n, n, n, n, n, W, W, e,
            n, n, n, n, W, W, W, e,
            n, n, n, W, W, W, W, e,
            n, n, W, W, W, W, e, e,
            n, e, e, e, e, e, e, e
        };

        private static short[][] entranceVariants = {
            entrance1,
            entrance2,
            entrance3,
            entrance4
        };

        private void BuildEntrance()
        {
            entrance = 16 + 25 * Width();

            //entrance area
            int NW = entrance - 7 - 7 * Width();
            int NE = entrance + 7 - 7 * Width();
            int SE = entrance + 7 + 7 * Width();
            int SW = entrance - 7 + 7 * Width();

            short[] entranceTiles = Rnd.OneOf(entranceVariants);
            for (int i = 0; i < entranceTiles.Length; ++i)
            {
                if (i % 8 == 0 && i != 0)
                {
                    NW += (Width() - 8);
                    NE += (Width() + 8);
                    SE -= (Width() - 8);
                    SW -= (Width() + 8);
                }

                if (entranceTiles[i] != n)
                {
                    map[NW] = map[NE] = map[SE] = map[SW] = entranceTiles[i];
                }

                ++NW; 
                --NE; 
                ++SW; 
                --SE;
            }

            Painter.Set(this, entrance, Terrain.ENTRANCE);
        }

        private static short[] corner1 = {
            W, W, W, W, W, W, W, W, W, W,
            W, s, s, s, e, e, e, W, W, W,
            W, s, s, s, W, W, e, e, W, W,
            W, s, s, s, W, W, W, e, e, W,
            W, e, W, W, W, W, W, W, e, n,
            W, e, W, W, W, W, W, n, n, n,
            W, e, e, W, W, W, n, n, n, n,
            W, W, e, e, W, n, n, n, n, n,
            W, W, W, e, e, n, n, n, n, n,
            W, W, W, W, n, n, n, n, n, n
        };

        private static short[] corner2 = {
            W, W, W, W, W, W, W, W, W, W,
            W, s, s, s, W, W, W, W, W, W,
            W, s, s, s, e, e, e, e, e, W,
            W, s, s, s, W, W, W, W, e, e,
            W, W, e, W, W, W, W, W, W, e,
            W, W, e, W, W, W, W, n, n, n,
            W, W, e, W, W, W, n, n, n, n,
            W, W, e, W, W, n, n, n, n, n,
            W, W, e, e, W, n, n, n, n, n,
            W, W, W, e, e, n, n, n, n, n
         };

        private static short[] corner3 = {
            W, W, W, W, W, W, W, W, W, W,
            W, s, s, s, W, W, W, W, W, W,
            W, s, s, s, e, e, e, e, W, W,
            W, s, s, s, W, W, W, e, W, W,
            W, W, e, W, W, W, W, e, W, n,
            W, W, e, W, W, W, W, e, e, n,
            W, W, e, W, W, W, n, n, n, n,
            W, W, e, e, e, e, n, n, n, n,
            W, W, W, W, W, e, n, n, n, n,
            W, W, W, W, n, n, n, n, n, n
        };

        private static short[] corner4 = {
            W, W, W, W, W, W, W, W, W, W,
            W, s, s, s, W, W, W, W, W, W,
            W, s, s, s, e, e, e, W, W, W,
            W, s, s, s, W, W, e, W, W, W,
            W, W, e, W, W, W, e, W, W, n,
            W, W, e, W, W, W, e, e, n, n,
            W, W, e, e, e, e, e, n, n, n,
            W, W, W, W, W, e, n, n, n, n,
            W, W, W, W, W, n, n, n, n, n,
            W, W, W, W, n, n, n, n, n, n
        };

        private static short[][] cornerVariants = {
            corner1,
            corner2,
            corner3,
            corner4
        };

        private void BuildCorners()
        {
            int NW = 2 + 11 * Width();
            int NE = 30 + 11 * Width();
            int SE = 30 + 39 * Width();
            int SW = 2 + 39 * Width();

            short[] cornerTiles = Rnd.OneOf(cornerVariants);
            for (int i = 0; i < cornerTiles.Length; ++i)
            {
                if (i % 10 == 0 && i != 0)
                {
                    NW += (Width() - 10);
                    NE += (Width() + 10);
                    SE -= (Width() - 10);
                    SW -= (Width() + 10);
                }

                if (cornerTiles[i] != n)
                    map[NW] = map[NE] = map[SE] = map[SW] = cornerTiles[i];

                ++NW;
                --NE;
                ++SW;
                --SE;
            }
        }

        [SPDStatic]
        public class CityEntrance : CustomTilemap
        {
            public CityEntrance()
            {
                texture = Assets.Environment.CAVES_BOSS;
            }

            private static short[] entryWay = new short[]{
                -1,  7,  7,  7, -1,
                -1,  1,  2,  3, -1,
                 8,  1,  2,  3, 12,
                16,  9, 10, 11, 20,
                16, 16, 18, 20, 20,
                16, 17, 18, 19, 20,
                16, 16, 18, 20, 20,
                16, 17, 18, 19, 20,
                16, 16, 18, 20, 20,
                16, 17, 18, 19, 20,
                24, 25, 26, 27, 28
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int[] data = new int[tileW * tileH];
                int entryPos = 0;
                for (int i = 0; i < data.Length; ++i)
                {
                    if (i % tileW == tileW / 2 - 2)
                    {
                        //override the entryway
                        data[i++] = entryWay[entryPos++];
                        data[i++] = entryWay[entryPos++];
                        data[i++] = entryWay[entryPos++];
                        data[i++] = entryWay[entryPos++];
                        data[i] = entryWay[entryPos++];
                    }
                    else
                    {
                        //otherwise check if we are on row 2 or 3, in which case we need to override walls
                        if (i / tileW == 2)
                            data[i] = 13;
                        else if (i / tileW == 3)
                            data[i] = 21;
                        else
                            data[i] = -1;
                    }
                }
                v.Map(data, tileW);
                return v;
            }
        }

        [SPDStatic]
        public class EntranceOverhang : CustomTilemap
        {
            public EntranceOverhang()
            {
                texture = Assets.Environment.CAVES_BOSS;
            }

            private static short[] entryWay = new short[]{
                 0,  7,  7,  7,  4,
                 0, 15, 15, 15,  4,
                -1, 23, 23, 23, -1,
                -1, -1, -1, -1, -1,
                -1,  6, -1, 14, -1,
                -1, -1, -1, -1, -1,
                -1,  6, -1, 14, -1,
                -1, -1, -1, -1, -1,
                -1,  6, -1, 14, -1,
                -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int[] data = new int[tileW * tileH];
                int entryPos = 0;
                for (int i = 0; i < data.Length; ++i)
                {
                    //copy over this row of the entryway
                    if (i % tileW == tileW / 2 - 2)
                    {
                        data[i++] = entryWay[entryPos++];
                        data[i++] = entryWay[entryPos++];
                        data[i++] = entryWay[entryPos++];
                        data[i++] = entryWay[entryPos++];
                        data[i] = entryWay[entryPos++];
                    }
                    else
                    {
                        data[i] = -1;
                    }
                }
                v.Map(data, tileW);
                return v;
            }
        }

        [SPDStatic]
        public class ArenaVisuals : CustomTilemap
        {
            public ArenaVisuals()
            {
                texture = Assets.Environment.CAVES_BOSS;
            }

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
                    int[] data = new int[tileW * tileH];
                    int j = Dungeon.level.Width() * tileY;
                    for (int i = 0; i < data.Length; ++i)
                    {
                        if (Dungeon.level.map[j] == Terrain.EMPTY_SP)
                        {
                            foreach (int k in pylonPositions)
                            {
                                if (k == j)
                                {
                                    if (Dungeon.level.locked &&
                                        !(Actor.FindChar(k) is Pylon))
                                    {
                                        data[i] = 38;
                                    }
                                    else
                                    {
                                        data[i] = -1;
                                    }
                                }
                                else if (Dungeon.level.Adjacent(k, j))
                                {
                                    int w = Dungeon.level.width;
                                    data[i] = 54 + (j % w + 8 * (j / w)) - (k % w + 8 * (k / w));
                                }
                            }
                        }
                        else if (Dungeon.level.map[j] == Terrain.INACTIVE_TRAP)
                        {
                            data[i] = 37;
                        }
                        else if (gate.Inside(Dungeon.level.CellToPoint(j)))
                        {
                            int idx = Dungeon.level.solid[j] ? 40 : 32;
                            data[i++] = idx++;
                            data[i++] = idx++;
                            data[i++] = idx++;
                            data[i++] = idx++;
                            data[i] = idx;
                            j += 4;
                        }
                        else
                        {
                            data[i] = -1;
                        }

                        ++j;
                    }
                    vis.Map(data, tileW);
                }
            }

            public override string Name(int tileX, int tileY)
            {
                int i = tileX + tileW * (tileY + this.tileY);
                if (Dungeon.level.map[i] == Terrain.INACTIVE_TRAP)
                {
                    return Messages.Get(typeof(NewCavesBossLevel), "wires_name");
                }
                else if (gate.Inside(Dungeon.level.CellToPoint(i)))
                {
                    return Messages.Get(typeof(NewCavesBossLevel), "gate_name");
                }

                return base.Name(tileX, tileY);
            }

            public override string Desc(int tileX, int tileY)
            {
                int i = tileX + tileW * (tileY + this.tileY);
                if (Dungeon.level.map[i] == Terrain.INACTIVE_TRAP)
                {
                    return Messages.Get(typeof(NewCavesBossLevel), "wires_desc");
                }
                else if (gate.Inside(Dungeon.level.CellToPoint(i)))
                {
                    if (Dungeon.level.solid[i])
                    {
                        return Messages.Get(typeof(NewCavesBossLevel), "gate_desc");
                    }
                    else
                    {
                        return Messages.Get(typeof(NewCavesBossLevel), "gate_desc_broken");
                    }
                }
                return base.Desc(tileX, tileY);
            }

            public override Image Image(int tileX, int tileY)
            {
                int i = tileX + tileW * (tileY + this.tileY);
                foreach (int k in pylonPositions)
                {
                    if (Dungeon.level.Distance(i, k) <= 1)
                    {
                        return null;
                    }
                }

                return base.Image(tileX, tileY);
            }
        }

        [SPDStatic]
        public class PylonEnergy : Blob
        {
            protected override void Evolve()
            {
                for (int cell = 0; cell < Dungeon.level.Length(); ++cell)
                {
                    if (Dungeon.level.InsideMap(cell))
                    {
                        off[cell] = cur[cell];

                        //instantly spreads to water cells
                        if (off[cell] == 0 && Dungeon.level.water[cell])
                        {
                            ++off[cell];
                        }

                        volume += off[cell];

                        if (off[cell] > 0)
                        {
                            Character ch = Actor.FindChar(cell);
                            if (ch != null && !(ch is NewDM300))
                            {
                                Sample.Instance.Play(Assets.Sounds.LIGHTNING);
                                ch.Damage(Rnd.NormalIntRange(6, 12), typeof(Electricity));
                                ch.sprite.Flash();

                                if (ch == Dungeon.hero && !ch.IsAlive())
                                {
                                    Dungeon.Fail(typeof(NewDM300));
                                    GLog.Negative(Messages.Get(typeof(Electricity), "ondeath"));
                                }
                            }
                        }
                    }
                }
            }

            public override void FullyClear()
            {
                base.FullyClear();
                energySourceSprite = null;
            }

            public static CharSprite energySourceSprite;

            private static DirectedSparks DIRECTED_SPARKS = new DirectedSparks();

            class DirectedSparks : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    if (energySourceSprite == null)
                    {
                        foreach (var c in Actor.Chars())
                        {
                            if (c is Pylon && c.alignment != Character.Alignment.NEUTRAL)
                            {
                                energySourceSprite = c.sprite;
                                break;
                            }
                            else if (c is OldDM300)
                            {
                                energySourceSprite = c.sprite;
                            }
                        }
                        if (energySourceSprite == null)
                        {
                            return;
                        }
                    }

                    SparkParticle s = ((SparkParticle)emitter.Recycle<SparkParticle>());
                    s.ResetStatic(x, y);
                    s.speed.Set((energySourceSprite.x + energySourceSprite.width / 2f) - x,
                            (energySourceSprite.y + energySourceSprite.height / 2f) - y);
                    s.speed.Normalize().Scale(DungeonTilemap.SIZE * 2f);

                    //offset the particles slightly so they don't go too far outside of the cell
                    s.x -= s.speed.x / 8f;
                    s.y -= s.speed.y / 8f;
                }

                public override bool LightMode()
                {
                    return true;
                }
            }

            public override string TileDesc()
            {
                return Messages.Get(typeof(NewCavesBossLevel), "energy_desc");
            }

            public override void Use(BlobEmitter emitter)
            {
                base.Use(emitter);
                energySourceSprite = null;
                emitter.Pour(DIRECTED_SPARKS, 0.125f);
            }
        }
    }
}