using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.weapon;
using spdd.items.armor;
using spdd.items.wands;
using spdd.effects;
using spdd.effects.particles;
using spdd.utils;
using spdd.sprites;
using spdd.windows;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfRemoveCurse : InventoryScroll
    {
        public ScrollOfRemoveCurse()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_REMCURSE;
            mode = WndBag.Mode.UNCURSABLE;
        }

        public override void OnItemSelected(Item item)
        {
            new Flare(6, 32).Show(curUser.sprite, 2f);

            bool procced = Uncurse(curUser, item);

            actors.buffs.Degrade.Detach<Degrade>(curUser);

            if (procced)
                GLog.Positive(Messages.Get(this, "cleansed"));
            else
                GLog.Information(Messages.Get(this, "not_cleansed"));
        }

        public static bool Uncurse(Hero hero, params Item[] items)
        {
            bool procced = false;
            foreach (Item item in items)
            {
                if (item != null)
                {
                    item.cursedKnown = true;
                    if (item.cursed)
                    {
                        procced = true;
                        item.cursed = false;
                    }
                }
                if (item is Weapon)
                {
                    Weapon w = (Weapon)item;
                    if (w.HasCurseEnchant())
                    {
                        w.Enchant(null);
                        procced = true;
                    }
                }
                if (item is Armor)
                {
                    Armor a = (Armor)item;
                    if (a.HasCurseGlyph())
                    {
                        a.Inscribe(null);
                        procced = true;
                    }
                }
                if (item is Wand)
                {
                    ((Wand)item).UpdateLevel();
                }
            }

            if (procced && hero != null)
            {
                hero.sprite.Emitter().Start(ShadowParticle.Up, 0.05f, 10);
                hero.UpdateHT(false); //for ring of might
                UpdateQuickslot();
            }

            return procced;
        }

        public static bool Uncursable(Item item)
        {
            if (item.IsEquipped(Dungeon.hero) && Dungeon.hero.FindBuff<Degrade>() != null)
            {
                return true;
            }

            if ((item is EquipableItem || item is Wand) && (!item.IsIdentified() || item.cursed))
            {
                return true;
            }
            else if (item is Weapon)
            {
                return ((Weapon)item).HasCurseEnchant();
            }
            else if (item is Armor)
            {
                return ((Armor)item).HasCurseGlyph();
            }
            else if (item.GetLevel() != item.BuffedLvl())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int Value()
        {
            return IsKnown() ? 30 * quantity : base.Value();
        }
    }
}