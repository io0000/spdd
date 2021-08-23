using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using SharpFont;
using watabou.glwrap;

namespace watabou.utils
{
    public class FreeTypeFontGenerator : IDisposable
    {
        private Library library;
        private Face face;

        private string name;
        private int pixelWidth, pixelHeight;

        public static int ToInt(int value)
        {
            return ((value + 63) & -64) >> 6;
        }

        public FreeTypeFontGenerator(string fontFile)
        {
            const int faceIndex = 0;
            name = NameWithoutExtension(fontFile);
            library = new Library();
            face = new Face(library, fontFile, faceIndex);
        }

        private void GetLoadingFlags(FreeTypeFontParameter parameter, ref LoadFlags flags, ref LoadTarget target)
        {
            flags = LoadFlags.Default;      // Default: 0
            target = LoadTarget.Normal;     // Normal: 0

            switch (parameter.hinting)
            {
                case Hinting.None:
                    flags |= LoadFlags.NoHinting;
                    break;
                case Hinting.Slight:
                    target |= LoadTarget.Light;
                    break;
                case Hinting.Medium:
                    target |= LoadTarget.Normal;
                    break;
                case Hinting.Full:
                    target |= LoadTarget.Mono;
                    break;
                case Hinting.AutoSlight:
                    flags |= LoadFlags.ForceAutohint;
                    target |= LoadTarget.Light;
                    break;
                case Hinting.AutoMedium:
                    flags |= LoadFlags.ForceAutohint;
                    target |= LoadTarget.Normal;
                    break;
                case Hinting.AutoFull:
                    flags |= LoadFlags.ForceAutohint;
                    target |= LoadTarget.Mono;
                    break;
            }
        }

        private string NameWithoutExtension(string name)
        {
            int dotIndex = name.LastIndexOf('.');
            if (dotIndex == -1)
                return name;
            return name.Substring(0, dotIndex);
        }

        // LoadFlags flags, LoadTarget target
        private bool LoadChar(int c, LoadFlags flags, LoadTarget target)
        {
            try
            {
                face.LoadChar((uint)c, flags, target);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public BitmapFont GenerateFont(FreeTypeFontParameter parameter)
        {
            return GenerateFont(parameter, new FreeTypeBitmapFontData());
        }

        public BitmapFont GenerateFont(FreeTypeFontParameter parameter, FreeTypeBitmapFontData data)
        {
            bool updateTextureRegions = data.regions == null && parameter.packer != null;
            if (updateTextureRegions)
                data.regions = new List<TextureRegion>();

            GenerateData(parameter, data);

            if (updateTextureRegions)
                parameter.packer.UpdateTextureRegions(data.regions, parameter.minFilter, parameter.magFilter, parameter.genMipMaps);

            if (data.regions.Count == 0)
                throw new Exception("Unable to create a font with no texture regions.");

            BitmapFont font = new BitmapFont(data, data.regions);
            //font.SetOwnsTexture(parameter.packer == null);
            return font;
        }

        void SetPixelSizes(int pixelWidth, int pixelHeight)
        {
            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;

            try
            {
                face.SetPixelSizes((uint)pixelWidth, (uint)pixelHeight);
            }
            catch (Exception)
            {
                throw new Exception("Couldn't set size for font");
            }
        }

        public void GenerateData(FreeTypeFontParameter parameter, FreeTypeBitmapFontData data)
        {
            data.name = name + "-" + parameter.size;
            parameter = parameter == null ? new FreeTypeFontParameter() : parameter;
            char[] characters = parameter.characters.ToCharArray();
            int charactersLength = characters.Length;

            LoadFlags flags = LoadFlags.Default;
            LoadTarget target = LoadTarget.Normal;

            GetLoadingFlags(parameter, ref flags, ref target);

            SetPixelSizes(0, parameter.size);

            // set general font data
            var fontMetrics = face.Size.Metrics;
            data.flipped = parameter.flip;
            data.ascent = ToInt(fontMetrics.Ascender.Value);
            data.descent = ToInt(fontMetrics.Descender.Value);
            data.lineHeight = ToInt(fontMetrics.Height.Value);

            float baseLine = data.ascent;

            data.lineHeight += parameter.spaceY;

            // determine space width
            if (LoadChar(' ', flags, target) || LoadChar('l', flags, target))
            {
                data.spaceXadvance = ToInt(face.Glyph.Metrics.HorizontalAdvance.Value);
            }
            else
            {
                data.spaceXadvance = face.MaxAdvanceWidth;  // Possibly very wrong.
            }

            // determine x-height
            foreach (char xChar in data.xChars)
            {
                if (!LoadChar(xChar, flags, target))
                    continue;
                data.xHeight = ToInt(face.Glyph.Metrics.Height.Value);
                break;
            }
            if (data.xHeight == 0)
                throw new Exception("No x-height character found in font");

            // determine cap height
            foreach (char capChar in data.capChars)
            {
                if (!LoadChar(capChar, flags, target))
                    continue;
                data.capHeight = ToInt(face.Glyph.Metrics.Height.Value) + 0; // Math.Abs(parameter.shadowOffsetY);
                break;
            }
            if (data.capHeight == 1)
                throw new Exception("No cap character found in font");

            data.ascent -= data.capHeight;
            data.down = -data.lineHeight;
            if (parameter.flip)
            {
                data.ascent = -data.ascent;
                data.down = -data.down;
            }

            PixmapPacker packer = parameter.packer;

            data.freetypeGlyphs = new List<BitmapFont.Glyph>();

            Stroker stroker = null;
            if (parameter.borderWidth > 0)
            {
                stroker = new Stroker(library);
                var miterLimit = new Fixed16Dot16(0);
                stroker.Set((int)(parameter.borderWidth * 64f),
                    parameter.borderStraight ? StrokerLineCap.Butt : StrokerLineCap.Round,
                    parameter.borderStraight ? StrokerLineJoin.MiterFixed : StrokerLineJoin.Round, miterLimit);
            }

            // Create glyphs largest height first for best packing.
            int[] heights = new int[charactersLength];
            for (int i = 0; i < charactersLength; ++i)
            {
                char c = characters[i];

                int height = LoadChar(c, flags, target) ? ToInt(face.Glyph.Metrics.Height.Value) : 0;
                heights[i] = height;

                if (c == '\0')
                {
                    BitmapFont.Glyph missingGlyph = CreateGlyph('\0', data, parameter, stroker, baseLine, packer);
                    if (missingGlyph != null && missingGlyph.width != 0 && missingGlyph.height != 0)
                    {
                        data.SetGlyph('\0', missingGlyph);
                        data.missingGlyph = missingGlyph;
                        data.freetypeGlyphs.Add(missingGlyph);
                    }
                }
            }

            int heightsCount = heights.Length;
            while (heightsCount > 0)
            {
                int best = 0, maxHeight = heights[0];
                for (int i = 1; i < heightsCount; ++i)
                {
                    int height = heights[i];
                    if (height > maxHeight)
                    {
                        maxHeight = height;
                        best = i;
                    }
                }

                char c = characters[best];
                if (data.GetGlyph(c) == null)
                {
                    BitmapFont.Glyph glyph = CreateGlyph(c, data, parameter, stroker, baseLine, packer);
                    if (glyph != null)
                    {
                        data.SetGlyph(c, glyph);
                        data.freetypeGlyphs.Add(glyph);
                    }
                }

                --heightsCount;
                heights[best] = heights[heightsCount];
                char tmpChar = characters[best];
                characters[best] = characters[heightsCount];
                characters[heightsCount] = tmpChar;
            }

            data.generator = this;
            data.parameter = parameter;
            data.stroker = stroker;
            data.packer = packer;

            // Generate kerning.
            parameter.kerning &= face.HasKerning;
            if (parameter.kerning)
            {
                for (int i = 0; i < charactersLength; ++i)
                {
                    char firstChar = characters[i];
                    BitmapFont.Glyph first = data.GetGlyph(firstChar);
                    if (first == null)
                        continue;
                    uint firstIndex = face.GetCharIndex(firstChar);
                    for (int ii = i; ii < charactersLength; ++ii)
                    {
                        char secondChar = characters[ii];
                        BitmapFont.Glyph second = data.GetGlyph(secondChar);
                        if (second == null)
                            continue;
                        uint secondIndex = face.GetCharIndex(secondChar);

                        var kerningVec = face.GetKerning(firstIndex, secondIndex, 0); // FT_KERNING_DEFAULT (scaled then rounded).
                        int kerning = kerningVec.X.Value;
                        if (kerning != 0)
                            first.SetKerning(secondChar, ToInt(kerning));

                        kerningVec = face.GetKerning(secondIndex, firstIndex, 0); // FT_KERNING_DEFAULT (scaled then rounded).
                        kerning = kerningVec.X.Value;
                        if (kerning != 0)
                            second.SetKerning(firstChar, ToInt(kerning));
                    }
                }
            }

            // Set space glyph.
            BitmapFont.Glyph spaceGlyph = data.GetGlyph(' ');
            if (spaceGlyph == null)
            {
                spaceGlyph = new BitmapFont.Glyph();
                spaceGlyph.xadvance = (int)data.spaceXadvance + parameter.spaceX;
                spaceGlyph.id = (int)' ';
                data.SetGlyph(' ', spaceGlyph);
            }
            if (spaceGlyph.width == 0)
                spaceGlyph.width = (int)(spaceGlyph.xadvance + data.padRight);
        }

        BitmapFont.Glyph CreateGlyph(char c, FreeTypeBitmapFontData data, FreeTypeFontParameter parameter, Stroker stroker, float baseLine, PixmapPacker packer)
        {
            bool missing = face.GetCharIndex(c) == 0 && c != 0;
            if (missing)
                return null;

            LoadFlags flags = LoadFlags.Default;
            LoadTarget target = LoadTarget.Normal;

            GetLoadingFlags(parameter, ref flags, ref target);

            if (!LoadChar(c, flags, target))
                return null;

            GlyphSlot slot = face.Glyph;
            var mainGlyph = slot.GetGlyph();    // SharpFont.Glyph
            FTVector26Dot6 orgin = new FTVector26Dot6(0, 0);

            try
            {
                mainGlyph.ToBitmap(parameter.mono ? RenderMode.Mono : RenderMode.Normal, orgin, true);
            }
            catch (Exception)
            {
                mainGlyph.Dispose();
                //Console.WriteLine("Couldn't render char: {0}",  c);
                return null;
            }

            FTBitmap mainBitmap = mainGlyph.ToBitmapGlyph().Bitmap;
            Pixmap mainPixmap = GetPixmap(mainBitmap, parameter.color, parameter.gamma);

            // TODO - stroker
            if (mainBitmap.Width != 0 && mainBitmap.Rows != 0)
            {
                int offsetX = 0, offsetY = 0;
                if (parameter.borderWidth > 0)
                {
                    // execute stroker; this generates a glyph "extended" along the outline
                    var bitmapGlyph = mainGlyph.ToBitmapGlyph();
                    int top = bitmapGlyph.Top, left = bitmapGlyph.Left;
                    var borderGlyph = slot.GetGlyph();
                    borderGlyph = borderGlyph.StrokeBorder(stroker, false, false);

                    borderGlyph.ToBitmap(parameter.mono ? RenderMode.Mono : RenderMode.Normal, orgin, true);
                    var borderBitmapGlyph = borderGlyph.ToBitmapGlyph();
                    offsetX = left - borderBitmapGlyph.Left;
                    offsetY = -(top - borderBitmapGlyph.Top);

                    // Render border (pixmap is bigger than main).
                    FTBitmap borderBitmap = borderBitmapGlyph.Bitmap;
                    Pixmap borderPixmap = GetPixmap(borderBitmap, parameter.borderColor, parameter.borderGamma);

                    // Draw main glyph on top of border.
                    for (int i = 0, n = parameter.renderCount; i < n; ++i)
                        borderPixmap.DrawPixmap(mainPixmap, offsetX, offsetY);

                    //mainPixmap.Dispose();
                    mainGlyph.Dispose();
                    mainPixmap = borderPixmap;
                    mainGlyph = borderGlyph;
                }
            }

            var metrics = slot.Metrics;
            var glyph = new BitmapFont.Glyph();
            glyph.id = c;
            glyph.width = mainPixmap.GetWidth();
            glyph.height = mainPixmap.GetHeight();
            glyph.xoffset = mainGlyph.ToBitmapGlyph().Left;
            if (parameter.flip)
                glyph.yoffset = -mainGlyph.ToBitmapGlyph().Top + (int)baseLine;
            else
                glyph.yoffset = -(glyph.height - mainGlyph.ToBitmapGlyph().Top) - (int)baseLine;
            glyph.xadvance = ToInt(metrics.HorizontalAdvance.Value) + (int)parameter.borderWidth + parameter.spaceX;

            RectF rect = packer.Pack(mainPixmap);
            //Console.WriteLine("id:{0} -({1},{2},{3},{4})", glyph.id, rect.left, rect.top, rect.right, rect.bottom);

            glyph.page = packer.GetPages().Count - 1; // Glyph is always packed into the last page for now.
            glyph.srcX = (int)rect.left;
            glyph.srcY = (int)rect.top;

            // If a page was added, create a new texture region for the incrementally added glyph.
            if (data.regions != null && data.regions.Count <= glyph.page)
                packer.UpdateTextureRegions(data.regions, parameter.minFilter, parameter.magFilter, parameter.genMipMaps);

            //mainPixmap.Dispose();
            mainGlyph.Dispose();

            return glyph;
        }

        // CreateGlyph에서 사용되는 임시 pixmap을 생성
        private Pixmap GetPixmap(FTBitmap bitmap, Color color, float gamma)
        {
            int width = bitmap.Width;
            int rows = bitmap.Rows;

            int rowBytes = Math.Abs(bitmap.Pitch);
            var pixelMode = bitmap.PixelMode;

            var pixmap = new Pixmap(width, rows);
            byte[] pixels = pixmap.GetPixels();

            IntPtr source = bitmap.Buffer;
            int index = 0;
            int a = color.A;

            unsafe
            {
                for (int y = 0; y < rows; ++y)
                {
                    byte* src = (byte*)source + y * rowBytes;

                    for (int x = 0; x < width; ++x)
                    {
                        int alpha = src[x];

                        int modAlpha;
                        if (alpha == 0)
                            modAlpha = 0;
                        else if (alpha == 255)
                            modAlpha = a;
                        else
                            modAlpha = (int)(a * (float)Math.Pow(alpha / 255f, gamma)); // Inverse gamma.

                        pixels[index + 0] = color.R;
                        pixels[index + 1] = color.G;
                        pixels[index + 2] = color.B;
                        pixels[index + 3] = (byte)modAlpha;

                        index += 4;
                    }
                }
            }

            return pixmap;
        }

        public void Dispose()
        {
            this.face.Dispose();
            this.library.Dispose();
        }

        public class FreeTypeBitmapFontData : BitmapFont.BitmapFontData, IDisposable
        {
            public List<TextureRegion> regions;

            // Fields for incremental glyph generation.
            internal FreeTypeFontGenerator generator;
            internal FreeTypeFontParameter parameter;
            internal Stroker stroker;
            internal PixmapPacker packer;
            public List<BitmapFont.Glyph> freetypeGlyphs;
            private bool dirty;

            public override BitmapFont.Glyph GetGlyph(char ch)
            {
                BitmapFont.Glyph glyph = base.GetGlyph(ch);
                if (glyph == null && generator != null)
                {
                    generator.SetPixelSizes(0, parameter.size);
                    float baseline = ((flipped ? -ascent : ascent) + capHeight) / scaleY;
                    glyph = generator.CreateGlyph(ch, this, parameter, stroker, baseline, packer);
                    if (glyph == null)
                        return missingGlyph;

                    SetGlyphRegion(glyph, regions[glyph.page]);
                    SetGlyph(ch, glyph);
                    freetypeGlyphs.Add(glyph);
                    dirty = true;

                    Face face = generator.face;
                    if (parameter.kerning)
                    {
                        uint glyphIndex = face.GetCharIndex(ch);
                        for (int i = 0, n = freetypeGlyphs.Count; i < n; ++i)
                        {
                            var other = freetypeGlyphs[i];
                            uint otherIndex = face.GetCharIndex((uint)other.id);

                            var kerning = face.GetKerning(glyphIndex, otherIndex, 0);
                            if (kerning.X != 0)
                                glyph.SetKerning((char)other.id, ToInt(kerning.X.Value));

                            kerning = face.GetKerning(otherIndex, glyphIndex, 0);
                            if (kerning.X != 0)
                                other.SetKerning(ch, ToInt(kerning.X.Value));
                        }
                    }
                }
                return glyph;
            }

            public override void GetGlyphs(GlyphLayout.GlyphRun run, string str, int start, int end, BitmapFont.Glyph lastGlyph)
            {
                if (packer != null)
                    packer.SetPackToTexture(true); // All glyphs added after this are packed directly to the texture.
                base.GetGlyphs(run, str, start, end, lastGlyph);
                if (dirty)
                {
                    dirty = false;
                    packer.UpdateTextureRegions(regions, parameter.minFilter, parameter.magFilter, parameter.genMipMaps);
                }
            }

            public void Dispose()
            {
                if (stroker != null)
                    stroker.Dispose();
                if (packer != null)
                    packer.Dispose();
            }
        }

        /** Font smoothing algorithm. */
        public enum Hinting
        {
            /** Disable hinting. Generated glyphs will look blurry. */
            None,
            /** Light hinting with fuzzy edges, but close to the original shape */
            Slight,
            /** Average hinting */
            Medium,
            /** Strong hinting with crisp edges at the expense of shape fidelity */
            Full,
            /** Light hinting with fuzzy edges, but close to the original shape. Uses the FreeType auto-hinter. */
            AutoSlight,
            /** Average hinting. Uses the FreeType auto-hinter. */
            AutoMedium,
            /** Strong hinting with crisp edges at the expense of shape fidelity. Uses the FreeType auto-hinter. */
            AutoFull,
        }

        public class FreeTypeFontParameter
        {
            /** The size in pixels */
            public int size = 16;
            /** If true, font smoothing is disabled. */
            public bool mono;
            /** Strength of hinting */
            public Hinting hinting;
            /** Foreground color (required for non-black borders) */
            public Color color;
            /** Glyph gamma. Values > 1 reduce antialiasing. */
            public float gamma;
            /** Number of times to render the glyph. Useful with a shadow or border, so it doesn't show through the glyph. */
            public int renderCount;
            /** Border width in pixels, 0 to disable */
            public float borderWidth;
            /** Border color; only used if borderWidth > 0 */
            public Color borderColor;
            /** true for straight (mitered), false for rounded borders */
            public bool borderStraight;
            /** Values < 1 increase the border size. */
            public float borderGamma;
            /** Offset of text shadow on X axis in pixels, 0 to disable */
            //public int shadowOffsetX;
            /** Offset of text shadow on Y axis in pixels, 0 to disable */
            //public int shadowOffsetY;
            /** Shadow color; only used if shadowOffset > 0. If alpha component is 0, no shadow is drawn but characters are still offset
             * by shadowOffset. */
            //public Color shadowColor = new Color(0x00, 0x00, 0x00, 192);    // 0.75f
            /** Pixels to add to glyph spacing when text is rendered. Can be negative. */
            public int spaceX, spaceY;
            /** Pixels to add to the glyph in the texture. Cannot be negative. */
            //public int padTop, padLeft, padBottom, padRight;
            /** The characters the font should contain. If '\0' is not included then {@link BitmapFontData#missingGlyph} is not set. */
            public string characters;
            /** Whether the font should include kerning */
            public bool kerning;
            /** The optional PixmapPacker to use for packing multiple fonts into a single texture.
             * @see FreeTypeFontParameter */
            public PixmapPacker packer;
            /** Whether to flip the font vertically */
            public bool flip;
            /** Whether to generate mip maps for the resulting texture */
            public bool genMipMaps;
            /** Minification filter */
            public TextureMinFilter minFilter;
            /** Magnification filter */
            public TextureMagFilter magFilter;

            public FreeTypeFontParameter()
            {
                this.hinting = FreeTypeFontGenerator.Hinting.AutoMedium;
                this.color = new Color(0xFF, 0xFF, 0xFF, 0xFF);
                this.gamma = 1.8F;
                this.renderCount = 2;
                this.borderWidth = 0.0F;
                this.borderColor = new Color(0x00, 0x00, 0x00, 0xFF);
                this.borderStraight = false;
                this.borderGamma = 1.8F;
                //this.shadowOffsetX = 0;
                //this.shadowOffsetY = 0;
                //this.shadowColor = new Color(0.0F, 0.0F, 0.0F, 0.75F);
                this.kerning = true;
                this.packer = null;
                this.flip = false;
                this.genMipMaps = false;
                this.minFilter = TextureMinFilter.Nearest;
                this.magFilter = TextureMagFilter.Nearest;
            }
        }
    }
}