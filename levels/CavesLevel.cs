using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors.mobs.npcs;
using spdd.levels.painters;
using spdd.levels.rooms;
using spdd.levels.traps;
using spdd.messages;
using spdd.tiles;

namespace spdd.levels
{
    public class CavesLevel : RegularLevel
    {
        public CavesLevel()
        {
            color1 = new Color(0x53, 0x4f, 0x3e, 0xff);
            color2 = new Color(0xb9, 0xd6, 0x61, 0xff);
        }

        public override List<Room> InitRooms()
        {
            return Blacksmith.Quest.Spawn(base.InitRooms());
        }

        protected override int StandardRooms()
        {
            //6 to 9, average 7.333
            return 6 + Rnd.Chances(new float[] { 2, 3, 3, 1 });
        }

        protected override int SpecialRooms()
        {
            //1 to 3, average 2.2
            return 1 + Rnd.Chances(new float[] { 2, 4, 4 });
        }

        protected override Painter Painter()
        {
            return new CavesPainter()
                    .SetWater(feeling == Feeling.WATER ? 0.85f : 0.30f, 6)
                    .SetGrass(feeling == Feeling.GRASS ? 0.65f : 0.15f, 3)
                    .SetTraps(NTraps(), TrapClasses(), TrapChances());
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_CAVES;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_CAVES;
        }

        protected override Type[] TrapClasses()
        {
            return new Type[]{
                typeof(BurningTrap), typeof(PoisonDartTrap), typeof(FrostTrap), typeof(StormTrap), typeof(CorrosionTrap),
                typeof(GrippingTrap), typeof(RockfallTrap),  typeof(GuardianTrap),
                typeof(ConfusionTrap), typeof(SummoningTrap), typeof(WarpingTrap), typeof(PitfallTrap)
            };
        }

        protected override float[] TrapChances()
        {
            return new float[]{
                4, 4, 4, 4, 4,
                2, 2, 2,
                1, 1, 1, 1};
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
                default:
                    return base.TileName(tile);
            }
        }

        public override string TileDesc(int tile)
        {
            switch (tile)
            {
                case Terrain.ENTRANCE:
                    return Messages.Get(typeof(CavesLevel), "entrance_desc");
                case Terrain.EXIT:
                    return Messages.Get(typeof(CavesLevel), "exit_desc");
                case Terrain.HIGH_GRASS:
                    return Messages.Get(typeof(CavesLevel), "high_grass_desc");
                case Terrain.WALL_DECO:
                    return Messages.Get(typeof(CavesLevel), "wall_deco_desc");
                case Terrain.BOOKSHELF:
                    return Messages.Get(typeof(CavesLevel), "bookshelf_desc");
                default:
                    return base.TileDesc(tile);
            }
        }

        public override Group AddVisuals()
        {
            base.AddVisuals();
            AddCavesVisuals(this, visuals);
            return visuals;
        }

        public static void AddCavesVisuals(Level level, Group group)
        {
            for (int i = 0; i < level.Length(); ++i)
            {
                if (level.map[i] == Terrain.WALL_DECO)
                {
                    group.Add(new Vein(i));
                }
            }
        }

        private class Vein : Group
        {
            private readonly int pos;

            private float delay;

            public Vein(int pos)
            {
                this.pos = pos;

                delay = Rnd.Float(2);
            }

            public override void Update()
            {
                if (visible = (pos < Dungeon.level.heroFOV.Length && Dungeon.level.heroFOV[pos]))
                {
                    base.Update();

                    if ((delay -= Game.elapsed) <= 0)
                    {
                        //pickaxe can remove the ore, should remove the sparkling too.
                        if (Dungeon.level.map[pos] != Terrain.WALL_DECO)
                        {
                            Kill();
                            return;
                        }

                        delay = Rnd.Float();

                        PointF p = DungeonTilemap.TileToWorld(pos);
                        (Recycle<Sparkle>()).Reset(
                           p.x + Rnd.Float(DungeonTilemap.SIZE),
                           p.y + Rnd.Float(DungeonTilemap.SIZE));
                    }
                }
            }
        }

        private class Sparkle : PixelParticle
        {
            public void Reset(float x, float y)
            {
                Revive();

                base.x = x;
                base.y = y;

                left = lifespan = 0.5f;
            }

            public override void Update()
            {
                base.Update();

                var p = left / lifespan;
                Size((am = p < 0.5f ? p * 2 : (1 - p) * 2) * 2);
            }
        }
    }
}