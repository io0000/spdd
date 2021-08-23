using System;
using System.IO;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.scenes;
using spdd.ui;
using spdd.sprites;
using spdd.actors.hero;
using spdd.messages;

namespace spdd.windows
{
    public class WndGameInProgress : Window
    {
        private const int WIDTH = 120;
        private const int HEIGHT = 120;

        private int GAP = 6;

        private float pos;

        public WndGameInProgress(int slot)
        {
            GamesInProgress.Info info = GamesInProgress.Check(slot);

            string className = null;
            if (info.subClass != HeroSubClass.NONE)
            {
                className = info.subClass.Title();
            }
            else
            {
                className = info.heroClass.Title();
            }

            IconTitle title = new IconTitle();
            title.Icon(HeroSprite.Avatar(info.heroClass, info.armorTier));
            title.Label(Messages.Get(this, "title", info.level, className).ToUpperInvariant());
            title.Color(Window.SHPX_COLOR);
            title.SetRect(0, 0, WIDTH, 0);
            Add(title);

            //manually produces debug information about a run, mainly useful for levelgen errors
            var debug = new WndGameInProgressButton();
            debug.action = () =>
            {
                try
                {
                    Bundle bundle = FileUtils.BundleFromFile(GamesInProgress.GameFile(slot));
                    ShatteredPixelDungeonDash.Scene().AddToFront(new WndMessage("_Debug Info:_\n\n" +
                            "Version: " + Game.version + " (" + Game.versionCode + ")\n" +
                            "Seed: " + bundle.GetLong("seed") + "\n" +
                            "Challenge Mask: " + info.challenges));
                }
                catch (IOException)
                { }
            };

            debug.SetRect(0, 0, title.imIcon.Width(), title.imIcon.height);
            Add(debug);

            if (info.challenges > 0)
                GAP -= 2;

            pos = title.Bottom() + GAP;

            if (info.challenges > 0)
            {
                var btnChallenges = new ActionRedButton(Messages.Get(this, "challenges"));
                btnChallenges.action = () => Game.Scene().Add(new WndChallenges(info.challenges, false));

                float btnW = btnChallenges.ReqWidth() + 2;
                btnChallenges.SetRect((WIDTH - btnW) / 2, pos, btnW, btnChallenges.ReqHeight() + 2);
                Add(btnChallenges);

                pos = btnChallenges.Bottom() + GAP;
            }

            pos += GAP;

            StatSlot(Messages.Get(this, "str"), info.str);
            if (info.shld > 0)
                StatSlot(Messages.Get(this, "health"), info.hp + "+" + info.shld + "/" + info.ht);
            else
                StatSlot(Messages.Get(this, "health"), (info.hp) + "/" + info.ht);
            StatSlot(Messages.Get(this, "exp"), info.exp + "/" + Hero.MaxExp(info.level));

            pos += GAP;
            StatSlot(Messages.Get(this, "gold"), info.goldCollected);
            StatSlot(Messages.Get(this, "depth"), info.maxDepth);

            pos += GAP;

            var cont = new ActionRedButton(Messages.Get(this, "continue"));
            cont.action = () =>
            {
                GamesInProgress.curSlot = slot;

                Dungeon.hero = null;
                ActionIndicator.action = null;
                InterlevelScene.mode = InterlevelScene.Mode.CONTINUE;
                ShatteredPixelDungeonDash.SwitchScene(typeof(InterlevelScene));
            };
            cont.SetRect(0, HEIGHT - 20, WIDTH / 2 - 1, 20);
            Add(cont);

            var erase = new ActionRedButton(Messages.Get(this, "erase"));
            erase.action = () =>
            {
                var wnd = new WndOptions(
                    Messages.Get(typeof(WndGameInProgress), "erase_warn_title"),
                    Messages.Get(typeof(WndGameInProgress), "erase_warn_body"),
                    Messages.Get(typeof(WndGameInProgress), "erase_warn_yes"),
                    Messages.Get(typeof(WndGameInProgress), "erase_warn_no"));
                wnd.selectAction = (index) =>
                {
                    if (index == 0)
                    {
                        FileUtils.DeleteDir(GamesInProgress.GameFolder(slot));
                        GamesInProgress.SetUnknown(slot);
                        ShatteredPixelDungeonDash.SwitchNoFade(typeof(StartScene));
                    }
                };
                ShatteredPixelDungeonDash.Scene().Add(wnd);
            };
            erase.SetRect(WIDTH / 2 + 1, HEIGHT - 20, WIDTH / 2 - 1, 20);
            Add(erase);

            Resize(WIDTH, HEIGHT);
        }

        private class WndGameInProgressButton : Button
        {
            public Action action;
            protected override bool OnLongClick()
            {
                action();
                return true;
            }
        }

        private void StatSlot(string label, string value)
        {
            RenderedTextBlock txt = PixelScene.RenderTextBlock(label, 8);
            txt.SetPos(0, pos);
            Add(txt);

            txt = PixelScene.RenderTextBlock(value, 8);
            txt.SetPos(WIDTH * 0.6f, pos);
            PixelScene.Align(txt);
            Add(txt);

            pos += GAP + txt.Height();
        }

        private void StatSlot(string label, int value)
        {
            StatSlot(label, value.ToString());
        }
    }
}