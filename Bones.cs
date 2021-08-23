using System.Collections.Generic;
using System.IO;
using watabou.utils;
using spdd.actors.hero;
using spdd.items;
using spdd.items.artifacts;
using spdd.items.weapon.missiles;

namespace spdd
{
    public class Bones
    {
        private const string BONES_FILE = "bones.dat";

        private const string LEVEL = "level";
        private const string ITEM = "item";

        private static int depth = -1;
        private static Item item;

        public static void Leave()
        {
            depth = Dungeon.depth;

            //heroes which have won the game, who die far above their farthest depth, or who are challenged drop no bones.
            if (Statistics.amuletObtained || (Statistics.deepestFloor - 5) >= depth || Dungeon.challenges > 0)
            {
                depth = -1;
                return;
            }

            item = PickItem(Dungeon.hero);

            Bundle bundle = new Bundle();
            bundle.Put(LEVEL, depth);
            bundle.Put(ITEM, item);

            try
            {
                FileUtils.BundleToFile(BONES_FILE, bundle);
            }
            catch (IOException e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
            }
        }

        private static Item PickItem(Hero hero)
        {
            Item item = null;
            if (Rnd.Int(3) != 0)
            {
                switch (Rnd.Int(7))
                {
                    case 0:
                        item = hero.belongings.weapon;
                        break;
                    case 1:
                        item = hero.belongings.armor;
                        break;
                    case 2:
                        item = hero.belongings.artifact;
                        break;
                    case 3:
                        item = hero.belongings.misc;
                        break;
                    case 4:
                        item = hero.belongings.ring;
                        break;
                    case 5:
                    case 6:
                        item = Dungeon.quickslot.RandomNonePlaceholder();
                        break;
                }
                if (item == null || !item.bones)
                {
                    return PickItem(hero);
                }
            }
            else
            {
                var iterator = hero.belongings.backpack.GetEnumerator();
                Item curItem;
                List<Item> items = new List<Item>();
                while (iterator.HasNext())
                {
                    curItem = iterator.Current;
                    if (curItem.bones)
                        items.Add(curItem);
                }

                if (Rnd.Int(3) < items.Count)
                {
                    item = Rnd.Element(items);
                    if (item.stackable)
                        item.Quantity(Rnd.NormalIntRange(1, (item.Quantity() + 1) / 2));
                }
                else
                {
                    if (Dungeon.gold > 100)
                        item = new Gold(Rnd.NormalIntRange(50, Dungeon.gold / 2));
                    else
                        item = new Gold(50);
                }
            }

            return item;
        }

        public static Item Get()
        {
            if (depth == -1)
            {
                try
                {
                    Bundle bundle = FileUtils.BundleFromFile(BONES_FILE);

                    depth = bundle.GetInt(LEVEL);
                    item = (Item)bundle.Get(ITEM);

                    return Get();

                }
                catch (IOException)
                {
                    return null;
                }
            }
            else
            {
                //heroes who are challenged cannot find bones
                if (depth == Dungeon.depth && Dungeon.challenges == 0)
                {
                    FileUtils.DeleteFile(BONES_FILE);
                    depth = 0;

                    if (item == null)
                        return null;

                    //Enforces artifact uniqueness
                    if (item is Artifact)
                    {
                        if (Generator.RemoveArtifact(((Artifact)item).GetType()))
                        {
                            //generates a new artifact of the same type, always +0
                            var artifact = (Artifact)Reflection.NewInstance(((Artifact)item).GetType());
                            if (artifact == null)
                                return new Gold(item.Value());

                            artifact.cursed = true;
                            artifact.cursedKnown = true;

                            return artifact;
                        }
                        else
                        {
                            return new Gold(item.Value());
                        }
                    }

                    if (item.IsUpgradable() && !(item is MissileWeapon))
                    {
                        item.cursed = true;
                        item.cursedKnown = true;
                    }

                    if (item.IsUpgradable())
                    {
                        //caps at +3
                        if (item.GetLevel() > 3)
                        {
                            item.Degrade(item.GetLevel() - 3);
                        }
                        //thrown weapons are always IDed, otherwise set unknown
                        item.levelKnown = item is MissileWeapon;
                    }

                    item.Reset();

                    return item;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}