using System;
using System.Collections.Generic;
using System.Linq;
using watabou.utils;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.items.armor;
using spdd.items.artifacts;
using spdd.items.bags;
using spdd.items.bombs;
using spdd.items.food;
using spdd.items.potions;
using spdd.items.scrolls;
using spdd.items.stones;
using spdd.items.weapon.melee;
using spdd.items.weapon.missiles.darts;
using spdd.levels.painters;

namespace spdd.levels.rooms.special
{
    public class ShopRoom : SpecialRoom
    {
        private List<Item> itemsToSpawn;

        public override int MinWidth()
        {
            return Math.Max(7, (int)(Math.Sqrt(ItemCount()) + 3));
        }

        public override int MinHeight()
        {
            return Math.Max(7, (int)(Math.Sqrt(ItemCount()) + 3));
        }

        public int ItemCount()
        {
            if (itemsToSpawn == null)
                itemsToSpawn = GenerateItems();

            return itemsToSpawn.Count;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);

            PlaceShopkeeper(level);

            PlaceItems(level);

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
            }
        }

        protected virtual void PlaceShopkeeper(Level level)
        {
            int pos = level.PointToCell(Center());

            Mob shopkeeper = new Shopkeeper();
            shopkeeper.pos = pos;
            level.mobs.Add(shopkeeper);
        }

        protected void PlaceItems(Level level)
        {
            if (itemsToSpawn == null)
                itemsToSpawn = GenerateItems();

            Point itemPlacement = new Point(Entrance());
            if (itemPlacement.y == top)
                ++itemPlacement.y;
            else if (itemPlacement.y == bottom)
                --itemPlacement.y;
            else if (itemPlacement.x == left)
                ++itemPlacement.x;
            else
                --itemPlacement.x;

            foreach (Item item in itemsToSpawn)
            {
                if (itemPlacement.x == left + 1 && itemPlacement.y != top + 1)
                    --itemPlacement.y;
                else if (itemPlacement.y == top + 1 && itemPlacement.x != right - 1)
                    ++itemPlacement.x;
                else if (itemPlacement.x == right - 1 && itemPlacement.y != bottom - 1)
                    ++itemPlacement.y;
                else
                    --itemPlacement.x;

                int cell = level.PointToCell(itemPlacement);

                if (level.heaps[cell] != null)
                {
                    do
                    {
                        cell = level.PointToCell(Random());
                    }
                    while (level.heaps[cell] != null || level.FindMob(cell) != null);
                }

                level.Drop(item, cell).type = Heap.Type.FOR_SALE;
            }
        }

        protected static List<Item> GenerateItems()
        {
            List<Item> itemsToSpawn = new List<Item>();

            MeleeWeapon w;
            switch (Dungeon.depth)
            {
                case 6:
                default:
                    w = (MeleeWeapon)Generator.Random(Generator.wepTiers[1]);
                    itemsToSpawn.Add(Generator.Random(Generator.misTiers[1]).Quantity(2).Identify());
                    itemsToSpawn.Add((new LeatherArmor()).Identify());
                    break;

                case 11:
                    w = (MeleeWeapon)Generator.Random(Generator.wepTiers[2]);
                    itemsToSpawn.Add(Generator.Random(Generator.misTiers[2]).Quantity(2).Identify());
                    itemsToSpawn.Add(new MailArmor().Identify());
                    break;

                case 16:
                    w = (MeleeWeapon)Generator.Random(Generator.wepTiers[3]);
                    itemsToSpawn.Add(Generator.Random(Generator.misTiers[3]).Quantity(2).Identify());
                    itemsToSpawn.Add(new ScaleArmor().Identify());
                    break;

                case 20:
                case 21:
                    w = (MeleeWeapon)Generator.Random(Generator.wepTiers[4]);
                    itemsToSpawn.Add(Generator.Random(Generator.misTiers[4]).Quantity(2).Identify());
                    itemsToSpawn.Add(new PlateArmor().Identify());
                    itemsToSpawn.Add(new Torch());
                    itemsToSpawn.Add(new Torch());
                    itemsToSpawn.Add(new Torch());
                    break;
            }
            w.Enchant(null);
            w.cursed = false;
            w.SetLevel(0);
            w.Identify();
            itemsToSpawn.Add(w);

            itemsToSpawn.Add(TippedDart.RandomTipped(2));

            itemsToSpawn.Add(new MerchantsBeacon());

            itemsToSpawn.Add(ChooseBag(Dungeon.hero.belongings));

            itemsToSpawn.Add(new PotionOfHealing());
            itemsToSpawn.Add(Generator.RandomUsingDefaults(Generator.Category.POTION));
            itemsToSpawn.Add(Generator.RandomUsingDefaults(Generator.Category.POTION));

            itemsToSpawn.Add(new ScrollOfIdentify());
            itemsToSpawn.Add(new ScrollOfRemoveCurse());
            itemsToSpawn.Add(new ScrollOfMagicMapping());

            for (int i = 0; i < 2; ++i)
            {
                itemsToSpawn.Add(Rnd.Int(2) == 0 ?
                        Generator.RandomUsingDefaults(Generator.Category.POTION) :
                        Generator.RandomUsingDefaults(Generator.Category.SCROLL));
            }

            itemsToSpawn.Add(new SmallRation());
            itemsToSpawn.Add(new SmallRation());

            switch (Rnd.Int(4))
            {
                case 0:
                    itemsToSpawn.Add(new Bomb());
                    break;
                case 1:
                case 2:
                    itemsToSpawn.Add(new Bomb.DoubleBomb());
                    break;
                case 3:
                    itemsToSpawn.Add(new Honeypot());
                    break;
            }

            itemsToSpawn.Add(new Ankh());
            itemsToSpawn.Add(new StoneOfAugmentation());

            var hourglass = Dungeon.hero.belongings.GetItem<TimekeepersHourglass>();
            if (hourglass != null)
            {
                int bags = 0;
                //creates the given float percent of the remaining bags to be dropped.
                //this way players who get the hourglass late can still max it, usually.
                switch (Dungeon.depth)
                {
                    case 6:
                        bags = (int)Math.Ceiling((5 - hourglass.sandBags) * 0.20f);
                        break;
                    case 11:
                        bags = (int)Math.Ceiling((5 - hourglass.sandBags) * 0.25f);
                        break;
                    case 16:
                        bags = (int)Math.Ceiling((5 - hourglass.sandBags) * 0.50f);
                        break;
                    case 20:
                    case 21:
                        bags = (int)Math.Ceiling((5 - hourglass.sandBags) * 0.80f);
                        break;
                }

                for (int i = 1; i <= bags; ++i)
                {
                    itemsToSpawn.Add(new TimekeepersHourglass.SandBag());
                    ++hourglass.sandBags;
                }
            }

            Item rare;
            switch (Rnd.Int(10))
            {
                case 0:
                    rare = Generator.Random(Generator.Category.WAND);
                    rare.SetLevel(0);
                    break;
                case 1:
                    rare = Generator.Random(Generator.Category.RING);
                    rare.SetLevel(0);
                    break;
                case 2:
                    rare = Generator.Random(Generator.Category.ARTIFACT);
                    break;
                default:
                    rare = new Stylus();
                    break;
            }
            rare.cursed = false;
            rare.cursedKnown = true;
            itemsToSpawn.Add(rare);

            //hard limit is 63 items + 1 shopkeeper, as shops can't be bigger than 8x8=64 internally
            if (itemsToSpawn.Count > 63)
                throw new Exception("Shop attempted to carry more than 63 items!");

            Rnd.Shuffle(itemsToSpawn);
            return itemsToSpawn;
        }

        protected static Bag ChooseBag(Belongings pack)
        {
            //generate a hashmap of all valid bags.
            Dictionary<Bag, int> bags = new Dictionary<Bag, int>();
            if (!Dungeon.LimitedDrops.VELVET_POUCH.Dropped())
                bags.Add(new VelvetPouch(), 1);
            if (!Dungeon.LimitedDrops.SCROLL_HOLDER.Dropped())
                bags.Add(new ScrollHolder(), 0);
            if (!Dungeon.LimitedDrops.POTION_BANDOLIER.Dropped())
                bags.Add(new PotionBandolier(), 0);
            if (!Dungeon.LimitedDrops.MAGICAL_HOLSTER.Dropped())
                bags.Add(new MagicalHolster(), 0);

            if (bags.Count == 0)
                return null;

            //count up items in the main bag
            foreach (Item item in pack.backpack.items)
            {
                foreach (var bag in bags.Keys.ToList())
                {
                    if (bag.CanHold(item))
                    {
                        bags[bag] = bags[bag] + 1;
                    }
                }
            }

            //find which bag will result in most inventory savings, drop that.
            Bag bestBag = null;
            foreach (var pair in bags)
            {
                Bag bag = pair.Key;
                int value = pair.Value;
                if (bestBag == null)
                {
                    bestBag = bag;
                }
                else if (value > bags[bestBag])
                {
                    bestBag = bag;
                }
            }

            if (bestBag is VelvetPouch)
            {
                Dungeon.LimitedDrops.VELVET_POUCH.Drop();
            }
            else if (bestBag is ScrollHolder)
            {
                Dungeon.LimitedDrops.SCROLL_HOLDER.Drop();
            }
            else if (bestBag is PotionBandolier)
            {
                Dungeon.LimitedDrops.POTION_BANDOLIER.Drop();
            }
            else if (bestBag is MagicalHolster)
            {
                Dungeon.LimitedDrops.MAGICAL_HOLSTER.Drop();
            }

            return bestBag;
        }
    }
}