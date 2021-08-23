using System;
using System.Collections.Generic;
using watabou.glwrap;

namespace watabou.utils
{
    // Defines a rectangular area of a texture. 
    // The coordinate system used has its origin in the upper left corner with the x-axis
    // pointing to the right and the y axis pointing downwards.
    public class TextureRegion
    {
        Texture texture;        // Page에 있는 texture참조
        internal float u, v;
        internal float u2, v2;
        int regionWidth, regionHeight;

        public TextureRegion()
        { }

        public TextureRegion(Texture texture)
        {
            this.texture = texture;
            SetRegion(0, 0, texture.width, texture.height);
        }

        public void SetRegion(int x, int y, int width, int height)
        {
            float invTexWidth = 1f / texture.width;
            float invTexHeight = 1f / texture.height;
            SetRegion(x * invTexWidth, y * invTexHeight, (x + width) * invTexWidth, (y + height) * invTexHeight);
            regionWidth = Math.Abs(width);
            regionHeight = Math.Abs(height);
        }

        public void SetRegion(float u, float v, float u2, float v2)
        {
            int texWidth = texture.width, texHeight = texture.height;
            regionWidth = (int)Math.Round(Math.Abs(u2 - u) * texWidth, MidpointRounding.AwayFromZero);
            regionHeight = (int)Math.Round(Math.Abs(v2 - v) * texHeight, MidpointRounding.AwayFromZero);

            // For a 1x1 region, adjust UVs toward pixel center to avoid filtering artifacts on AMD GPUs when drawing very stretched.
            if (regionWidth == 1 && regionHeight == 1)
            {
                float adjustX = 0.25f / texWidth;
                u += adjustX;
                u2 -= adjustX;
                float adjustY = 0.25f / texHeight;
                v += adjustY;
                v2 -= adjustY;
            }

            this.u = u;
            this.v = v;
            this.u2 = u2;
            this.v2 = v2;
        }

        public Texture GetTexture()
        {
            return texture;
        }

        //public int GetRegionWidth()
        //{
        //    return regionWidth;
        //}
        //
        //public int GetRegionHeight()
        //{
        //    return regionHeight;
        //}
    }

    public class BitmapFont
    {
        private const int LOG2_PAGE_SIZE = 9;
        private const int PAGE_SIZE = 1 << LOG2_PAGE_SIZE;
        private const int PAGES = 0x10000 / PAGE_SIZE;

        public BitmapFontData data;
        public List<TextureRegion> regions;
        private BitmapFontCache cache;
        //private bool flipped; true
        //bool integer;  true
        //private bool ownsTexture; false

        public BitmapFont(BitmapFontData data, List<TextureRegion> pageRegions)
        {
            this.data = data;

            regions = pageRegions;

            cache = NewFontCache();

            Load(data);
        }

        protected void Load(BitmapFontData data)
        {
            foreach (Glyph[] page in data.glyphs)
            {
                if (page == null)
                    continue;

                foreach (Glyph glyph in page)
                {
                    if (glyph != null)
                        data.SetGlyphRegion(glyph, regions[glyph.page]);
                }
            }

            if (data.missingGlyph != null)
                data.SetGlyphRegion(data.missingGlyph, regions[data.missingGlyph.page]);
        }

        public List<TextureRegion> GetRegions()
        {
            return regions;
        }

        public void Draw(IBatch batch, string str, float x, float y)
        {
            cache.Clear();
            cache.AddText(str, x, y);
            cache.Draw(batch);
        }

        public Color GetColor()
        {
            return cache.GetColor();
        }

        //public void Dispose()
        //{
        //}

        public BitmapFontData GetData()
        {
            return data;
        }

        public BitmapFontCache NewFontCache()
        {
            return new BitmapFontCache(this);
        }

        public class Glyph
        {
            public int id;
            public int srcX;
            public int srcY;
            public int width, height;
            public float u, v, u2, v2;
            public int xoffset, yoffset;
            public int xadvance;
            public sbyte[][] kerning;
            public bool fixedWidth;

            /** The index to the texture page that holds this glyph. */
            public int page;

            public int GetKerning(char ch)
            {
                if (kerning != null)
                {
                    sbyte[] page = kerning[(uint)ch >> LOG2_PAGE_SIZE]; // kerning[ch / 512]
                    if (page != null)
                        return page[ch & PAGE_SIZE - 1];
                }
                return 0;
            }

            public void SetKerning(char ch, int value)
            {
                if (kerning == null)
                    kerning = new sbyte[PAGES][];
                sbyte[] page = kerning[(uint)ch >> LOG2_PAGE_SIZE];
                if (page == null)
                    kerning[(uint)ch >> LOG2_PAGE_SIZE] = page = new sbyte[PAGE_SIZE];
                page[ch & PAGE_SIZE - 1] = (sbyte)value;
            }

            public override string ToString()
            {
                return ((char)id).ToString();
            }
        }

        public class BitmapFontData
        {
            /** The name of the font, or null. */
            public string name;
            /** An array of the image paths, for multiple texture pages. */
            public string[] imagePaths;
            //public FileHandle fontFile;
            public bool flipped;
            public float padTop, padRight, padBottom, padLeft;
            /** The distance from one line of text to the next. To set this value, use {@link #setLineHeight(float)}. */
            public float lineHeight;
            /** The distance from the top of most uppercase characters to the baseline. Since the drawing position is the cap height of
             * the first line, the cap height can be used to get the location of the baseline. */
            public float capHeight = 1;
            /** The distance from the cap height to the top of the tallest glyph. */
            public float ascent;
            /** The distance from the bottom of the glyph that extends the lowest to the baseline. This number is negative. */
            public float descent;
            /** The distance to move down when \n is encountered. */
            public float down;
            /** Multiplier for the line height of blank lines. down * blankLineHeight is used as the distance to move down for a blank
             * line. */
            public float blankLineScale = 1;
            public float scaleX = 1, scaleY = 1;
            //public bool markupEnabled;
            /** The amount to add to the glyph X position when drawing a cursor between glyphs. This field is not set by the BMFont
             * file, it needs to be set manually depending on how the glyphs are rendered on the backing textures. */
            public float cursorX;

            public BitmapFont.Glyph[][] glyphs = new BitmapFont.Glyph[PAGES][];
            ///** The glyph to display for characters not in the font. May be null. */
            public BitmapFont.Glyph missingGlyph;

            /** The width of the space character. */
            public float spaceXadvance;
            /** The x-height, which is the distance from the top of most lowercase characters to the baseline. */
            public float xHeight = 1;

            /** Additional characters besides whitespace where text is wrapped. Eg, a hypen (-). */
            public char[] breakChars;
            public char[] xChars = { 'x', 'e', 'a', 'o', 'n', 's', 'r', 'c', 'u', 'm', 'v', 'w', 'z' };
            public char[] capChars = {'M', 'N', 'B', 'D', 'C', 'E', 'F', 'K', 'A', 'G', 'H', 'I', 'J', 'L', 'O', 'P', 'Q', 'R', 'S',
                'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};

            public void SetGlyphRegion(BitmapFont.Glyph glyph, TextureRegion region)
            {
                Texture texture = region.GetTexture();
                float invTexWidth = 1.0f / texture.width;
                float invTexHeight = 1.0f / texture.height;

                float u = region.u;
                float v = region.v;
                //float regionWidth = region.GetRegionWidth();
                //float regionHeight = region.GetRegionHeight();

                float x = glyph.srcX;
                float x2 = glyph.srcX + glyph.width;
                float y = glyph.srcY;
                float y2 = glyph.srcY + glyph.height;

                glyph.u = u + x * invTexWidth;
                glyph.u2 = u + x2 * invTexWidth;
                if (flipped)
                {
                    glyph.v = v + y * invTexHeight;
                    glyph.v2 = v + y2 * invTexHeight;
                }
                else
                {
                    glyph.v2 = v + y * invTexHeight;
                    glyph.v = v + y2 * invTexHeight;
                }
            }

            public void SetGlyph(int ch, BitmapFont.Glyph glyph)
            {
                BitmapFont.Glyph[] page = glyphs[ch / PAGE_SIZE];
                if (page == null)
                    glyphs[ch / PAGE_SIZE] = page = new BitmapFont.Glyph[PAGE_SIZE];
                page[ch & PAGE_SIZE - 1] = glyph;
            }

            public virtual BitmapFont.Glyph GetGlyph(char ch)
            {
                BitmapFont.Glyph[] page = glyphs[ch / PAGE_SIZE];
                if (page != null)
                    return page[ch & PAGE_SIZE - 1];
                return null;
            }

            public virtual void GetGlyphs(GlyphLayout.GlyphRun run, string str, int start, int end, BitmapFont.Glyph lastGlyph)
            {
                //bool markupEnabled = this.markupEnabled;
                float scaleX = this.scaleX;
                BitmapFont.Glyph missingGlyph = this.missingGlyph;
                var glyphs = run.glyphs;
                var xAdvances = run.xAdvances;

                // Guess at number of glyphs needed.
                //glyphs.ensureCapacity(end - start);
                //xAdvances.ensureCapacity(end - start + 1);

                while (start < end)
                {
                    char ch = str[start++];
                    if (ch == '\r')
                        continue; // Ignore.
                    var glyph = GetGlyph(ch);
                    if (glyph == null)
                    {
                        if (missingGlyph == null)
                            continue;
                        glyph = missingGlyph;
                    }

                    glyphs.Add(glyph);

                    if (lastGlyph == null) // First glyph on line, adjust the position so it isn't drawn left of 0.
                        xAdvances.Add(glyph.fixedWidth ? 0 : -glyph.xoffset * scaleX - padLeft);
                    else
                        xAdvances.Add((lastGlyph.xadvance + lastGlyph.GetKerning(ch)) * scaleX);
                    lastGlyph = glyph;

                    //// "[[" is an escaped left square bracket, skip second character.
                    //if (markupEnabled && ch == '[' && start < end && str[start] == '[') 
                    //    start++;
                }

                if (lastGlyph != null)
                {
                    float lastGlyphWidth = lastGlyph.fixedWidth ? lastGlyph.xadvance * scaleX
                        : (lastGlyph.width + lastGlyph.xoffset) * scaleX - padRight;
                    xAdvances.Add(lastGlyphWidth);
                }
            }
        }
    }
}