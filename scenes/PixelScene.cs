using System;
using System.Collections.Generic;
using watabou.glwrap;
using watabou.input;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.effects;
using spdd.messages;
using spdd.ui;

namespace spdd.scenes
{
    public class PixelScene : Scene
    {
        // Minimum virtual display size for portrait orientation
        public const float MIN_WIDTH_P = 135;
        public const float MIN_HEIGHT_P = 225;

        // Minimum virtual display size for landscape orientation
        public const float MIN_WIDTH_L = 240;
        public const float MIN_HEIGHT_L = 160;

        public static int defaultZoom = 0;
        public static int maxDefaultZoom = 0;
        public static int maxScreenZoom = 0;
        public static float minZoom;
        public static float maxZoom;

        public static Camera uiCamera;

        //stylized 3x5 bitmapped pixel font. Only latin characters supported.
        public static BitmapText.Font pixelFont;

        public override void Create()
        {
            base.Create();

            GameScene.scene = null;

            float minWidth, minHeight;
            if (Landscape())
            {
                minWidth = MIN_WIDTH_L;
                minHeight = MIN_HEIGHT_L;
            }
            else
            {
                minWidth = MIN_WIDTH_P;
                minHeight = MIN_HEIGHT_P;
            }

            maxDefaultZoom = (int)Math.Min(Game.width / minWidth, Game.height / minHeight);
            maxScreenZoom = (int)Math.Min(Game.dispWidth / minWidth, Game.dispHeight / minHeight);
            defaultZoom = SPDSettings.Scale();

            defaultZoom = 3;

            if (defaultZoom < Math.Ceiling(Game.density * 2) || defaultZoom > maxDefaultZoom)
            {
                defaultZoom = (int)Math.Ceiling(Game.density * 2.5);
                while ((
                    Game.width / defaultZoom < minWidth ||
                    Game.height / defaultZoom < minHeight
                ) && defaultZoom > 1)
                {
                    --defaultZoom;
                }
            }

            minZoom = 1;
            maxZoom = defaultZoom * 2;

            Camera.Reset(new PixelCamera(defaultZoom));

            float uiZoom = defaultZoom;
            uiCamera = Camera.CreateFullscreen(uiZoom);
            Camera.Add(uiCamera);

            if (pixelFont == null)
            {
                // 3x5 (6)
                pixelFont = BitmapText.Font.ColorMarked(
                    BitmapCache.Get(Assets.Fonts.PIXELFONT),
                    new Color(0x00, 0x00, 0x00, 0x00),
                    BitmapText.Font.LATIN_FULL);
                pixelFont.baseLine = 6;
                pixelFont.tracking = -1;
            }

            //set up the texture size which rendered text will use for any new glyphs.
            int renderedTextPageSize;
            if (defaultZoom <= 3)
                renderedTextPageSize = 256;
            else if (defaultZoom <= 8)
                renderedTextPageSize = 512;
            else
                renderedTextPageSize = 1024;

            //asian languages have many more unique characters, so increase texture size to anticipate that
            if (Messages.Lang() == Languages.KOREAN ||
                Messages.Lang() == Languages.CHINESE ||
                Messages.Lang() == Languages.JAPANESE)
            {
                renderedTextPageSize *= 2;
            }

            Game.platform.SetupFontGenerators(renderedTextPageSize, SPDSettings.SystemFont());
        }

        //FIXME this system currently only works for a subset of windows
        private static List<Type> savedWindows = new List<Type>();  //Class<?extends Window>
        private static Type savedClass;                             // Class<?extends PixelScene>

        public void SaveWindows()
        {
            if (members == null)
                return;

            savedWindows.Clear();
            savedClass = GetType();
            foreach (Gizmo g in members.ToArray())
            {
                if (g is Window)
                {
                    savedWindows.Add(g.GetType());
                }
            }
        }

        public void RestoreWindows()
        {
            if (GetType().Equals(savedClass))
            {
                foreach (Type w in savedWindows)
                {
                    try
                    {
                        Add((Window)Reflection.NewInstanceUnhandled(w));
                    }
                    catch (Exception)
                    {
                        //window has no public zero-arg constructor, just eat the exception
                    }
                }
            }
            savedWindows.Clear();
        }

        public override void Destroy()
        {
            base.Destroy();
            PointerEvent.ClearListeners();
        }

        public static RenderedTextBlock RenderTextBlock(int size)
        {
            return RenderTextBlock("", size);
        }

        public static RenderedTextBlock RenderTextBlock(string text, int size)
        {
            RenderedTextBlock result = new RenderedTextBlock(text, (int)(size * defaultZoom));
            result.Zoom(1 / (float)defaultZoom);
            return result;
        }

        /**
         * These methods align UI elements to device pixels.
         * e.g. if we have a scale of 3x then valid positions are #.0, #.33, #.67
         */
        public static float Align(float pos)
        {
            return (float)Math.Round(pos * defaultZoom, MidpointRounding.AwayFromZero) / (float)defaultZoom;
        }

        public static float Align(Camera camera, float pos)
        {
            return (float)Math.Round(pos * camera.zoom, MidpointRounding.AwayFromZero) / camera.zoom;
        }

        public static void Align(Visual v)
        {
            v.x = Align(v.x);
            v.y = Align(v.y);
        }

        public static void Align(Component c)
        {
            c.SetPos(Align(c.Left()), Align(c.Top()));
        }

        public static bool noFade;

        public virtual void FadeIn()
        {
            if (noFade)
            {
                noFade = false;
            }
            else
            {
                FadeIn(new Color(0x00, 0x00, 0x00, 0xff), false);
            }
        }

        public virtual void FadeIn(Color color, bool light)
        {
            Add(new Fader(color, light));
        }

        public static void ShowBadge(Badges.Badge badge)
        {
            var banner = BadgeBanner.Show(badge.GetImage());
            banner.camera = uiCamera;
            banner.x = Align(banner.camera, (banner.camera.width - banner.width) / 2);
            banner.y = Align(banner.camera, (banner.camera.height - banner.height) / 3);
            Game.Scene().Add(banner);
        }

        public class Fader : ColorBlock
        {
            private const float FADE_TIME = 1f;

            private readonly bool light;

            private float time;

            public Fader(Color color, bool light)
                : base(uiCamera.width, uiCamera.height, color)
            {
                this.light = light;

                camera = uiCamera;

                Alpha(1f);
                time = FADE_TIME;
            }

            public override void Update()
            {
                base.Update();

                if ((time -= Game.elapsed) <= 0)
                {
                    Alpha(0f);
                    parent.Remove(this);
                }
                else
                {
                    Alpha(time / FADE_TIME);
                }
            }

            public override void Draw()
            {
                if (light)
                {
                    Blending.SetLightMode();
                    base.Draw();
                    Blending.SetNormalMode();
                }
                else
                {
                    base.Draw();
                }
            }
        }

        private class PixelCamera : Camera
        {
            public PixelCamera(float zoom)
                : base((int)(Game.width - Math.Ceiling(Game.width / zoom) * zoom) / 2,
                      (int)(Game.height - Math.Ceiling(Game.height / zoom) * zoom) / 2,
                      (int)Math.Ceiling(Game.width / zoom),
                      (int)Math.Ceiling(Game.height / zoom),
                      zoom)
            {
                fullScreen = true;
            }

            protected override void UpdateMatrix()
            {
                float sx = Align(this, scroll.x + shakeX);
                float sy = Align(this, scroll.y + shakeY);

                matrix[0] = +zoom * invW2;
                matrix[5] = -zoom * invH2;

                matrix[12] = -1 + x * invW2 - sx * matrix[0];
                matrix[13] = +1 - y * invH2 - sy * matrix[5];
            }
        }
    }
}