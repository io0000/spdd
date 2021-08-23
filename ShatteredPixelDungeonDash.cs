using System;
using OpenTK.Windowing.Desktop;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.scenes;


namespace spdd
{
    public class ShatteredPixelDungeonDash : Game
    {
        ////variable constants for specific older versions of shattered, used for data conversion
        ////versions older than v0.7.3b are no longer supported, and data from them is ignored
        //public const int v0_7_3b = 349;
        //public const int v0_7_4c = 362;
        //public const int v0_7_5e = 382;
        //
        //public const int v0_8_0 = 412;
        //public const int v0_8_1a = 422;
        public const int v0_8_2 = 447;
        private GameWindow gameWindow;

        public ShatteredPixelDungeonDash(PlatformSupport platform, GameWindow gameWindow)
            : base(sceneClass == null ? typeof(WelcomeScene) : sceneClass, platform)
        {
            this.gameWindow = gameWindow;
        }

        public override void Create(float density, int dispWidth, int dispHeight)
        {
            base.Create(density, dispWidth, dispHeight);

            UpdateSystemUI();
            SPDAction.LoadBindings();

            Music.Instance.Enable(SPDSettings.Music());
            Music.Instance.Volume(SPDSettings.MusicVol() * SPDSettings.MusicVol() / 100f);
            Sample.Instance.Enable(SPDSettings.SoundFx());
            Sample.Instance.Volume(SPDSettings.SFXVol() * SPDSettings.SFXVol() / 100f);

            Sample.Instance.Load(Assets.Sounds.all);
        }

        // Class<? extends PixelScene>
        public static void SwitchNoFade(Type c)
        {
            SwitchNoFade(c, null);
        }

        // Class<? extends PixelScene>
        public static void SwitchNoFade(Type c, ISceneChangeCallback callback)
        {
            PixelScene.noFade = true;
            SwitchScene(c, callback);
        }

        public static void SeamlessResetScene(ISceneChangeCallback callback)
        {
            if (Scene() is PixelScene)
            {
                ((PixelScene)Scene()).SaveWindows();
                SwitchNoFade(sceneClass, callback);
            }
            else
            {
                ResetScene();
            }
        }

        public static void SeamlessResetScene()
        {
            SeamlessResetScene(null);
        }

        protected override void SwitchScene()
        {
            base.SwitchScene();
            if (scene is PixelScene)
            {
                ((PixelScene)scene).RestoreWindows();
            }
        }

        public override void Resize(int width, int height)
        {
            if (width == 0 || height == 0)
                return;

            if (scene is PixelScene &&
                (height != Game.height || width != Game.width))
            {
                PixelScene.noFade = true;
                ((PixelScene)scene).SaveWindows();
            }

            base.Resize(width, height);

            UpdateDisplaySize();
        }

        public override void Destroy()
        {
            base.Destroy();
            GameScene.EndActorThread();
        }

        public void UpdateDisplaySize()
        {
            platform.UpdateDisplaySize();
        }

        public static void UpdateSystemUI()
        {
            platform.UpdateSystemUI();
        }

        public override void Finish()
        {
            //gameWindow.Exit();
            gameWindow.Close();
        }
    }
}