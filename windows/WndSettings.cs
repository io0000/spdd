using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using spdd.messages;
using spdd.scenes;
using spdd.services.news;
using spdd.services.updates;
using spdd.sprites;
using spdd.ui;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.ui;
using watabou.utils;

//using Languages = spdd.messages.Languages;

namespace spdd.windows
{
    public class WndSettings : WndTabbed
    {
        private const int WIDTH_P = 122;
        private const int WIDTH_L = 223;

        private const int SLIDER_HEIGHT = 24;
        private const int BTN_HEIGHT = 18;
        private const float GAP = 2;

        private DisplayTab display;
        private UITab ui;
        private DataTab data;
        private AudioTab audio;
        private LangsTab langs;

        public static int last_index = 0;

        public WndSettings()
        {
            float height;

            int width = PixelScene.Landscape() ? WIDTH_L : WIDTH_P;

            display = new DisplayTab();
            display.SetSize(width, 0);
            height = display.Height();
            Add(display);

            var iconTabDisplay = new IconTabEx(this, Icons.DISPLAY.Get());
            iconTabDisplay.action = (value) =>
            {
                display.visible = display.active = value;
                if (value)
                    last_index = 0;
            };
            Add(iconTabDisplay);

            ui = new UITab();
            ui.SetSize(width, 0);
            height = Math.Max(height, ui.Height());
            Add(ui);

            var iconTabPrefs = new IconTabEx(this, Icons.PREFS.Get());
            iconTabPrefs.action = (value) =>
            {
                ui.visible = ui.active = value;
                if (value)
                    last_index = 1;
            };
            Add(iconTabPrefs);

            data = new DataTab();
            data.SetSize(width, 0);
            height = Math.Max(height, data.Height());
            Add(data);

            var iconTabData = new IconTabEx(this, Icons.DATA.Get());
            iconTabData.action = (value) =>
            {
                data.visible = data.active = value;
                if (value)
                    last_index = 2;
            };
            Add(iconTabData);

            audio = new AudioTab();
            audio.SetSize(width, 0);
            height = Math.Max(height, audio.Height());
            Add(audio);

            var iconTabAudio = new IconTabEx(this, Icons.AUDIO.Get());
            iconTabAudio.action = (value) =>
            {
                audio.visible = audio.active = value;
                if (value)
                    last_index = 3;
            };
            Add(iconTabAudio);

            langs = new LangsTab();
            langs.SetSize(width, 0);
            height = Math.Max(height, langs.Height());
            Add(langs);

            var iconTabLangs = new IconTabLangs(this, Icons.LANGS.Get());
            iconTabLangs.action = (value) =>
            {
                langs.visible = langs.active = value;
                if (value)
                    last_index = 4;
            };
            Add(iconTabLangs);

            Resize(width, (int)Math.Ceiling(height));

            LayoutTabs();

            Select(last_index);
        }

        public class IconTabEx : IconTab
        {
            internal Action<bool> action;

            public IconTabEx(WndTabbed owner, Image icon)
                : base(owner, icon)
            { }

            public override void Select(bool value)
            {
                base.Select(value);
                action(value);
            }
        }

        public class IconTabLangs : IconTabEx
        {
            public IconTabLangs(WndTabbed owner, Image icon)
                : base(owner, icon)
            { }

            protected override void CreateChildren()
            {
                base.CreateChildren();
                switch (Messages.Lang().GetStatus())
                {
                    case Languages.Status.INCOMPLETE:
                        icon.Hardlight(1.5f, 0, 0);
                        break;
                    case Languages.Status.UNREVIEWED:
                        icon.Hardlight(1.5f, 0.75f, 0f);
                        break;
                }
            }
        }

        public override void Hide()
        {
            base.Hide();

            var callback = new ActionSceneChangeCallback();
            callback.before = () => Game.platform.ResetGenerators();

            //resets generators because there's no need to retain chars for languages not selected
            ShatteredPixelDungeonDash.SeamlessResetScene(callback);
        }

        private class ActionSceneChangeCallback : Game.ISceneChangeCallback
        {
            internal Action before = null;
            internal Action after = null;

            public void BeforeCreate()
            {
                if (before != null)
                    before();
            }
            public void AfterCreate()
            {
                if (after != null)
                    after();
            }
        }

        private class ActionOptionSlider : OptionSlider
        {
            public Action action;
            public ActionOptionSlider(string title, string minTxt, string maxTxt, int minVal, int maxVal)
                : base(title, minTxt, maxTxt, minVal, maxVal)
            { }

            protected override void OnChange()
            {
                action();
            }
        }

        private class ActionCheckBox : CheckBox
        {
            public Action action;

            public ActionCheckBox(string label)
                : base(label)
            { }

            protected override void OnClick()
            {
                base.OnClick();
                action();
            }
        }

        private class DisplayTab : Component
        {
            RenderedTextBlock title;
            ColorBlock sep1;
            ActionOptionSlider optScale;
            ActionCheckBox chkSaver;
            ActionRedButton btnOrientation;
            ColorBlock sep2;
            ActionOptionSlider optBrightness;
            ActionOptionSlider optVisGrid;

            protected override void CreateChildren()
            {
                title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
                title.Hardlight(TITLE_COLOR);
                Add(title);

                sep1 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep1);

                if ((int)Math.Ceiling(2 * Game.density) < PixelScene.maxDefaultZoom)
                {
                    optScale = new ActionOptionSlider(Messages.Get(this, "scale"),
                            (int)Math.Ceiling(2 * Game.density) + "X",
                            PixelScene.maxDefaultZoom + "X",
                            (int)Math.Ceiling(2 * Game.density),
                            PixelScene.maxDefaultZoom);
                    optScale.action = () =>
                    {
                        if (optScale.GetSelectedValue() != SPDSettings.Scale())
                        {
                            SPDSettings.Scale(optScale.GetSelectedValue());
                            ShatteredPixelDungeonDash.SeamlessResetScene();
                        }
                    };

                    optScale.SetSelectedValue(PixelScene.defaultZoom);
                    Add(optScale);
                }

                if (!DeviceCompat.IsDesktop() && PixelScene.maxScreenZoom >= 2)
                {
                    chkSaver = new ActionCheckBox(Messages.Get(this, "saver"));
                    chkSaver.action = () =>
                    {
                        if (chkSaver.Checked())
                        {
                            chkSaver.Checked(!chkSaver.Checked());

                            var wnd = new WndOptions(
                                    Messages.Get(typeof(DisplayTab), "saver"),
                                    Messages.Get(typeof(DisplayTab), "saver_desc"),
                                    Messages.Get(typeof(DisplayTab), "okay"),
                                    Messages.Get(typeof(DisplayTab), "cancel"));
                            wnd.selectAction = (index) =>
                            {
                                if (index == 0)
                                {
                                    chkSaver.Checked(!chkSaver.Checked());
                                    SPDSettings.PowerSaver(chkSaver.Checked());
                                }
                            };
                            ShatteredPixelDungeonDash.Scene().Add(wnd);
                        }
                        else
                        {
                            SPDSettings.PowerSaver(chkSaver.Checked());
                        }
                    };

                    chkSaver.Checked(SPDSettings.PowerSaver());
                    Add(chkSaver);
                }

                if (!DeviceCompat.IsDesktop())
                {
                    btnOrientation = new ActionRedButton(PixelScene.Landscape() ? Messages.Get(this, "portrait") : Messages.Get(this, "landscape"));
                    btnOrientation.action = () => SPDSettings.Landscape(!PixelScene.Landscape());
                    Add(btnOrientation);
                }

                sep2 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep2);

                optBrightness = new ActionOptionSlider(Messages.Get(this, "brightness"),
                        Messages.Get(this, "dark"), Messages.Get(this, "bright"), -1, 1);

                optBrightness.action = () => SPDSettings.Brightness(optBrightness.GetSelectedValue());
                optBrightness.SetSelectedValue(SPDSettings.Brightness());
                Add(optBrightness);

                optVisGrid = new ActionOptionSlider(Messages.Get(this, "visual_grid"),
                        Messages.Get(this, "off"), Messages.Get(this, "high"), -1, 2);
                optVisGrid.action = () => SPDSettings.VisualGrid(optVisGrid.GetSelectedValue());
                optVisGrid.SetSelectedValue(SPDSettings.VisualGrid());
                Add(optVisGrid);
            }

            protected override void Layout()
            {
                float bottom = y;

                title.SetPos((width - title.Width()) / 2, bottom + GAP);
                sep1.Size(width, 1);
                sep1.y = title.Bottom() + 2 * GAP;

                bottom = sep1.y + 1;

                if (optScale != null)
                {
                    optScale.SetRect(0, bottom + GAP, width, SLIDER_HEIGHT);
                    bottom = optScale.Bottom();
                }

                if (width > 200 && chkSaver != null && btnOrientation != null)
                {
                    chkSaver.SetRect(0, bottom + GAP, width / 2 - 1, BTN_HEIGHT);
                    btnOrientation.SetRect(chkSaver.Right() + GAP, bottom + GAP, width / 2 - 1, BTN_HEIGHT);
                    bottom = btnOrientation.Bottom();
                }
                else
                {
                    if (chkSaver != null)
                    {
                        chkSaver.SetRect(0, bottom + GAP, width, BTN_HEIGHT);
                        bottom = chkSaver.Bottom();
                    }

                    if (btnOrientation != null)
                    {
                        btnOrientation.SetRect(0, bottom + GAP, width, BTN_HEIGHT);
                        bottom = btnOrientation.Bottom();
                    }
                }

                sep2.Size(width, 1);
                sep2.y = bottom + GAP;
                bottom = sep2.y + 1;

                if (width > 200)
                {
                    optBrightness.SetRect(0, bottom + GAP, width / 2 - GAP / 2, SLIDER_HEIGHT);
                    optVisGrid.SetRect(optBrightness.Right() + GAP, optBrightness.Top(), width / 2 - GAP / 2, SLIDER_HEIGHT);
                }
                else
                {
                    optBrightness.SetRect(0, bottom + GAP, width, SLIDER_HEIGHT);
                    optVisGrid.SetRect(0, optBrightness.Bottom() + GAP, width, SLIDER_HEIGHT);
                }

                height = optVisGrid.Bottom();
            }
        }

        private class UITab : Component
        {
            RenderedTextBlock title;
            ColorBlock sep1;
            RenderedTextBlock barDesc;
            ActionRedButton btnSplit; ActionRedButton btnGrouped; ActionRedButton btnCentered;
            ActionCheckBox chkFlipToolbar;
            ActionCheckBox chkFlipTags;
            ColorBlock sep2;
            ActionCheckBox chkFullscreen;
            ActionCheckBox chkFont;
            ColorBlock sep3;
            ActionRedButton btnKeyBindings;

            protected override void CreateChildren()
            {
                title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
                title.Hardlight(TITLE_COLOR);
                Add(title);

                sep1 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep1);

                barDesc = PixelScene.RenderTextBlock(Messages.Get(this, "mode"), 9);
                Add(barDesc);


                btnSplit = new ActionRedButton(Messages.Get(this, "split"));
                btnSplit.action = () =>
                {
                    btnSplit.TextColor(TITLE_COLOR);
                    btnGrouped.TextColor(WHITE);
                    btnCentered.TextColor(WHITE);
                    SPDSettings.ToolbarMode(Toolbar.Mode.SPLIT.ToString());
                    Toolbar.UpdateLayout();
                };
                if (SPDSettings.ToolbarMode().Equals(Toolbar.Mode.SPLIT.ToString()))
                    btnSplit.TextColor(TITLE_COLOR);
                Add(btnSplit);


                btnGrouped = new ActionRedButton(Messages.Get(this, "group"));
                btnGrouped.action = () =>
                {
                    btnSplit.TextColor(WHITE);
                    btnGrouped.TextColor(TITLE_COLOR);
                    btnCentered.TextColor(WHITE);
                    SPDSettings.ToolbarMode(Toolbar.Mode.GROUP.ToString());
                    Toolbar.UpdateLayout();
                };
                if (SPDSettings.ToolbarMode().Equals(Toolbar.Mode.GROUP.ToString()))
                    btnGrouped.TextColor(TITLE_COLOR);
                Add(btnGrouped);

                btnCentered = new ActionRedButton(Messages.Get(this, "center"));
                btnCentered.action = () =>
                {
                    btnSplit.TextColor(WHITE);
                    btnGrouped.TextColor(WHITE);
                    btnCentered.TextColor(TITLE_COLOR);
                    SPDSettings.ToolbarMode(Toolbar.Mode.CENTER.ToString());
                    Toolbar.UpdateLayout();
                };

                if (SPDSettings.ToolbarMode().Equals(Toolbar.Mode.CENTER.ToString()))
                    btnCentered.TextColor(TITLE_COLOR);
                Add(btnCentered);

                chkFlipToolbar = new ActionCheckBox(Messages.Get(this, "flip_toolbar"));
                chkFlipToolbar.action = () =>
                {
                    SPDSettings.FlipToolbar(chkFlipToolbar.Checked());
                    Toolbar.UpdateLayout();
                };
                chkFlipToolbar.Checked(SPDSettings.FlipToolbar());
                Add(chkFlipToolbar);

                chkFlipTags = new ActionCheckBox(Messages.Get(this, "flip_indicators"));
                chkFlipTags.action = () =>
                {
                    SPDSettings.FlipTags(chkFlipTags.Checked());
                    GameScene.LayoutTags();
                };

                chkFlipTags.Checked(SPDSettings.FlipTags());
                Add(chkFlipTags);

                sep2 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep2);

                chkFullscreen = new ActionCheckBox(Messages.Get(this, "fullscreen"));
                chkFullscreen.action = () =>
                {
                    SPDSettings.Fullscreen(chkFullscreen.Checked());
                };

                chkFullscreen.Checked(SPDSettings.Fullscreen());
                chkFullscreen.Enable(DeviceCompat.SupportsFullScreen());
                Add(chkFullscreen);

                chkFont = new ActionCheckBox(Messages.Get(this, "system_font"));
                chkFont.action = () =>
                {
                    var callback = new ActionSceneChangeCallback();
                    callback.before = () => SPDSettings.SystemFont(chkFont.Checked());

                    ShatteredPixelDungeonDash.SeamlessResetScene(callback);
                };

                chkFont.Checked(SPDSettings.SystemFont());
                Add(chkFont);

                if (DeviceCompat.HasHardKeyboard())
                {
                    sep3 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                    Add(sep3);

                    btnKeyBindings = new ActionRedButton(Messages.Get(this, "key_bindings"));
                    btnKeyBindings.action = () => ShatteredPixelDungeonDash.Scene().AddToFront(new WndKeyBindings());

                    Add(btnKeyBindings);
                }
            }

            protected override void Layout()
            {
                title.SetPos((width - title.Width()) / 2, y + GAP);
                sep1.Size(width, 1);
                sep1.y = title.Bottom() + 2 * GAP;

                barDesc.SetPos((width - barDesc.Width()) / 2f, sep1.y + 1 + GAP);
                PixelScene.Align(barDesc);

                int btnWidth = (int)(width - 2 * GAP) / 3;
                btnSplit.SetRect(0, barDesc.Bottom() + GAP, btnWidth, 16);
                btnGrouped.SetRect(btnSplit.Right() + GAP, btnSplit.Top(), btnWidth, 16);
                btnCentered.SetRect(btnGrouped.Right() + GAP, btnSplit.Top(), btnWidth, 16);

                if (width > 200)
                {
                    chkFlipToolbar.SetRect(0, btnGrouped.Bottom() + GAP, width / 2 - 1, BTN_HEIGHT);
                    chkFlipTags.SetRect(chkFlipToolbar.Right() + GAP, chkFlipToolbar.Top(), width / 2 - 1, BTN_HEIGHT);
                    sep2.Size(width, 1);
                    sep2.y = chkFlipTags.Bottom() + 2;
                    chkFullscreen.SetRect(0, sep2.y + 1 + GAP, width / 2 - 1, BTN_HEIGHT);
                    chkFont.SetRect(chkFullscreen.Right() + GAP, chkFullscreen.Top(), width / 2 - 1, BTN_HEIGHT);
                }
                else
                {
                    chkFlipToolbar.SetRect(0, btnGrouped.Bottom() + GAP, width, BTN_HEIGHT);
                    chkFlipTags.SetRect(0, chkFlipToolbar.Bottom() + GAP, width, BTN_HEIGHT);
                    sep2.Size(width, 1);
                    sep2.y = chkFlipTags.Bottom() + 2;
                    chkFullscreen.SetRect(0, sep2.y + 1 + GAP, width, BTN_HEIGHT);
                    chkFont.SetRect(0, chkFullscreen.Bottom() + GAP, width, BTN_HEIGHT);
                }

                if (btnKeyBindings != null)
                {
                    sep3.Size(width, 1);
                    sep3.y = chkFont.Bottom() + 2;
                    btnKeyBindings.SetRect(0, sep3.y + 1 + GAP, width, BTN_HEIGHT);
                    height = btnKeyBindings.Bottom();
                }
                else
                {
                    height = chkFont.Bottom();
                }
            }
        }

        private class DataTab : Component
        {
            RenderedTextBlock title;
            ColorBlock sep1;
            ActionCheckBox chkNews;
            ActionCheckBox chkUpdates;
            ActionCheckBox chkWifi;

            protected override void CreateChildren()
            {
                title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
                title.Hardlight(TITLE_COLOR);
                Add(title);

                sep1 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep1);

                chkNews = new ActionCheckBox(Messages.Get(this, "news"));
                chkNews.action = () =>
                {
                    SPDSettings.News(chkNews.Checked());
                    News.ClearArticles();
                };

                chkNews.Checked(SPDSettings.News());
                Add(chkNews);

                chkUpdates = new ActionCheckBox(Messages.Get(this, "updates"));
                chkUpdates.action = () =>
                {
                    SPDSettings.Updates(chkUpdates.Checked());
                    Updates.ClearUpdate();
                };

                chkUpdates.Checked(SPDSettings.Updates());
                Add(chkUpdates);

                if (!DeviceCompat.IsDesktop())
                {
                    chkWifi = new ActionCheckBox(Messages.Get(this, "wifi"));
                    chkWifi.action = () =>
                    {
                        SPDSettings.WiFi(chkWifi.Checked());
                    };
                    chkWifi.Checked(SPDSettings.WiFi());
                    Add(chkWifi);
                }
            }

            protected override void Layout()
            {
                title.SetPos((width - title.Width()) / 2, y + GAP);
                sep1.Size(width, 1);
                sep1.y = title.Bottom() + 2 * GAP;

                if (width > 200)
                {
                    chkNews.SetRect(0, sep1.y + 1 + GAP, width / 2 - 1, BTN_HEIGHT);
                    chkUpdates.SetRect(chkNews.Right() + GAP, chkNews.Top(), width / 2 - 1, BTN_HEIGHT);
                }
                else
                {
                    chkNews.SetRect(0, sep1.y + 1 + GAP, width, BTN_HEIGHT);
                    chkUpdates.SetRect(0, chkNews.Bottom() + GAP, width, BTN_HEIGHT);
                }

                float pos = chkUpdates.Bottom();
                if (chkWifi != null)
                {
                    chkWifi.SetRect(0, pos + GAP, width, BTN_HEIGHT);
                    pos = chkWifi.Bottom();
                }

                height = pos;
            }
        }

        private class AudioTab : Component
        {
            RenderedTextBlock title;
            ColorBlock sep1;
            ActionOptionSlider optMusic;
            ActionCheckBox chkMusicMute;
            ColorBlock sep2;
            ActionOptionSlider optSFX;
            ActionCheckBox chkMuteSFX;

            protected override void CreateChildren()
            {
                title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
                title.Hardlight(TITLE_COLOR);
                Add(title);

                sep1 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep1);

                optMusic = new ActionOptionSlider(Messages.Get(this, "music_vol"), "0", "10", 0, 10);
                optMusic.action = () =>
                {
                    SPDSettings.MusicVol(optMusic.GetSelectedValue());
                };

                optMusic.SetSelectedValue(SPDSettings.MusicVol());
                Add(optMusic);

                chkMusicMute = new ActionCheckBox(Messages.Get(this, "music_mute"));
                chkMusicMute.action = () =>
                {
                    SPDSettings.Music(!chkMusicMute.Checked());
                };

                chkMusicMute.Checked(!SPDSettings.Music());
                Add(chkMusicMute);

                sep2 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep2);

                optSFX = new ActionOptionSlider(Messages.Get(this, "sfx_vol"), "0", "10", 0, 10);
                optSFX.action = () =>
                {
                    SPDSettings.SFXVol(optSFX.GetSelectedValue());
                    if (Rnd.Int(100) == 0)
                    {
                        Sample.Instance.Play(Assets.Sounds.MIMIC);
                    }
                    else
                    {
                        Sample.Instance.Play(Rnd.OneOf(Assets.Sounds.GOLD,
                                Assets.Sounds.HIT,
                                Assets.Sounds.ITEM,
                                Assets.Sounds.SHATTER,
                                Assets.Sounds.EVOKE,
                                Assets.Sounds.SECRET));
                    }
                };
                optSFX.SetSelectedValue(SPDSettings.SFXVol());
                Add(optSFX);

                chkMuteSFX = new ActionCheckBox(Messages.Get(this, "sfx_mute"));
                chkMuteSFX.action = () =>
                {
                    SPDSettings.SoundFx(!chkMuteSFX.Checked());
                    Sample.Instance.Play(Assets.Sounds.CLICK);
                };

                chkMuteSFX.Checked(!SPDSettings.SoundFx());
                Add(chkMuteSFX);
            }

            protected override void Layout()
            {
                title.SetPos((width - title.Width()) / 2, y + GAP);
                sep1.Size(width, 1);
                sep1.y = title.Bottom() + 2 * GAP;

                optMusic.SetRect(0, sep1.y + 1 + GAP, width, SLIDER_HEIGHT);
                chkMusicMute.SetRect(0, optMusic.Bottom() + GAP, width, BTN_HEIGHT);

                sep2.Size(width, 1);
                sep2.y = chkMusicMute.Bottom() + GAP;

                optSFX.SetRect(0, sep2.y + 1 + GAP, width, SLIDER_HEIGHT);
                chkMuteSFX.SetRect(0, optSFX.Bottom() + GAP, width, BTN_HEIGHT);

                height = chkMuteSFX.Bottom();
            }
        }

        private class LangsTab : Component
        {
            const int COLS_P = 3;
            const int COLS_L = 4;

            const int BTN_HEIGHT = 11;

            RenderedTextBlock title;
            ColorBlock sep1;
            RenderedTextBlock txtLangName;
            RenderedTextBlock txtLangInfo;
            ColorBlock sep2;
            ActionRedButton[] lanBtns;
            ColorBlock sep3;
            RenderedTextBlock txtTranifex;
            ActionRedButton btnCredits;

            protected override void CreateChildren()
            {
                title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
                title.Hardlight(TITLE_COLOR);
                Add(title);

                sep1 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep1);

                var langs = Languages.Values().ToList();

                Languages nativeLang = Languages.MatchLocale(CultureInfo.CurrentCulture);
                langs.Remove(nativeLang);
                //move the native language to the top.
                langs.Insert(0, nativeLang);

                Languages currLang = Messages.Lang();

                txtLangName = PixelScene.RenderTextBlock(Messages.TitleCase(currLang.NativeName()), 9);
                if (currLang.GetStatus() == Languages.Status.REVIEWED) 
                    txtLangName.Hardlight(TITLE_COLOR);
                else if (currLang.GetStatus() == Languages.Status.UNREVIEWED) 
                    txtLangName.Hardlight(CharSprite.WARNING);
                else if (currLang.GetStatus() == Languages.Status.INCOMPLETE) 
                    txtLangName.Hardlight(CharSprite.NEGATIVE);
                Add(txtLangName);

                txtLangInfo = PixelScene.RenderTextBlock(6);
                
                if (currLang == Languages.ENGLISH) 
                    txtLangInfo.Text("This is the source language, written by the developer.");
                else if (currLang.GetStatus() == Languages.Status.REVIEWED) 
                    txtLangInfo.Text(Messages.Get(this, "completed"));
                else if (currLang.GetStatus() == Languages.Status.UNREVIEWED) 
                    txtLangInfo.Text(Messages.Get(this, "unreviewed"));
                else if (currLang.GetStatus() == Languages.Status.INCOMPLETE) 
                    txtLangInfo.Text(Messages.Get(this, "unfinished"));

                if (currLang.GetStatus() == Languages.Status.UNREVIEWED) 
                    txtLangInfo.SetHightlighting(true, CharSprite.WARNING);
                else if (currLang.GetStatus() == Languages.Status.INCOMPLETE) 
                    txtLangInfo.SetHightlighting(true, CharSprite.NEGATIVE);
                Add(txtLangInfo);

                sep2 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep2);

                lanBtns = new ActionRedButton[langs.Count];
                for (int i = 0; i < langs.Count; ++i)
                {
                    int langIndex = i;

                    var btn = new ActionRedButton(Messages.TitleCase(langs[i].NativeName()), 8);
                    btn.action = () =>
                    {
                        Messages.Setup(langs[langIndex]);

                        var callback = new ActionSceneChangeCallback();
                        callback.before = () =>
                        {
                            SPDSettings.Language(langs[langIndex]);
                            GameLog.Wipe();
                            Game.platform.ResetGenerators();
                        };

                        ShatteredPixelDungeonDash.SeamlessResetScene(callback);
                    };

                    if (currLang == langs[i])
                    {
                        btn.TextColor(TITLE_COLOR);
                    }
                    else
                    {
                        switch (langs[i].GetStatus())
                        {
                            case Languages.Status.INCOMPLETE:
                                btn.TextColor(new Color(0x88, 0x88, 0x88, 0xFF));
                                break;
                            case Languages.Status.UNREVIEWED:
                                btn.TextColor(new Color(0xBB, 0xBB, 0xBB, 0xFF));
                                break;
                        }
                    }
                    lanBtns[i] = btn;
                    Add(btn);
                }

                sep3 = new ColorBlock(1, 1, new Color(0x00, 0x00, 0x00, 0xFF));
                Add(sep3);

                txtTranifex = PixelScene.RenderTextBlock(6);
                txtTranifex.Text(Messages.Get(this, "transifex"));
                Add(txtTranifex);

                if (currLang != Languages.ENGLISH)
                {
                    string credText = Messages.TitleCase(Messages.Get(this, "credits"));

                    btnCredits = new ActionRedButton(credText, credText.Length > 9 ? 6 : 9);
                    btnCredits.action = () =>
                    {
                        string creds = "";
                        string creds2 = "";
                        string[] reviewers = currLang.Reviewers();
                        string[] translators = currLang.Translators();

                        List<string> total = new List<string>();
                        total.AddRange(reviewers.ToList());
                        total.AddRange(translators.ToList());
                        int translatorIdx = reviewers.Length;

                        //we have 2 columns in wide mode
                        bool wide = (2 * reviewers.Length + translators.Length) > (PixelScene.Landscape() ? 15 : 30);

                        int i;
                        if (reviewers.Length > 0)
                        {
                            creds += Messages.TitleCase(Messages.Get(typeof(LangsTab), "reviewers"));
                            creds2 += "";
                            bool col2 = false;

                            for (i = 0; i < total.Count; ++i)
                            {
                                if (i == translatorIdx)
                                {
                                    creds += "\n\n" + Messages.TitleCase(Messages.Get(typeof(LangsTab), "translators"));
                                    creds2 += "\n\n";
                                    if (col2) creds2 += "\n";
                                    col2 = false;
                                }
                                if (wide && col2)
                                {
                                    creds2 += "\n-" + total[i];
                                }
                                else
                                {
                                    creds += "\n-" + total[i];
                                }
                                col2 = !col2 && wide;
                            }
                        }

                        Window credits = new Window(0, 0, 0, Chrome.Get(Chrome.Type.TOAST));

                        int w = wide ? 125 : 60;

                        RenderedTextBlock title = PixelScene.RenderTextBlock(6);
                        title.Text(Messages.TitleCase(Messages.Get(typeof(LangsTab), "credits")), w);
                        title.Hardlight(SHPX_COLOR);
                        title.SetPos((w - title.Width()) / 2, 0);
                        credits.Add(title);

                        RenderedTextBlock text = PixelScene.RenderTextBlock(5);
                        text.SetHightlighting(false);
                        text.Text(creds, 65);
                        text.SetPos(0, title.Bottom() + 2);
                        credits.Add(text);

                        if (wide)
                        {
                            RenderedTextBlock rightColumn = PixelScene.RenderTextBlock(5);
                            rightColumn.SetHightlighting(false);
                            rightColumn.Text(creds2, 65);
                            rightColumn.SetPos(65, title.Bottom() + 6);
                            credits.Add(rightColumn);
                        }

                        credits.Resize(w, (int)text.Bottom() + 2);
                        ShatteredPixelDungeonDash.Scene().AddToFront(credits);
                    };

                    Add(btnCredits);
                }
            }

            protected override void Layout()
            {
                title.SetPos((width - title.Width()) / 2, y + GAP);
                sep1.Size(width, 1);
                sep1.y = title.Bottom() + 2 * GAP;

                txtLangName.SetPos((width - txtLangName.Width()) / 2f, sep1.y + 1 + GAP);
                PixelScene.Align(txtLangName);

                txtLangInfo.SetPos(0, txtLangName.Bottom() + 2 * GAP);
                txtLangInfo.MaxWidth((int)width);

                y = txtLangInfo.Bottom() + GAP;
                int x = 0;

                sep2.Size(width, 1);
                sep2.y = y;
                y += 2;

                int cols = PixelScene.Landscape() ? COLS_L : COLS_P;
                int btnWidth = (int)Math.Floor((width - (cols - 1)) / cols);
                foreach (RedButton btn in lanBtns)
                {
                    btn.SetRect(x, y, btnWidth, BTN_HEIGHT);
                    btn.SetPos(x, y);
                    x += btnWidth + 1;
                    if (x + btnWidth > width)
                    {
                        x = 0;
                        y += BTN_HEIGHT + 1;
                    }
                }
                if (x > 0)
                {
                    y += BTN_HEIGHT + 1;
                }

                sep3.Size(width, 1);
                sep3.y = y;
                y += 2;

                if (btnCredits != null)
                {
                    btnCredits.SetSize(btnCredits.ReqWidth() + 2, 16);
                    btnCredits.SetPos(width - btnCredits.Width(), y);

                    txtTranifex.SetPos(0, y);
                    txtTranifex.MaxWidth((int)btnCredits.Left());

                    height = Math.Max(btnCredits.Bottom(), txtTranifex.Bottom());
                }
                else
                {
                    txtTranifex.SetPos(0, y);
                    txtTranifex.MaxWidth((int)width);

                    height = txtTranifex.Bottom();
                }
            }
        }
    }
}