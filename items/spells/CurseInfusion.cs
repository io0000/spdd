using System;
using watabou.noosa.audio;
using spdd.items.quest;
using spdd.items.armor;
using spdd.items.wands;
using spdd.items.scrolls;
using spdd.items.weapon;
using spdd.items.weapon.melee;
using spdd.sprites;
using spdd.windows;
using spdd.effects;
using spdd.effects.particles;

namespace spdd.items.spells
{
    public class CurseInfusion : InventorySpell
    {
        public CurseInfusion()
        {
            image = ItemSpriteSheet.CURSE_INFUSE;
            mode = WndBag.Mode.CURSABLE;
        }

        protected override void OnItemSelected(Item item)
        {
            CellEmitter.Get(curUser.pos).Burst(ShadowParticle.Up, 5);
            Sample.Instance.Play(Assets.Sounds.CURSED);

            item.cursed = true;
            if (item is MeleeWeapon || item is SpiritBow)
            {
                Weapon w = (Weapon)item;
                if (w.enchantment != null)
                    w.Enchant(Weapon.Enchantment.RandomCurse(w.enchantment.GetType()));
                else
                    w.Enchant(Weapon.Enchantment.RandomCurse());

                w.curseInfusionBonus = true;
                if (w is MagesStaff)
                    ((MagesStaff)w).UpdateWand(true);
            }
            else if (item is Armor)
            {
                Armor a = (Armor)item;
                if (a.glyph != null)
                    a.Inscribe(Armor.Glyph.RandomCurse(a.glyph.GetType()));
                else
                    a.Inscribe(Armor.Glyph.RandomCurse());

                a.curseInfusionBonus = true;
            }
            else if (item is Wand)
            {
                ((Wand)item).curseInfusionBonus = true;
                ((Wand)item).UpdateLevel();
            }
            UpdateQuickslot();
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((30 + 100) / 3f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(ScrollOfRemoveCurse), typeof(MetalShard) };
                inQuantity = new int[] { 1, 1 };

                cost = 4;

                output = typeof(CurseInfusion);
                outQuantity = 3;
            }
        }
    }
}