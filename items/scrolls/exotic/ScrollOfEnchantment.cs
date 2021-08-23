using System;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.effects;
using spdd.scenes;
using spdd.windows;
using spdd.utils;
using spdd.items.armor;
using spdd.items.weapon;
using spdd.items.stones;
using spdd.messages;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfEnchantment : ExoticScroll
    {
        public ScrollOfEnchantment()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_ENCHANT;

            unique = true;

            itemSelector = new ItemSelector(this);
        }

        public override void DoRead()
        {
            SetKnown();

            GameScene.SelectItem(itemSelector, WndBag.Mode.ENCHANTABLE, Messages.Get(this, "inv_title"));
        }

        private ItemSelector itemSelector;

        class ItemSelector : WndBag.IListener
        {
            ScrollOfEnchantment scroll;
            public ItemSelector(ScrollOfEnchantment scroll)
            {
                this.scroll = scroll;
            }

            public void OnSelect(Item item)
            {
                scroll.OnSelect(item);
            }
        }

        public void OnSelect(Item item)
        {
            if (item is Weapon)
            {
                var enchants = new Weapon.Enchantment[3];

                Type existing = ((Weapon)item).enchantment != null ? ((Weapon)item).enchantment.GetType() : null;
                enchants[0] = Weapon.Enchantment.RandomCommon(existing);
                enchants[1] = Weapon.Enchantment.RandomUncommon(existing);
                enchants[2] = Weapon.Enchantment.Random(existing, enchants[0].GetType(), enchants[1].GetType());

                var type = typeof(ScrollOfEnchantment);

                var wnd = new WndOptions(Messages.TitleCase(Name()),
                        Messages.Get(type, "weapon") + "\n\n" + Messages.Get(type, "cancel_warn"),
                        enchants[0].Name(),
                        enchants[1].Name(),
                        enchants[2].Name(),
                        Messages.Get(type, "cancel"));
                wnd.selectAction = (index) =>
                {
                    if (index < 3)
                    {
                        ((Weapon)item).Enchant(enchants[index]);
                        GLog.Positive(Messages.Get(typeof(StoneOfEnchantment), "weapon"));
                        ((ScrollOfEnchantment)curItem).ReadAnimation();

                        Sample.Instance.Play(Assets.Sounds.READ);
                        Enchanting.Show(curUser, item);
                    }
                };
                wnd.skipBackPressed = true;

                GameScene.Show(wnd);
            }
            else if (item is Armor)
            {
                var glyphs = new Armor.Glyph[3];

                Type existing = ((Armor)item).glyph != null ? ((Armor)item).glyph.GetType() : null;
                glyphs[0] = Armor.Glyph.RandomCommon(existing);
                glyphs[1] = Armor.Glyph.RandomUncommon(existing);
                glyphs[2] = Armor.Glyph.Random(existing, glyphs[0].GetType(), glyphs[1].GetType());

                var type = typeof(ScrollOfEnchantment);

                var wnd = new WndOptions(Messages.TitleCase(Name()),
                        Messages.Get(type, "armor") + "\n\n" + Messages.Get(type, "cancel_warn"),
                        glyphs[0].Name(),
                        glyphs[1].Name(),
                        glyphs[2].Name(),
                        Messages.Get(type, "cancel"));
                wnd.selectAction = (index) =>
                {
                    if (index < 3)
                    {
                        ((Armor)item).Inscribe(glyphs[index]);
                        GLog.Positive(Messages.Get(typeof(StoneOfEnchantment), "armor"));
                        ((ScrollOfEnchantment)curItem).ReadAnimation();

                        Sample.Instance.Play(Assets.Sounds.READ);
                        Enchanting.Show(curUser, item);
                    }
                };
                wnd.skipBackPressed = true;

                GameScene.Show(wnd);
            }
            else
            {
                //TODO if this can ever be found un-IDed, need logic for that
                curItem.Collect();
            }
        }
    }
}
