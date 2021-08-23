using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.items.quest;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;
using spdd.utils;

namespace spdd.windows
{
    public class WndImp : Window
    {
        private const int WIDTH = 120;
        private const int BTN_HEIGHT = 20;
        private const float GAP = 2;

        public WndImp(Imp imp, DwarfToken tokens)
        {
            IconTitle titlebar = new IconTitle();
            titlebar.Icon(new ItemSprite(tokens.Image(), null));
            titlebar.Label(Messages.TitleCase(tokens.Name()));
            titlebar.SetRect(0, 0, WIDTH, 0);
            Add(titlebar);

            RenderedTextBlock message = PixelScene.RenderTextBlock(Messages.Get(this, "message"), 6);
            message.MaxWidth(WIDTH);
            message.SetPos(0, titlebar.Bottom() + GAP);
            Add(message);

            var btnReward = new ActionRedButton(Messages.Get(this, "reward"));
            btnReward.action = () => TakeReward(imp, tokens, Imp.Quest.reward);
            btnReward.SetRect(0, message.Top() + message.Height() + GAP, WIDTH, BTN_HEIGHT);
            Add(btnReward);

            Resize(WIDTH, (int)btnReward.Bottom());
        }

        private void TakeReward(Imp imp, DwarfToken tokens, Item reward)
        {
            Hide();

            tokens.DetachAll(Dungeon.hero.belongings.backpack);
            if (reward == null)
                return;

            reward.Identify();
            if (reward.DoPickUp(Dungeon.hero))
                GLog.Information(Messages.Get(Dungeon.hero, "you_now_have", reward.Name()));
            else
                Dungeon.level.Drop(reward, imp.pos).sprite.Drop();

            imp.Flee();

            Imp.Quest.Complete();
        }
    }
}