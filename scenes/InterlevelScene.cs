using System;
using System.IO;
using watabou.gltextures;
using watabou.glwrap;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.levels;
using spdd.levels.features;
using spdd.levels.rooms.special;
using spdd.messages;
using spdd.services.updates;
using spdd.ui;
using spdd.windows;

namespace spdd.scenes
{
    public class InterlevelScene : PixelScene
    {
        //slow fade on entering a new region
        private const float SLOW_FADE = 1f; //.33 in, 1.33 steady, .33 out, 2 seconds total
        //norm fade when loading, falling, returning, or descending to a new floor
        private const float NORM_FADE = 0.67f; //.33 in, .67 steady, .33 out, 1.33 seconds total
        //fast fade when ascending, or descending to a floor you've been on
        private const float FAST_FADE = 0.50f; //.33 in, .33 steady, .33 out, 1 second total

        private static float fadeTime;

        public enum Mode
        {
            DESCEND, ASCEND, CONTINUE, RESURRECT, RETURN, FALL, RESET, NONE
        }
        public static Mode mode;

        public static int returnDepth;
        public static int returnPos;

        public static bool noStory;

        public static bool fallIntoPit;

        private enum Phase
        {
            FADE_IN, STATIC, FADE_OUT
        }
        private Phase phase;
        private float timeLeft;

        private RenderedTextBlock message;

        //private static Thread thread;
        private static Exception error = null;
        private float waitingTime;

        public override void Create()
        {
            base.Create();

            string loadingAsset;
            int loadingDepth;
            float scrollSpeed;
            fadeTime = NORM_FADE;

            switch (mode)
            {
                default:
                    loadingDepth = Dungeon.depth;
                    scrollSpeed = 0;
                    break;
                case Mode.CONTINUE:
                    loadingDepth = GamesInProgress.Check(GamesInProgress.curSlot).depth;
                    scrollSpeed = 5;
                    break;
                case Mode.DESCEND:
                    if (Dungeon.hero == null)
                    {
                        loadingDepth = 1;
                        fadeTime = SLOW_FADE;
                    }
                    else
                    {
                        loadingDepth = Dungeon.depth + 1;
                        if (!(Statistics.deepestFloor < loadingDepth))
                        {
                            fadeTime = FAST_FADE;
                        }
                        else if (loadingDepth == 6 ||
                            loadingDepth == 11 ||
                            loadingDepth == 16 ||
                            loadingDepth == 21)
                        {
                            fadeTime = SLOW_FADE;
                        }
                    }
                    scrollSpeed = 5;
                    break;
                case Mode.FALL:
                    loadingDepth = Dungeon.depth + 1;
                    scrollSpeed = 50;
                    break;
                case Mode.ASCEND:
                    fadeTime = FAST_FADE;
                    loadingDepth = Dungeon.depth - 1;
                    scrollSpeed = -5;
                    break;
                case Mode.RETURN:
                    loadingDepth = returnDepth;
                    scrollSpeed = returnDepth > Dungeon.depth ? 15 : -15;
                    break;
            }

            if (loadingDepth <= 5)
                loadingAsset = Assets.Interfaces.LOADING_SEWERS;
            else if (loadingDepth <= 10)
                loadingAsset = Assets.Interfaces.LOADING_PRISON;
            else if (loadingDepth <= 15)
                loadingAsset = Assets.Interfaces.LOADING_CAVES;
            else if (loadingDepth <= 20)
                loadingAsset = Assets.Interfaces.LOADING_CITY;
            else if (loadingDepth <= 25)
                loadingAsset = Assets.Interfaces.LOADING_HALLS;
            else
                loadingAsset = Assets.Interfaces.SHADOW;

            //slow down transition when displaying an install prompt
            if (Updates.IsInstallable())
            {
                fadeTime += 0.5f; //adds 1 second total
            }
            //speed up transition when debugging
            else if (DeviceCompat.IsDebug())
            {
                fadeTime /= 2;
            }

            var bg = new InterlevelSceneBg(Camera.main.width, Camera.main.height, loadingAsset);
            bg.scrollSpeed = scrollSpeed;
            bg.Scale(4, 4);
            Add(bg);

            Color[] colorArr = new Color[]
            {
                new Color(0x00, 0x00, 0x00, 0xAA),
                new Color(0x00, 0x00, 0x00, 0xBB),
                new Color(0x00, 0x00, 0x00, 0xCC),
                new Color(0x00, 0x00, 0x00, 0xDD),
                new Color(0x00, 0x00, 0x00, 0xFF)
            };

            var im = new InterlevelSceneImage(TextureCache.CreateGradient(colorArr));
            im.scene = this;
            im.angle = 90;
            im.x = Camera.main.width;
            im.scale.x = Camera.main.height / 5f;
            im.scale.y = Camera.main.width;
            Add(im);

            string text = Messages.Get(typeof(Mode), mode.ToString());

            message = PixelScene.RenderTextBlock(text, 9);
            message.SetPos(
                (Camera.main.width - message.Width()) / 2,
                (Camera.main.height - message.Height()) / 2
            );
            Align(message);
            Add(message);

            if (Updates.IsInstallable())
            {
                var install = new InterlevelSceneStyleButton(Chrome.Type.GREY_BUTTON_TR, Messages.Get(this, "install"));
                install.scene = this;

                install.Icon(Icons.CHANGES.Get());
                install.TextColor(Window.SHPX_COLOR);
                install.SetSize(install.ReqWidth() + 5, 20);
                install.SetPos((Camera.main.width - install.Width()) / 2, (Camera.main.height - message.Bottom()) / 3 + message.Bottom());
                Add(install);
            }

            phase = Phase.FADE_IN;
            timeLeft = fadeTime;

            waitingTime = 0f;  // 여기로 옮김( multithread로 처리하지 않기 때문에 )

            //ThreadRun();
        }

        private void ThreadRun()
        {
            try
            {
                if (Dungeon.hero != null)
                    Dungeon.hero.SpendToWhole();

                Actor.FixTime();

                switch (mode)
                {
                    case Mode.DESCEND:
                        Descend();
                        break;
                    case Mode.ASCEND:
                        Ascend();
                        break;
                    case Mode.CONTINUE:
                        Restore();
                        break;
                    case Mode.RESURRECT:
                        Resurrect();
                        break;
                    case Mode.RETURN:
                        ReturnTo();
                        break;
                    case Mode.FALL:
                        Fall();
                        break;
                    case Mode.RESET:
                        Reset();
                        break;
                }
            }
            catch (Exception e)
            {
                error = e;
            }

            if (phase == Phase.STATIC && error == null)
            {
                phase = Phase.FADE_OUT;
                timeLeft = fadeTime;
            }
        }

        private class InterlevelSceneBg : SkinnedBlock
        {
            internal float scrollSpeed;

            public InterlevelSceneBg(float width, float height, object tx)
                : base(width, height, tx)
            { }

            public override void Draw()
            {
                Blending.Disable();
                base.Draw();
                Blending.Enable();
            }

            public override void Update()
            {
                base.Update();
                Offset(0, Game.elapsed * scrollSpeed);
            }
        }

        private class InterlevelSceneImage : Image
        {
            internal InterlevelScene scene;
            public InterlevelSceneImage(object tx)
                : base(tx)
            { }

            public override void Update()
            {
                base.Update();
                if (scene.phase == Phase.FADE_IN)
                    aa = Math.Max(0, (scene.timeLeft - (fadeTime - 0.333f)));
                else if (scene.phase == Phase.FADE_OUT)
                    aa = Math.Max(0, (0.333f - scene.timeLeft));
                else
                    aa = 0;
            }
        }

        private class InterlevelSceneStyleButton : StyledButton
        {
            internal InterlevelScene scene;

            public InterlevelSceneStyleButton(Chrome.Type type, string label)
                : base(type, label)
            { }

            public override void Update()
            {
                base.Update();
                float p = scene.timeLeft / fadeTime;
                if (scene.phase == Phase.FADE_IN)
                    Alpha(1 - p);
                else if (scene.phase == Phase.FADE_OUT)
                    Alpha(p);
                else
                    Alpha(1);
            }

            protected override void OnClick()
            {
                base.OnClick();
                Updates.LaunchInstall();
            }
        }

        public override void Update()
        {
            base.Update();

            waitingTime += Game.elapsed;

            float p = timeLeft / fadeTime;

            switch (phase)
            {
                case Phase.FADE_IN:
                    message.Alpha(1 - p);
                    if ((timeLeft -= Game.elapsed) <= 0)
                    {
                        ThreadRun();

                        //if (!thread.isAlive() && error == null) {
                        if (error == null)
                        {
                            phase = Phase.FADE_OUT;
                            timeLeft = fadeTime;
                        }
                        else
                        {
                            phase = Phase.STATIC;
                        }
                    }
                    break;

                case Phase.FADE_OUT:
                    message.Alpha(p);

                    if ((timeLeft -= Game.elapsed) <= 0)
                    {
                        Game.SwitchScene(typeof(GameScene));
                        //thread = null;
                        error = null;
                    }
                    break;

                case Phase.STATIC:
                    if (error != null)
                    {
                        string errorMsg;
                        if (error is FileNotFoundException)
                        {
                            errorMsg = Messages.Get(this, "file_not_found");
                        }
                        else if (error is IOException)
                        {
                            errorMsg = Messages.Get(this, "io_error");
                        }
                        //else if (error.GetMessage() != null && error.GetMessage().equals("old save")) errorMsg = Messages.Get(this, "io_error");
                        else
                        {
                            throw new Exception("fatal error occured while moving between floors. " +
                                    "Seed:" + Dungeon.seed + " depth:" + Dungeon.depth, error);
                        }

                        Add(new InterlevelSceneWndError(errorMsg));
                        //thread = null;
                        error = null;
                    }
                    //else if (thread != null && (int)waitingTime == 10)
                    //{
                    //    waitingTime = 11f;
                    //    string s = "";
                    //    for (StackTraceElement t : thread.getStackTrace())
                    //    {
                    //        s += "\n";
                    //        s += t.toString();
                    //    }
                    //    ShatteredPixelDungeon.reportException(
                    //            new RuntimeException("waited more than 10 seconds on levelgen. " +
                    //                    "Seed:" + Dungeon.seed + " depth:" + Dungeon.depth + " trace:" +
                    //                    s)
                    //    );
                    //}
                    break;
            }
        }

        private class InterlevelSceneWndError : WndError
        {
            public InterlevelSceneWndError(string message)
                : base(message)
            { }

            public override void OnBackPressed()
            {
                base.OnBackPressed();
                Game.SwitchScene(typeof(StartScene));
            }
        }

        private void Descend()
        {
            if (Dungeon.hero == null)
            {
                Mob.ClearHeldAllies();
                Dungeon.Init();
                if (noStory)
                {
                    Dungeon.chapters.Add(WndStory.ID_SEWERS);
                    noStory = false;
                }
                GameLog.Wipe();
            }
            else
            {
                Mob.HoldAllies(Dungeon.level);
                Dungeon.SaveAll();
            }

            Level level;
            if (Dungeon.depth >= Statistics.deepestFloor)
            {
                level = Dungeon.NewLevel();
            }
            else
            {
                ++Dungeon.depth;
                level = Dungeon.LoadLevel(GamesInProgress.curSlot);
            }
            Dungeon.SwitchLevel(level, level.entrance);
        }

        private void Fall()
        {
            Mob.HoldAllies(Dungeon.level);

            Buff.Affect<Chasm.Falling>(Dungeon.hero);
            Dungeon.SaveAll();

            Level level;
            if (Dungeon.depth >= Statistics.deepestFloor)
            {
                level = Dungeon.NewLevel();
            }
            else
            {
                ++Dungeon.depth;
                level = Dungeon.LoadLevel(GamesInProgress.curSlot);
            }
            Dungeon.SwitchLevel(level, level.FallCell(fallIntoPit));
        }

        private void Ascend()
        {
            Mob.HoldAllies(Dungeon.level);

            Dungeon.SaveAll();
            --Dungeon.depth;
            Level level = Dungeon.LoadLevel(GamesInProgress.curSlot);
            Dungeon.SwitchLevel(level, level.exit);
        }

        private void ReturnTo()
        {
            Mob.HoldAllies(Dungeon.level);

            Dungeon.SaveAll();
            Dungeon.depth = returnDepth;
            Level level = Dungeon.LoadLevel(GamesInProgress.curSlot);
            Dungeon.SwitchLevel(level, returnPos);
        }

        private void Restore()
        {
            Mob.ClearHeldAllies();

            GameLog.Wipe();

            Dungeon.LoadGame(GamesInProgress.curSlot);
            if (Dungeon.depth == -1)
            {
                Dungeon.depth = Statistics.deepestFloor;
                Dungeon.SwitchLevel(Dungeon.LoadLevel(GamesInProgress.curSlot), -1);
            }
            else
            {
                Level level = Dungeon.LoadLevel(GamesInProgress.curSlot);
                Dungeon.SwitchLevel(level, Dungeon.hero.pos);
            }
        }

        private void Resurrect()
        {
            Mob.HoldAllies(Dungeon.level);

            if (Dungeon.level.locked)
            {
                Dungeon.hero.Resurrect(Dungeon.depth);
                --Dungeon.depth;
                Level level = Dungeon.NewLevel();
                Dungeon.SwitchLevel(level, level.entrance);
            }
            else
            {
                Dungeon.hero.Resurrect(-1);
                Dungeon.ResetLevel();
            }
        }

        private void Reset()
        {
            Mob.HoldAllies(Dungeon.level);

            SpecialRoom.ResetPitRoom(Dungeon.depth + 1);

            --Dungeon.depth;
            Level level = Dungeon.NewLevel();
            Dungeon.SwitchLevel(level, level.entrance);
        }

        public override void OnBackPressed()
        {
            //Do nothing
        }
    }
}