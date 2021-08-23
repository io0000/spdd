using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa;
using watabou.noosa.particles;
using spdd.levels.rooms;
using spdd.levels.painters;
using spdd.levels.traps;
using spdd.actors.mobs.npcs;
using spdd.effects.particles;
using spdd.messages;
using spdd.tiles;

namespace spdd.levels
{
    public class PrisonLevel : RegularLevel
    {
        public PrisonLevel()
        {
            color1 = new Color(0x6a, 0x72, 0x3d, 0xff);
            color2 = new Color(0x88, 0x92, 0x4c, 0xff);
        }

        public override List<Room> InitRooms()
        {
            return Wandmaker.Quest.SpawnRoom(base.InitRooms());
        }

        protected override int StandardRooms()
        {
            //6 to 8, average 6.66
            return 6 + Rnd.Chances(new float[] { 4, 2, 2 });
        }

        protected override int SpecialRooms()
        {
            //1 to 3, average 1.83
            return 1 + Rnd.Chances(new float[] { 3, 4, 3 });
        }

        protected override Painter Painter()
        {
            return new PrisonPainter()
                    .SetWater(feeling == Feeling.WATER ? 0.90f : 0.30f, 4)
                    .SetGrass(feeling == Feeling.GRASS ? 0.80f : 0.20f, 3)
                    .SetTraps(NTraps(), TrapClasses(), TrapChances());
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_PRISON;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_PRISON;
        }

        protected override Type[] TrapClasses()
        {
            return new Type[]{
                    typeof(ChillingTrap), typeof(ShockingTrap), typeof(ToxicTrap), typeof(BurningTrap), typeof(PoisonDartTrap),
                    typeof(AlarmTrap), typeof(OozeTrap), typeof(GrippingTrap),
                    typeof(ConfusionTrap), typeof(FlockTrap), typeof(SummoningTrap), typeof(TeleportationTrap) };
        }

        protected override float[] TrapChances()
        {
            return new float[]{
                    4, 4, 4, 4, 4,
                    2, 2, 2,
                    1, 1, 1, 1 };
        }

        public override string TileName(int tile)
        {
            switch (tile)
            {
                case Terrain.WATER:
                    return Messages.Get(typeof(PrisonLevel), "water_name");
                default:
                    return base.TileName(tile);
            }
        }

        public override string TileDesc(int tile)
        {
            switch (tile)
            {
                case Terrain.EMPTY_DECO:
                    return Messages.Get(typeof(PrisonLevel), "empty_deco_desc");
                case Terrain.BOOKSHELF:
                    return Messages.Get(typeof(PrisonLevel), "bookshelf_desc");
                default:
                    return base.TileDesc(tile);
            }
        }

        public override Group AddVisuals()
        {
            base.AddVisuals();
            AddPrisonVisuals(this, visuals);
            return visuals;
        }

        public static void AddPrisonVisuals(Level level, Group group)
        {
            for (int i = 0; i < level.Length(); ++i)
            {
                if (level.map[i] == Terrain.WALL_DECO)
                {
                    group.Add(new Torch(i));
                }
            }
        }

        public class Torch : Emitter
        {
            private int pos;

            public Torch(int pos)
            {
                this.pos = pos;

                PointF p = DungeonTilemap.TileCenterToWorld(pos);
                Pos(p.x - 1, p.y + 2, 2, 0);

                Pour(FlameParticle.Factory, 0.15f);

                Add(new Halo(12, new Color(0xFF, 0xFF, 0xCC, 0xFF), 0.4f).Point(p.x, p.y + 1));
            }

            public override void Update()
            {
                if (visible = (pos < Dungeon.level.heroFOV.Length && Dungeon.level.heroFOV[pos]))
                {
                    base.Update();
                }
            }
        }
    }
}