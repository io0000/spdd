using System;
using System.Collections.Generic;
using System.Globalization;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.sprites;
using spdd.effects;
using spdd.items.potions;
using spdd.items.potions.exotic;
using spdd.items.bombs;
using spdd.items.armor;
using spdd.items.weapon;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.items.spells;
using spdd.items.stones;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfWealth : Ring
    {
        public RingOfWealth()
        {
            icon = ItemSpriteSheet.Icons.RING_WEALTH;
        }

        private float triesToDrop = float.Epsilon;     // 1.4E-45F;
        private int dropsToRare = int.MinValue;        // -2147483648;

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value = 100f * (Math.Pow(1.20f, SoloBuffedBonus()) - 1f);
                return Messages.Get(this, "stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                var value = 20f;
                return Messages.Get(this, "typical_stats", value.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        private const string TRIES_TO_DROP = "tries_to_drop";
        private const string DROPS_TO_RARE = "drops_to_rare";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(TRIES_TO_DROP, triesToDrop);
            bundle.Put(DROPS_TO_RARE, dropsToRare);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            triesToDrop = bundle.GetFloat(TRIES_TO_DROP);
            dropsToRare = bundle.GetInt(DROPS_TO_RARE);
        }

        public override RingBuff Buff()
        {
            return new Wealth(this);
        }

        public static float DropChanceMultiplier(Character target)
        {
            return (float)Math.Pow(1.20, GetBuffedBonus<Wealth>(target));
        }

        public static List<Item> TryForBonusDrop(Character target, int tries)
        {
            int bonus = GetBuffedBonus<Wealth>(target);

            if (bonus <= 0)
                return null;

            var buffs = target.Buffs<Wealth>();
            float triesToDrop = float.Epsilon;     // 1.4E-45F;
            int dropsToEquip = int.MinValue;       // -2147483648;

            //find the largest count (if they aren't synced yet)
            foreach (Wealth w in buffs)
            {
                if (w.TriesToDrop() > triesToDrop)
                {
                    triesToDrop = w.TriesToDrop();
                    dropsToEquip = w.DropsToRare();
                }
            }

            //reset (if needed), decrement, and store counts
            if (triesToDrop == float.Epsilon)
            {
                triesToDrop = Rnd.NormalIntRange(0, 25);
                dropsToEquip = Rnd.NormalIntRange(4, 8);
            }

            //now handle reward logic
            List<Item> drops = new List<Item>();

            triesToDrop -= tries;
            while (triesToDrop <= 0)
            {
                if (dropsToEquip <= 0)
                {
                    Item i;
                    do
                    {
                        i = GenEquipmentDrop(bonus - 1);
                    }
                    while (Challenges.IsItemBlocked(i));
                    drops.Add(i);
                    dropsToEquip = Rnd.NormalIntRange(4, 8);
                }
                else
                {
                    Item i;
                    do
                    {
                        i = GenConsumableDrop(bonus - 1);
                    }
                    while (Challenges.IsItemBlocked(i));
                    drops.Add(i);
                    --dropsToEquip;
                }
                triesToDrop += Rnd.NormalIntRange(0, 25);
            }

            //store values back into rings
            foreach (Wealth w in buffs)
            {
                w.TriesToDrop(triesToDrop);
                w.DropsToRare(dropsToEquip);
            }

            return drops;
        }

        //used for visuals
        // 1/2/3 used for low/mid/high tier consumables
        // 3 used for +0-1 equips, 4 used for +2 or higher equips
        private static int latestDropTier;

        public static void ShowFlareForBonusDrop(Visual vis)
        {
            switch (latestDropTier)
            {
                default:
                    break; //do nothing
                case 1:
                    new Flare(6, 20).Color(new Color(0x00, 0xFF, 0x00, 0xFF), true).Show(vis, 3f);
                    break;
                case 2:
                    new Flare(6, 24).Color(new Color(0x00, 0xAA, 0xFF, 0xFF), true).Show(vis, 3.33f);
                    break;
                case 3:
                    new Flare(6, 28).Color(new Color(0xAA, 0x00, 0xFF, 0xFF), true).Show(vis, 3.67f);
                    break;
                case 4:
                    new Flare(6, 32).Color(new Color(0xFF, 0xAA, 0x00, 0xFF), true).Show(vis, 4f);
                    break;
            }
            latestDropTier = 0;
        }

        public static Item GenConsumableDrop(int level)
        {
            float roll = Rnd.Float();
            //60% chance - 4% per level. Starting from +15: 0%
            if (roll < (0.6f - 0.04f * level))
            {
                latestDropTier = 1;
                return GenLowValueConsumable();
            }
            //30% chance + 2% per level. Starting from +15: 60%-2%*(lvl-15)
            else if (roll < (0.9f - 0.02f * level))
            {
                latestDropTier = 2;
                return GenMidValueConsumable();
            }
            //10% chance + 2% per level. Starting from +15: 40%+2%*(lvl-15)
            else
            {
                latestDropTier = 3;
                return GenHighValueConsumable();
            }
        }

        public static Item GenLowValueConsumable()
        {
            switch (Rnd.Int(4))
            {
                case 0:
                default:
                    Item i = new Gold().Random();
                    return i.Quantity(i.Quantity() / 2);
                case 1:
                    return Generator.Random(Generator.Category.STONE);
                case 2:
                    return Generator.Random(Generator.Category.POTION);
                case 3:
                    return Generator.Random(Generator.Category.SCROLL);
            }
        }

        public static Item GenMidValueConsumable()
        {
            switch (Rnd.Int(6))
            {
                case 0:
                default:
                    Item i = GenLowValueConsumable();
                    return i.Quantity(i.Quantity() * 2);
                case 1:
                    i = Generator.RandomUsingDefaults(Generator.Category.POTION);
                    return (Potion)Reflection.NewInstance(ExoticPotion.regToExo[i.GetType()]);
                case 2:
                    i = Generator.RandomUsingDefaults(Generator.Category.SCROLL);
                    return (Scroll)Reflection.NewInstance(ExoticScroll.regToExo[i.GetType()]);
                case 3:
                    //return (pdsharp.utils.Random.Int(2) == 0) ? new ArcaneCatalyst() : new AlchemicalCatalyst();
                    var value = Rnd.Int(2);
                    if (value == 0)
                        return new ArcaneCatalyst();
                    else
                        return new AlchemicalCatalyst();
                case 4:
                    return new Bomb();
                case 5:
                    return new Honeypot();
            }
        }

        public static Item GenHighValueConsumable()
        {
            switch (Rnd.Int(4))
            {
                case 0:
                default:
                    Item i = GenMidValueConsumable();
                    if (i is Bomb)
                    {
                        return new Bomb.DoubleBomb();
                    }
                    else
                    {
                        return i.Quantity(i.Quantity() * 2);
                    }
                case 1:
                    return new StoneOfEnchantment();
                case 2:
                    return new PotionOfExperience();
                case 3:
                    return new ScrollOfTransmutation();
            }
        }

        public static Item GenEquipmentDrop(int level)
        {
            Item result;
            //each upgrade increases depth used for calculating drops by 1
            int floorset = (Dungeon.depth + level) / 5;
            switch (Rnd.Int(5))
            {
                default:
                case 0:
                case 1:
                    Weapon w = Generator.RandomWeapon(floorset);
                    if (!w.HasGoodEnchant() && Rnd.Int(10) < level)
                        w.Enchant();
                    else if (w.HasCurseEnchant())
                        w.Enchant(null);
                    result = w;
                    break;
                case 2:
                    Armor a = Generator.RandomArmor(floorset);
                    if (!a.HasGoodGlyph() && Rnd.Int(10) < level)
                        a.Inscribe();
                    else if (a.HasCurseGlyph())
                        a.Inscribe(null);
                    result = a;
                    break;
                case 3:
                    result = Generator.Random(Generator.Category.RING);
                    break;
                case 4:
                    result = Generator.Random(Generator.Category.ARTIFACT);
                    break;
            }
            //minimum level is 1/2/3/4/5/6 when ring level is 1/3/6/10/15/21
            if (result.IsUpgradable())
            {
                int minLevel = (int)Math.Floor((Math.Sqrt(8 * level + 1) - 1) / 2f);
                if (result.GetLevel() < minLevel)
                {
                    result.SetLevel(minLevel);
                }
            }
            result.cursed = false;
            result.cursedKnown = true;
            if (result.GetLevel() >= 2)
            {
                latestDropTier = 4;
            }
            else
            {
                latestDropTier = 3;
            }
            return result;
        }

        public class Wealth : RingBuff
        {
            public Wealth(Ring ring)
                : base(ring)
            { }

            public void TriesToDrop(float val)
            {
                var r = (RingOfWealth)ring;
                r.triesToDrop = val;
            }

            public float TriesToDrop()
            {
                var r = (RingOfWealth)ring;
                return r.triesToDrop;
            }

            public void DropsToRare(int val)
            {
                var r = (RingOfWealth)ring;
                r.dropsToRare = val;
            }

            public int DropsToRare()
            {
                var r = (RingOfWealth)ring;
                return r.dropsToRare;
            }
        }
    }
}