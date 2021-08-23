using System;
using watabou.input;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.effects;
using spdd.ui;

namespace spdd.scenes
{
    public class AboutScene : PixelScene
    {
        public override void Create()
        {
            base.Create();

            const float colWidth = 120;
            float fullWidth = colWidth * (Landscape() ? 2 : 1);

            int w = Camera.main.width;
            int h = Camera.main.height;

            ScrollPane list = new ScrollPane(new Component());
            Add(list);

            Component content = list.Content();
            content.Clear();

            //*** Shattered Pixel Dungeon Credits ***

            string shpxLink = "https://ShatteredPixel.com";
            //tracking codes, so that the website knows where this pageview came from
            shpxLink += "?utm_source=shatteredpd";
            shpxLink += "&utm_medium=about_page";
            shpxLink += "&utm_campaign=ingame_link";

            CreditsBlock shpx = new CreditsBlock(true, Window.SHPX_COLOR,
                    "Shattered Pixel Dungeon",
                    Icons.SHPX.Get(),
                    "Developed by: _Evan Debenham_\nBased on Pixel Dungeon's open source",
                    "ShatteredPixel.com",
                    shpxLink);
            shpx.SetRect((w - fullWidth) / 2f, 6, 120, 0);
            content.Add(shpx);

            CreditsBlock alex = new CreditsBlock(false, Window.SHPX_COLOR,
                            "Hero Art & Design:",
                            Icons.ALEKS.Get(),
                            "Aleksandar Komitov",
                            "alekskomitov.com",
                            "https://www.alekskomitov.com");
            alex.SetSize(colWidth / 2f, 0);
            if (Landscape())
            {
                alex.SetPos(shpx.Right(), shpx.Top() + (shpx.Height() - alex.Height()) / 2f);
            }
            else
            {
                alex.SetPos(w / 2f - colWidth / 2f, shpx.Bottom() + 5);
            }
            content.Add(alex);

            CreditsBlock charlie = new CreditsBlock(false, Window.SHPX_COLOR,
                    "Sound Effects:",
                    Icons.CHARLIE.Get(),
                    "Charlie",
                    "s9menine.itch.io",
                    "https://s9menine.itch.io");
            charlie.SetRect(alex.Right(), alex.Top(), colWidth / 2f, 0);
            content.Add(charlie);

            //*** Pixel Dungeon Credits ***

            var WATA_COLOR = new Color(0x55, 0xAA, 0xFF, 0xFF);
            CreditsBlock wata = new CreditsBlock(true, WATA_COLOR,
                    "Pixel Dungeon",
                    Icons.WATA.Get(),
                    "Developed by: _Watabou_\nInspired by Brian Walker's Brogue",
                    "pixeldungeon.watabou.ru",
                    "http://pixeldungeon.watabou.ru");
            if (Landscape())
            {
                wata.SetRect(shpx.Left(), shpx.Bottom() + 8, colWidth, 0);
            }
            else
            {
                wata.SetRect(shpx.Left(), alex.Bottom() + 8, colWidth, 0);
            }
            content.Add(wata);

            AddLine(wata.Top() - 4, content);

            CreditsBlock cube = new CreditsBlock(false, WATA_COLOR,
                    "Music:",
                    Icons.CUBE_CODE.Get(),
                    "Cube Code",
                    null,
                    null);
            cube.SetSize(colWidth / 2f, 0);
            if (Landscape())
            {
                cube.SetPos(wata.Right(), wata.Top() + (wata.Height() - cube.Height()) / 2f);
            }
            else
            {
                cube.SetPos(alex.Left(), wata.Bottom() + 5);
            }
            content.Add(cube);

            //*** LibGDX Credits ***

            var GDX_COLOR = new Color(0xE4, 0x4D, 0x3C, 0xFF);
            CreditsBlock gdx = new CreditsBlock(true,
                    GDX_COLOR,
                    null,
                    Icons.LIBGDX.Get(),
                    "ShatteredPD is powered by _LibGDX_!",
                    "libgdx.badlogicgames.com",
                    "http://libgdx.badlogicgames.com");
            if (Landscape())
            {
                gdx.SetRect(wata.Left(), wata.Bottom() + 8, colWidth, 0);
            }
            else
            {
                gdx.SetRect(wata.Left(), cube.Bottom() + 8, colWidth, 0);
            }
            content.Add(gdx);

            AddLine(gdx.Top() - 4, content);

            //blocks the rays from the LibGDX icon going above the line
            ColorBlock blocker = new ColorBlock(w, 8, new Color(0x00, 0x00, 0x00, 0xFF));
            blocker.y = gdx.Top() - 12;
            content.AddToBack(blocker);
            content.SendToBack(gdx);

            CreditsBlock arcnor = new CreditsBlock(false, GDX_COLOR,
                    "Pixel Dungeon GDX:",
                    Icons.ARCNOR.Get(),
                    "Edu García",
                    "twitter.com/arcnor",
                    "https://twitter.com/arcnor");
            arcnor.SetSize(colWidth / 2f, 0);
            if (Landscape())
            {
                arcnor.SetPos(gdx.Right(), gdx.Top() + (gdx.Height() - arcnor.Height()) / 2f);
            }
            else
            {
                arcnor.SetPos(alex.Left(), gdx.Bottom() + 5);
            }
            content.Add(arcnor);

            CreditsBlock purigro = new CreditsBlock(false, GDX_COLOR,
                    "Shattered GDX Help:",
                    Icons.PURIGRO.Get(),
                    "Kevin MacMartin",
                    "github.com/prurigro",
                    "https://github.com/prurigro/");
            purigro.SetRect(arcnor.Right() + 2, arcnor.Top(), colWidth / 2f, 0);
            content.Add(purigro);

            //*** Transifex Credits ***

            CreditsBlock transifex = new CreditsBlock(true,
                    Window.TITLE_COLOR,
                    null,
                    null,
                    "ShatteredPD is community-translated via _Transifex_! Thank you so much to all of Shattered's volunteer translators!",
                    "www.transifex.com/shattered-pixel/",
                    "https://www.transifex.com/shattered-pixel/shattered-pixel-dungeon/");
            transifex.SetRect((Camera.main.width - colWidth) / 2f, purigro.Bottom() + 8, colWidth, 0);
            content.Add(transifex);

            AddLine(transifex.Top() - 4, content);

            AddLine(transifex.Bottom() + 4, content);

            //*** Freesound Credits ***

            CreditsBlock freesound = new CreditsBlock(true,
                    Window.TITLE_COLOR,
                    null,
                    null,
                    "Shattered Pixel Dungeon uses the following sound samples from _freesound.org_:\n\n" +

                    "Creative Commons Attribution License:\n" +
                    "_SFX ATTACK SWORD 001.wav_ by _JoelAudio_\n" +
                    "_Pack: Slingshots and Longbows_ by _saturdaysoundguy_\n" +
                    "_Cracking/Crunching, A.wav_ by _InspectorJ_\n" +
                    "_Extracting a sword.mp3_ by _Taira Komori_\n" +
                    "_Pack: Uni Sound Library_ by _timmy h123_\n\n" +

                    "Creative Commons Zero License:\n" +
                    "_Pack: Movie Foley: Swords_ by _Black Snow_\n" +
                    "_machine gun shot 2.flac_ by _qubodup_\n" +
                    "_m240h machine gun burst 4.flac_ by _qubodup_\n" +
                    "_Pack: Onomatopoeia_ by _Adam N_\n" +
                    "_Pack: Watermelon_ by _lolamadeus_\n" +
                    "_metal chain_ by _Mediapaja2009_\n" +
                    "_Pack: Sword Clashes Pack_ by _JohnBuhr_\n" +
                    "_Pack: Metal Clangs and Pings_ by _wilhellboy_\n" +
                    "_Pack: Stabbing Stomachs & Crushing Skulls_ by _TheFilmLook_\n" +
                    "_Sheep bleating_ by _zachrau_\n" +
                    "_Lemon,Juicy,Squeeze,Fruit.wav_ by _Filipe Chagas_\n" +
                    "_Lemon,Squeeze,Squishy,Fruit.wav_ by _Filipe Chagas_",
                    "www.freesound.org",
                    "https://www.freesound.org");
            freesound.SetRect(transifex.Left() - 10, transifex.Bottom() + 8, colWidth + 20, 0);
            content.Add(freesound);

            content.SetSize(fullWidth, freesound.Bottom() + 10);

            list.SetRect(0, 0, w, h);
            list.ScrollTo(0, 0);

            ExitButton btnExit = new ExitButton();
            btnExit.SetPos(Camera.main.width - btnExit.Width(), 0);
            Add(btnExit);

            FadeIn();
        }

        public override void OnBackPressed()
        {
            ShatteredPixelDungeonDash.SwitchNoFade(typeof(TitleScene));
        }

        private void AddLine(float y, Group content)
        {
            ColorBlock line = new ColorBlock(Camera.main.width, 1, new Color(0x33, 0x33, 0x33, 0xFF));
            line.y = y;
            content.Add(line);
        }

        private class CreditsBlock : Component
        {
            bool large;
            RenderedTextBlock title;
            Image avatar;
            Flare flare;
            RenderedTextBlock body;

            RenderedTextBlock link;
            ColorBlock linkUnderline;
            CreditBlockPointerArea linkButton;

            //many elements can be null, but body is assumed to have content.
            public CreditsBlock(bool large, Color highlight, string title, Image avatar, string body, string linkText, string linkUrl)
            {
                var highlightValue = highlight.i32value;

                this.large = large;

                if (title != null)
                {
                    this.title = PixelScene.RenderTextBlock(title, large ? 8 : 6);
                    if (highlightValue != -1)
                        this.title.Hardlight(highlight);
                    Add(this.title);
                }

                if (avatar != null)
                {
                    this.avatar = avatar;
                    Add(this.avatar);
                }

                if (large && highlightValue != -1 && this.avatar != null)
                {
                    this.flare = new Flare(7, 24).Color(highlight, true).Show(this.avatar, 0);
                    this.flare.angularSpeed = 20;
                }

                this.body = PixelScene.RenderTextBlock(body, 6);
                if (highlightValue != -1)
                    this.body.SetHightlighting(true, highlight);
                if (large)
                    this.body.Align(RenderedTextBlock.CENTER_ALIGN);
                Add(this.body);

                if (linkText != null && linkUrl != null)
                {
                    Color color = new Color(0xFF, 0xFF, 0xFF, 0xFF);
                    if (highlightValue != -1)
                    {
                        color.A = 0xFF;
                        color.R = (byte)((byte)0x00 | highlight.R);
                        color.G = (byte)((byte)0x00 | highlight.G);
                        color.B = (byte)((byte)0x00 | highlight.B);
                    }

                    this.linkUnderline = new ColorBlock(1, 1, color);
                    Add(this.linkUnderline);

                    this.link = PixelScene.RenderTextBlock(linkText, 6);
                    if (highlightValue != -1) this.link.Hardlight(highlight);
                    Add(this.link);

                    linkButton = new CreditBlockPointerArea(0, 0, 0, 0);
                    linkButton.linkUrl = linkUrl;
                    Add(linkButton);
                }
            }

            public class CreditBlockPointerArea : PointerArea
            {
                internal string linkUrl;

                public CreditBlockPointerArea(float x, float y, float width, float height)
                    : base(x, y, width, height)
                { }

                public override void OnClick(PointerEvent e)
                {
                    DeviceCompat.OpenURI(linkUrl);
                }
            }

            protected override void Layout()
            {
                base.Layout();

                float topY = Top();

                if (title != null)
                {
                    title.MaxWidth((int)Width());
                    title.SetPos(x + (Width() - title.Width()) / 2f, topY);
                    topY += title.Height() + (large ? 2 : 1);
                }

                if (large)
                {
                    if (avatar != null)
                    {
                        avatar.x = x + (Width() - avatar.Width()) / 2f;
                        avatar.y = topY;
                        PixelScene.Align(avatar);
                        if (flare != null)
                        {
                            flare.Point(avatar.Center());
                        }
                        topY = avatar.y + avatar.Height() + 2;
                    }

                    body.MaxWidth((int)Width());
                    body.SetPos(x + (Width() - body.Width()) / 2f, topY);
                    topY += body.Height() + 2;
                }
                else
                {
                    if (avatar != null)
                    {
                        avatar.x = x;
                        body.MaxWidth((int)(Width() - avatar.width - 1));

                        if (avatar.Height() > body.Height())
                        {
                            avatar.y = topY;
                            body.SetPos(avatar.x + avatar.Width() + 1, topY + (avatar.Height() - body.Height()) / 2f);
                            topY += avatar.Height() + 1;
                        }
                        else
                        {
                            avatar.y = topY + (body.Height() - avatar.Height()) / 2f;
                            PixelScene.Align(avatar);
                            body.SetPos(avatar.x + avatar.Width() + 1, topY);
                            topY += body.Height() + 2;
                        }
                    }
                    else
                    {
                        topY += 1;
                        body.MaxWidth((int)Width());
                        body.SetPos(x, topY);
                        topY += body.Height() + 2;
                    }
                }

                if (link != null)
                {
                    if (large) topY += 1;
                    link.MaxWidth((int)Width());
                    link.SetPos(x + (Width() - link.Width()) / 2f, topY);
                    topY += link.Height() + 2;

                    linkButton.x = link.Left() - 1;
                    linkButton.y = link.Top() - 1;
                    linkButton.width = link.Width() + 2;
                    linkButton.height = link.Height() + 2;

                    linkUnderline.Size(link.Width(), PixelScene.Align(0.49f));
                    linkUnderline.x = link.Left();
                    linkUnderline.y = link.Bottom() + 1;
                }

                topY -= 2;

                height = Math.Max(height, topY - Top());
            }
        }
    }
}