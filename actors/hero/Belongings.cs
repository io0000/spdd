using System;
using System.Collections;
using System.Collections.Generic;
using watabou.utils;
using spdd.items;
using spdd.items.armor;
using spdd.items.bags;
using spdd.items.keys;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.items.wands;
using spdd.items.artifacts;

namespace spdd.actors.hero
{
    public class Belongings : IEnumerable
    {
        private Hero owner;

        public Bag backpack;

        public KindOfWeapon weapon;
        public Armor armor;
        public Artifact artifact;
        public KindofMisc misc;
        public Ring ring;

        //used when thrown weapons temporary occupy the weapon slot
        public KindOfWeapon stashedWeapon;

        public Belongings(Hero owner)
        {
            this.owner = owner;

            backpack = new BelongingsBag();
            backpack.owner = owner;
        }

        private class BelongingsBag : Bag
        {
            public override int Capacity()
            {
                int cap = base.Capacity();
                foreach (var item in items)
                {
                    if (item is Bag)
                        ++cap;
                }
                return cap;
            }
        }

        private const string WEAPON = "weapon";
        private const string ARMOR = "armor";
        private const string ARTIFACT = "artifact";
        private const string MISC = "misc";
        private const string RING = "ring";

        public virtual void StoreInBundle(Bundle bundle)
        {
            backpack.StoreInBundle(bundle);

            bundle.Put(WEAPON, weapon);
            bundle.Put(ARMOR, armor);
            bundle.Put(ARTIFACT, artifact);
            bundle.Put(MISC, misc);
            bundle.Put(RING, ring);
        }

        public virtual void RestoreFromBundle(Bundle bundle)
        {
            backpack.Clear();
            backpack.RestoreFromBundle(bundle);

            weapon = (KindOfWeapon)bundle.Get(WEAPON);
            if (weapon != null)
                weapon.Activate(owner);

            armor = (Armor)bundle.Get(ARMOR);
            if (armor != null)
                armor.Activate(owner);

            ////pre-0.8.2
            //if (bundle.contains("misc1") || bundle.contains("misc2"))
            //{
            //    artifact = null;
            //    misc = null;
            //    ring = null;
            //
            //    KindofMisc m = (KindofMisc)bundle.get("misc1");
            //    if (m instanceof Artifact){
            //        artifact = (Artifact)m;
            //    } else if (m instanceof Ring) {
            //        ring = (Ring)m;
            //    }
            //
            //    m = (KindofMisc)bundle.get("misc2");
            //    if (m instanceof Artifact){
            //        if (artifact == null) artifact = (Artifact)m;
            //        else misc = (Artifact)m;
            //    } else if (m instanceof Ring) {
            //        if (ring == null) ring = (Ring)m;
            //        else misc = (Ring)m;
            //    }
            //
            //}
            //else
            {
                artifact = (Artifact)bundle.Get(ARTIFACT);
                misc = (KindofMisc)bundle.Get(MISC);
                ring = (Ring)bundle.Get(RING);
            }

            if (artifact != null)
                artifact.Activate(owner);

            if (misc != null)
                misc.Activate(owner);

            if (ring != null)
                ring.Activate(owner);
        }

        public static void Preview(GamesInProgress.Info info, Bundle bundle)
        {
            if (bundle.Contains(ARMOR))
            {
                info.armorTier = ((Armor)bundle.Get(ARMOR)).tier;
            }
            else
            {
                info.armorTier = 0;
            }
        }

        public T GetItem<T>() where T : Item
        {
            Type type = typeof(T);

            return (T)GetItem(type);
        }

        public object GetItem(Type type)
        {
            //for (Item item : this)
            //{
            //    if (itemClass.isInstance(item))
            //    {
            //        return (T)item;
            //    }
            //}

            foreach (var item in this)
            {
                if (type.IsAssignableFrom(item.GetType()))
                    return item;
            }

            return null;
        }

        public bool Contains(Item contains)
        {
            foreach (var item in this)
            {
                if (contains == item)
                {
                    return true;
                }
            }

            return false;
        }

        public Item GetSimilar(Item similar)
        {
            foreach (var item in this)
            {
                if (similar != item && similar.IsSimilar(item))
                {
                    return item;
                }
            }

            return null;
        }

        public List<Item> GetAllSimilar(Item similar)
        {
            List<Item> result = new List<Item>();

            foreach (var item in this)
            {
                if (similar != item && similar.IsSimilar(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public void Identify()
        {
            foreach (var item in this)
            {
                item.Identify();
            }
        }

        public void Observe()
        {
            if (weapon != null)
            {
                weapon.Identify();
                BadgesExtensions.ValidateItemLevelAquired(weapon);
            }

            if (armor != null)
            {
                armor.Identify();
                BadgesExtensions.ValidateItemLevelAquired(armor);
            }

            if (artifact != null)
            {
                artifact.Identify();
                BadgesExtensions.ValidateItemLevelAquired(artifact);
            }

            if (misc != null)
            {
                misc.Identify();
                BadgesExtensions.ValidateItemLevelAquired(misc);
            }

            if (ring != null)
            {
                ring.Identify();
                BadgesExtensions.ValidateItemLevelAquired(ring);
            }

            foreach (var item in backpack)
            {
                if (item is EquipableItem || item is Wand)
                    item.cursedKnown = true;
            }
        }

        public void UncurseEquipped()
        {
            ScrollOfRemoveCurse.Uncurse(owner, armor, weapon, artifact, misc, ring);
        }

        public Item RandomUnequipped()
        {
            return Rnd.Element(backpack.items);
        }

        public void Resurrect(int depth)
        {
            foreach (var item in backpack.items.ToArray())
            {
                if (item is Key)
                {
                    var key = item as Key;
                    if (key.depth == depth)
                        key.DetachAll(backpack);
                }
                else if (item.unique)
                {
                    item.DetachAll(backpack);
                    //you keep the bag itself, not its contents.
                    if (item is Bag)
                        ((Bag)item).Resurrect();

                    item.Collect();
                }
                else if (!item.IsEquipped(owner))
                {
                    item.DetachAll(backpack);
                }
            }

            if (weapon != null)
            {
                weapon.cursed = false;
                weapon.Activate(owner);
            }

            if (armor != null)
            {
                armor.cursed = false;
                armor.Activate(owner);
            }

            if (artifact != null)
            {
                artifact.cursed = false;
                artifact.Activate(owner);
            }

            if (misc != null)
            {
                misc.cursed = false;
                misc.Activate(owner);
            }

            if (ring != null)
            {
                ring.cursed = false;
                ring.Activate(owner);
            }
        }

        public int Charge(float charge)
        {
            var count = 0;

            foreach(var charger in owner.Buffs<Wand.Charger>())
            {
                charger.GainCharge(charge);
                ++count;
            }

            return count;
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public BelongingsEnumerator GetEnumerator()
        {
            return new BelongingsEnumerator(this);
        }
    } // Belongings

    public class BelongingsEnumerator : IEnumerator
    {
        private Belongings belongings;
        private int index = 0;
        private Item[] equipped = { null, null, null, null, null };
        private const int backpackIndex = 5; //  equipped.length;
        private IEnumerator backpackEnumerator;

        public BelongingsEnumerator(Belongings belongings)
        {
            this.belongings = belongings;

            if (this.belongings.weapon != null)
                equipped[0] = this.belongings.weapon;

            if (this.belongings.armor != null)
                equipped[1] = this.belongings.armor;

            if (this.belongings.artifact != null)
                equipped[2] = this.belongings.artifact;

            if (this.belongings.misc != null)
                equipped[3] = this.belongings.misc;

            if (this.belongings.ring != null)
                equipped[4] = this.belongings.ring;

            backpackEnumerator = belongings.backpack.GetEnumerator();
        }

        // MoveNext가 호출되고 Current가 호출됨
        public bool MoveNext()
        {
            while (index < backpackIndex)
            {
                if (equipped[index] != null)
                    return true;

                ++index;
            }

            return backpackEnumerator.MoveNext();
        }

        public Item Current
        {
            get
            {
                try
                {
                    if (index < backpackIndex)
                        return equipped[index++];

                    return (Item)backpackEnumerator.Current;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void Reset()
        {
            index = 0;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }
    } // BelongingsEnumerator
}
