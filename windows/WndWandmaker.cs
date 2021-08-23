using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.items.quest;
using spdd.items.wands;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;
using spdd.utils;

namespace spdd.windows
{
    public class WndWandmaker : Window
    {
        private const int WIDTH = 120;
        private const int BTN_HEIGHT = 20;
        private const float GAP = 2;

        public WndWandmaker(Wandmaker wandmaker, Item item)
        {
            IconTitle titlebar = new IconTitle();
            titlebar.Icon(new ItemSprite(item.Image(), null));
            titlebar.Label(Messages.TitleCase(item.Name()));
            titlebar.SetRect(0, 0, WIDTH, 0);
            Add(titlebar);

            string msg = "";
            if (item is CorpseDust)
            {
                msg = Messages.Get(this, "dust");
            }
            else if (item is Embers)
            {
                msg = Messages.Get(this, "ember");
            }
            else if (item is Rotberry.Seed)
            {
                msg = Messages.Get(this, "berry");
            }

            RenderedTextBlock message = PixelScene.RenderTextBlock(msg, 6);
            message.MaxWidth(WIDTH);
            message.SetPos(0, titlebar.Bottom() + GAP);
            Add(message);

            var btnWand1 = new ActionRedButton(Messages.TitleCase(Wandmaker.Quest.wand1.Name()));
            btnWand1.action = () =>
            {
                SelectReward(wandmaker, item, Wandmaker.Quest.wand1);
            };

            btnWand1.SetRect(0, message.Top() + message.Height() + GAP, WIDTH, BTN_HEIGHT);
            Add(btnWand1);

            var btnWand2 = new ActionRedButton(Messages.TitleCase(Wandmaker.Quest.wand2.Name()));
            btnWand2.action = () =>
            {
                SelectReward(wandmaker, item, Wandmaker.Quest.wand2);
            };

            btnWand2.SetRect(0, btnWand1.Bottom() + GAP, WIDTH, BTN_HEIGHT);
            Add(btnWand2);

            Resize(WIDTH, (int)btnWand2.Bottom());
        }

        private void SelectReward(Wandmaker wandmaker, Item item, Wand reward)
        {
            if (reward == null)
                return;

            Hide();

            item.Detach(Dungeon.hero.belongings.backpack);

            reward.Identify();
            if (reward.DoPickUp(Dungeon.hero))
            {
                GLog.Information(Messages.Get(Dungeon.hero, "you_now_have", reward.Name()));
            }
            else
            {
                Dungeon.level.Drop(reward, wandmaker.pos).sprite.Drop();
            }

            wandmaker.Yell(Messages.Get(this, "farewell", Dungeon.hero.Name()));
            wandmaker.Destroy();

            wandmaker.sprite.Die();

            Wandmaker.Quest.Complete();
        }
    }
}