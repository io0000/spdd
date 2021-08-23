using watabou.noosa;
using watabou.utils;
using spdd.levels.rooms.standard;
using spdd.tiles;

namespace spdd.levels.rooms.sewerboss
{
    public abstract class GooBossRoom : StandardRoom
    {
        public override float[] SizeCatProbs()
        {
            return new float[] { 0, 1, 0 };
        }

        public static GooBossRoom RandomGooRoom()
        {
            switch (Rnd.Int(4))
            {
                case 0:
                default:
                    return new DiamondGooRoom();
                case 1:
                    return new WalledGooRoom();
                case 2:
                    return new ThinPillarsGooRoom();
                case 3:
                    return new ThickPillarsGooRoom();
            }
        }

        protected void SetupGooNest(Level level)
        {
            GooNest nest = new GooNest();
            nest.SetRect(left + Width() / 2 - 2, top + Height() / 2 - 2, 4 + Width() % 2, 4 + Height() % 2);

            level.customTiles.Add(nest);
        }

        [SPDStatic]
        public class GooNest : CustomTilemap
        {
            public GooNest()
            {
                texture = Assets.Environment.SEWER_BOSS;
            }

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int[] data = new int[tileW * tileH];
                for (int x = 0; x < tileW; ++x)
                {
                    for (int y = 0; y < tileH; ++y)
                    {
                        if ((x == 0 || x == tileW - 1) && (y == 0 || y == tileH - 1))
                        {
                            //corners
                            data[x + tileW * y] = -1;
                        }
                        else if ((x == 1 && y == 0) || (x == 0 && y == 1))
                        {
                            //adjacent to corners
                            data[x + tileW * y] = 0;
                        }
                        else if ((x == tileW - 2 && y == 0) || (x == tileW - 1 && y == 1))
                        {
                            data[x + tileW * y] = 1;
                        }
                        else if ((x == 1 && y == tileH - 1) || (x == 0 && y == tileH - 2))
                        {
                            data[x + tileW * y] = 2;
                        }
                        else if ((x == tileW - 2 && y == tileH - 1) || (x == tileW - 1 && y == tileH - 2))
                        {
                            data[x + tileW * y] = 3;
                        }
                        else if (x == 0)
                        {
                            //sides
                            data[x + tileW * y] = 4;
                        }
                        else if (y == 0)
                        {
                            data[x + tileW * y] = 5;
                        }
                        else if (x == tileW - 1)
                        {
                            data[x + tileW * y] = 6;
                        }
                        else if (y == tileH - 1)
                        {
                            data[x + tileW * y] = 7;
                        }
                        else
                        {
                            //inside
                            data[x + tileW * y] = 8;
                        }

                    }
                }
                v.Map(data, tileW);
                return v;
            }

            public override Image Image(int tileX, int tileY)
            {
                return null;
            }
        }
    }
}