using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using watabou.noosa;
using watabou.utils;

namespace spdd.desktop
{
    public class DesktopPlatformSupport : PlatformSupport
    {
        public override void UpdateDisplaySize()
        {
            if (!SPDSettings.Fullscreen())
            {
                SPDSettings.WindowResolution(new Point(Game.width, Game.height));
            }
        }

        public override void UpdateSystemUI()
        {
            //Gdx.app.postRunnable( new Runnable() {
            //	@Override
            //	public void run () {
            //		if (SPDSettings.fullscreen()){
            //			Gdx.graphics.setFullscreenMode( Gdx.graphics.getDisplayMode() );
            //		} else {
            //			Point p = SPDSettings.windowResolution();
            //			Gdx.graphics.setWindowedMode( p.x, p.y );
            //		}
            //	}
            //} );
        }

        public override bool ConnectedToUnmeteredNetwork()
        {
            return true; //no easy way to check this in desktop, just assume user doesn't care
        }

        //@Override
        ////FIXME tinyfd_inputBox isn't a full solution for this. No support for multiline, looks ugly. Ideally we'd have an opengl-based input box
        //public void promptTextInput(String title, String hintText, int maxLen, boolean multiLine, String posTxt, String negTxt, TextCallback callback) {
        //	String result = TinyFileDialogs.tinyfd_inputBox(title, title, hintText);
        //	if (result == null){
        //		callback.onSelect(false, "");
        //	} else {
        //		if (result.contains("\r\n"))    result = result.substring(0, result.indexOf("\r\n"));
        //		if (result.contains("\n"))      result = result.substring(0, result.indexOf("\n"));
        //		if (result.length() > maxLen)   result = result.substring(0, maxLen);
        //		callback.onSelect(true, result.replace("\r\n", "").replace("\n", ""));
        //	}
        //}  

        private int pageSize;
        private PixmapPacker packer;

        private static FreeTypeFontGenerator fontGenerator;
        private static Dictionary<int, BitmapFont> fonts;

        public override void SetupFontGenerators(int pageSize, bool systemFont)
        {
            //don't bother doing anything if nothing has changed
            if (fonts != null && this.pageSize == pageSize)
                return;

            this.pageSize = pageSize;
            //this.systemfont = systemfont;

            if (fonts != null)
            {
                //foreach (BitmapFont f in fonts.Values)
                //    f.Dispose();

                fonts.Clear();
                fontGenerator.Dispose();

                if (packer != null)
                {
                    foreach (PixmapPacker.Page p in packer.GetPages())
                    {
                        //p.GetTexture().Dispose();
                        p.GetTexture().Delete();
                    }
                    packer.Dispose();
                }
            }

            fontGenerator = new FreeTypeFontGenerator("fonts/droid_sans.ttf");

            fonts = new Dictionary<int, BitmapFont>();

            packer = new PixmapPacker(pageSize, pageSize);
        }

        public override void ResetGenerators()
        {
            if (fonts != null)
            {
                //foreach (BitmapFont f in fonts.Values)
                //    f.Dispose();

                fonts.Clear();
                fontGenerator.Dispose();

                if (packer != null)
                {
                    foreach (PixmapPacker.Page p in packer.GetPages())
                    {
                        //p.GetTexture().Dispose();
                        p.GetTexture().Delete();
                    }
                    packer.Dispose();
                }
                fonts = null;
            }
            SetupFontGenerators(pageSize, false);   // false - systemFont
        }

        public override BitmapFont GetFont(int size, string text)
        {
            if (!fonts.ContainsKey(size))
            {
                var parameters = new FreeTypeFontGenerator.FreeTypeFontParameter();
                parameters.size = size;
                parameters.flip = true;
                parameters.borderWidth = parameters.size / 10f;
                parameters.renderCount = 3;
                parameters.hinting = FreeTypeFontGenerator.Hinting.None;
                parameters.spaceX = -(int)parameters.borderWidth;
                parameters.characters = "�";
                parameters.packer = packer;

                try
                {
                    BitmapFont font = fontGenerator.GenerateFont(parameters);
                    font.GetData().missingGlyph = font.GetData().GetGlyph('�');
                    fonts.Add(size, font);
                }
                catch (Exception e)
                {
                    Game.ReportException(e);
                    return null;
                }
            }

            return fonts[size];
        }

        const string rs1 = @"(?<=\n)|(?=\n)|(?<=_)|(?=_)|" +
                        @"(?<=\p{IsHiragana})|(?=\p{IsHiragana})|" +
                        @"(?<=\p{IsKatakana})|(?=\p{IsKatakana})|" +
                        @"(?<=\p{IsCJKUnifiedIdeographs})|(?=\p{IsCJKUnifiedIdeographs})|" +
                        @"(?<=\p{IsCJKSymbolsandPunctuation})|(?=\p{IsCJKSymbolsandPunctuation})";
        static private Regex regularsplitter = new Regex(rs1);

        const string rs2 = @"(?<= )|(?= )|(?<=\n)|(?=\n)|(?<=_)|(?=_)|" +
                        @"(?<=\p{IsHiragana})|(?=\p{IsHiragana})|" +
                        @"(?<=\p{IsKatakana})|(?=\p{IsKatakana})|" +
                        @"(?<=\p{IsCJKUnifiedIdeographs})|(?=\p{IsCJKUnifiedIdeographs})|" +
                        @"(?<=\p{IsCJKSymbolsandPunctuation})|(?=\p{IsCJKSymbolsandPunctuation})";
        static private Regex regularsplitterMultiline = new Regex(rs2);

        public override string[] SplitforTextBlock(string text, bool multiline)
        {
            if (multiline)
            {
                return regularsplitterMultiline.Split(text);
            }
            else
            {
                return regularsplitter.Split(text);
            }
        }
    }
}