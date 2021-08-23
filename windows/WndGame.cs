using System.IO;
using watabou.noosa;
using spdd.messages;
using spdd.scenes;
using spdd.services.updates;
using spdd.ui;

namespace spdd.windows
{
    public class WndGame : Window
    {
        private const int WIDTH = 120;
        private const int BTN_HEIGHT = 20;
        private const int GAP = 2;

        private int pos;

        public WndGame()
        {
            ActionRedButton curBtn;
            AddButton(curBtn = new ActionRedButton(Messages.Get(this, "settings")));
            curBtn.action = () =>
            {
                Hide();
                GameScene.Show(new WndSettings());
            };

            curBtn.Icon(Icons.PREFS.Get());

            //install prompt
            if (Updates.IsInstallable())
            {
                AddButton(curBtn = new ActionRedButton(Messages.Get(this, "install")));
                curBtn.action = () => Updates.LaunchInstall();

                curBtn.TextColor(Window.SHPX_COLOR);
                curBtn.Icon(Icons.CHANGES.Get());
            }

            // Challenges window
            if (Dungeon.challenges > 0)
            {
                AddButton(curBtn = new ActionRedButton(Messages.Get(this, "challenges")));
                curBtn.action = () =>
                {
                    Hide();
                    GameScene.Show(new WndChallenges(Dungeon.challenges, false));
                };
                curBtn.Icon(Icons.CHALLENGE_ON.Get());
            }

            // Restart
            if (Dungeon.hero == null || !Dungeon.hero.IsAlive())
            {
                AddButton(curBtn = new ActionRedButton(Messages.Get(this, "start")));
                curBtn.action = () =>
                {
                    InterlevelScene.noStory = true;
                    GamesInProgress.selectedClass = Dungeon.hero.heroClass;
                    GamesInProgress.curSlot = GamesInProgress.FirstEmpty();
                    ShatteredPixelDungeonDash.SwitchScene(typeof(HeroSelectScene));
                };

                curBtn.Icon(Icons.ENTER.Get());
                curBtn.TextColor(Window.TITLE_COLOR);

                AddButton(curBtn = new ActionRedButton(Messages.Get(this, "rankings")));
                curBtn.action = () =>
                {
                    InterlevelScene.mode = InterlevelScene.Mode.DESCEND;
                    ShatteredPixelDungeonDash.SwitchScene(typeof(RankingsScene));
                };

                curBtn.Icon(Icons.RANKINGS.Get());
            }

            var btn1 = new ActionRedButton(Messages.Get(this, "menu"));
            btn1.action = () =>
            {
                try
                {
                    Dungeon.SaveAll();
                }
                catch (IOException e)
                {
                    ShatteredPixelDungeonDash.ReportException(e);
                }
                Game.SwitchScene(typeof(TitleScene));
            };

            var btn2 = new ActionRedButton(Messages.Get(this, "exit"));
            btn2.action = () =>
            {
                try
                {
                    Dungeon.SaveAll();
                }
                catch (IOException e)
                {
                    ShatteredPixelDungeonDash.ReportException(e);
                }
                Game.instance.Finish();
            };

            AddButtons(
                // Main menu
                btn1,
                // Quit
                btn2
            );

            // Cancel
            var btn3 = new ActionRedButton(Messages.Get(this, "return"));
            btn3.action = () => Hide();
            AddButton(btn3);

            Resize(WIDTH, pos);
        }

        private void AddButton(RedButton btn)
        {
            Add(btn);
            btn.SetRect(0, pos > 0 ? pos += GAP : 0, WIDTH, BTN_HEIGHT);
            pos += BTN_HEIGHT;
        }

        private void AddButtons(RedButton btn1, RedButton btn2)
        {
            Add(btn1);
            btn1.SetRect(0, pos > 0 ? pos += GAP : 0, (WIDTH - GAP) / 2, BTN_HEIGHT);
            Add(btn2);
            btn2.SetRect(btn1.Right() + GAP, btn1.Top(), WIDTH - btn1.Right() - GAP, BTN_HEIGHT);
            pos += BTN_HEIGHT;
        }
    }
}