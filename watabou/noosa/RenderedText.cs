using System;
using watabou.glwrap;
using watabou.utils;

namespace watabou.noosa
{
    public class RenderedText : Image
    {
        private BitmapFont font;
        private int size;
        private string text;

        public RenderedText()
        {
            text = null;
        }

        public RenderedText(int size)
        {
            text = null;
            this.size = size;
        }

        public RenderedText(string text, int size)
        {
            this.text = text;
            this.size = size;

            Measure();
        }

        public void Text(string text)
        {
            this.text = text;

            Measure();
        }

        public string Text()
        {
            return text;
        }

        public void Size(int size)
        {
            this.size = size;
            Measure();
        }

        private void Measure()
        {
            //if (Thread.currentThread().getName().equals("SHPD Actor Thread"))
            //{
            //	throw new RuntimeException("Text measured from the actor thread!");
            //}

            if (text == null || text.Equals(""))
            {
                text = "";
                width = height = 0;
                visible = false;
                return;
            }
            else
            {
                visible = true;
            }

            font = Game.platform.GetFont(size, text);

            if (font != null)
            {
                GlyphLayout glyphs = new GlyphLayout(font, text);

                foreach (char c in text)
                {
                    BitmapFont.Glyph g = font.GetData().GetGlyph(c);
                    if (g == null || (g.id != c))
                    {
                        Game.ReportException(new Exception("font file " + font.ToString() + " could not render " + c));
                    }
                }

                //We use the xadvance of the last glyph in some cases to fix issues
                // with fullwidth punctuation marks in some asian scripts
                BitmapFont.Glyph lastGlyph = font.GetData().GetGlyph(text[text.Length - 1]);
                if (lastGlyph != null && lastGlyph.xadvance > lastGlyph.width * 1.5f)
                {
                    width = glyphs.width - lastGlyph.width + lastGlyph.xadvance;
                }
                else
                {
                    width = glyphs.width;
                }

                //this is identical to l.height in most cases, but we force this for consistency.
                height = (float)Math.Round(size * 0.75f, MidpointRounding.AwayFromZero);
                renderedHeight = glyphs.height;
            }
        }

        private float renderedHeight;

        protected override void UpdateMatrix()
        {
            base.UpdateMatrix();
            //sometimes the font is rendered oddly, so we offset here to put it in the correct spot
            if (renderedHeight != height)
            {
                Matrix.Translate(matrix, 0, (float)Math.Round(height - renderedHeight, MidpointRounding.AwayFromZero));
            }
        }

        private static TextRenderBatch textRenderer = new TextRenderBatch();

        public override void Draw()
        {
            if (font != null)
            {
                UpdateMatrix();
                TextRenderBatch.textBeingRendered = this;
                font.Draw(textRenderer, text, 0, 0);
            }
        }

        //implements regular PD rendering within a LibGDX batch so that our rendering logic
        //can interface with the freetype font generator
        private class TextRenderBatch : IBatch
        {
            //this isn't as good as only updating once, like with bitmaptext
            // but it skips almost all allocations, which is almost as good
            public static RenderedText textBeingRendered = null;

            public void Draw(Texture texture, float[] spriteVertices, int offset, int count)
            {
                Visual v = textBeingRendered;
                int n = count / 20;
                float[] vertices = new float[16 * n];
                //FloatBuffer toOpenGL;
                //if (buffers.containsKey(count/20)){
                //	toOpenGL = buffers.get(count/20);
                //	toOpenGL.position(0);
                //} else {
                //	toOpenGL = Quad.createSet(count / 20);
                //	buffers.put(count/20, toOpenGL);
                //}

                int j = 0;

                for (int i = 0; i < count; i += 20, j += 16)
                {
                    vertices[j + 0] = spriteVertices[i + 0];
                    vertices[j + 1] = spriteVertices[i + 1];

                    vertices[j + 2] = spriteVertices[i + 3];
                    vertices[j + 3] = spriteVertices[i + 4];

                    vertices[j + 4] = spriteVertices[i + 5];
                    vertices[j + 5] = spriteVertices[i + 6];

                    vertices[j + 6] = spriteVertices[i + 8];
                    vertices[j + 7] = spriteVertices[i + 9];

                    vertices[j + 8] = spriteVertices[i + 10];
                    vertices[j + 9] = spriteVertices[i + 11];

                    vertices[j + 10] = spriteVertices[i + 13];
                    vertices[j + 11] = spriteVertices[i + 14];

                    vertices[j + 12] = spriteVertices[i + 15];
                    vertices[j + 13] = spriteVertices[i + 16];

                    vertices[j + 14] = spriteVertices[i + 18];
                    vertices[j + 15] = spriteVertices[i + 19];

                    //toOpenGL.put(vertices);
                }

                //toOpenGL.position(0);

                NoosaScript script = NoosaScript.Get();

                texture.Bind();
                //Texture.Clear();

                script.Camera(v.GetCamera());

                script.uModel.ValueM4(v.matrix);
                script.Lighting(
                        v.rm, v.gm, v.bm, v.am,
                        v.ra, v.ga, v.ba, v.aa);

                script.DrawQuadSet(vertices, count / 20);
            }
        }
    }
}