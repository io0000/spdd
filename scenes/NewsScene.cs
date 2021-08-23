using System;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.messages;
using spdd.services.news;
using spdd.sprites;
using spdd.ui;
using spdd.windows;

namespace spdd.scenes
{
    public class NewsScene : PixelScene
    {
        bool displayingNoArticles;

        private const int BTN_HEIGHT = 22;
        private const int BTN_WIDTH = 100;

        public override void Create()
        {
            base.Create();

            uiCamera.visible = false;

            int w = Camera.main.width;
            int h = Camera.main.height;

            int fullWidth = PixelScene.Landscape() ? 202 : 100;
            int left = (w - fullWidth) / 2;

            Archs archs = new Archs();
            archs.SetSize(w, h);
            Add(archs);

            ExitButton btnExit = new ExitButton();
            btnExit.SetPos(w - btnExit.Width(), 0);
            Add(btnExit);

            RenderedTextBlock title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
            title.Hardlight(Window.TITLE_COLOR);
            title.SetPos(
                    (w - title.Width()) / 2f,
                    (20 - title.Height()) / 2f
            );
            Align(title);
            Add(title);

            float top = 20;

            displayingNoArticles = !News.ArticlesAvailable();
            if (displayingNoArticles || Messages.Lang() != Languages.ENGLISH)
            {
                Component newsInfo = new NewsInfo();
                newsInfo.SetRect(left, 20, fullWidth, 0);
                Add(newsInfo);

                top = newsInfo.Bottom();
            }

            if (!displayingNoArticles)
            {
                var articles = News.Articles();

                float articleSpace = h - top - 2;
                int rows = articles.Count;
                if (PixelScene.Landscape())
                    rows /= 2;
                ++rows;

                while ((articleSpace) / (BTN_HEIGHT + 1) < rows)
                {
                    articles.RemoveAt(articles.Count - 1);
                    if (PixelScene.Landscape())
                        articles.RemoveAt(articles.Count - 1);

                    --rows;
                }

                float gap = ((articleSpace) - (BTN_HEIGHT * rows)) / (float)rows;

                bool rightCol = false;
                foreach (NewsArticle article in articles)
                {
                    StyledButton b = new ArticleButton(article);
                    if (!rightCol)
                    {
                        top += gap;
                        b.SetRect(left, top, BTN_WIDTH, BTN_HEIGHT);
                    }
                    else
                    {
                        b.SetRect(left + fullWidth - BTN_WIDTH, top, BTN_WIDTH, BTN_HEIGHT);
                    }
                    Align(b);
                    Add(b);
                    if (!PixelScene.Landscape())
                    {
                        top += BTN_HEIGHT;
                    }
                    else
                    {
                        if (rightCol)
                        {
                            top += BTN_HEIGHT;
                        }
                        rightCol = !rightCol;
                    }
                }
                top += gap;
            }
            else
            {
                top += 20;
            }

            var btnSite = new NewsSceneStyleButton(Chrome.Type.GREY_BUTTON_TR, Messages.Get(this, "read_more"));
            btnSite.scene = this;
            btnSite.Icon(Icons.NEWS.Get());
            btnSite.TextColor(Window.TITLE_COLOR);
            btnSite.SetRect(left, top, fullWidth, BTN_HEIGHT);
            Add(btnSite);
        }

        private class NewsSceneStyleButton : StyledButton
        {
            internal NewsScene scene;

            public NewsSceneStyleButton(Chrome.Type type, string label)
                : base(type, label)
            { }

            protected override void OnClick()
            {
                base.OnClick();
                string link = "https://ShatteredPixel.com";
                //tracking codes, so that the website knows where this pageview came from
                link += "?utm_source=shatteredpd";
                link += "&utm_medium=news_page";
                link += "&utm_campaign=ingame_link";
                DeviceCompat.OpenURI(link);
            }
        }

        public override void OnBackPressed()
        {
            ShatteredPixelDungeonDash.SwitchNoFade(typeof(TitleScene));
        }

        public override void Update()
        {
            if (displayingNoArticles && News.ArticlesAvailable())
            {
                ShatteredPixelDungeonDash.SeamlessResetScene();
            }
            base.Update();
        }

        private class NewsInfo : Component
        {
            NinePatch bg;
            RenderedTextBlock text;
            ActionRedButton button;

            protected override void CreateChildren()
            {
                bg = Chrome.Get(Chrome.Type.GREY_BUTTON_TR);
                Add(bg);

                string message = "";

                if (Messages.Lang() != Languages.ENGLISH)
                {
                    message += Messages.Get(this, "english_warn");
                }

                if (!News.ArticlesAvailable())
                {
                    if (SPDSettings.News())
                    {
                        if (SPDSettings.WiFi() && !Game.platform.ConnectedToUnmeteredNetwork())
                        {
                            message += "\n\n" + Messages.Get(this, "metered_network");

                            button = new ActionRedButton(Messages.Get(this, "enable_data"));
                            button.action = () =>
                            {
                                //base.onClick(); << do nothing
                                SPDSettings.WiFi(false);
                                News.CheckForNews();
                                ShatteredPixelDungeonDash.SeamlessResetScene();
                            };

                            Add(button);
                        }
                        else
                        {
                            message += "\n\n" + Messages.Get(this, "no_internet");
                        }
                    }
                    else
                    {
                        message += "\n\n" + Messages.Get(this, "news_disabled");

                        button = new ActionRedButton(Messages.Get(this, "enable_news"));
                        button.action = () =>
                        {
                            //base.onClick(); << do nothing
                            SPDSettings.News(true);
                            News.CheckForNews();
                            ShatteredPixelDungeonDash.SeamlessResetScene();
                        };

                        Add(button);
                    }
                }

                if (message.StartsWith("\n\n"))
                    message = ReplaceFirst(message, "\n\n", "");

                text = PixelScene.RenderTextBlock(message, 6);
                text.Hardlight(CharSprite.WARNING);
                Add(text);
            }

            private static string ReplaceFirst(string message, string str1, string str2)
            {
                int index = message.IndexOf(str1, StringComparison.Ordinal);
                if (index >= 0)
                {
                    message = message.Remove(index, str1.Length);
                    message = message.Insert(index, str2);
                }
                return message;
            }

            protected override void Layout()
            {
                bg.x = x;
                bg.y = y;

                text.MaxWidth((int)width - bg.MarginHor());
                text.SetPos(x + bg.MarginLeft(), y + bg.MarginTop());

                height = (text.Bottom()) - y;

                if (button != null)
                {
                    height += 4;
                    button.SetSize(button.ReqWidth() + 2, 16);
                    button.SetPos(x + (width - button.Width()) / 2, y + height);
                    height = button.Bottom() - y;
                }

                height += bg.MarginBottom();

                bg.Size(width, height);
            }
        }

        private class ArticleButton : StyledButton
        {
            NewsArticle article;

            BitmapText date;

            public ArticleButton(NewsArticle article)
                : base(Chrome.Type.GREY_BUTTON_TR, article.title, 6)
            {
                this.article = article;

                Icon(News.ParseArticleIcon(article));
                long lastRead = SPDSettings.NewsLastRead();
                if (lastRead > 0 && article.date.Ticks > lastRead)
                {
                    TextColor(Window.SHPX_COLOR);
                }

                //Calendar cal = Calendar.getInstance();
                //cal.setTime(article.date);
                //date = new BitmapText(News.ParseArticleDate(article), pixelFont);
                date = new BitmapText("2020-01-01", pixelFont);
                date.scale.Set(PixelScene.Align(0.5f));
                date.Hardlight(new Color(0x88, 0x88, 0x88, 0xFF));
                date.Measure();
                Add(date);
            }

            protected override void Layout()
            {
                text.MaxWidth((int)(width - icon.Width() - bg.MarginHor() - 2));

                base.Layout();

                icon.x = x + bg.MarginLeft() + (16 - icon.Width()) / 2f;
                PixelScene.Align(icon);
                text.SetPos(x + bg.MarginLeft() + 18, text.Top());

                if (date != null)
                {
                    date.x = x + width - bg.MarginRight() - date.Width() + 2;
                    date.y = y + height - bg.MarginBottom() - date.Height() + 3.5f;
                    Align(date);
                }
            }

            protected override void OnClick()
            {
                base.OnClick();
                TextColor(Window.WHITE);
                if (article.date.Ticks > SPDSettings.NewsLastRead())
                {
                    SPDSettings.NewsLastRead(article.date.Ticks);
                }
                ShatteredPixelDungeonDash.Scene().AddToFront(new WndArticle(article));
            }
        }

        private class WndArticle : WndTitledMessage
        {
            public WndArticle(NewsArticle article)
                : base(News.ParseArticleIcon(article), article.title, article.summary)
            {
                var link = new ActionRedButton(Messages.Get(typeof(NewsScene), "read_more"));
                link.action = () =>
                {
                    //base.onClick(); do nothing
                    string str = article.URL;
                    //tracking codes, so that the website knows where this pageview came from
                    str += "?utm_source=shatteredpd";
                    str += "&utm_medium=news_page";
                    str += "&utm_campaign=ingame_link";
                    DeviceCompat.OpenURI(str);
                };

                link.SetRect(0, height + 2, width, BTN_HEIGHT);
                Add(link);
                Resize(width, (int)link.Bottom());
            }
        }
    }
}