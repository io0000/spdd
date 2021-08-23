using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.scenes;

namespace spdd.ui
{
    public class RenderedTextBlock : Component
    {
        private int maxWidth = int.MaxValue;
        public int nLines;

        private static RenderedText SPACE = new RenderedText();
        private static RenderedText NEWLINE = new RenderedText();

        protected string text;
        protected string[] tokens;
        protected List<RenderedText> words = new List<RenderedText>();
        protected bool multiline;

        private int size;
        private float zoom;
        private Color color;

        private Color hightlightColor = Window.TITLE_COLOR;
        private bool highlightingEnabled = true;

        public const int LEFT_ALIGN = 1;
        public const int CENTER_ALIGN = 2;
        public const int RIGHT_ALIGN = 3;
        private int alignment = LEFT_ALIGN;

        public RenderedTextBlock(int size)
        {
            this.size = size;
            color.i32value = -1;
        }

        public RenderedTextBlock(string text, int size)
        {
            this.size = size;
            color.i32value = -1;
            Text(text);
        }

        public void Text(string text)
        {
            this.text = text;

            if (text != null && !text.Equals(""))
            {
                tokens = Game.platform.SplitforTextBlock(text, multiline);
                Build();
            }
        }

        public void Text(string text, int maxWidth)
        {
            this.maxWidth = maxWidth;
            multiline = true;
            Text(text);
        }

        public string Text()
        {
            return text;
        }

        public void MaxWidth(int maxWidth)
        {
            if (this.maxWidth != maxWidth)
            {
                this.maxWidth = maxWidth;
                multiline = true;
                Text(text);
            }
        }

        public int MaxWidth()
        {
            return maxWidth;
        }

        private void Build()
        {
            if (tokens == null)
                return;

            Clear();
            words = new List<RenderedText>();
            bool highlighting = false;
            foreach (var str in tokens)
            {
                if (str.Equals("_") && highlightingEnabled)
                {
                    highlighting = !highlighting;
                }
                else if (str.Equals("\n"))
                {
                    words.Add(NEWLINE);
                }
                else if (str.Equals(" "))
                {
                    words.Add(SPACE);
                }
                else
                {
                    RenderedText word = new RenderedText(str, size);

                    if (highlighting)
                        word.Hardlight(hightlightColor);
                    else if (color.i32value != -1)
                        word.Hardlight(color);
                    word.scale.Set(zoom);

                    words.Add(word);
                    Add(word);

                    if (height < word.Height())
                        height = word.Height();
                }
            }
            Layout();
        }

        public void Zoom(float zoom)
        {
            this.zoom = zoom;
            foreach (RenderedText word in words)
            {
                if (word != null)
                    word.scale.Set(zoom);
            }
            Layout();
        }

        public void Hardlight(Color color)
        {
            this.color = color;
            foreach (RenderedText word in words)
            {
                if (word != null)
                    word.Hardlight(color);
            }
        }

        public void ResetColor()
        {
            this.color.i32value = -1;
            foreach (RenderedText word in words)
            {
                if (word != null)
                    word.ResetColor();
            }
        }

        public void Alpha(float value)
        {
            foreach (RenderedText word in words)
            {
                if (word != null)
                    word.Alpha(value);
            }
        }

        public void SetHightlighting(bool enabled)
        {
            SetHightlighting(enabled, Window.TITLE_COLOR);
        }

        public void SetHightlighting(bool enabled, Color color)
        {
            if (enabled != highlightingEnabled || color.i32value != hightlightColor.i32value)
            {
                hightlightColor = color;
                highlightingEnabled = enabled;
                Build();
            }
        }

        public void Invert()
        {
            if (words != null)
            {
                foreach (RenderedText word in words)
                {
                    if (word != null)
                    {
                        word.ra = 0.77f;
                        word.ga = 0.73f;
                        word.ba = 0.62f;
                        word.rm = -0.77f;
                        word.gm = -0.73f;
                        word.bm = -0.62f;
                    }
                }
            }
        }

        public void Align(int align)
        {
            alignment = align;
            Layout();
        }

        protected override void Layout()
        {
            base.Layout();
            float x = this.x;
            float y = this.y;
            float height = 0;
            nLines = 1;

            List<List<RenderedText>> lines = new List<List<RenderedText>>();
            List<RenderedText> curLine = new List<RenderedText>();
            lines.Add(curLine);

            width = 0;
            foreach (RenderedText word in words)
            {
                if (word == SPACE)
                {
                    x += 1.5f;
                }
                else if (word == NEWLINE)
                {
                    y += height + 2f;
                    x = this.x;
                    ++nLines;
                    curLine = new List<RenderedText>();
                    lines.Add(curLine);
                }
                else
                {
                    if (word.Height() > height)
                        height = word.Height();

                    if ((x - this.x) + word.Width() > maxWidth && curLine.Count != 0)
                    {
                        y += height + 2f;
                        x = this.x;
                        ++nLines;
                        curLine = new List<RenderedText>();
                        lines.Add(curLine);
                    }

                    word.x = x;
                    word.y = y;
                    PixelScene.Align(word);
                    x += word.Width();
                    curLine.Add(word);

                    if ((x - this.x) > width)
                        width = (x - this.x);

                    //TODO spacing currently doesn't factor in halfwidth and fullwidth characters
                    //(e.g. Ideographic full stop)
                    x -= 0.5f;
                }
            }

            this.height = (y - this.y) + height;

            if (alignment != LEFT_ALIGN)
            {
                foreach (List<RenderedText> line in lines)
                {
                    if (line.Count == 0)
                        continue;

                    float lineWidth = line[line.Count - 1].Width() + line[line.Count - 1].x - this.x;
                    if (alignment == CENTER_ALIGN)
                    {
                        foreach (RenderedText text in line)
                        {
                            text.x += (Width() - lineWidth) / 2f;
                            PixelScene.Align(text);
                        }
                    }
                    else if (alignment == RIGHT_ALIGN)
                    {
                        foreach (RenderedText text in line)
                        {
                            text.x += Width() - lineWidth;
                            PixelScene.Align(text);
                        }
                    }
                }
            }
        }
    }
}