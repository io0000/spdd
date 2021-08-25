using System;
using System.ComponentModel;
using System.IO;

using Dear_ImGui_Sample;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using watabou.noosa;
using watabou.utils;
using spdd.desktop;
using spdd.levels;
using spdd.levels.rooms;
using spdd.levels.builders;

namespace spdd
{
    public class SPDDWindow : GameWindow
    {
        ShatteredPixelDungeonDash game;
        int mouseX, mouseY;
        int mousePressed;
        ImGuiController controller;

        public SPDDWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            unsafe
            {
                // https://www.glfw.org/docs/3.3/window_guide.html#window_size
                GLFW.SetWindowSizeLimits(WindowPtr, 480, 320, GLFW.DontCare, GLFW.DontCare);
            }
        }

        protected override void OnLoad()
        {
            float density = 0.0f;
            int dispWidth = 0;
            int dispHeight = 0;
            CalcDensity(ref density, ref dispWidth, ref dispHeight);

            var platform = new DesktopPlatformSupport();
            game = new ShatteredPixelDungeonDash(platform, this);
            game.Create(density, dispWidth, dispHeight);

            base.OnLoad();

            controller = new ImGuiController(ClientSize.X, ClientSize.Y);
        }

        private void CalcDensity(ref float density, ref int dispWidth, ref int dispHeight)
        {
            Monitors.TryGetMonitorInfo(FindMonitor(), out MonitorInfo info);

            // step1. getPpcX()
            int sizeX = info.PhysicalWidth;  // GetMonitorPhysicalSize

            //var videoMode = GLFW.GetVideoMode(HandleAsPtr);
            //ClientArea = new Box2i(x, y, x + videoMode->Width, y + videoMode->Height);

            // public int HorizontalResolution => ClientArea.Size.X;

            int modeWidth = info.HorizontalResolution;
            float ppcX = (float)modeWidth / (float)sizeX * 10.0f;

            // step2. float getPpiX() -> this.getPpcX() / 0.393701F;
            float ppix = ppcX / 0.393701f;

            // step3. float getDensity() - this.getPpiX() / 160.0F;
            density = ppix / 160.0f;

            dispWidth = info.HorizontalResolution;
            dispHeight = info.VerticalResolution;   // ClientArea.Size.Y
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (debug_wireframe)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            game.Render();

            if (debug_wireframe)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            RenderImgui(e);

            SwapBuffers();

            base.OnRenderFrame(e);
        }

        private bool show_debug = false;
        private bool debug_wireframe = false;

        protected void RenderImgui(FrameEventArgs e)
        {
            controller.Update(this, (float)e.Time);

            //GL.ClearColor(new Color4(0, 32, 48, 255));
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            //ImGui.ShowDemoWindow();
            //ImGui.ShowAboutWindow();

            if (show_debug)
            {
                var level = Dungeon.level;
                if (level != null)
                {
                    var heroPoint = level.CellToPoint(Dungeon.hero.pos);
                    PointF heroPointF = new PointF(heroPoint);

                    Room from = null;
                    Room room = null;
                    float angle = 0.0f;

                    RegularLevel regularLevel = level as RegularLevel;
                    if (regularLevel != null)
                    {
                        from = regularLevel.roomEntrance;
                        PointF fromPoint = new PointF((from.left + from.right) / 2f, (from.top + from.bottom) / 2f);
                        angle = Builder.AngleBetweenPoints(fromPoint, heroPointF);
                        room = regularLevel.Room(Dungeon.hero.pos);
                    }

                    string strPos = string.Format("x:{0} y:{1} - pos:{2}", heroPoint.x, heroPoint.y, Dungeon.hero.pos);
                    string strAngle = string.Format("angle:{0}", angle);
                    string strRoom = string.Format("room:{0} hash:{1}", room?.ToString(), room?.GetHashCode());

                    ImGui.Begin("Debug");
                    ImGui.Text(strPos);
                    ImGui.Text(strAngle);
                    ImGui.Text(strRoom);
                    ImGui.Checkbox("Wireframe", ref debug_wireframe);
                    if (ImGui.Button("Close Me"))
                        show_debug = false;
                    ImGui.End();
                }
            }

            controller.Render();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            Util.CheckGLError("End of frame");
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            const int pointer = 0;
            const int button = 0;
            if (e.Button == MouseButton.Left || e.Button == MouseButton.Right)
            {
                ++mousePressed;
                Game.inputHandler.TouchDown(mouseX, mouseY, pointer, button);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            const int pointer = 0;
            const int button = 0;
            if (e.Button == MouseButton.Left || e.Button == MouseButton.Right)
            {
                mousePressed = Math.Max(0, mousePressed - 1);
                Game.inputHandler.TouchUp(mouseX, mouseY, pointer, button);
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            mouseX = (int)e.X;
            mouseY = (int)e.Y;

            if (mousePressed > 0)
            {
                Game.inputHandler.TouchDragged(mouseX, mouseY, 0);
            }
            else
            {
                Game.inputHandler.MouseMoved(mouseX, mouseY);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            // 큐에 추가. 처리는 InputHandler.ProcessAllEvents() 에서
            Game.inputHandler.ScrollCallback(e.OffsetY);

            controller.MouseScroll(e.Offset);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Game.Scene().OnPause();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.F1)
                show_debug = !show_debug;

            Game.inputHandler.KeyDown(e.Key);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Game.inputHandler.KeyUp(e.Key);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);

            // Tell ImGui of the new size
            controller.WindowResized(Size.X, Size.Y);

            game.Resize(Size.X, Size.Y);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            controller.PressChar((char)e.Unicode);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            base.OnUnload();
        }
    }

    class App
    {
        static void Main(string[] args)
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appdata = Path.Combine(appdata, "spdd");
            FileUtils.SetDefaultFileProperties(FileType.External, appdata);

            // 선행 조건: FileUtils.SetDefaultFileProperties 호출된 후
            SPDSettings.Load();

            // TODO01 하드코딩 제거
            Game.versionCode = 464;
#if DEBUG
            Game.version = "0.8.2d-INDEV";
#else
            Game.version = "0.8.2d";
#endif
            Point p = SPDSettings.WindowResolution();

            GameWindowSettings gameWindowSettings = GameWindowSettings.Default;
            gameWindowSettings.RenderFrequency = 60.0;

            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(p.x, p.y),
                Title = "Shattered Pixel Dungeon Dash",
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Compatability
            };

            using (var win = new SPDDWindow(gameWindowSettings, nativeWindowSettings))
            {
                win.Run();
            }
        }
    }
}
