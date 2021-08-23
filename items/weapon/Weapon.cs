using System;
using System.Collections.Generic;
using System.Linq;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.rings;
using spdd.items.weapon.curses;
using spdd.items.weapon.enchantments;
using spdd.messages;
using spdd.sprites;
using spdd.utils;

namespace spdd.items.weapon
{
    public static class WeaponAugmentExtensions
    {
        // Augment의 SPEED, DAMAGE, NONE 순서로 저장
        static float[] damageFactor = { 0.7f, 1.5f, 1.0f };
        static float[] delayFactor = { 0.6667f, 1.6667f, 1.0000f };

        public static int DamageFactor(this Weapon.Augment augment, int dmg)
        {
            int index = (int)augment;
            return (int)Math.Round(dmg * damageFactor[index], MidpointRounding.AwayFromZero);
        }

        public static float DelayFactor(this Weapon.Augment augment, float dly)
        {
            int index = (int)augment;
            return dly * delayFactor[index];
        }
    }

    abstract public class Weapon : KindOfWeapon
    {
        public float ACC = 1f;  // Accuracy modifier
        public float DLY = 1f;  // Speed modifier
        public int RCH = 1;     // Reach modifier (only applies to melee hits)

        public enum Augment
        {
            SPEED,
            DAMAGE,
            NONE
        };

        public Augment augment = Augment.NONE;

        private const int USES_TO_ID = 20;
        private int usesLeftToID = USES_TO_ID;
        private float availableUsesToID = USES_TO_ID / 2f;

        public Enchantment enchantment;
        public bool curseInfusionBonus;

        public override int Proc(Character attacker, Character defender, int damage)
        {
            if (enchantment != null && attacker.FindBuff<MagicImmune>() == null)
            {
                damage = enchantment.Proc(this, attacker, defender, damage);
            }

            if (!levelKnown && attacker == Dungeon.hero && availableUsesToID >= 1)
            {
                --availableUsesToID;
                --usesLeftToID;
                if (usesLeftToID <= 0)
                {
                    Identify();
                    GLog.Positive(Messages.Get(typeof(Weapon), "identify"));
                    BadgesExtensions.ValidateItemLevelAquired(this);
                }
            }

            return damage;
        }

        public override void OnHeroGainExp(float levelPercent, Hero hero)
        {
            if (!levelKnown && IsEquipped(hero) && availableUsesToID <= USES_TO_ID / 2f)
            {
                //gains enough uses to ID over 0.5 levels
                availableUsesToID = Math.Min(USES_TO_ID / 2f, availableUsesToID + levelPercent * USES_TO_ID);
            }
        }

        private const string USES_LEFT_TO_ID = "uses_left_to_id";
        private const string AVAILABLE_USES = "available_uses";
        private const string ENCHANTMENT = "enchantment";
        private const string CURSE_INFUSION_BONUS = "curse_infusion_bonus";
        private const string AUGMENT = "augment";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(USES_LEFT_TO_ID, usesLeftToID);
            bundle.Put(AVAILABLE_USES, availableUsesToID);
            bundle.Put(ENCHANTMENT, enchantment);
            bundle.Put(CURSE_INFUSION_BONUS, curseInfusionBonus);
            bundle.Put(AUGMENT, augment.ToString());    // enum
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            usesLeftToID = bundle.GetInt(USES_LEFT_TO_ID);
            availableUsesToID = bundle.GetInt(AVAILABLE_USES);
            enchantment = (Enchantment)bundle.Get(ENCHANTMENT);
            curseInfusionBonus = bundle.GetBoolean(CURSE_INFUSION_BONUS);
            augment = bundle.GetEnum<Augment>(AUGMENT);
        }

        public override void Reset()
        {
            base.Reset();
            usesLeftToID = USES_TO_ID;
            availableUsesToID = USES_TO_ID / 2f;
        }

        public override float AccuracyFactor(Character owner)
        {
            int encumbrance = 0;

            if (owner is Hero)
                encumbrance = STRReq() - ((Hero)owner).GetSTR();

            if (HasEnchant(typeof(Wayward), owner))
                encumbrance = Math.Max(2, encumbrance + 2);

            float ACC = this.ACC;

            return encumbrance > 0 ? (float)(ACC / Math.Pow(1.5, encumbrance)) : ACC;
        }

        public override float SpeedFactor(Character owner)
        {
            int encumbrance = 0;
            if (owner is Hero)
                encumbrance = STRReq() - ((Hero)owner).GetSTR();

            float DLY = augment.DelayFactor(this.DLY);

            DLY *= RingOfFuror.AttackDelayMultiplier(owner);

            return (encumbrance > 0 ? (float)(DLY * Math.Pow(1.2, encumbrance)) : DLY);
        }

        public override int ReachFactor(Character owner)
        {
            return HasEnchant(typeof(Projecting), owner) ? RCH + 1 : RCH;
        }

        public int STRReq()
        {
            return STRReq(GetLevel());
        }

        public abstract int STRReq(int lvl);

        public override int GetLevel()
        {
            return base.GetLevel() + (curseInfusionBonus ? 1 : 0);
        }

        //overrides as other things can equip these
        public override int BuffedLvl()
        {
            if (IsEquipped(Dungeon.hero) || Dungeon.hero.belongings.Contains(this))
            {
                return base.BuffedLvl();
            }
            else
            {
                return GetLevel();
            }
        }

        public override Item Upgrade()
        {
            return Upgrade(false);
        }

        public virtual Item Upgrade(bool enchant)
        {
            if (enchant)
            {
                if (enchantment == null || HasCurseEnchant())
                {
                    Enchant(Enchantment.Random());
                }
            }
            else
            {
                if (HasCurseEnchant())
                {
                    if (Rnd.Int(3) == 0)
                        Enchant(null);
                }
                else if (GetLevel() >= 4 && Rnd.Float(10) < Math.Pow(2, GetLevel() - 4))
                {
                    Enchant(null);
                }
            }

            cursed = false;

            return base.Upgrade();
        }

        public override string Name()
        {
            return enchantment != null && (cursedKnown || !enchantment.Curse()) ? enchantment.Name(base.Name()) : base.Name();
        }

        public override Item Random()
        {
            //+0: 75% (3/4)
            //+1: 20% (4/20)
            //+2: 5%  (1/20)
            int n = 0;
            if (Rnd.Int(4) == 0)
            {
                ++n;
                if (Rnd.Int(5) == 0)
                    ++n;
            }
            SetLevel(n);

            //30% chance to be cursed
            //10% chance to be enchanted
            float effectRoll = Rnd.Float();
            if (effectRoll < 0.3f)
            {
                Enchant(Enchantment.RandomCurse());
                cursed = true;
            }
            else if (effectRoll >= 0.9f)
            {
                Enchant();
            }

            return this;
        }

        public virtual Weapon Enchant(Enchantment ench)
        {
            if (ench == null || !ench.Curse())
                curseInfusionBonus = false;
            enchantment = ench;
            UpdateQuickslot();
            return this;
        }

        public Weapon Enchant()
        {
            Type oldEnchantment = enchantment != null ? enchantment.GetType() : null;
            Enchantment ench = Enchantment.Random(oldEnchantment);

            return Enchant(ench);
        }

        public virtual bool HasEnchant(Type type, Character owner)
        {
            return enchantment != null &&
                enchantment.GetType() == type &&
                owner.FindBuff<MagicImmune>() == null;
        }

        //these are not used to process specific enchant effects, so magic immune doesn't affect them
        public bool HasGoodEnchant()
        {
            return enchantment != null && !enchantment.Curse();
        }

        public bool HasCurseEnchant()
        {
            return enchantment != null && enchantment.Curse();
        }

        public override ItemSprite.Glowing Glowing()
        {
            return enchantment != null && (cursedKnown || !enchantment.Curse()) ? enchantment.Glowing() : null;
        }

        public abstract class Enchantment : IBundlable
        {
            private static Type[] common =
            {
                typeof(Blazing),
                typeof(Chilling),
                typeof(Kinetic),
                typeof(Shocking)
            };

            private static Type[] uncommon =
            {
                typeof(Blocking),
                typeof(Blooming),
                typeof(Elastic),
                typeof(Lucky),
                typeof(Projecting),
                typeof(Unstable)
            };

            private static Type[] rare =
            {
                typeof(Corrupting),
                typeof(Grim),
                typeof(Vampiric)
            };

            private static float[] typeChances =
            {
                50, //12.5% each
                40, //6.67% each
                10  //3.33% each
            };

            private static Type[] curses =
            {
                typeof(Annoying),
                typeof(Displacing),
                typeof(Exhausting),
                typeof(Fragile),
                typeof(Sacrificial),
                typeof(Wayward),
                typeof(Polarized),
                typeof(Friendly)
            };

            public abstract int Proc(Weapon weapon, Character attacker, Character defender, int damage);

            public string Name()
            {
                if (!Curse())
                    return Name(Messages.Get(this, "enchant"));
                else
                    return Name(Messages.Get(typeof(Item), "curse"));
            }

            public virtual string Name(string weaponName)
            {
                return Messages.Get(this, "name", weaponName);
            }

            public string Desc()
            {
                return Messages.Get(this, "desc");
            }

            public virtual bool Curse()
            {
                return false;
            }

            public virtual void RestoreFromBundle(Bundle bundle)
            { }

            public virtual void StoreInBundle(Bundle bundle)
            { }

            public abstract ItemSprite.Glowing Glowing();

            public static Enchantment Random(params Type[] toIgnore)
            {
                int n = Rnd.Chances(typeChances);

                switch (n)
                {
                    case 0:
                    default:
                        return RandomCommon(toIgnore);
                    case 1:
                        return RandomUncommon(toIgnore);
                    case 2:
                        return RandomRare(toIgnore);
                }
            }

            public static Enchantment RandomCommon(params Type[] toIgnore)
            {
                List<Type> list = new List<Type>(common.ToList());
                var enchants = list.Except(toIgnore).ToList();

                if (enchants.Count == 0)
                {
                    return Random();
                }
                else
                {
                    return (Enchantment)Reflection.NewInstance(Rnd.Element(enchants));
                }
            }

            public static Enchantment RandomUncommon(params Type[] toIgnore)
            {
                List<Type> list = new List<Type>(uncommon.ToList());
                var enchants = list.Except(toIgnore).ToList();

                if (enchants.Count == 0)
                {
                    return Random();
                }
                else
                {
                    return (Enchantment)Reflection.NewInstance(Rnd.Element(enchants));
                }
            }

            public static Enchantment RandomRare(params Type[] toIgnore)
            {
                List<Type> list = new List<Type>(rare.ToList());
                var enchants = list.Except(toIgnore).ToList();

                if (enchants.Count == 0)
                {
                    return Random();
                }
                else
                {
                    return (Enchantment)Reflection.NewInstance(Rnd.Element(enchants));
                }
            }

            public static Enchantment RandomCurse(params Type[] toIgnore)
            {
                List<Type> list = new List<Type>(curses.ToList());
                var enchants = list.Except(toIgnore).ToList();

                if (enchants.Count == 0)
                {
                    return Random();
                }
                else
                {
                    return (Enchantment)Reflection.NewInstance(Rnd.Element(enchants));
                }
            }
        }
    }
}