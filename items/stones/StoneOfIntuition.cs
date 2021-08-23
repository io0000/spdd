using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.utils;
using spdd.sprites;
using spdd.scenes;
using spdd.effects;
using spdd.windows;
using spdd.ui;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.items.potions;
using spdd.items.potions.exotic;
using spdd.utils;
using spdd.messages;

namespace spdd.items.stones
{
    public class StoneOfIntuition : InventoryStone
    {
        public StoneOfIntuition()
        {
            mode = WndBag.Mode.UNIDED_POTION_OR_SCROLL;
            image = ItemSpriteSheet.STONE_INTUITION;
        }

        protected override void OnItemSelected(Item item)
        {
            GameScene.Show(new WndGuess(this, item));
        }

        private static Type curGuess;

        public class WndGuess : Window
        {
            private const int WIDTH = 120;
            private const int BTN_SIZE = 20;

            public WndGuess(StoneOfIntuition stone, Item item)
            {
                IconTitle titlebar = new IconTitle();
                titlebar.Icon(new ItemSprite(ItemSpriteSheet.STONE_INTUITION, null));
                titlebar.Label(Messages.TitleCase(Messages.Get(typeof(StoneOfIntuition), "name")));
                titlebar.SetRect(0, 0, WIDTH, 0);
                Add(titlebar);

                RenderedTextBlock text = PixelScene.RenderTextBlock(6);
                text.Text(Messages.Get(this, "text"));
                text.SetPos(0, titlebar.Bottom());
                text.MaxWidth(WIDTH);
                Add(text);

                var guess = new ActionRedButton("");
                guess.action = () =>
                {
                    //base.OnClick();
                    stone.UseAnimation();

                    if (item.GetType() == StoneOfIntuition.curGuess)
                    {
                        item.Identify();
                        GLog.Positive(Messages.Get(typeof(WndGuess), "correct"));
                        curUser.sprite.parent.Add(new Identification(curUser.sprite.Center().Offset(0, -16)));
                    }
                    else
                    {
                        GLog.Negative(Messages.Get(typeof(WndGuess), "incorrect"));
                    }
                    StoneOfIntuition.curGuess = null;
                    Hide();
                };

                guess.visible = false;
                guess.Icon(new ItemSprite(item));
                guess.Enable(false);
                guess.SetRect(0, 80, WIDTH, 20);
                Add(guess);

                float left;
                float top = text.Bottom() + 5;
                int rows;
                int placed = 0;

                var unIDed = new HashSet<Type>();
                Type[] all;

                if (item.IsIdentified())
                {
                    Hide();
                    return;
                }
                else if (item is Potion)
                {
                    unIDed.UnionWith(Potion.GetUnknown());
                    all = (Type[])Generator.Category.POTION.GetClasses().Clone();

                    if (item is ExoticPotion)
                    {
                        for (int i = 0; i < all.Length; ++i)
                            all[i] = ExoticPotion.regToExo.Get(all[i]);

                        var exoUID = new HashSet<Type>();
                        foreach (var i in unIDed)
                            exoUID.Add(ExoticPotion.regToExo.Get(i));

                        unIDed = exoUID;
                    }
                }
                else if (item is Scroll)
                {
                    unIDed.UnionWith(Scroll.GetUnknown());
                    all = (Type[])Generator.Category.SCROLL.GetClasses().Clone();

                    if (item is ExoticScroll)
                    {
                        for (int i = 0; i < all.Length; ++i)
                            all[i] = ExoticScroll.regToExo.Get(all[i]);

                        var exoUID = new HashSet<Type>();
                        foreach (var i in unIDed)
                            exoUID.Add(ExoticScroll.regToExo.Get(i));

                        unIDed = exoUID;
                    }
                }
                else
                {
                    Hide();
                    return;
                }

                if (unIDed.Count < 6)
                {
                    rows = 1;
                    top += BTN_SIZE / 2f;
                    left = (WIDTH - BTN_SIZE * unIDed.Count) / 2f;
                }
                else
                {
                    rows = 2;
                    left = (WIDTH - BTN_SIZE * ((unIDed.Count + 1) / 2)) / 2f;
                }

                for (int i = 0; i < all.Length; ++i)
                {
                    if (!unIDed.Contains(all[i]))
                        continue;

                    int j = i;

                    var btn = new ActionIconButton();
                    btn.action = () =>
                    {
                        curGuess = all[j];
                        guess.visible = true;
                        guess.Text(Messages.TitleCase(Messages.Get(curGuess, "name")));
                        guess.Enable(true);
                        //super.onClick();
                    };

                    Image im = new Image(Assets.Sprites.ITEM_ICONS);
                    Item newItem = (Item)Reflection.NewInstance(all[i]);
                    im.Frame(ItemSpriteSheet.Icons.film.Get(newItem.icon));
                    im.scale.Set(2f);
                    btn.Icon(im);
                    btn.SetRect(left + placed * BTN_SIZE, top, BTN_SIZE, BTN_SIZE);
                    Add(btn);

                    ++placed;
                    if (rows == 2 && placed == ((unIDed.Count + 1) / 2))
                    {
                        placed = 0;
                        if (unIDed.Count % 2 == 1)
                            left += BTN_SIZE / 2f;

                        top += BTN_SIZE;
                    }
                }

                Resize(WIDTH, 100);
            }

            public override void OnBackPressed()
            {
                base.OnBackPressed();
                new StoneOfIntuition().Collect();
            }
        }
    }
}