using System;
using watabou.noosa;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors.mobs.npcs;
using spdd.effects;
using spdd.items;
using spdd.levels.painters;
using spdd.levels.traps;
using spdd.messages;
using spdd.scenes;
using spdd.tiles;

namespace spdd.levels
{
    public class SewerLevel : RegularLevel
    {
        public SewerLevel()
        {
            color1 = new Color(0x48, 0x76, 0x3c, 0xFF);
            color2 = new Color(0x59, 0x99, 0x4a, 0xFF);
        }

        protected override int StandardRooms()
        {
            //5 to 7, average 5.57
            return 5 + Rnd.Chances(new float[] { 4, 2, 1 });
        }

        protected override int SpecialRooms()
        {
            //1 to 3, average 1.67
            return 1 + Rnd.Chances(new float[] { 4, 4, 2 });
        }

        protected override Painter Painter()
        {
            return new SewerPainter()
                    .SetWater(feeling == Feeling.WATER ? 0.85f : 0.30f, 5)
                    .SetGrass(feeling == Feeling.GRASS ? 0.80f : 0.20f, 4)
                    .SetTraps(NTraps(), TrapClasses(), TrapChances());
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_SEWERS;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_SEWERS;
        }

        protected override Type[] TrapClasses()
        {
            return Dungeon.depth == 1 ?
                    //new Type[] { typeof(ShockingTrap) } : // tt
                    new Type[] { typeof(WornDartTrap) } :
                    new Type[]{
                            typeof(ChillingTrap), typeof(ShockingTrap), typeof(ToxicTrap), typeof(WornDartTrap),
                            typeof(AlarmTrap), typeof(OozeTrap),
                            typeof(ConfusionTrap), typeof(FlockTrap), typeof(SummoningTrap), typeof(TeleportationTrap) };
        }


        protected override float[] TrapChances()
        {
            return Dungeon.depth == 1 ?
                    new float[] { 1 } :
                    new float[]{
                            4, 4, 4, 4,
                            2, 2,
                            1, 1, 1, 1};
        }

        public override void CreateItems()
        {
            //AddItemToSpawn(new CorpseDust()); //test

            if (!Dungeon.LimitedDrops.DEW_VIAL.Dropped())
            {
                AddItemToSpawn(new DewVial());
                Dungeon.LimitedDrops.DEW_VIAL.Drop();
            }

            Ghost.Quest.Spawn(this);

            base.CreateItems();
        }

        public override Group AddVisuals()
        {
            base.AddVisuals();
            AddSewerVisuals(this, visuals);
            return visuals;
        }

        public static void AddSewerVisuals(Level level, Group group)
        {
            for (int i = 0; i < level.Length(); ++i)
            {
                if (level.map[i] == Terrain.WALL_DECO)
                {
                    group.Add(new Sink(i));
                }
            }
        }

        public override string TileName(int tile)
        {
            switch (tile)
            {
                case Terrain.WATER:
                    return Messages.Get(typeof(SewerLevel), "water_name");
                default:
                    return base.TileName(tile);
            }
        }

        public override string TileDesc(int tile)
        {
            switch (tile)
            {
                case Terrain.EMPTY_DECO:
                    return Messages.Get(typeof(SewerLevel), "empty_deco_desc");
                case Terrain.BOOKSHELF:
                    return Messages.Get(typeof(SewerLevel), "bookshelf_desc");
                default:
                    return base.TileDesc(tile);
            }
        }

        private class Sink : Emitter
        {
            private readonly int pos;
            private float rippleDelay;

            private new static readonly Factory factory = new SinkEmitterFactory();

            class SinkEmitterFactory : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    var positive = emitter.Recycle<SewerLevel.WaterParticle>();
                    positive.Reset(x, y);
                }
            }

            public Sink(int pos)
            {
                this.pos = pos;

                var p = DungeonTilemap.TileCenterToWorld(pos);
                Pos(p.x - 2, p.y + 3, 4, 0);

                Pour(factory, 0.1f);
            }

            public override void Update()
            {
                if (visible = (pos < Dungeon.level.heroFOV.Length && Dungeon.level.heroFOV[pos]))
                {
                    base.Update();

                    if (!IsFrozen() && (rippleDelay -= Game.elapsed) <= 0)
                    {
                        Ripple ripple = GameScene.Ripple(pos + Dungeon.level.Width());
                        if (ripple != null)
                        {
                            ripple.y -= DungeonTilemap.SIZE / 2;
                            rippleDelay = Rnd.Float(0.4f, 0.6f);
                        }
                    }
                }
            }
        }

        public class WaterParticle : PixelParticle
        {
            public WaterParticle()
            {
                acc.y = 50;
                am = 0.5f;

                var color1 = new Color(0xb6, 0xcc, 0xc2, 0xff);
                var color2 = new Color(0x3b, 0x66, 0x53, 0xff);
                SetColor(ColorMath.Random(color1, color2));
                Size(2);
            }

            public void Reset(float x, float y)
            {
                Revive();

                base.x = x;
                base.y = y;

                speed.Set(Rnd.Float(-2, +2), 0);

                left = lifespan = 0.4f;
            }
        }
    }
}