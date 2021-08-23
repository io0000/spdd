using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;
using spdd.utils;

namespace spdd.windows
{
    public class WndSadGhost : Window
    {
        private const int WIDTH = 120;
        private const int BTN_HEIGHT = 20;
        private const float GAP = 2;

        public WndSadGhost(Ghost ghost, int type)
        {
            IconTitle titlebar = new IconTitle();
            RenderedTextBlock message;
            switch (type)
            {
                case 1:
                default:
                    titlebar.Icon(new FetidRatSprite());
                    titlebar.Label(Messages.Get(this, "rat_title"));
                    message = PixelScene.RenderTextBlock(Messages.Get(this, "rat") + Messages.Get(this, "give_item"), 6);
                    break;
                case 2:
                    titlebar.Icon(new GnollTricksterSprite());
                    titlebar.Label(Messages.Get(this, "gnoll_title"));
                    message = PixelScene.RenderTextBlock(Messages.Get(this, "gnoll") + Messages.Get(this, "give_item"), 6);
                    break;
                case 3:
                    titlebar.Icon(new GreatCrabSprite());
                    titlebar.Label(Messages.Get(this, "crab_title"));
                    message = PixelScene.RenderTextBlock(Messages.Get(this, "crab") + Messages.Get(this, "give_item"), 6);
                    break;
            }

            titlebar.SetRect(0, 0, WIDTH, 0);
            Add(titlebar);

            message.MaxWidth(WIDTH);
            message.SetPos(0, titlebar.Bottom() + GAP);
            Add(message);

            var btnWeapon = new ActionRedButton(Messages.Get(this, "weapon"));
            btnWeapon.action = () => SelectReward(ghost, Ghost.Quest.weapon);
            btnWeapon.SetRect(0, message.Top() + message.Height() + GAP, WIDTH, BTN_HEIGHT);
            Add(btnWeapon);

            if (!Dungeon.IsChallenged(Challenges.NO_ARMOR))
            {
                var btnArmor = new ActionRedButton(Messages.Get(this, "armor"));
                btnArmor.action = () => SelectReward(ghost, Ghost.Quest.armor);

                btnArmor.SetRect(0, btnWeapon.Bottom() + GAP, WIDTH, BTN_HEIGHT);
                Add(btnArmor);

                Resize(WIDTH, (int)btnArmor.Bottom());
            }
            else
            {
                Resize(WIDTH, (int)btnWeapon.Bottom());
            }
        }

        private void SelectReward(Ghost ghost, Item reward)
        {
            Hide();

            if (reward == null)
                return;

            reward.Identify();
            if (reward.DoPickUp(Dungeon.hero))
            {
                GLog.Information(Messages.Get(Dungeon.hero, "you_now_have", reward.Name()));
            }
            else
            {
                Dungeon.level.Drop(reward, ghost.pos).sprite.Drop();
            }

            ghost.Yell(Messages.Get(this, "farewell"));
            ghost.Die(null);

            Ghost.Quest.Complete();
        }
    }
}