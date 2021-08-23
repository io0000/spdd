using watabou.noosa;
using watabou.noosa.particles;
using watabou.utils;
using spdd.effects.particles;
using spdd.actors;
using spdd.tiles;

namespace spdd.effects
{
    public class MagicMissile : Emitter
    {
        private const float SPEED = 200f;

        private ICallback callback;

        private PointF to;

        private float sx;
        private float sy;
        private new float time;

        //missile types
        public const int MAGIC_MISSILE = 0;
        public const int FROST = 1;
        public const int FIRE = 2;
        public const int CORROSION = 3;
        public const int FOLIAGE = 4;
        public const int FORCE = 5;
        public const int BEACON = 6;
        public const int SHADOW = 7;
        public const int RAINBOW = 8;
        public const int EARTH = 9;
        public const int WARD = 10;

        public const int SHAMAN_RED = 11;
        public const int SHAMAN_BLUE = 12;
        public const int SHAMAN_PURPLE = 13;
        public const int TOXIC_VENT = 14;
        public const int ELMO = 15;

        public const int FIRE_CONE = 100;
        public const int FOLIAGE_CONE = 101;

        public void Reset(int type, int from, int to, ICallback callback)
        {
            Reset(type,
                    DungeonTilemap.RaisedTileCenterToWorld(from),
                    DungeonTilemap.RaisedTileCenterToWorld(to),
                    callback);
        }

        public void Reset(int type, Visual from, Visual to, ICallback callback)
        {
            Reset(type,
                    from.Center(),
                    to.Center(),
                    callback);
        }

        public void Reset(int type, Visual from, int to, ICallback callback)
        {
            Reset(type,
                    from.Center(),
                    DungeonTilemap.RaisedTileCenterToWorld(to),
                    callback);
        }

        public void Reset(int type, PointF from, PointF to, ICallback callback)
        {
            this.callback = callback;

            this.to = to;

            x = from.x;
            y = from.y;
            width = 0;
            height = 0;

            PointF d = PointF.Diff(to, from);
            var speed = new PointF(d).Normalize().Scale(SPEED);
            sx = speed.x;
            sy = speed.y;
            time = d.Length() / SPEED;

            switch (type)
            {
                case MAGIC_MISSILE:
                default:
                    Size(4);
                    Pour(WhiteParticle.Factory, 0.01f);
                    break;
                case FROST:
                    Pour(MagicParticle.Factory, 0.01f);
                    break;
                case FIRE:
                    Size(4);
                    Pour(FlameParticle.Factory, 0.01f);
                    break;
                case CORROSION:
                    Size(3);
                    Pour(CorrosionParticle.Missile, 0.01f);
                    break;
                case FOLIAGE:
                    Size(4);
                    Pour(LeafParticle.General, 0.01f);
                    break;
                case FORCE:
                    Pour(SlowParticle.Factory, 0.01f);
                    break;
                case BEACON:
                    Pour(ForceParticle.Factory, 0.01f);
                    break;
                case SHADOW:
                    Size(4);
                    Pour(ShadowParticle.Missile, 0.01f);
                    break;
                case RAINBOW:
                    Size(4);
                    Pour(RainbowParticle.Burst, 0.01f);
                    break;
                case EARTH:
                    Size(4);
                    Pour(EarthParticle.Factory, 0.01f);
                    break;
                case WARD:
                    Size(4);
                    Pour(WardParticle.Factory, 0.01f);
                    break;

                case SHAMAN_RED:
                    Size(2);
                    Pour(ShamanParticle.Red, 0.01f);
                    break;
                case SHAMAN_BLUE:
                    Size(2);
                    Pour(ShamanParticle.Blue, 0.01f);
                    break;
                case SHAMAN_PURPLE:
                    Size(2);
                    Pour(ShamanParticle.Purple, 0.01f);
                    break;
                case TOXIC_VENT:
                    Size(10);
                    Pour(Speck.Factory(Speck.TOXIC), 0.02f);
                    break;
                case ELMO:
                    Size(5);
                    Pour(ElmoParticle.Factory, 0.01f);
                    break;

                case FIRE_CONE:
                    Size(10);
                    Pour(FlameParticle.Factory, 0.03f);
                    break;
                case FOLIAGE_CONE:
                    Size(10);
                    Pour(LeafParticle.General, 0.03f);
                    break;
            }

            Revive();
        }

        public void Size(float size)
        {
            x -= size / 2;
            y -= size / 2;
            width = height = size;
        }

        public void SetSpeed(float newSpeed)
        {
            PointF d = PointF.Diff(to, new PointF(x, y));
            PointF speed = new PointF(d).Normalize().Scale(newSpeed);
            sx = speed.x;
            sy = speed.y;
            time = d.Length() / newSpeed;
        }

        //convenience method for the common case of a bolt going from a character to a tile or enemy
        public static MagicMissile BoltFromChar(Group group, int type, Visual sprite, int to, ICallback callback)
        {
            var missile = group.Recycle<MagicMissile>();

            if (Actor.FindChar(to) != null)
            {
                missile.Reset(type, sprite.Center(), Actor.FindChar(to).sprite.DestinationCenter(), callback);
            }
            else
            {
                missile.Reset(type, sprite, to, callback);
            }

            return missile;
        }

        protected override bool IsFrozen()
        {
            return false; //cannot be frozen
        }

        public override void Update()
        {
            base.Update();
            if (on)
            {
                float d = Game.elapsed;
                x += sx * d;
                y += sy * d;
                if ((time -= d) <= 0)
                {
                    on = false;
                    if (callback != null)
                        callback.Call();
                }
            }
        }

        public class MagicParticle : PixelParticle
        {
            public static Emitter.Factory Factory = new MagicParticleFactory1();
            public static Emitter.Factory Attracting = new MagicParticleFactory2();

            public class MagicParticleFactory1 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<MagicParticle>().Reset(x, y);
                }

                public override bool LightMode()
                {
                    return true;
                }
            }

            public class MagicParticleFactory2 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<MagicParticle>().ResetAttract(x, y);
                }
                public override bool LightMode()
                {
                    return true;
                }
            }

            public MagicParticle()
            {
                SetColor(new Color(0x88, 0xCC, 0xFF, 0xFF));
                lifespan = 0.5f;

                speed.Set(Rnd.Float(-10, +10), Rnd.Float(-10, +10));
            }

            public void Reset(float x, float y)
            {
                Revive();

                this.x = x;
                this.y = y;

                left = lifespan;
            }

            public void ResetAttract(float x, float y)
            {
                Revive();

                //size = 8;
                left = lifespan;

                speed.Polar(Rnd.Float(PointF.PI2), Rnd.Float(16, 32));
                this.x = x - speed.x * lifespan;
                this.y = y - speed.y * lifespan;
            }

            public override void Update()
            {
                base.Update();
                // alpha: 1 -> 0; size: 1 -> 4
                Size(4 - (am = left / lifespan) * 3);
            }
        }

        public class EarthParticle : PixelParticle.Shrinking
        {
            public static Emitter.Factory Factory = new EarthParticleFactory1();
            public static Emitter.Factory Burst = new EarthParticleFactory2();
            public static Emitter.Factory Attract = new EarthParticleFactory3();

            public class EarthParticleFactory1 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<EarthParticle>().Reset(x, y);
                }
            }

            public class EarthParticleFactory2 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<EarthParticle>().ResetBurst(x, y);
                }
            }

            public class EarthParticleFactory3 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<EarthParticle>().ResetAttract(x, y);
                }
            }

            public EarthParticle()
            {
                lifespan = 0.5f;

                acc.Set(0, +40);
            }

            public void Reset(float x, float y)
            {
                Revive();

                this.x = x;
                this.y = y;

                left = lifespan;
                size = 4;

                if (Rnd.Int(10) == 0)
                {
                    var c1 = new Color(0xFF, 0xF2, 0x66, 0xFF);
                    var c2 = new Color(0x80, 0x77, 0x1A, 0xFF);
                    SetColor(ColorMath.Random(c1, c2));
                }
                else
                {
                    var c1 = new Color(0x80, 0x55, 0x00, 0xFF);
                    var c2 = new Color(0x33, 0x25, 0x00, 0xFF);
                    SetColor(ColorMath.Random(c1, c2));
                }

                speed.Set(Rnd.Float(-10, +10), Rnd.Float(-10, +10));
            }

            public void ResetBurst(float x, float y)
            {
                Reset(x, y);

                speed.Polar(Rnd.Float(PointF.PI2), Rnd.Float(40, 60));
            }

            public void ResetAttract(float x, float y)
            {
                Reset(x, y);

                speed.Polar(Rnd.Float(PointF.PI2), Rnd.Float(24, 32));

                this.x = x - speed.x * lifespan;
                this.y = y - speed.y * lifespan;

                acc.Set(0, 0);
            }
        }

        public class ShamanParticle : EarthParticle
        {
            public static Emitter.Factory Red = new ShamanParticleFactory1();
            public static Emitter.Factory Blue = new ShamanParticleFactory2();
            public static Emitter.Factory Purple = new ShamanParticleFactory3();

            public class ShamanParticleFactory1 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    var c1 = new Color(0xFF, 0x4D, 0x4D, 0xFF);
                    var c2 = new Color(0x80, 0x1A, 0x1A, 0xFF);
                    var result = ColorMath.Random(c1, c2);

                    emitter.Recycle<ShamanParticle>().Reset(x, y, result);
                }
            }

            public class ShamanParticleFactory2 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    var c1 = new Color(0x66, 0x99, 0xFF, 0xFF);
                    var c2 = new Color(0x1A, 0x3C, 0x80, 0xFF);
                    var result = ColorMath.Random(c1, c2);

                    emitter.Recycle<ShamanParticle>().Reset(x, y, result);
                }
            }

            public class ShamanParticleFactory3 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    var c1 = new Color(0xBB, 0x33, 0xFF, 0xFF);
                    var c2 = new Color(0x5E, 0x1A, 0x80, 0xFF);
                    var result = ColorMath.Random(c1, c2);

                    emitter.Recycle<ShamanParticle>().Reset(x, y, result);
                }
            }

            Color startColor;
            Color endColor;

            public ShamanParticle()
            {
                lifespan = 0.6f;
                acc.Set(0, 0);
            }

            public void Reset(float x, float y, Color endColor)
            {
                base.Reset(x, y);

                Size(1);

                this.endColor = endColor;
                var c1 = new Color(0x80, 0x55, 0x00, 0xFF);
                var c2 = new Color(0x33, 0x25, 0x00, 0xFF);
                startColor = ColorMath.Random(c1, c2);

                speed.Set(Rnd.Float(-10, +10), Rnd.Float(-10, +10));
            }

            public override void Update()
            {
                base.Update();
                SetColor(ColorMath.Interpolate(endColor, startColor, (left / lifespan)));
            }
        }

        public class WhiteParticle : PixelParticle
        {
            public static Emitter.Factory Factory = new WhiteParticleFactory();

            public class WhiteParticleFactory : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<WhiteParticle>().Reset(x, y);
                }
                public override bool LightMode()
                {
                    return true;
                }
            }

            public WhiteParticle()
            {
                lifespan = 0.4f;

                am = 0.5f;
            }

            public void Reset(float x, float y)
            {
                Revive();

                this.x = x;
                this.y = y;

                left = lifespan;
            }

            public override void Update()
            {
                base.Update();
                // size: 3 -> 0
                Size((left / lifespan) * 3);
            }
        }

        public class SlowParticle : PixelParticle
        {
            private Emitter emitter;

            public static Emitter.Factory Factory = new SlowParticleFactory();

            public class SlowParticleFactory : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<SlowParticle>().Reset(x, y, emitter);
                }
                public override bool LightMode()
                {
                    return true;
                }
            }

            public SlowParticle()
            {
                lifespan = 0.6f;

                SetColor(new Color(0x66, 0x44, 0x22, 0xFF));
                Size(2);
            }

            public void Reset(float x, float y, Emitter emitter)
            {
                Revive();

                this.x = x;
                this.y = y;
                this.emitter = emitter;

                left = lifespan;

                acc.Set(0);
                speed.Set(Rnd.Float(-20, +20), Rnd.Float(-20, +20));
            }

            public override void Update()
            {
                base.Update();

                am = left / lifespan;
                acc.Set((emitter.x - x) * 10, (emitter.y - y) * 10);
            }
        }

        public class ForceParticle : PixelParticle.Shrinking
        {
            public static Emitter.Factory Factory = new ForceParticleFactory();
            public class ForceParticleFactory : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<ForceParticle>().Reset(index, x, y);
                }
            }

            public void Reset(int index, float x, float y)
            {
                base.Reset(x, y, new Color(0xFF, 0xFF, 0xFF, 0xFF), 8, 0.5f);

                speed.Polar(PointF.PI2 / 8 * index, 12);
                this.x -= speed.x * lifespan;
                this.y -= speed.y * lifespan;
            }

            public override void Update()
            {
                base.Update();

                am = (1 - left / lifespan) / 2;
            }
        }

        public class WardParticle : PixelParticle.Shrinking
        {
            public static Emitter.Factory Factory = new WardParticleFactory1();

            public class WardParticleFactory1 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<WardParticle>().Reset(x, y);
                }
                public override bool LightMode()
                {
                    return true;
                }
            }

            public static Emitter.Factory Up = new WardParticleFactory2();
            public class WardParticleFactory2 : Emitter.Factory
            {
                public override void Emit(Emitter emitter, int index, float x, float y)
                {
                    emitter.Recycle<WardParticle>().ResetUp(x, y);
                }
                public override bool LightMode()
                {
                    return true;
                }
            }

            public WardParticle()
            {
                lifespan = 0.6f;

                SetColor(new Color(0x88, 0x22, 0xFF, 0xFF));
            }

            public void Reset(float x, float y)
            {
                Revive();

                base.x = x;
                base.y = y;

                left = lifespan;
                size = 8;
            }

            public void ResetUp(float x, float y)
            {
                Reset(x, y);

                speed.Set(Rnd.Float(-8, +8), Rnd.Float(-32, -48));
            }

            public override void Update()
            {
                base.Update();

                am = 1 - left / lifespan;
            }
        }
    }
}