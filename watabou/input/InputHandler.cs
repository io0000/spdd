using System;
using watabou.utils;
using watabou.noosa;

namespace watabou.input
{
    // 터치, 스크롤, 키다운 이벤트를 모두 처리
    public class InputHandler
    {
        public InputHandler()
        { }

        public void ProcessAllEvents()
        {
            PointerEvent.ProcessPointerEvents();
            KeyEvent.ProcessKeyEvents();
            ScrollEvent.ProcessScrollEvents();
        }

        // *********************
        // *** Pointer Input ***
        // *********************
        public bool TouchDown(int screenX, int screenY, int pointer, int button)
        {
            screenX = (int)(screenX / (Game.dispWidth / (float)Game.width));
            screenY = (int)(screenY / (Game.dispHeight / (float)Game.height));
            PointerEvent.AddPointerEvent(new PointerEvent(screenX, screenY, pointer, true));
            return true;
        }

        public bool TouchUp(int screenX, int screenY, int pointer, int button)
        {
            screenX = (int)(screenX / (Game.dispWidth / (float)Game.width));
            screenY = (int)(screenY / (Game.dispHeight / (float)Game.height));
            PointerEvent.AddPointerEvent(new PointerEvent(screenX, screenY, pointer, false));
            return true;
        }

        public bool TouchDragged(int screenX, int screenY, int pointer)
        {
            screenX = (int)(screenX / (Game.dispWidth / (float)Game.width));
            screenY = (int)(screenY / (Game.dispHeight / (float)Game.height));
            PointerEvent.AddPointerEvent(new PointerEvent(screenX, screenY, pointer, true));
            return true;
        }

        //TODO tracking this should probably be in PointerEvent
        private static PointF pointerHoverPos = new PointF();

        public bool MouseMoved(int screenX, int screenY)
        {
            screenX = (int)(screenX / (Game.dispWidth / (float)Game.width));
            screenY = (int)(screenY / (Game.dispHeight / (float)Game.height));
            pointerHoverPos.x = screenX;
            pointerHoverPos.y = screenY;
            return true;
        }

        // *****************
        // *** Key Input ***
        // *****************
        // libgdx-master\backends\gdx-backend-lwjgl3\src\com\badlogic\gdx\backends\lwjgl3\DefaultLwjgl3Input.java
        // public int getGdxKeyCode (int lwjglKeyCode)
        static public int GetGdxKeyCode(OpenTK.Windowing.GraphicsLibraryFramework.Keys opentkKeyCode)
        {
            switch (opentkKeyCode)
            {
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space:
                    return Keys.SPACE;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Apostrophe:
                    return Keys.APOSTROPHE;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Comma:
                    return Keys.COMMA;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Minus:
                    return Keys.MINUS;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Period:
                    return Keys.PERIOD;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Slash:
                    return Keys.SLASH;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D0:
                    return Keys.NUM_0;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D1:
                    return Keys.NUM_1;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D2:
                    return Keys.NUM_2;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D3:
                    return Keys.NUM_3;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D4:
                    return Keys.NUM_4;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D5:
                    return Keys.NUM_5;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D6:
                    return Keys.NUM_6;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D7:
                    return Keys.NUM_7;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D8:
                    return Keys.NUM_8;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D9:
                    return Keys.NUM_9;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Semicolon:
                    return Keys.SEMICOLON;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Equal:
                    return Keys.EQUALS;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.A:
                    return Keys.A;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.B:
                    return Keys.B;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.C:
                    return Keys.C;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.D:
                    return Keys.D;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.E:
                    return Keys.E;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F:
                    return Keys.F;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.G:
                    return Keys.G;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.H:
                    return Keys.H;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.I:
                    return Keys.I;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.J:
                    return Keys.J;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.K:
                    return Keys.K;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.L:
                    return Keys.L;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.M:
                    return Keys.M;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.N:
                    return Keys.N;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.O:
                    return Keys.O;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.P:
                    return Keys.P;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Q:
                    return Keys.Q;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.R:
                    return Keys.R;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.S:
                    return Keys.S;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.T:
                    return Keys.T;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.U:
                    return Keys.U;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.V:
                    return Keys.V;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.W:
                    return Keys.W;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.X:
                    return Keys.X;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Y:
                    return Keys.Y;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Z:
                    return Keys.Z;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftBracket:
                    return Keys.LEFT_BRACKET;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backslash:
                    return Keys.BACKSLASH;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightBracket:
                    return Keys.RIGHT_BRACKET;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.GraveAccent:
                    return Keys.GRAVE;
                //case GLFW.GLFW_KEY_WORLD_1:
                //case GLFW.GLFW_KEY_WORLD_2:
                //    return Input.Keys.UNKNOWN;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape:
                    return Keys.ESCAPE;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter:
                    return Keys.ENTER;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab:
                    return Keys.TAB;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace:
                    return Keys.BACKSPACE;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Insert:
                    return Keys.INSERT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Delete:
                    return Keys.FORWARD_DEL;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right:
                    return Keys.RIGHT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left:
                    return Keys.LEFT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down:
                    return Keys.DOWN;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up:
                    return Keys.UP;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.PageUp:
                    return Keys.PAGE_UP;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.PageDown:
                    return Keys.PAGE_DOWN;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Home:
                    return Keys.HOME;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.End:
                    return Keys.END;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.CapsLock:
                    return Keys.CAPS_LOCK;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.ScrollLock:
                    return Keys.SCROLL_LOCK;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.PrintScreen:
                    return Keys.PRINT_SCREEN;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Pause:
                    return Keys.PAUSE;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F1:
                    return Keys.F1;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F2:
                    return Keys.F2;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F3:
                    return Keys.F3;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F4:
                    return Keys.F4;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F5:
                    return Keys.F5;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F6:
                    return Keys.F6;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F7:
                    return Keys.F7;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F8:
                    return Keys.F8;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F9:
                    return Keys.F9;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F10:
                    return Keys.F10;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F11:
                    return Keys.F11;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F12:
                    return Keys.F12;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F13:
                    return Keys.F13;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F14:
                    return Keys.F14;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F15:
                    return Keys.F15;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F16:
                    return Keys.F16;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F17:
                    return Keys.F17;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F18:
                    return Keys.F18;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F19:
                    return Keys.F19;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F20:
                    return Keys.F20;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F21:
                    return Keys.F21;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F22:
                    return Keys.F22;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F23:
                    return Keys.F23;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F24:
                    return Keys.F24;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.F25:
                    return Keys.UNKNOWN;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.NumLock:
                    return Keys.NUM_LOCK;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad0:
                    return Keys.NUMPAD_0;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad1:
                    return Keys.NUMPAD_1;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad2:
                    return Keys.NUMPAD_2;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad3:
                    return Keys.NUMPAD_3;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad4:
                    return Keys.NUMPAD_4;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad5:
                    return Keys.NUMPAD_5;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad6:
                    return Keys.NUMPAD_6;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad7:
                    return Keys.NUMPAD_7;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad8:
                    return Keys.NUMPAD_8;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad9:
                    return Keys.NUMPAD_9;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadDecimal:
                    return Keys.NUMPAD_DOT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadDivide:
                    return Keys.NUMPAD_DIVIDE;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadMultiply:
                    return Keys.NUMPAD_MULTIPLY;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadSubtract:
                    return Keys.NUMPAD_SUBTRACT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadAdd:
                    return Keys.NUMPAD_ADD;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadEnter:
                    return Keys.NUMPAD_ENTER;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadEqual:
                    return Keys.NUMPAD_EQUALS;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift:
                    return Keys.SHIFT_LEFT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl:
                    return Keys.CONTROL_LEFT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt:
                    return Keys.ALT_LEFT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftSuper:
                    return Keys.SYM;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightShift:
                    return Keys.SHIFT_RIGHT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightControl:
                    return Keys.CONTROL_RIGHT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightAlt:
                    return Keys.ALT_RIGHT;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightSuper:
                    return Keys.SYM;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Menu:
                    return Keys.MENU;
                default:
                    return Keys.UNKNOWN;
            }
        }

        public bool KeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys opentkKeyCode)
        {
            int keyCode = GetGdxKeyCode(opentkKeyCode);

            if (KeyBindings.IsKeyBound(keyCode))
            {
                KeyEvent.AddKeyEvent(new KeyEvent(keyCode, true));
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool KeyUp(OpenTK.Windowing.GraphicsLibraryFramework.Keys opentkKeyCode)
        {
            int keyCode = GetGdxKeyCode(opentkKeyCode);

            if (KeyBindings.IsKeyBound(keyCode))
            {
                KeyEvent.AddKeyEvent(new KeyEvent(keyCode, false));
                return true;
            }
            else
            {
                return false;
            }
        }

        // ********************
        // *** Scroll Input ***
        // ********************

        /*
        step1. 큐에 넣을 때

        //////////////////////////////////
        /// glfw
        //////////////////////////////////
        case WM_MOUSEWHEEL:
        {
            _glfwInputScroll(window, 0.0, (SHORT) HIWORD(wParam) / (double) WHEEL_DELTA);
            return 0;
        }

        case WM_MOUSEHWHEEL:
        {
            // This message is only sent on Windows Vista and later
            // NOTE: The X-axis is inverted for consistency with macOS and X11
            _glfwInputScroll(window, -((SHORT) HIWORD(wParam) / (double) WHEEL_DELTA), 0.0);
            return 0;
        }

        // Notifies shared code of a scroll event
        //
        void _glfwInputScroll(_GLFWwindow* window, double xoffset, double yoffset)
        {
            if (window->callbacks.scroll)
                window->callbacks.scroll((GLFWwindow*) window, xoffset, yoffset);
        }

        //////////////////////////////////////
        /// libgdx
        //////////////////////////////////////
        private GLFWScrollCallback scrollCallback = new GLFWScrollCallback() {
            private long pauseTime = 250000000L;
            private float scrollYRemainder;
            private long lastScrollEventTime;

            public void invoke(long window, double scrollX, double scrollY) {
                Lwjgl3Input.this.window.getGraphics().requestRendering();
                int scrollAmount;
                if ((this.scrollYRemainder <= 0.0F || scrollY >= 0.0D) && (this.scrollYRemainder >= 0.0F || scrollY <= 0.0D) && TimeUtils.nanoTime() - this.lastScrollEventTime <= this.pauseTime) {
                    for(this.scrollYRemainder = (float)((double)this.scrollYRemainder + scrollY); Math.abs(this.scrollYRemainder) >= 1.0F; this.scrollYRemainder += (float)scrollAmount) {
                        scrollAmount = (int)(-Math.signum(scrollY));
                        Lwjgl3Input.this.eventQueue.scrolled(scrollAmount);
                        this.lastScrollEventTime = TimeUtils.nanoTime();
                    }
                } else {
                    this.scrollYRemainder = 0.0F;
                    scrollAmount = (int)(-Math.signum(scrollY));
                    Lwjgl3Input.this.eventQueue.scrolled(scrollAmount);
                    this.lastScrollEventTime = TimeUtils.nanoTime();
                }

            }
        };

        public synchronized boolean scrolled (int amount) {
            queue.add(SCROLLED);
            queueTime();
            queue.add(amount);
            return false;
        }

        step2. 큐에서 꺼내서 scrolled함수 호출

        case SCROLLED:
            localProcessor.scrolled(q[i++]);
        */

        private float scrollYRemainder;
        private long lastScrollEventTime;

        public void ScrollCallback(float scrollY)
        {
            const long TicksPerMillisecond = 10000;
            const long pauseTime = 250 * TicksPerMillisecond;   // tick단위로
            int scrollAmount;

            if ((scrollYRemainder <= 0.0F || scrollY >= 0.0f) &&
                (scrollYRemainder >= 0.0F || scrollY <= 0.0f) &&
                (System.DateTime.Now.Ticks - lastScrollEventTime <= pauseTime)) // TimeUtils.nanoTime() - this.lastScrollEventTime <= this.pauseTime
            {
                for (this.scrollYRemainder = (float)((double)this.scrollYRemainder + scrollY); Math.Abs(this.scrollYRemainder) >= 1.0F; this.scrollYRemainder += (float)scrollAmount)
                {
                    scrollAmount = (int)-Math.Sign(scrollY);
                    ScrollEvent.AddScrollEvent(new ScrollEvent(pointerHoverPos, scrollAmount));
                    this.lastScrollEventTime = System.DateTime.Now.Ticks;
                }
            }
            else
            {
                scrollYRemainder += scrollY;
                scrollAmount = (int)-Math.Sign(scrollY);
                ScrollEvent.AddScrollEvent(new ScrollEvent(pointerHoverPos, scrollAmount));
                this.lastScrollEventTime = System.DateTime.Now.Ticks;
            }
        }

        //
        //public static void Scrolled(int amount)
        //{
        //    //ScrollEvent.AddScrollEvent(new ScrollEvent(_pointerHoverPos, amount));
        //}
    }
}
