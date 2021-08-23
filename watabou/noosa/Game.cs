using System;
using OpenTK.Graphics.OpenGL4;
using watabou.glscripts;
using watabou.gltextures;
using watabou.glwrap;
using watabou.input;
using watabou.noosa.audio;
using watabou.utils;

namespace watabou.noosa
{
    public class Game
    {
        public static Game instance;

        //actual size of the display
        public static int dispWidth;
        public static int dispHeight;

        // Size of the EGL surface view
        public static int width;
        public static int height;

        // Density: mdpi=1, hdpi=1.5, xhdpi=2...
        public static float density = 1.0f;

        public static string version;
        public static int versionCode;

        // Current scene
        protected Scene scene;
        // New scene we are going to switch to
        protected Scene requestedScene;
        // true if scene switch is requested
        protected bool requestedReset = true;
        // callback to perform logic during scene change
        protected ISceneChangeCallback onChange;
        // New scene class
        protected static Type sceneClass;  // Class<? extends Scene> 

        // 초단위 
        public static float timeScale = 1f;
        public static float elapsed;
        public static float timeTotal;
        //public static long realTime = 0;

        // Current time in ten-millionth of a second
        private long now;
        // ten-millionth of a second passed since previous update 
        private long step;

        public static InputHandler inputHandler;

        public static PlatformSupport platform;

        public Game(Type sceneToShow, PlatformSupport platform)
        {
            sceneClass = sceneToShow;

            instance = this;
            Game.platform = platform;

            now = System.DateTime.Now.Ticks;
        }

        private bool paused;

        public bool IsPaused()
        {
            return paused;
        }

        public virtual void Create(float density, int dispWidth, int dispHeight)
        {
            //density = Gdx.graphics.getDensity();
            //dispHeight = Gdx.graphics.getDisplayMode().height;  
            //dispWidth = Gdx.graphics.getDisplayMode().width;
            Game.density = density;
            Game.dispWidth = dispWidth;
            Game.dispHeight = dispHeight;

            inputHandler = new InputHandler();

            //refreshes texture and vertex data stored on the gpu
            //versionContextRef = Gdx.graphics.getGLVersion();
            Blending.UseDefault();
            TextureCache.Reload();
            //Vertexbuffer.refreshAllBuffers();
        }

        //private GLVersion versionContextRef;

        public virtual void Resize(int width, int height)
        {
            if (width == 0 || height == 0)
            {
                return;
            }

            ////If the EGL context was destroyed, we need to refresh some data stored on the GPU.
            //// This checks that by seeing if GLVersion has a new object reference
            //if (versionContextRef != Gdx.graphics.getGLVersion())
            //{
            //    versionContextRef = Gdx.graphics.getGLVersion();
            //    Blending.useDefault();
            //    TextureCache.reload();
            //    Vertexbuffer.refreshAllBuffers();
            //}

            if (height != Game.height || width != Game.width)
            {
                Game.width = width;
                Game.height = height;

                //TODO might be better to put this in platform support
                //if (Gdx.app.getType() != Application.ApplicationType.Android)
                {
                    Game.dispWidth = Game.width;
                    Game.dispHeight = Game.height;
                }

                ResetScene();
            }
        }

        // Lwjgl3Application::loop
        //      Lwjgl3Window::update
        //          Game::Render
        public virtual void Render()
        {
            if (width == 0 || height == 0)
                return;

            NoosaScript.Get().ResetCamera();
            NoosaScriptNoLighting.Get().ResetCamera();

            //Gdx.gl.glDisable(Gdx.gl.GL_SCISSOR_TEST);
            //Gdx.gl.glClear(Gdx.gl.GL_COLOR_BUFFER_BIT);
            //draw();
            //
            //Gdx.gl.glDisable(Gdx.gl.GL_SCISSOR_TEST);

            GL.Disable(EnableCap.ScissorTest);
            //GL.Clear(ClearBufferMask.ColorBufferBit);
            //GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Draw();

            GL.Disable(EnableCap.ScissorTest);

            Step();
        }

        public virtual void Pause()
        {
            paused = true;

            if (scene != null)
                scene.OnPause();

            Script.Reset();
        }

        public virtual void Resume()
        {
            paused = false;
        }

        public virtual void Finish()
        {
            // Gdx.app.exit();
            // 하위 클래스에서 처리하게
        }

        public virtual void Destroy()
        {
            if (scene != null)
            {
                scene.Destroy();
                scene = null;
            }

            sceneClass = null;
            Music.Instance.Stop();
            Sample.Instance.Reset();
        }

        public virtual void Dispose()
        {
            Destroy();
        }

        public static void ResetScene()
        {
            SwitchScene(sceneClass);
        }

        // Class<? extends Scene> c
        public static void SwitchScene(Type c)
        {
            SwitchScene(c, null);
        }

        // Class<? extends Scene> c
        public static void SwitchScene(Type c, ISceneChangeCallback callback)
        {
            sceneClass = c;
            instance.requestedReset = true;
            instance.onChange = callback;
        }

        public static Scene Scene()
        {
            return instance.scene;
        }

        protected void Step()
        {
            if (requestedReset)
            {
                requestedReset = false;

                requestedScene = (Scene)Reflection.NewInstance(sceneClass);
                if (requestedScene != null)
                {
                    SwitchScene();
                }
            }

            Update();
        }

        protected void Draw()
        {
            if (scene != null)
                scene.Draw();
        }

        protected virtual void SwitchScene()
        {
            Camera.Reset();

            if (scene != null)
                scene.Destroy();

            scene = requestedScene;
            if (onChange != null)
                onChange.BeforeCreate();
            scene.Create();
            if (onChange != null)
                onChange.AfterCreate();
            onChange = null;

            Game.elapsed = 0.0f;
            Game.timeScale = 1f;
            Game.timeTotal = 0f;
        }

        protected void Update()
        {
            long rightNow = DateTime.Now.Ticks;
            step = (now == 0) ? 0 : rightNow - now;
            now = rightNow;

            if (step <= 0)
                return;

            //Game.elapsed = Game.timeScale * Gdx.graphics.getDeltaTime();
            Game.elapsed = Game.timeScale * step * 0.0000001f;     // Tick to Second
#if DEBUG
            Game.elapsed = (Game.elapsed > 1.0f) ? 1.0f : Game.elapsed;
#endif
            Game.timeTotal += elapsed;

            inputHandler.ProcessAllEvents();

            Sample.Instance.Update();
            scene.Update();
            Camera.UpdateAll();
        }

        public static void ReportException(Exception tr)
        {
            if (instance != null)
            {
                instance.LogException(tr);
            }
            else
            {
                //fallback if error happened in initialization
                Console.WriteLine(tr.ToString());
            }
        }

        protected void LogException(Exception tr)
        {
            // 안드로이드로 실행할 경우 ADT와 동일하게 LogCat 탭에 로그가 출력된다.
            //Gdx.app.error("GAME", sw.toString());
            Console.WriteLine("Exception:{0}", tr.StackTrace);
        }

        //public static void runOnRenderThread(Callback c){
        //    Gdx.app.postRunnable(new Runnable() {
        //        @Override
        //        public void run() {
        //            c.call();
        //        }
        //    });
        //}

        public static void Vibrate(int milliseconds)
        {
            //Gdx.input.vibrate(milliseconds);
        }

        public interface ISceneChangeCallback
        {
            void BeforeCreate();

            void AfterCreate();
        }
    }
}