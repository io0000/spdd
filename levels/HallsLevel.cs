using System;
using System.Collections.Generic;
using watabou.glwrap;
using watabou.noosa;
using watabou.noosa.particles;
using watabou.utils;
using spdd.items;
using spdd.levels.painters;
using spdd.levels.rooms;
using spdd.levels.rooms.special;
using spdd.levels.traps;
using spdd.messages;
using spdd.tiles;

namespace spdd.levels
{
    public class HallsLevel : RegularLevel
    {
        public HallsLevel()
        {
            viewDistance = Math.Min(26 - Dungeon.depth, viewDistance);

            color1 = new Color(0x80, 0x15, 0x00, 0xff);
            color2 = new Color(0xa6, 0x85, 0x21, 0xff);
        }

        public override List<Room> InitRooms()
        {
            List<Room> rooms = base.InitRooms();

            rooms.Add(new DemonSpawnerRoom());

            return rooms;
        }

        public override int NMobs()
        {
            //remove one mob to account for ripper demon spawners
            return base.NMobs() - 1;
        }

        protected override int StandardRooms()
        {
            //8 to 10, average 8.67
            return 8 + Rnd.Chances(new float[] { 3, 2, 1 });
        }

        protected override int SpecialRooms()
        {
            //2 to 3, average 2.5
            return 2 + Rnd.Chances(new float[] { 1, 1 });
        }

        protected override Painter Painter()
        {
            return new HallsPainter()
                    .SetWater(feeling == Feeling.WATER ? 0.70f : 0.15f, 6)
                    .SetGrass(feeling == Feeling.GRASS ? 0.65f : 0.10f, 3)
                    .SetTraps(NTraps(), TrapClasses(), TrapChances());
        }

        public override void Create()
        {
            AddItemToSpawn(new Torch());
            AddItemToSpawn(new Torch());
            base.Create();
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_HALLS;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_HALLS;
        }

        protected override Type[] TrapClasses()
        {
            return new Type[]{
                    typeof(FrostTrap), typeof(StormTrap), typeof(CorrosionTrap), typeof(BlazingTrap), typeof(DisintegrationTrap),
                    typeof(RockfallTrap), typeof(FlashingTrap), typeof(GuardianTrap), typeof(WeakeningTrap),
                    typeof(DisarmingTrap), typeof(SummoningTrap), typeof(WarpingTrap), typeof(CursingTrap), typeof(GrimTrap), typeof(PitfallTrap), typeof(DistortionTrap)
            };
        }

        protected override float[] TrapChances()
        {
            return new float[]{
                    4, 4, 4, 4, 4,
                    2, 2, 2, 2,
                    1, 1, 1, 1, 1, 1, 1 };
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
            AddHallsVisuals(this, visuals);
            return visuals;
        }

        public static void AddHallsVisuals(Level level, Group group)
        {
            for (int i = 0; i < level.Length(); ++i)
            {
                if (level.map[i] == Terrain.WATER)
                {
                    group.Add(new Stream(i));
                }
            }
        }

        private class Stream : Group
        {
            private readonly int pos;

            private float delay;

            public Stream(int pos)
            {
                this.pos = pos;

                delay = Rnd.Float(2);
            }

            public override void Update()
            {
                if (!Dungeon.level.water[pos])
                {
                    KillAndErase();
                    return;
                }

                if (visible = (pos < Dungeon.level.heroFOV.Length && Dungeon.level.heroFOV[pos]))
                {
                    base.Update();

                    if ((delay -= Game.elapsed) <= 0)
                    {
                        delay = Rnd.Float(2);

                        PointF p = DungeonTilemap.TileToWorld(pos);
                        Recycle<FireParticle>().Reset(
                           p.x + Rnd.Float(DungeonTilemap.SIZE),
                           p.y + Rnd.Float(DungeonTilemap.SIZE));
                    }
                }
            }

            public override void Draw()
            {
                Blending.SetLightMode();
                base.Draw();
                Blending.SetNormalMode();
            }
        }

        public class FireParticle : PixelParticle.Shrinking
        {
            public FireParticle()
            {
                SetColor(new Color(0xEE, 0x77, 0x22, 0xff));
                lifespan = 1f;

                acc.Set(0, +80);
            }

            public void Reset(float x, float y)
            {
                Revive();

                base.x = x;
                base.y = y;

                left = lifespan;

                speed.Set(0, -40);
                size = 4;
            }

            public override void Update()
            {
                base.Update();
                var p = left / lifespan;
                am = p > 0.8f ? (1 - p) * 5 : 1;
            }
        }
    }
}