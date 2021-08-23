using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.hero;
using spdd.utils;
using spdd.messages;

namespace spdd.items.armor
{
    public abstract class ClassArmor : Armor
    {
        private const string AC_SPECIAL = "SPECIAL";

        private void InitInstance()
        {
            levelKnown = true;
            cursedKnown = true;
            defaultAction = AC_SPECIAL;

            bones = false;
        }

        private int armorTier;

        protected float charge;

        protected ClassArmor()
            : base(6)
        {
            InitInstance();
        }

        public static ClassArmor Upgrade(Hero owner, Armor armor)
        {
            ClassArmor classArmor = null;

            switch (owner.heroClass)
            {
                case HeroClass.WARRIOR:
                    classArmor = new WarriorArmor();
                    BrokenSeal seal = armor.CheckSeal();
                    if (seal != null)
                        classArmor.AffixSeal(seal);
                    break;

                case HeroClass.ROGUE:
                    classArmor = new RogueArmor();
                    break;

                case HeroClass.MAGE:
                    classArmor = new MageArmor();
                    break;

                case HeroClass.HUNTRESS:
                    classArmor = new HuntressArmor();
                    break;
            }

            classArmor.SetLevel(armor.GetLevel() - (armor.curseInfusionBonus ? 1 : 0));
            classArmor.armorTier = armor.tier;
            classArmor.augment = armor.augment;
            classArmor.Inscribe(armor.glyph);
            classArmor.cursed = armor.cursed;
            classArmor.curseInfusionBonus = armor.curseInfusionBonus;
            classArmor.Identify();

            classArmor.charge = 0;
            if (owner.lvl > 18)
            {
                classArmor.charge += (owner.lvl - 18) * 25;
                if (classArmor.charge > 100)
                    classArmor.charge = 100;
            }

            return classArmor;
        }

        private const string ARMOR_TIER = "armortier";
        private const string CHARGE = "charge";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(ARMOR_TIER, armorTier);
            bundle.Put(CHARGE, charge);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            armorTier = bundle.GetInt(ARMOR_TIER);
            charge = bundle.GetFloat(CHARGE);
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);

            if (hero.HP >= 3 && IsEquipped(hero))
                actions.Add(AC_SPECIAL);

            return actions;
        }

        public override string Status()
        {
            return Messages.Format("%.0f%%", charge);
            //return Messages.Format("{0:0.0}%", charge);
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_SPECIAL))
            {
                if (!IsEquipped(hero))
                {
                    GLog.Warning(Messages.Get(this, "not_equipped"));
                }
                else if (charge < 35)
                {
                    GLog.Warning(Messages.Get(this, "low_charge"));
                }
                else
                {
                    curUser = hero;
                    DoSpecial();
                }
            }
        }

        public override void OnHeroGainExp(float levelPercent, Hero hero)
        {
            base.OnHeroGainExp(levelPercent, hero);
            charge += 50 * levelPercent;
            if (charge > 100)
                charge = 100;
            UpdateQuickslot();
        }

        public abstract void DoSpecial();

        public override int STRReq(int lvl)
        {
            lvl = Math.Max(0, lvl);

            //strength req decreases at +1,+3,+6,+10,etc.
            //return (8 + Math.Round(armorTier * 2)) - (int)(Math.sqrt(8 * lvl + 1) - 1) / 2;
            return (8 + (armorTier * 2)) - (int)(Math.Sqrt(8 * lvl + 1) - 1) / 2;
        }

        public override int DRMax(int lvl)
        {
            int max = armorTier * (2 + lvl) + augment.DefenseFactor(lvl);
            if (lvl > max)
            {
                return ((lvl - max) + 1) / 2;
            }
            else
            {
                return max;
            }
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override int Value()
        {
            return 0;
        }
    }
}