using System.Collections.Generic;
using watabou.noosa;
using watabou.utils;
using spdd.scenes;
using spdd.tiles;
using spdd.ui;

namespace spdd.effects
{
    public class FloatingText : RenderedTextBlock
    {
        private const float LIFESPAN = 1f;
        private const float DISTANCE = DungeonTilemap.SIZE;

        private float timeLeft;

        private int key = -1;

        private static readonly SparseArray<List<FloatingText>> stacks = new SparseArray<List<FloatingText>>();

        public FloatingText()
            : base(9 * PixelScene.defaultZoom)
        {
            SetHightlighting(false);
        }

        public override void Update()
        {
            base.Update();

            if (timeLeft > 0)
            {
                if ((timeLeft -= Game.elapsed) <= 0)
                {
                    Kill();
                }
                else
                {
                    float p = timeLeft / LIFESPAN;
                    Alpha(p > 0.5f ? 1 : p * 2);

                    float yMove = (DISTANCE / LIFESPAN) * Game.elapsed;
                    y -= yMove;
                    foreach (RenderedText t in words)
                    {
                        t.y -= yMove;
                    }
                }
            }
        }

        public override void Kill()
        {
            if (key != -1)
            {
                stacks[key].Remove(this);
                key = -1;
            }
            base.Kill();
        }

        public override void Destroy()
        {
            Kill();
            base.Destroy();
        }

        public virtual void Reset(float x, float y, string text, Color color)
        {
            Revive();

            Zoom(1 / (float)PixelScene.defaultZoom);

            Text(text);
            Hardlight(color);

            SetPos(
                PixelScene.Align(Camera.main, x - Width() / 2),
                PixelScene.Align(Camera.main, y - Height())
            );

            timeLeft = LIFESPAN;
        }

        /* STATIC METHODS */

        public static void Show(float x, float y, string text, Color color)
        {
            var txt = GameScene.Status();
            if (txt != null)
            {
                txt.Reset(x, y, text, color);
            }
        }

        public static void Show(float x, float y, int key, string text, Color color)
        {
            var txt = GameScene.Status();
            if (txt != null)
            {
                txt.Reset(x, y, text, color);
                Push(txt, key);
            }
        }

        private static void Push(FloatingText txt, int key)
        {
            txt.key = key;

            List<FloatingText> stack = stacks[key];
            if (stack == null)
            {
                stack = new List<FloatingText>();
                stacks.Add(key, stack);
            }

            if (stack.Count > 0)
            {
                var below = txt;
                var aboveIndex = stack.Count - 1;
                while (aboveIndex >= 0)
                {
                    var above = stack[aboveIndex];
                    if (above.Bottom() + 4 > below.Top())
                    {
                        above.SetPos(above.Left(), below.Top() - above.Height() - 4);

                        below = above;
                        --aboveIndex;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            stack.Add(txt);
        }
    }
}