using System;
using watabou.gltextures;
using watabou.glwrap;
using watabou.input;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.hero;
using spdd.items.artifacts;
using spdd.items.wands;
using spdd.items.weapon.melee;
using spdd.messages;
using spdd.sprites;
using spdd.ui;

namespace spdd.scenes
{
    public class SurfaceScene : PixelScene
    {
        private const int FRAME_WIDTH = 88;
        private const int FRAME_HEIGHT = 125;

        private const int FRAME_MARGIN_TOP = 9;
        private const int FRAME_MARGIN_X = 4;

        private const int BUTTON_HEIGHT = 20;

        private const int SKY_WIDTH = 80;
        private const int SKY_HEIGHT = 112;

        private const int NSTARS = 100;
        private const int NCLOUDS = 5;

        private Camera viewport;

        public override void Create()
        {
            base.Create();

            Music.Instance.Play(Assets.Music.SURFACE, true);

            uiCamera.visible = false;

            var w = Camera.main.width;
            var h = Camera.main.height;

            var archs = new Archs();
            archs.reversed = true;
            archs.SetSize(w, h);
            Add(archs);

            var vx = Align((w - SKY_WIDTH) / 2f);
            float vy = Align((h - SKY_HEIGHT - BUTTON_HEIGHT) / 2f);

            Point s = Camera.main.CameraToScreen(vx, vy);
            viewport = new Camera(s.x, s.y, SKY_WIDTH, SKY_HEIGHT, defaultZoom);
            Camera.Add(viewport);

            Group window = new Group();
            window.camera = viewport;
            Add(window);

            //bool dayTime = Calendar.getInstance().get(Calendar.HOUR_OF_DAY) >= 7;
            bool dayTime = DateTime.Now.Hour >= 7;

            Sky sky = new Sky(dayTime);
            sky.scale.Set(SKY_WIDTH, SKY_HEIGHT);
            window.Add(sky);

            if (!dayTime)
            {
                for (int i = 0; i < NSTARS; ++i)
                {
                    float size = Rnd.Float();
                    ColorBlock star = new ColorBlock(size, size, new Color(0xFF, 0xFF, 0xFF, 0xFF));
                    star.x = Rnd.Float(SKY_WIDTH) - size / 2;
                    star.y = Rnd.Float(SKY_HEIGHT) - size / 2;
                    star.am = size * (1 - star.y / SKY_HEIGHT);
                    window.Add(star);
                }
            }

            float range = SKY_HEIGHT * 2 / 3;
            for (int i = 0; i < NCLOUDS; ++i)
            {
                Cloud cloud = new Cloud((NCLOUDS - 1 - i) * (range / NCLOUDS) + Rnd.Float(range / NCLOUDS), dayTime);
                window.Add(cloud);
            }

            int nPatches = (int)(sky.Width() / GrassPatch.WIDTH + 1);

            for (int i = 0; i < nPatches * 4; ++i)
            {
                GrassPatch patch = new GrassPatch((i - 0.75f) * GrassPatch.WIDTH / 4, SKY_HEIGHT + 1, dayTime);
                patch.Brightness(dayTime ? 0.7f : 0.4f);
                window.Add(patch);
            }

            Avatar a = new Avatar(Dungeon.hero.heroClass);
            // Removing semitransparent contour
            a.am = 2; a.aa = -1;
            a.x = (SKY_WIDTH - a.width) / 2;
            a.y = SKY_HEIGHT - a.height;
            Align(a);

            Pet pet = new Pet();
            pet.rm = pet.gm = pet.bm = 1.2f;
            pet.x = SKY_WIDTH / 2 + 2;
            pet.y = SKY_HEIGHT - pet.height;
            Align(pet);

            //allies. Attempts to pick highest level, but prefers rose > earth > ward.
            //Rose level is halved because it's easier to upgrade
            CharSprite allySprite = null;

            //picks the highest between ghost's weapon, armor, and rose level/2
            int roseLevel = 0;
            var rose = Dungeon.hero.belongings.GetItem<DriedRose>();
            if (rose != null)
            {
                roseLevel = rose.GetLevel() / 2;
                if (rose.GhostWeapon() != null)
                {
                    roseLevel = Math.Max(roseLevel, rose.GhostWeapon().GetLevel());
                }
                if (rose.GhostArmor() != null)
                {
                    roseLevel = Math.Max(roseLevel, rose.GhostArmor().GetLevel());
                }
            }

            int earthLevel = Dungeon.hero.belongings.GetItem<WandOfLivingEarth>() == null ? 0 : Dungeon.hero.belongings.GetItem<WandOfLivingEarth>().GetLevel();
            int wardLevel = Dungeon.hero.belongings.GetItem<WandOfWarding>() == null ? 0 : Dungeon.hero.belongings.GetItem<WandOfWarding>().GetLevel();

            MagesStaff staff = Dungeon.hero.belongings.GetItem<MagesStaff>();
            if (staff != null)
            {
                if (staff.WandClass() == typeof(WandOfLivingEarth))
                {
                    earthLevel = Math.Max(earthLevel, staff.GetLevel());
                }
                else if (staff.WandClass() == typeof(WandOfWarding))
                {
                    wardLevel = Math.Max(wardLevel, staff.GetLevel());
                }
            }

            if (roseLevel >= 3 && roseLevel >= earthLevel && roseLevel >= wardLevel)
            {
                allySprite = new GhostSprite();
                if (dayTime) allySprite.Alpha(0.4f);
            }
            else if (earthLevel >= 3 && earthLevel >= wardLevel)
            {
                allySprite = new EarthGuardianSprite();
            }
            else if (wardLevel >= 3)
            {
                allySprite = new WardSprite();
                ((WardSprite)allySprite).UpdateTier(Math.Min(wardLevel + 2, 6));
            }

            if (allySprite != null)
            {
                allySprite.Add(CharSprite.State.PARALYSED);
                allySprite.scale = new PointF(2, 2);
                allySprite.x = a.x - allySprite.Width() * 0.75f;
                allySprite.y = SKY_HEIGHT - allySprite.Height();
                Align(allySprite);
                window.Add(allySprite);
            }

            window.Add(a);
            window.Add(pet);

            var pa = new SurfaceScenePointerArea(sky);
            pa.pet = pet;
            window.Add(pa);

            for (int i = 0; i < nPatches; ++i)
            {
                GrassPatch patch = new GrassPatch((i - 0.5f) * GrassPatch.WIDTH, SKY_HEIGHT, dayTime);
                patch.Brightness(dayTime ? 1.0f : 0.8f);
                window.Add(patch);
            }

            Image frame = new Image(Assets.Interfaces.SURFACE);

            frame.Frame(0, 0, FRAME_WIDTH, FRAME_HEIGHT);
            frame.x = vx - FRAME_MARGIN_X;
            frame.y = vy - FRAME_MARGIN_TOP;
            Add(frame);

            if (dayTime)
            {
                a.Brightness(1.2f);
                pet.Brightness(1.2f);
            }
            else
            {
                frame.Hardlight(new Color(0xDD, 0xEE, 0xFF, 0xFF));
            }

            var gameOver = new ActionRedButton(Messages.Get(this, "exit"));
            gameOver.action = () => Game.SwitchScene(typeof(RankingsScene));

            gameOver.SetSize(SKY_WIDTH - FRAME_MARGIN_X * 2, BUTTON_HEIGHT);
            gameOver.SetPos(frame.x + FRAME_MARGIN_X * 2, frame.y + frame.height + 4);
            Add(gameOver);

            BadgesExtensions.ValidateHappyEnd();

            FadeIn();
        }

        class SurfaceScenePointerArea : PointerArea
        {
            internal Pet pet;

            public SurfaceScenePointerArea(Visual target)
                : base(target)
            { }

            public override void OnClick(PointerEvent touch)
            {
                pet.Jump();
            }
        }

        public override void Destroy()
        {
            BadgesExtensions.SaveGlobal();

            Camera.Remove(viewport);
            base.Destroy();
        }

        public override void OnBackPressed()
        { }

        private class Sky : Visual
        {
            private static Color[] day = { new Color(0x44, 0x88, 0xFF, 0xFF), new Color(0xCC, 0xEE, 0xFF, 0xFF) };
            private static Color[] night = { new Color(0x00, 0x11, 0x55, 0xFF), new Color(0x33, 0x59, 0x80, 0xFF) };

            private readonly Texture texture;
            // 		private FloatBuffer verticesBuffer;
            private readonly float[] verticesBuffer;

            public Sky(bool dayTime)
                : base(0, 0, 1, 1)
            {
                texture = TextureCache.CreateGradient(dayTime ? day : night);

                verticesBuffer = new float[16];

                verticesBuffer[2] = 0.25f;
                verticesBuffer[6] = 0.25f;
                verticesBuffer[10] = 0.75f;
                verticesBuffer[14] = 0.75f;

                verticesBuffer[3] = 0;
                verticesBuffer[7] = 1;
                verticesBuffer[11] = 1;
                verticesBuffer[15] = 0;

                verticesBuffer[0] = 0;
                verticesBuffer[1] = 0;

                verticesBuffer[4] = 1;
                verticesBuffer[5] = 0;

                verticesBuffer[8] = 1;
                verticesBuffer[9] = 1;

                verticesBuffer[12] = 0;
                verticesBuffer[13] = 1;
            }

            public override void Draw()
            {
                base.Draw();

                var script = NoosaScript.Get();

                texture.Bind();

                script.Camera(GetCamera());

                script.uModel.ValueM4(matrix);
                script.Lighting(
                    rm, gm, bm, am,
                    ra, ga, ba, aa);

                script.DrawQuad(verticesBuffer);
            }
        }

        private class Cloud : Image
        {
            private static int lastIndex = -1;

            public Cloud(float y, bool dayTime)
                : base(Assets.Interfaces.SURFACE)
            {
                int index;
                do
                {
                    index = Rnd.Int(3);
                }
                while (index == lastIndex);

                switch (index)
                {
                    case 0:
                        Frame(88, 0, 49, 20);
                        break;
                    case 1:
                        Frame(88, 20, 49, 22);
                        break;
                    case 2:
                        Frame(88, 42, 50, 18);
                        break;
                }

                lastIndex = index;

                this.y = y;

                scale.Set(1 - y / SKY_HEIGHT);
                x = Rnd.Float(SKY_WIDTH + Width()) - Width();
                speed.x = scale.x * (dayTime ? +8 : -8);

                if (dayTime)
                {
                    Tint(new Color(0xCC, 0xEE, 0xFF, 0xFF), 1 - scale.y);
                }
                else
                {
                    rm = gm = bm = +3.0f;
                    ra = ga = ba = -2.1f;
                }
            }

            public override void Update()
            {
                base.Update();
                if (speed.x > 0 && x > SKY_WIDTH)
                    x = -Width();
                else if (speed.x < 0 && x < -Width())
                    x = SKY_WIDTH;
            }
        }

        private class Avatar : Image
        {
            private const int WIDTH = 24;
            private const int HEIGHT = 32;

            public Avatar(HeroClass cl)
                : base(Assets.Sprites.AVATARS)
            {
                Frame(new TextureFilm(texture, WIDTH, HEIGHT).Get((int)cl));
            }
        }

        private class Pet : RatSprite
        {
            public void Jump()
            {
                Play(run);
            }

            public override void OnComplete(Animation anim)
            {
                if (anim == run)
                    Idle();
            }
        }

        private class GrassPatch : Image
        {
            public const int WIDTH = 16;
            public const int HEIGHT = 14;

            private float tx;
            private float ty;

            private double a = Rnd.Float(5);
            private new double angle;

            private bool forward;

            public GrassPatch(float tx, float ty, bool forward)
                : base(Assets.Interfaces.SURFACE)
            {
                Frame(88 + Rnd.Int(4) * WIDTH, 60, WIDTH, HEIGHT);

                this.tx = tx;
                this.ty = ty;

                this.forward = forward;
            }

            public override void Update()
            {
                base.Update();
                a += Rnd.Float(Game.elapsed * 5);
                angle = (2 + Math.Cos(a)) * (forward ? +0.2 : -0.2);

                scale.y = (float)Math.Cos(angle);

                x = tx + (float)Math.Tan(angle) * width;
                y = ty - scale.y * height;
            }

            protected override void UpdateMatrix()
            {
                base.UpdateMatrix();
                watabou.glwrap.Matrix.SkewX(matrix, (float)(angle / watabou.glwrap.Matrix.G2RAD));
            }
        }
    }
}