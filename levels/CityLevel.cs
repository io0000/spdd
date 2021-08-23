using System;
using watabou.noosa;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors.mobs.npcs;
using spdd.levels.painters;
using spdd.levels.traps;
using spdd.messages;
using spdd.tiles;

namespace spdd.levels
{
    public class CityLevel : RegularLevel
    {
        public CityLevel()
        {
            color1 = new Color(0x4b, 0x66, 0x36, 0xff);
            color2 = new Color(0xf2, 0xf2, 0xf2, 0xff);
        }

        protected override int StandardRooms()
        {
            //7 to 10, average 7.9
            return 7 + Rnd.Chances(new float[] { 4, 3, 2, 1 });
        }

        protected override int SpecialRooms()
        {
            //2 to 3, average 2.33
            return 2 + Rnd.Chances(new float[] { 2, 1 });
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_CITY;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_CITY;
        }

        protected override Painter Painter()
        {
            return new CityPainter()
                    .SetWater(feeling == Feeling.WATER ? 0.90f : 0.30f, 4)
                    .SetGrass(feeling == Feeling.GRASS ? 0.80f : 0.20f, 3)
                    .SetTraps(NTraps(), TrapClasses(), TrapChances());
        }

        protected override Type[] TrapClasses()
        {
            return new Type[]{
                    typeof(FrostTrap), typeof(StormTrap), typeof(CorrosionTrap), typeof(BlazingTrap), typeof(DisintegrationTrap),
                    typeof(RockfallTrap), typeof(FlashingTrap), typeof(GuardianTrap), typeof(WeakeningTrap),
                    typeof(DisarmingTrap), typeof(SummoningTrap), typeof(WarpingTrap), typeof(CursingTrap), typeof(PitfallTrap), typeof(DistortionTrap) };
        }

        protected override float[] TrapChances()
        {
            return new float[]{
                    4, 4, 4, 4, 4,
                    2, 2, 2, 2,
                    1, 1, 1, 1, 1, 1 };
        }

        public override void CreateMobs()
        {
            Imp.Quest.Spawn(this);

            base.CreateMobs();
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
            AddCityVisuals(this, visuals);
            return visuals;
        }

        public static void AddCityVisuals(Level level, Group group)
        {
            for (int i = 0; i < level.Length(); ++i)
            {
                if (level.map[i] == Terrain.WALL_DECO)
                {
                    group.Add(new Smoke(i));
                }
            }
        }

        private class Smoke : Emitter
        {
            private readonly int pos;

            private new static readonly Factory factory = new CityLevelSmokeEmitterFactory();

            class CityLevelSmokeEmitterFactory : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    var p = emitter.Recycle<CityLevel.SmokeParticle>();
                    p.Reset(x, y);
                }
            }

            public Smoke(int pos)
            {
                this.pos = pos;

                var p = DungeonTilemap.TileCenterToWorld(pos);
                Pos(p.x - 6, p.y - 4, 12, 12);

                Pour(factory, 0.2f);
            }

            public override void Update()
            {
                if (visible = (pos < Dungeon.level.heroFOV.Length && Dungeon.level.heroFOV[pos]))
                {
                    base.Update();
                }
            }
        }

        public class SmokeParticle : PixelParticle
        {
            public SmokeParticle()
            {
                SetColor(new Color(0x00, 0x00, 0x00, 0xff));
                speed.Set(Rnd.Float(-2, 4), -Rnd.Float(3, 6));
            }

            public void Reset(float x, float y)
            {
                Revive();

                base.x = x;
                base.y = y;

                left = lifespan = 2f;
            }

            public override void Update()
            {
                base.Update();
                var p = left / lifespan;
                am = p > 0.8f ? 1 - p : p * 0.25f;
                Size(6 - p * 3);
            }
        }
    }
}