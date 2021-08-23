using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.tweeners;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.levels.painters;
using spdd.levels.rooms.standard;
using spdd.messages;
using spdd.scenes;
using spdd.tiles;

namespace spdd.levels
{
    public class NewCityBossLevel : Level
    {
        public NewCityBossLevel()
        {
            color1 = new Color(0x4b, 0x66, 0x36, 0xff);
            color2 = new Color(0xf2, 0xf2, 0xf2, 0xff);
        }

        private const int WIDTH = 15;
        private const int HEIGHT = 48;

        private static readonly Rect entry = new Rect(1, 37, 14, 48);
        private static readonly Rect arena = new Rect(1, 25, 14, 38);
        private static readonly Rect end = new Rect(0, 0, 15, 22);

        private static readonly int bottomDoor = 7 + (arena.bottom - 1) * 15;
        private static readonly int topDoor = 7 + arena.top * 15;

        public static int throne;
        private static int[] pedestals = new int[4];

        static NewCityBossLevel()
        {
            Point c = arena.Center();
            throne = c.x + (c.y) * WIDTH;
            pedestals[0] = c.x - 3 + (c.y - 3) * WIDTH;
            pedestals[1] = c.x + 3 + (c.y - 3) * WIDTH;
            pedestals[2] = c.x + 3 + (c.y + 3) * WIDTH;
            pedestals[3] = c.x - 3 + (c.y + 3) * WIDTH;
        }

        private ImpShopRoom impShop;

        public override string TilesTex()
        {
            return Assets.Environment.TILES_CITY;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_CITY;
        }

        private const string IMP_SHOP = "imp_shop";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(IMP_SHOP, impShop);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            impShop = (ImpShopRoom)bundle.Get(IMP_SHOP);
            if (map[topDoor] != Terrain.LOCKED_DOOR && Imp.Quest.IsCompleted() && !impShop.ShopSpawned())
            {
                SpawnShop();
            }
        }

        public override bool Build()
        {
            SetSize(WIDTH, HEIGHT);

            //entrance room
            Painter.Fill(this, entry, Terrain.WALL);
            Painter.Fill(this, entry, 1, Terrain.BOOKSHELF);
            Painter.Fill(this, entry, 2, Terrain.EMPTY);

            Painter.Fill(this, entry.left + 3, entry.top + 3, 1, 5, Terrain.BOOKSHELF);
            Painter.Fill(this, entry.right - 4, entry.top + 3, 1, 5, Terrain.BOOKSHELF);

            Point c = entry.Center();

            Painter.Fill(this, c.x - 1, c.y - 2, 3, 1, Terrain.STATUE);
            Painter.Fill(this, c.x - 1, c.y, 3, 1, Terrain.STATUE);
            Painter.Fill(this, c.x - 1, c.y + 2, 3, 1, Terrain.STATUE);
            Painter.Fill(this, c.x, entry.top + 1, 1, 6, Terrain.EMPTY_SP);

            Painter.Set(this, c.x, entry.top, Terrain.DOOR);

            entrance = c.x + (c.y + 2) * Width();
            Painter.Set(this, entrance, Terrain.ENTRANCE);

            //DK's throne room
            Painter.FillDiamond(this, arena, 1, Terrain.EMPTY);

            Painter.Fill(this, arena, 5, Terrain.EMPTY_SP);
            Painter.Fill(this, arena, 6, Terrain.SIGN);

            c = arena.Center();
            Painter.Set(this, c.x - 3, c.y, Terrain.STATUE);
            Painter.Set(this, c.x - 4, c.y, Terrain.STATUE);
            Painter.Set(this, c.x + 3, c.y, Terrain.STATUE);
            Painter.Set(this, c.x + 4, c.y, Terrain.STATUE);

            Painter.Set(this, pedestals[0], Terrain.PEDESTAL);
            Painter.Set(this, pedestals[1], Terrain.PEDESTAL);
            Painter.Set(this, pedestals[2], Terrain.PEDESTAL);
            Painter.Set(this, pedestals[3], Terrain.PEDESTAL);

            Painter.Set(this, c.x, arena.top, Terrain.LOCKED_DOOR);

            //exit hallway
            Painter.Fill(this, end, Terrain.CHASM);
            Painter.Fill(this, end.left + 4, end.top + 5, 7, 18, Terrain.EMPTY);
            Painter.Fill(this, end.left + 4, end.top + 5, 7, 4, Terrain.EXIT);
            exit = end.left + 7 + (end.top + 8) * Width();

            impShop = new ImpShopRoom();
            impShop.Set(end.left + 3, end.top + 12, end.left + 11, end.top + 20);
            Painter.Set(this, impShop.Center(), Terrain.PEDESTAL);

            Painter.Set(this, impShop.left + 2, impShop.top, Terrain.STATUE);
            Painter.Set(this, impShop.left + 6, impShop.top, Terrain.STATUE);

            Painter.Fill(this, end.left + 5, end.bottom + 1, 5, 1, Terrain.EMPTY);
            Painter.Fill(this, end.left + 6, end.bottom + 2, 3, 1, Terrain.EMPTY);

            new CityPainter().Paint(this, null);

            //pillars last, no deco on these
            Painter.Fill(this, end.left + 1, end.top + 2, 2, 2, Terrain.WALL);
            Painter.Fill(this, end.left + 1, end.top + 7, 2, 2, Terrain.WALL);
            Painter.Fill(this, end.left + 1, end.top + 12, 2, 2, Terrain.WALL);
            Painter.Fill(this, end.left + 1, end.top + 17, 2, 2, Terrain.WALL);

            Painter.Fill(this, end.right - 3, end.top + 2, 2, 2, Terrain.WALL);
            Painter.Fill(this, end.right - 3, end.top + 7, 2, 2, Terrain.WALL);
            Painter.Fill(this, end.right - 3, end.top + 12, 2, 2, Terrain.WALL);
            Painter.Fill(this, end.right - 3, end.top + 17, 2, 2, Terrain.WALL);

            CustomTilemap customVisuals = new CustomGroundVisuals();
            customVisuals.SetRect(0, 0, Width(), Height());
            customTiles.Add(customVisuals);

            customVisuals = new CustomWallVisuals();
            customVisuals.SetRect(0, 0, Width(), Height());
            customWalls.Add(customVisuals);

            return true;
        }

        //returns a random pedestal that doesn't already have a summon inbound on it
        public int GetSummoningPos()
        {
            Mob king = GetKing();
            var summons = king.Buffs<DwarfKing.Summoning>();
            List<int> positions = new List<int>();
            foreach (int pedestal in pedestals)
            {
                bool clear = true;
                foreach (DwarfKing.Summoning s in summons)
                {
                    if (s.GetPos() == pedestal)
                    {
                        clear = false;
                        break;
                    }
                }
                if (clear)
                {
                    positions.Add(pedestal);
                }
            }
            if (positions.Count == 0)
            {
                return -1;
            }
            else
            {
                return Rnd.Element(positions);
            }
        }

        private Mob GetKing()
        {
            foreach (Mob m in mobs)
            {
                if (m is DwarfKing)
                    return m;
            }
            return null;
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

        public override void OccupyCell(Character ch)
        {
            base.OccupyCell(ch);

            if (map[bottomDoor] != Terrain.LOCKED_DOOR &&
                map[topDoor] == Terrain.LOCKED_DOOR &&
                ch.pos < bottomDoor &&
                ch == Dungeon.hero)
            {
                Seal();
            }
        }

        public override void Seal()
        {
            base.Seal();

            foreach (Mob m in mobs)
            {
                //bring the first ally with you
                if (m.alignment == Character.Alignment.ALLY && !m.Properties().Contains(Character.Property.IMMOVABLE))
                {
                    m.pos = Dungeon.hero.pos + (Rnd.Int(2) == 0 ? +1 : -1);
                    m.sprite.Place(m.pos);
                    break;
                }
            }

            DwarfKing boss = new DwarfKing();
            boss.state = boss.WANDERING;
            boss.pos = PointToCell(arena.Center());
            GameScene.Add(boss);

            if (heroFOV[boss.pos])
            {
                boss.Notice();
                boss.sprite.Alpha(0);
                boss.sprite.parent.Add(new AlphaTweener(boss.sprite, 1, 0.1f));
            }

            Set(bottomDoor, Terrain.LOCKED_DOOR);
            GameScene.UpdateMap(bottomDoor);
            Dungeon.Observe();
        }

        public override void Unseal()
        {
            base.Unseal();

            Set(bottomDoor, Terrain.DOOR);
            GameScene.UpdateMap(bottomDoor);

            Set(topDoor, Terrain.DOOR);
            GameScene.UpdateMap(topDoor);

            if (Imp.Quest.IsCompleted())
            {
                SpawnShop();
            }
            Dungeon.Observe();
        }

        private void SpawnShop()
        {
            while (impShop.ItemCount() >= 7 * (impShop.Height() - 2))
                ++impShop.bottom;

            impShop.SpawnShop(this);
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

        //TODO need to change text for some of these tiles
        [SPDStatic]
        public class CustomGroundVisuals : CustomTilemap
        {
            public CustomGroundVisuals()
            {
                texture = Assets.Environment.CITY_BOSS;
                tileW = 15;
                tileH = 48;
            }

            private const int STAIR_ROWS = 7;

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int[] data = new int[tileW * tileH];

                int[] map = Dungeon.level.map;

                int stairsTop = -1;

                //upper part of the level, mostly demon halls tiles
                for (int i = tileW; i < tileW * 22; ++i)
                {
                    if (map[i] == Terrain.EXIT && stairsTop == -1)
                        stairsTop = i;

                    //pillars
                    if (map[i] == Terrain.WALL && map[i - tileW] == Terrain.CHASM)
                    {
                        data[i] = 13 * 8 + 6;
                        data[++i] = 13 * 8 + 7;
                    }
                    else if (map[i] == Terrain.WALL && map[i - tileW] == Terrain.WALL)
                    {
                        data[i] = 14 * 8 + 6;
                        data[++i] = 14 * 8 + 7;
                    }
                    else if (i > tileW && map[i] == Terrain.CHASM && map[i - tileW] == Terrain.WALL)
                    {
                        data[i] = 15 * 8 + 6;
                        data[++i] = 15 * 8 + 7;
                    }
                    //imp's pedestal
                    else if (map[i] == Terrain.PEDESTAL)
                    {
                        data[i] = 12 * 8 + 5;
                    }
                    //skull piles
                    else if (map[i] == Terrain.STATUE)
                    {
                        data[i] = 15 * 8 + 5;
                    }
                    //ground tiles
                    else if (map[i] == Terrain.EMPTY || map[i] == Terrain.EMPTY_DECO)
                    {
                        //final ground stiching with city tiles
                        if (i / tileW == 21)
                        {
                            data[i] = 11 * 8 + 0;
                            data[++i] = 11 * 8 + 1;
                            data[++i] = 11 * 8 + 2;
                            data[++i] = 11 * 8 + 3;
                            data[++i] = 11 * 8 + 4;
                            data[++i] = 11 * 8 + 5;
                            data[++i] = 11 * 8 + 6;
                        }
                        else
                        {
                            //regular ground tiles
                            if (map[i - 1] == Terrain.CHASM)
                                data[i] = 12 * 8 + 1;
                            else if (map[i + 1] == Terrain.CHASM)
                                data[i] = 12 * 8 + 3;
                            else if (map[i] == Terrain.EMPTY_DECO)
                                data[i] = 12 * 8 + 4;
                            else
                                data[i] = 12 * 8 + 2;
                        }
                    }
                    //otherwise no tile here
                    else
                    {
                        data[i] = -1;
                    }
                }

                //custom for stairs
                for (int i = 0; i < STAIR_ROWS; ++i)
                {
                    for (int j = 0; j < 7; ++j)
                        data[stairsTop + j] = (i + 4) * 8 + j;

                    stairsTop += tileW;
                }

                //lower part: statues, pedestals, and carpets
                for (int i = tileW * 22; i < tileW * tileH; ++i)
                {
                    //pedestal spawners
                    if (map[i] == Terrain.PEDESTAL)
                    {
                        data[i] = 13 * 8 + 4;
                    }
                    //statues that should face left instead of right
                    else if (map[i] == Terrain.STATUE && i % tileW > 7)
                    {
                        data[i] = 15 * 8 + 4;
                    }
                    //carpet tiles
                    else if (map[i] == Terrain.EMPTY_SP)
                    {
                        //top row of DK's throne
                        if (map[i + 1] == Terrain.EMPTY_SP && map[i + tileW] == Terrain.EMPTY_SP)
                        {
                            data[i] = 13 * 8 + 1;
                            data[++i] = 13 * 8 + 2;
                            data[++i] = 13 * 8 + 3;
                        }
                        //mid row of DK's throne
                        else if (map[i + 1] == Terrain.SIGN)
                        {
                            data[i] = 14 * 8 + 1;
                            data[++i] = 14 * 8 + 2; //TODO finalize throne visuals
                            data[++i] = 14 * 8 + 3;
                        }
                        //bottom row of DK's throne
                        else if (map[i + 1] == Terrain.EMPTY_SP && map[i - tileW] == Terrain.EMPTY_SP)
                        {
                            data[i] = 15 * 8 + 1;
                            data[++i] = 15 * 8 + 2;
                            data[++i] = 15 * 8 + 3;
                        }
                        //otherwise entrance carpet
                        else if (map[i - tileW] != Terrain.EMPTY_SP)
                        {
                            data[i] = 13 * 8 + 0;
                        }
                        else if (map[i + tileW] != Terrain.EMPTY_SP)
                        {
                            data[i] = 15 * 8 + 0;
                        }
                        else
                        {
                            data[i] = 14 * 8 + 0;
                        }

                        //otherwise no tile here
                    }
                    else
                    {
                        data[i] = -1;
                    }
                }

                v.Map(data, tileW);
                return v;
            }

            public override string Name(int tileX, int tileY)
            {
                int cell = (this.tileX + tileX) + (this.tileY + tileY) * tileW;

                //demon halls tiles
                if (cell < Dungeon.level.width * 22)
                {
                    if (Dungeon.level.map[cell] == Terrain.STATUE)
                    {
                        return Messages.Get(typeof(HallsLevel), "statue_name");
                    }
                }
                //DK arena tiles
                else
                {
                    if (Dungeon.level.map[cell] == Terrain.SIGN)
                    {
                        return Messages.Get(typeof(NewCityBossLevel), "throne_name");
                    }
                    else if (Dungeon.level.map[cell] == Terrain.PEDESTAL)
                    {
                        return Messages.Get(typeof(NewCityBossLevel), "summoning_name");
                    }
                }

                return base.Name(tileX, tileY);
            }

            public override string Desc(int tileX, int tileY)
            {
                int cell = (this.tileX + tileX) + (this.tileY + tileY) * tileW;

                //demon halls tiles
                if (cell < Dungeon.level.width * 22)
                {
                    if (Dungeon.level.map[cell] == Terrain.EXIT)
                    {
                        return Messages.Get(typeof(HallsLevel), "exit_desc");
                    }
                    else if (Dungeon.level.map[cell] == Terrain.STATUE)
                    {
                        return Messages.Get(typeof(HallsLevel), "statue_desc");
                    }
                    else if (Dungeon.level.map[cell] == Terrain.EMPTY_DECO)
                    {
                        return "";
                    }
                }
                //DK arena tiles
                else
                {
                    if (Dungeon.level.map[cell] == Terrain.SIGN)
                    {
                        return Messages.Get(typeof(NewCityBossLevel), "throne_desc");
                    }
                    else if (Dungeon.level.map[cell] == Terrain.PEDESTAL)
                    {
                        return Messages.Get(typeof(NewCityBossLevel), "summoning_desc");
                    }
                }

                return base.Desc(tileX, tileY);
            }
        }

        [SPDStatic]
        public class CustomWallVisuals : CustomTilemap
        {
            public CustomWallVisuals()
            {
                texture = Assets.Environment.CITY_BOSS;
                tileW = 15;
                tileH = 48;
            }

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int[] data = new int[tileW * tileH];

                int[] map = Dungeon.level.map;

                int shadowTop = -1;

                //upper part of the level, mostly demon halls tiles
                for (int i = tileW; i < tileW * 21; ++i)
                {
                    if (map[i] == Terrain.EXIT && shadowTop == -1)
                        shadowTop = i - tileW * 4;

                    //pillars
                    if (map[i] == Terrain.CHASM && map[i + tileW] == Terrain.WALL)
                    {
                        data[i] = 12 * 8 + 6;
                        data[++i] = 12 * 8 + 7;
                    }
                    else if (map[i] == Terrain.WALL && map[i - tileW] == Terrain.CHASM)
                    {
                        data[i] = 13 * 8 + 6;
                        data[++i] = 13 * 8 + 7;
                    }
                    //skull tops
                    else if (map[i + tileW] == Terrain.STATUE)
                    {
                        data[i] = 14 * 8 + 5;
                    }
                    //otherwise no tile here
                    else
                    {
                        data[i] = -1;
                    }
                }

                //custom shadow  for stairs
                for (int i = 0; i < 8; ++i)
                {
                    if (i < 4)
                    {
                        data[shadowTop] = i * 8 + 0;
                        data[shadowTop + 1] = data[shadowTop + 2] = data[shadowTop + 3] = data[shadowTop + 4] =
                                data[shadowTop + 5] = data[shadowTop + 6] = i * 8 + 1;
                        data[shadowTop + 7] = i * 8 + 2;
                    }
                    else
                    {
                        int j = i - 4;
                        data[shadowTop] = j * 8 + 3;
                        data[shadowTop + 1] = data[shadowTop + 2] = data[shadowTop + 3] = data[shadowTop + 4] =
                                data[shadowTop + 5] = data[shadowTop + 6] = j * 8 + 4;
                        data[shadowTop + 7] = j * 8 + 5;
                    }

                    shadowTop += tileW;
                }

                //lower part. Statues and DK's throne
                for (int i = tileW * 21; i < tileW * tileH; ++i)
                {
                    //Statues that need to face left instead of right
                    if (map[i] == Terrain.STATUE && i % tileW > 7)
                    {
                        data[i - tileW] = 14 * 8 + 4;
                    }
                    else if (map[i] == Terrain.SIGN)
                    {
                        data[i - tileW] = 13 * 8 + 5;
                    }

                    //always no tile here (as the above statements are modifying previous tiles)
                    data[i] = -1;
                }

                v.Map(data, tileW);
                return v;
            }
        }
    }
}