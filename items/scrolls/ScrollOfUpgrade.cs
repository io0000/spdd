using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.effects;
using spdd.utils;
using spdd.windows;
using spdd.sprites;
using spdd.effects.particles;
using spdd.items.armor;
using spdd.items.weapon;
using spdd.items.rings;
using spdd.items.wands;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfUpgrade : InventoryScroll
    {
        public ScrollOfUpgrade()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_UPGRADE;
            mode = WndBag.Mode.UPGRADEABLE;

            unique = true;
        }

        public override void OnItemSelected(Item item)
        {
            Upgrade(curUser);

            actors.buffs.Degrade.Detach<Degrade>(curUser);

            //logic for telling the user when item properties change from upgrades
            //...yes this is rather messy
            if (item is Weapon)
            {
                Weapon w = (Weapon)item;
                bool wasCursed = w.cursed;
                bool hadCursedEnchant = w.HasCurseEnchant();
                bool hadGoodEnchant = w.HasGoodEnchant();

                w.Upgrade();

                if (w.cursedKnown && hadCursedEnchant && !w.HasCurseEnchant())
                {
                    RemoveCurse(Dungeon.hero);
                }
                else if (w.cursedKnown && wasCursed && !w.cursed)
                {
                    WeakenCurse(Dungeon.hero);
                }

                if (hadGoodEnchant && !w.HasGoodEnchant())
                {
                    GLog.Warning(Messages.Get(typeof(Weapon), "incompatible"));
                }
            }
            else if (item is Armor)
            {
                Armor a = (Armor)item;
                bool wasCursed = a.cursed;
                bool hadCursedGlyph = a.HasCurseGlyph();
                bool hadGoodGlyph = a.HasGoodGlyph();

                a.Upgrade();

                if (a.cursedKnown && hadCursedGlyph && !a.HasCurseGlyph())
                {
                    RemoveCurse(Dungeon.hero);
                }
                else if (a.cursedKnown && wasCursed && !a.cursed)
                {
                    WeakenCurse(Dungeon.hero);
                }
                if (hadGoodGlyph && !a.HasGoodGlyph())
                {
                    GLog.Warning(Messages.Get(typeof(Armor), "incompatible"));
                }
            }
            else if (item is Wand || item is Ring)
            {
                bool wasCursed = item.cursed;

                item.Upgrade();

                if (wasCursed && !item.cursed)
                {
                    RemoveCurse(Dungeon.hero);
                }
            }
            else
            {
                item.Upgrade();
            }

            BadgesExtensions.ValidateItemLevelAquired(item);
            ++Statistics.upgradesUsed;
            BadgesExtensions.ValidateMageUnlock();
        }

        public static void Upgrade(Hero hero)
        {
            hero.sprite.Emitter().Start(Speck.Factory(Speck.UP), 0.2f, 3);
        }

        public static void WeakenCurse(Hero hero)
        {
            GLog.Positive(Messages.Get(typeof(ScrollOfUpgrade), "weaken_curse"));
            hero.sprite.Emitter().Start(ShadowParticle.Up, 0.05f, 5);
        }

        public static void RemoveCurse(Hero hero)
        {
            GLog.Positive(Messages.Get(typeof(ScrollOfUpgrade), "remove_curse"));
            hero.sprite.Emitter().Start(ShadowParticle.Up, 0.05f, 10);
        }

        public override int Value()
        {
            return IsKnown() ? 50 * quantity : base.Value();
        }
    }
}