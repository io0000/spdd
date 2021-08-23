using System;
using System.Collections.Generic;
using System.Text;

namespace watabou.utils
{
    public class GlyphLayout
    {
        public List<GlyphRun> runs = new List<GlyphRun>();
        public float width, height;

        public GlyphLayout()
        { }

        public GlyphLayout(BitmapFont font, string str)
        {
            SetText(font, str);
        }

        public void SetText(BitmapFont font, string str)
        {
            SetText(font, str, 0, str.Length, font.GetColor(), 0, Align.left);
        }

        public void SetText(BitmapFont font, string str, int start, int end, Color color, float targetWidth, int halign)
        {
            var fontData = font.data;

            this.runs.Clear();

            float x = 0, y = 0, width = 0;
            int lines = 0, blankLines = 0;
            BitmapFont.Glyph lastGlyph = null;

            int runStart = start;

            while (true)
            {
                // Each run is delimited by newline or left square bracket.
                int runEnd = -1;
                bool newline = false;
                if (start == end)
                {
                    if (runStart == end)
                        break; // End of string with no run to process, we're done.
                    runEnd = end; // End of string, process last run.
                }
                else
                {
                    switch (str[start++])
                    {
                        case '\n':
                            // End of line.
                            runEnd = start - 1;
                            newline = true;
                            break;
                        case '[':
                            break;
                    }
                }

                if (runEnd != -1)
                {
                    if (runEnd != runStart)
                    {   // Eg, when a color tag is at text start or a line is "\n".
                        // Store the run that has ended.
                        GlyphRun run = new GlyphRun(); //glyphRunPool.obtain();
                        run.color = color;
                        fontData.GetGlyphs(run, str, runStart, runEnd, lastGlyph);
                        if (run.glyphs.Count == 0)
                        {
                            //glyphRunPool.free(run);
                        }
                        else
                        {
                            if (lastGlyph != null)
                            {
                                // Move back the width of the last glyph from the previous run.
                                x -= lastGlyph.fixedWidth ? lastGlyph.xadvance * fontData.scaleX
                                    : (lastGlyph.width + lastGlyph.xoffset) * fontData.scaleX - fontData.padRight;
                            }
                            lastGlyph = run.glyphs[run.glyphs.Count - 1];
                            run.x = x;
                            run.y = y;
                            if (newline || runEnd == end)
                                AdjustLastGlyph(fontData, run);
                            runs.Add(run);

                            var xAdvances = run.xAdvances;
                            int n = run.xAdvances.Count;

                            float runWidth = 0;
                            for (int i = 0; i < n; ++i)
                                runWidth += xAdvances[i];
                            x += runWidth;
                            run.width = runWidth;
                        }
                    }

                    if (newline)
                    {
                        // Next run will be on the next line.
                        width = Math.Max(width, x);
                        x = 0;
                        float down = fontData.down;
                        if (runEnd == runStart)
                        {
                            // Blank line.
                            down *= fontData.blankLineScale;
                            ++blankLines;
                        }
                        else
                        {
                            ++lines;
                        }
                        y += down;
                        lastGlyph = null;
                    }

                    runStart = start;
                }
            }

            width = Math.Max(width, x);

            this.width = width;
            if (fontData.flipped)
                this.height = fontData.capHeight + lines * fontData.down + blankLines * fontData.down * fontData.blankLineScale;
            else
                this.height = fontData.capHeight + lines * -fontData.down + blankLines * -fontData.down * fontData.blankLineScale;
        }


        /** Adjusts the xadvance of the last glyph to use its width instead of xadvance. */
        private void AdjustLastGlyph(BitmapFont.BitmapFontData fontData, GlyphRun run)
        {
            var last = run.glyphs[run.glyphs.Count - 1];
            if (last.fixedWidth)
                return;
            float width = (last.width + last.xoffset) * fontData.scaleX - fontData.padRight;
            run.width += width - run.xAdvances[run.xAdvances.Count - 1]; // Can cause the run width to be > targetWidth, but the problem is minimal.
            run.xAdvances[run.xAdvances.Count - 1] = width;
        }

        /** Stores glyphs and positions for a piece of text which is a single color and does not span multiple lines.
         * @author Nathan Sweet */
        public class GlyphRun
        {
            public List<BitmapFont.Glyph> glyphs = new List<BitmapFont.Glyph>();
            /** Contains glyphs.size+1 entries: First entry is X offset relative to the drawing position. Subsequent entries are the X
             * advance relative to previous glyph position. Last entry is the width of the last glyph. */
            public List<float> xAdvances = new List<float>();
            public float x, y, width;
            public Color color = new Color();

            public void Reset()
            {
                glyphs.Clear();
                xAdvances.Clear();
                width = 0;
            }

            override public string ToString()
            {
                StringBuilder buffer = new StringBuilder();
                var glyphs = this.glyphs;
                for (int i = 0, n = glyphs.Count; i < n; ++i)
                {
                    var g = glyphs[i];
                    buffer.Append((char)g.id);
                }
                buffer.Append(", #");
                buffer.Append(color);
                buffer.Append(", ");
                buffer.Append(x);
                buffer.Append(", ");
                buffer.Append(y);
                buffer.Append(", ");
                buffer.Append(width);
                return buffer.ToString();
            }
        }
    }
}