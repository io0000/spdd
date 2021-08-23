using System;
using watabou.noosa;
using watabou.noosa.particles;
using watabou.utils;

namespace spdd.effects
{
    public class Speck : Image
    {
        public const int HEALING = 0;
        public const int STAR = 1;
        public const int LIGHT = 2;
        public const int QUESTION = 3;
        public const int UP = 4;
        public const int SCREAM = 5;
        public const int BONE = 6;
        public const int WOOL = 7;
        public const int ROCK = 8;
        public const int NOTE = 9;
        public const int CHANGE = 10;
        public const int HEART = 11;
        public const int BUBBLE = 12;
        public const int STEAM = 13;
        public const int COIN = 14;

        public const int DISCOVER = 101;
        public const int EVOKE = 102;
        public const int MASTERY = 103;
        public const int KIT = 104;
        public const int RATTLE = 105;
        public const int JET = 106;
        public const int TOXIC = 107;
        public const int CORROSION = 108;
        public const int PARALYSIS = 109;
        public const int DUST = 110;
        public const int STENCH = 111;
        public const int FORGE = 112;
        public const int CONFUSION = 113;
        public const int RED_LIGHT = 114;
        public const int CALM = 115;
        public const int SMOKE = 116;
        public const int STORM = 117;
        public const int INFERNO = 118;
        public const int BLIZZARD = 119;

        private const int SIZE = 7;

        private int type;
        private float lifespan;
        private float left;

        private static TextureFilm film;

        private static SparseArray<Emitter.Factory> factories = new SparseArray<Emitter.Factory>();

        public Speck()
        {
            Texture(Assets.Effects.SPECKS);

            if (film == null)
                film = new TextureFilm(texture, SIZE, SIZE);

            origin.Set(SIZE / 2f);
        }

        public void Reset(int index, float x, float y, int type)
        {
            Revive();

            this.type = type;
            switch (type)
            {
                case DISCOVER:
                case RED_LIGHT:
                    Frame(film.Get(LIGHT));
                    break;
                case EVOKE:
                case MASTERY:
                case KIT:
                case FORGE:
                    Frame(film.Get(STAR));
                    break;
                case RATTLE:
                    Frame(film.Get(BONE));
                    break;
                case JET:
                case TOXIC:
                case CORROSION:
                case PARALYSIS:
                case STENCH:
                case CONFUSION:
                case STORM:
                case DUST:
                case SMOKE:
                case BLIZZARD:
                case INFERNO:
                    Frame(film.Get(STEAM));
                    break;
                case CALM:
                    Frame(film.Get(SCREAM));
                    break;
                default:
                    Frame(film.Get(type));
                    break;
            }

            base.x = x - origin.x;
            base.y = y - origin.y;

            ResetColor();
            scale.Set(1);
            speed.Set(0);
            acc.Set(0);
            angle = 0;
            angularSpeed = 0;

            switch (type)
            {
                case HEALING:
                    speed.Set(0, -20);
                    lifespan = 1f;
                    break;

                case STAR:
                    speed.Polar(Rnd.Float(2 * 3.1415926f), Rnd.Float(128));
                    acc.Set(0, 128);
                    angle = Rnd.Float(360);
                    angularSpeed = Rnd.Float(-360, +360);
                    lifespan = 1f;
                    break;

                case FORGE:
                    speed.Polar(Rnd.Float(-3.1415926f, 0), Rnd.Float(64));
                    acc.Set(0, 128);
                    angle = Rnd.Float(360);
                    angularSpeed = Rnd.Float(-360, +360);
                    lifespan = 0.51f;
                    break;

                case EVOKE:
                    speed.Polar(Rnd.Float(-3.1415926f, 0), 50);
                    acc.Set(0, 50);
                    angle = Rnd.Float(360);
                    angularSpeed = Rnd.Float(-180, +180);
                    lifespan = 1f;
                    break;

                case KIT:
                    speed.Polar(index * 3.1415926f / 5, 50);
                    acc.Set(-speed.x, -speed.y);
                    angle = index * 36;
                    angularSpeed = 360;
                    lifespan = 1f;
                    break;

                case MASTERY:
                    speed.Set(Rnd.Int(2) == 0 ? Rnd.Float(-128, -64) : Rnd.Float(+64, +128), 0);
                    angularSpeed = speed.x < 0 ? -180 : +180;
                    acc.Set(-speed.x, 0);
                    lifespan = 0.5f;
                    break;

                case RED_LIGHT:
                    Tint(new Color(0xCC, 0x00, 0x00, 0xFF));
                    goto case LIGHT;
                case LIGHT:
                    angle = Rnd.Float(360);
                    angularSpeed = 90;
                    lifespan = 1f;
                    break;

                case DISCOVER:
                    angle = Rnd.Float(360);
                    angularSpeed = 90;
                    lifespan = 0.5f;
                    am = 0;
                    break;

                case QUESTION:
                    lifespan = 0.8f;
                    break;

                case UP:
                    speed.Set(0, -20);
                    lifespan = 1f;
                    break;

                case CALM:
                    SetColor(0, 1, 1);
                    goto case SCREAM;
                case SCREAM:
                    lifespan = 0.9f;
                    break;

                case BONE:
                    lifespan = 0.2f;
                    speed.Polar(Rnd.Float(2 * 3.1415926f), 24 / lifespan);
                    acc.Set(0, 128);
                    angle = Rnd.Float(360);
                    angularSpeed = 360;
                    break;

                case RATTLE:
                    lifespan = 0.5f;
                    speed.Set(0, -200);
                    acc.Set(0, -2 * speed.y / lifespan);
                    angle = Rnd.Float(360);
                    angularSpeed = 360;
                    break;

                case WOOL:
                    lifespan = 0.5f;
                    speed.Set(0, -50);
                    angle = Rnd.Float(360);
                    angularSpeed = Rnd.Float(-360, +360);
                    break;

                case ROCK:
                    angle = Rnd.Float(360);
                    angularSpeed = Rnd.Float(-360, +360);
                    scale.Set(Rnd.Float(1, 2));
                    speed.Set(0, 64);
                    lifespan = 0.2f;
                    this.y -= speed.y * lifespan;
                    break;

                case NOTE:
                    angularSpeed = Rnd.Float(-30, +30);
                    speed.Polar((angularSpeed - 90) * PointF.G2R, 30);
                    lifespan = 1f;
                    break;

                case CHANGE:
                    angle = Rnd.Float(360);
                    speed.Polar((angle - 90) * PointF.G2R, Rnd.Float(4, 12));
                    lifespan = 1.5f;
                    break;

                case HEART:
                    speed.Set(Rnd.Int(-10, +10), -40);
                    angularSpeed = Rnd.Float(-45, +45);
                    lifespan = 1f;
                    break;

                case BUBBLE:
                    speed.Set(0, -15);
                    scale.Set(Rnd.Float(0.8f, 1));
                    lifespan = Rnd.Float(0.8f, 1.5f);
                    break;

                case STEAM:
                    speed.y = -Rnd.Float(10, 15);
                    angularSpeed = Rnd.Float(+180);
                    angle = Rnd.Float(360);
                    lifespan = 1f;
                    break;

                case JET:
                    speed.y = +32;
                    acc.y = -64;
                    angularSpeed = Rnd.Float(180, 360);
                    angle = Rnd.Float(360);
                    lifespan = 0.5f;
                    break;

                case TOXIC:
                    Hardlight(new Color(0x50, 0xFF, 0x60, 0xFF));
                    angularSpeed = 30;
                    angle = Rnd.Float(360);
                    lifespan = Rnd.Float(1f, 3f);
                    break;

                case CORROSION:
                    Hardlight(new Color(0xAA, 0xAA, 0xAA, 0xFF));
                    angularSpeed = 30;
                    angle = Rnd.Float(360);
                    lifespan = Rnd.Float(1f, 3f);
                    break;

                case PARALYSIS:
                    Hardlight(new Color(0xFF, 0xFF, 0x66, 0xFF));
                    angularSpeed = -30;
                    angle = Rnd.Float(360);
                    lifespan = Rnd.Float(1f, 3f);
                    break;

                case STENCH:
                    Hardlight(new Color(0x00, 0x33, 0x00, 0xFF));
                    angularSpeed = -30;
                    angle = Rnd.Float(360);
                    lifespan = Rnd.Float(1f, 3f);
                    break;

                case CONFUSION:
                    // hardlight( Random.Int( 0x1000000 ) | 0x000080 );
                    byte r = (byte)Rnd.Int(0xff);
                    byte g = (byte)Rnd.Int(0xff);
                    byte b = (byte)Rnd.Int(0xff);
                    b |= (byte)0x80;

                    Hardlight(new Color(r, g, b, 0xff));
                    angularSpeed = Rnd.Float(-20, +20);
                    angle = Rnd.Float(360);
                    lifespan = Rnd.Float(1f, 3f);
                    break;

                case STORM:
                    Hardlight(new Color(0x8A, 0xD8, 0xD8, 0xFF));
                    angularSpeed = Rnd.Float(-20, +20);
                    angle = Rnd.Float(360);
                    lifespan = Rnd.Float(1f, 3f);
                    break;

                case INFERNO:
                    Hardlight(new Color(0xEE, 0x77, 0x22, 0xFF));
                    angularSpeed = Rnd.Float(200, 300) * (Rnd.Int(2) == 0 ? -1 : 1);
                    angle = Rnd.Float(360);
                    lifespan = Rnd.Float(1f, 3f);
                    break;

                case BLIZZARD:
                    Hardlight(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                    angularSpeed = Rnd.Float(200, 300) * (Rnd.Int(2) == 0 ? -1 : 1);
                    angle = Rnd.Float(360);
                    lifespan = Rnd.Float(1f, 3f);
                    break;

                case SMOKE:
                    Hardlight(new Color(0x00, 0x00, 0x00, 0xFF));
                    angularSpeed = 30;
                    angle = Rnd.Float(360);
                    lifespan = Rnd.Float(1f, 1.5f);
                    break;

                case DUST:
                    Hardlight(new Color(0xFF, 0xFF, 0x66, 0xFF));
                    angle = Rnd.Float(360);
                    speed.Polar(Rnd.Float(2 * 3.1415926f), Rnd.Float(16, 48));
                    lifespan = 0.5f;
                    break;

                case COIN:
                    speed.Polar(-PointF.PI * Rnd.Float(0.3f, 0.7f), Rnd.Float(48, 96));
                    acc.y = 256;
                    lifespan = -speed.y / acc.y * 2;
                    break;
            }

            left = lifespan;
        }

        public override void Update()
        {
            base.Update();

            left -= Game.elapsed;
            if (left <= 0)
            {
                Kill();
            }
            else
            {
                float p = 1 - left / lifespan; // 0 -> 1

                switch (type)
                {
                    case STAR:
                    case FORGE:
                        scale.Set(1 - p);
                        am = p < 0.2f ? p * 5f : (1 - p) * 1.25f;
                        break;

                    case KIT:
                    case MASTERY:
                        am = 1 - p * p;
                        break;

                    case EVOKE:
                    case HEALING:
                        am = p < 0.5f ? 1 : 2 - p * 2;
                        break;

                    case RED_LIGHT:
                    case LIGHT:
                        am = scale.Set(p < 0.2f ? p * 5f : (1 - p) * 1.25f).x;
                        break;

                    case DISCOVER:
                        am = 1 - p;
                        scale.Set((p < 0.5f ? p : 1 - p) * 2);
                        break;

                    case QUESTION:
                        scale.Set((float)(Math.Sqrt(p < 0.5f ? p : 1 - p) * 3));
                        break;

                    case UP:
                        scale.Set((float)(Math.Sqrt(p < 0.5f ? p : 1 - p) * 2));
                        break;

                    case CALM:
                    case SCREAM:
                        am = (float)Math.Sqrt((p < 0.5f ? p : 1 - p) * 2f);
                        scale.Set(p * 7);
                        break;

                    case BONE:
                    case RATTLE:
                        am = p < 0.9f ? 1 : (1 - p) * 10;
                        break;

                    case ROCK:
                        am = p < 0.2f ? p * 5 : 1;
                        break;

                    case NOTE:
                        am = 1 - p * p;
                        break;

                    case WOOL:
                        scale.Set(1 - p);
                        break;

                    case CHANGE:
                        am = (float)Math.Sqrt((p < 0.5f ? p : 1 - p) * 2);
                        scale.y = (1 + p) * 0.5f;
                        scale.x = scale.y * (float)Math.Cos(left * 15);
                        break;

                    case HEART:
                        scale.Set(1 - p);
                        am = 1 - p * p;
                        break;

                    case BUBBLE:
                        am = p < 0.2f ? p * 5 : 1;
                        break;

                    case STEAM:
                    case TOXIC:
                    case PARALYSIS:
                    case CONFUSION:
                    case STORM:
                    case BLIZZARD:
                    case INFERNO:
                    case DUST:
                        am = p < 0.5f ? p : 1 - p;
                        scale.Set(1 + p * 2);
                        break;

                    case CORROSION:
                        var c1 = new Color(0xAA, 0xAA, 0xAA, 0xFF);
                        var c2 = new Color(0xFF, 0x88, 0x00, 0xFF);
                        Hardlight(ColorMath.Interpolate(c1, c2, p));
                        goto case STENCH;
                    case STENCH:
                    case SMOKE:
                        am = (float)Math.Sqrt((p < 0.5f ? p : 1 - p));
                        scale.Set(1 + p);
                        break;

                    case JET:
                        am = (p < 0.5f ? p : 1 - p) * 2;
                        scale.Set(p * 1.5f);
                        break;

                    case COIN:
                        scale.x = (float)Math.Cos(left * 5);
                        rm = gm = bm = (Math.Abs(scale.x) + 1) * 0.5f;
                        am = p < 0.9f ? 1 : (1 - p) * 10;
                        break;
                }
            }
        }

        public static Emitter.Factory Factory(int type)
        {
            return Factory(type, false);
        }

        public static Emitter.Factory Factory(int type, bool lightMode)
        {
            var factory = factories[type];

            if (factory == null)
            {
                factory = new SpeckFactory(type, lightMode);
                factories.Add(type, factory);
            }

            return factory;
        }

        public class SpeckFactory : Emitter.Factory
        {
            private readonly int type;
            private readonly bool lightMode;

            public SpeckFactory(int type, bool lightMode)
            {
                this.type = type;
                this.lightMode = lightMode;
            }

            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                Speck p = emitter.Recycle<Speck>();
                p.Reset(index, x, y, type);
            }

            public override bool LightMode()
            {
                return lightMode;
            }
        }
    }
}