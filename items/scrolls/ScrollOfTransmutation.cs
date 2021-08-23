using System;
using watabou.utils;
using spdd.utils;
using spdd.sprites;
using spdd.windows;
using spdd.items.weapon;
using spdd.items.weapon.melee;
using spdd.items.weapon.missiles;
using spdd.items.weapon.missiles.darts;
using spdd.items.potions;
using spdd.items.potions.exotic;
using spdd.items.potions.elixirs;
using spdd.items.potions.brews;
using spdd.items.rings;
using spdd.items.wands;
using spdd.items.artifacts;
using spdd.items.stones;
using spdd.items.scrolls.exotic;
using spdd.plants;
using spdd.journal;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfTransmutation : InventoryScroll
    {
        public ScrollOfTransmutation()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_TRANSMUTE;
            mode = WndBag.Mode.TRANMSUTABLE;

            bones = true;
        }

        public static bool CanTransmute(Item item)
        {
            return item is MeleeWeapon ||
                    (item is MissileWeapon && !(item is Dart)) ||
                    (item is Potion && !(item is Elixir || item is Brew || item is AlchemicalCatalyst)) ||
                    item is Scroll ||
                    item is Ring ||
                    item is Wand ||
                    item is Plant.Seed ||
                    item is Runestone ||
                    item is Artifact;
        }

        public override void OnItemSelected(Item item)
        {
            Item result;

            if (item is MagesStaff)
            {
                result = ChangeStaff((MagesStaff)item);
            }
            else if (item is MeleeWeapon || item is MissileWeapon)
            {
                result = ChangeWeapon((Weapon)item);
            }
            else if (item is Scroll)
            {
                result = ChangeScroll((Scroll)item);
            }
            else if (item is Potion)
            {
                result = ChangePotion((Potion)item);
            }
            else if (item is Ring)
            {
                result = ChangeRing((Ring)item);
            }
            else if (item is Wand)
            {
                result = ChangeWand((Wand)item);
            }
            else if (item is Plant.Seed)
            {
                result = ChangeSeed((Plant.Seed)item);
            }
            else if (item is Runestone)
            {
                result = ChangeStone((Runestone)item);
            }
            else if (item is Artifact)
            {
                result = ChangeArtifact((Artifact)item);
            }
            else
            {
                result = null;
            }

            if (result == null)
            {
                //This shouldn't ever trigger
                GLog.Negative(Messages.Get(this, "nothing"));
                curItem.Collect(curUser.belongings.backpack);
            }
            else
            {
                if (item.IsEquipped(Dungeon.hero))
                {
                    item.cursed = false; //to allow it to be unequipped
                    ((EquipableItem)item).DoUnequip(Dungeon.hero, false);
                    ((EquipableItem)result).DoEquip(Dungeon.hero);
                }
                else
                {
                    item.Detach(Dungeon.hero.belongings.backpack);
                    if (!result.Collect())
                        Dungeon.level.Drop(result, curUser.pos).sprite.Drop();
                }

                if (result.IsIdentified())
                    CatalogExtensions.SetSeen(result.GetType());

                //TODO visuals
                GLog.Positive(Messages.Get(this, "morph"));
            }
        }

        private MagesStaff ChangeStaff(MagesStaff staff)
        {
            Type wandClass = staff.WandClass();

            if (wandClass == null)
            {
                return null;
            }
            else
            {
                Wand n;
                do
                {
                    n = (Wand)Generator.Random(Generator.Category.WAND);
                }
                while (Challenges.IsItemBlocked(n) || n.GetType() == wandClass);

                n.SetLevel(0);
                n.Identify();
                staff.ImbueWand(n, null);
            }

            return staff;
        }

        private Weapon ChangeWeapon(Weapon w)
        {
            Weapon n;
            Generator.Category c;
            if (w is MeleeWeapon)
            {
                c = Generator.wepTiers[((MeleeWeapon)w).tier - 1];
            }
            else
            {
                c = Generator.misTiers[((MissileWeapon)w).tier - 1];
            }

            do
            {
                var classes = c.GetClasses();
                var probs = c.GetProbs();
                n = (Weapon)Reflection.NewInstance(classes[Rnd.Chances(probs)]);
            }
            while (Challenges.IsItemBlocked(n) || n.GetType() == w.GetType());

            int level = w.GetLevel();
            if (w.curseInfusionBonus)
                --level;
            if (level > 0)
                n.Upgrade(level);
            else if (level < 0)
                n.Degrade(-level);

            n.enchantment = w.enchantment;
            n.curseInfusionBonus = w.curseInfusionBonus;
            n.levelKnown = w.levelKnown;
            n.cursedKnown = w.cursedKnown;
            n.cursed = w.cursed;
            n.augment = w.augment;

            return n;
        }

        private Ring ChangeRing(Ring r)
        {
            Ring n;
            do
            {
                n = (Ring)Generator.Random(Generator.Category.RING);
            }
            while (Challenges.IsItemBlocked(n) || n.GetType() == r.GetType());

            n.SetLevel(0);

            int level = r.GetLevel();
            if (level > 0)
            {
                n.Upgrade(level);
            }
            else if (level < 0)
            {
                n.Degrade(-level);
            }

            n.levelKnown = r.levelKnown;
            n.cursedKnown = r.cursedKnown;
            n.cursed = r.cursed;

            return n;
        }

        private Artifact ChangeArtifact(Artifact a)
        {
            Artifact n = Generator.RandomArtifact();

            if (n != null && !Challenges.IsItemBlocked(n))
            {
                n.cursedKnown = a.cursedKnown;
                n.cursed = a.cursed;
                n.levelKnown = a.levelKnown;
                n.TransferUpgrade(a.VisiblyUpgraded());
                return n;
            }

            return null;
        }

        private Wand ChangeWand(Wand w)
        {
            Wand n;
            do
            {
                n = (Wand)Generator.Random(Generator.Category.WAND);
            }
            while (Challenges.IsItemBlocked(n) || n.GetType() == w.GetType());

            n.SetLevel(0);
            int level = w.GetLevel();
            if (w.curseInfusionBonus)
                --level;
            n.Upgrade(level);

            n.levelKnown = w.levelKnown;
            n.cursedKnown = w.cursedKnown;
            n.cursed = w.cursed;
            n.curseInfusionBonus = w.curseInfusionBonus;

            return n;
        }

        private Plant.Seed ChangeSeed(Plant.Seed s)
        {
            Plant.Seed n;

            do
            {
                n = (Plant.Seed)Generator.Random(Generator.Category.SEED);
            }
            while (n.GetType() == s.GetType());

            return n;
        }

        private Runestone ChangeStone(Runestone r)
        {
            Runestone n;

            do
            {
                n = (Runestone)Generator.Random(Generator.Category.STONE);
            }
            while (n.GetType() == r.GetType());

            return n;
        }

        private Scroll ChangeScroll(Scroll s)
        {
            var type = s.GetType();

            if (s is ExoticScroll)
            {
                return (Scroll)Reflection.NewInstance(ExoticScroll.exoToReg.Get(type));
            }
            else
            {
                return (Scroll)Reflection.NewInstance(ExoticScroll.regToExo.Get(type));
            }
        }

        private Potion ChangePotion(Potion p)
        {
            var type = p.GetType();

            if (p is ExoticPotion)
            {
                return (Potion)Reflection.NewInstance(ExoticPotion.exoToReg.Get(type));
            }
            else
            {
                return (Potion)Reflection.NewInstance(ExoticPotion.regToExo.Get(type));
            }
        }

        public override int Value()
        {
            return IsKnown() ? 50 * quantity : base.Value();
        }
    }
}