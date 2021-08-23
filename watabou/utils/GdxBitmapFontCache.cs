using System;
using System.Collections.Generic;
using watabou.glwrap;

namespace watabou.utils
{
    public interface IBatch
    {
        void Draw(Texture texture, float[] spriteVertices, int offset, int count);
    }

    public class BitmapFontCache
    {       
        private readonly BitmapFont font;
        private bool integer;
        //private List<GlyphLayout> layouts = new List<GlyphLayout>();
        private Color color = new Color(0xff, 0xff, 0xff, 0xff);
        
        /** Vertex data per page. */
        private float[][] pageVertices;
        /** Number of vertex data entries per page. */
        private int[] idx;
        /** Used internally to ensure a correct capacity for multi-page font vertex data. */
        private int[] tempGlyphCount;

        public BitmapFontCache(BitmapFont font)
        {
            this.font = font;
            this.integer = true;

            int pageCount = font.regions.Count;
            if (pageCount == 0) 
                throw new Exception("The specified font must contain at least one texture page.");

            pageVertices = new float[pageCount][];
            idx = new int[pageCount];
            tempGlyphCount = new int[pageCount];
        }

        public Color GetColor()
        {
            return color;
        }

        public void Draw(IBatch spriteBatch)
        {
            List<TextureRegion> regions = font.GetRegions();
            for (int j = 0, n = pageVertices.Length; j < n; j++)
            {
                if (idx[j] > 0)
                { 
                    // ignore if this texture has no glyphs
                    float[] vertices = pageVertices[j];
                    spriteBatch.Draw(regions[j].GetTexture(), vertices, 0, idx[j]);
                }
            }
        }

        /** Removes all glyphs in the cache. */
        public void Clear()
        {
            //layouts.Clear();
            //for (int i = 0, n = idx.Length; i < n; ++i)
            //{
            //    idx[i] = 0;
            //}
            Array.Clear(idx, 0, idx.Length);
        }

        private void RequireGlyphs(GlyphLayout layout)
        {
            if (pageVertices.Length == 1)
            {
                // Simpler counting if we just have one page.
                int newGlyphCount = 0;
                for (int i = 0, n = layout.runs.Count; i < n; ++i)
                    newGlyphCount += layout.runs[i].glyphs.Count;
                RequirePageGlyphs(0, newGlyphCount);
            }
            else
            {
                int[] tempGlyphCount = this.tempGlyphCount;
                for (int i = 0, n = tempGlyphCount.Length; i < n; ++i)
                    tempGlyphCount[i] = 0;
                // Determine # of glyphs in each page.
                for (int i = 0, n = layout.runs.Count; i < n; ++i)
                {
                    var glyphs = layout.runs[i].glyphs;
                    for (int ii = 0, nn = glyphs.Count; ii < nn; ++ii)
                        ++tempGlyphCount[glyphs[ii].page];
                }
                // Require that many for each page.
                for (int i = 0, n = tempGlyphCount.Length; i < n; ++i)
                    RequirePageGlyphs(i, tempGlyphCount[i]);
            }
        }

        private void RequirePageGlyphs(int page, int glyphCount)
        {
            int vertexCount = idx[page] + glyphCount * 20;
            float[] vertices = pageVertices[page];
            if (vertices == null)
            {
                pageVertices[page] = new float[vertexCount];
            }
            else if (vertices.Length < vertexCount)
            {
                float[] newVertices = new float[vertexCount];
                Array.Copy(vertices, 0, newVertices, 0, idx[page]);
                pageVertices[page] = newVertices;
            }
        }

        private void AddToCache(GlyphLayout layout, float x, float y)
        {
            // Check if the number of font pages has changed.
            int pageCount = font.regions.Count;
            if (pageVertices.Length < pageCount)
            {
                float[][] newPageVertices = new float[pageCount][];
                Array.Copy(pageVertices, 0, newPageVertices, 0, pageVertices.Length);
                pageVertices = newPageVertices;

                int[] newIdx = new int[pageCount];
                Array.Copy(idx, 0, newIdx, 0, idx.Length);
                idx = newIdx;

                tempGlyphCount = new int[pageCount];
            }

            //layouts.Add(layout);
            RequireGlyphs(layout);
            for (int i = 0, n = layout.runs.Count; i < n; ++i)
            {
                var run = layout.runs[i];
                var glyphs = run.glyphs;        // List<BitmapFont.Glyph>
                var xAdvances = run.xAdvances;  // List<float>
                const float color = 0.0f;       // 사용하지 않음
                float gx = x + run.x, gy = y + run.y;
                for (int ii = 0, nn = glyphs.Count; ii < nn; ++ii)
                {
                    var glyph = glyphs[ii];
                    gx += xAdvances[ii];
                    AddGlyph(glyph, gx, gy, color);
                }
            }
        }

        public void AddText(string str, float x, float y)
        {
            int start = 0;
            int end = str.Length;
            float targetWidth = 0.0f;
            int halign = Align.left;

            GlyphLayout layout = new GlyphLayout();
            layout.SetText(font, str, start, end, color, targetWidth, halign);
            AddText(layout, x, y);
        }

        public void AddText(GlyphLayout layout, float x, float y)
        {
            AddToCache(layout, x, y + font.data.ascent);
        }

        private void AddGlyph(BitmapFont.Glyph glyph, float x, float y, float color)
        {
            float scaleX = font.data.scaleX, scaleY = font.data.scaleY;
            x += glyph.xoffset * scaleX;
            y += glyph.yoffset * scaleY;
            float width = glyph.width * scaleX, height = glyph.height * scaleY;
            float u = glyph.u, u2 = glyph.u2, v = glyph.v, v2 = glyph.v2;

            if (integer)
            {
                x = (float)Math.Round(x, MidpointRounding.AwayFromZero);
                y = (float)Math.Round(y, MidpointRounding.AwayFromZero);
                width = (float)Math.Round(width, MidpointRounding.AwayFromZero);
                height = (float)Math.Round(height, MidpointRounding.AwayFromZero);
            }
            float x2 = x + width, y2 = y + height;

            int page = glyph.page;
            int idx = this.idx[page];
            this.idx[page] += 20;

            float[] vertices = pageVertices[page];
            vertices[idx++] = x;
            vertices[idx++] = y;
            vertices[idx++] = color;
            vertices[idx++] = u;
            vertices[idx++] = v;

            vertices[idx++] = x;
            vertices[idx++] = y2;
            vertices[idx++] = color;
            vertices[idx++] = u;
            vertices[idx++] = v2;

            vertices[idx++] = x2;
            vertices[idx++] = y2;
            vertices[idx++] = color;
            vertices[idx++] = u2;
            vertices[idx++] = v2;

            vertices[idx++] = x2;
            vertices[idx++] = y;
            vertices[idx++] = color;
            vertices[idx++] = u2;
            vertices[idx  ] = v;
        }
    }
}