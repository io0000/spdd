using System.Collections.Generic;
using watabou.utils;
using spdd.items;

namespace spdd
{
    public class QuickSlot
    {
        /**
         * Slots contain objects which are also in a player's inventory. The one exception to this is when quantity is 0,
         * which can happen for a stackable item that has been 'used up', these are refered to a placeholders.
         */

        //note that the current max size is coded at 4, due to UI constraints, but it could be much much bigger with no issue.
        public const int SIZE = 4;
        private Item[] slots = new Item[SIZE];

        //direct array interaction methods, everything should build from these methods.
        public void SetSlot(int slot, Item item)
        {
            ClearItem(item); //we don't want to allow the same item in multiple slots.
            slots[slot] = item;
        }

        public void ClearSlot(int slot)
        {
            slots[slot] = null;
        }

        public void Reset()
        {
            slots = new Item[SIZE];
        }

        public Item GetItem(int slot)
        {
            return slots[slot];
        }

        //utility methods, for easier use of the internal array.
        public int GetSlot(Item item)
        {
            for (int i = 0; i < SIZE; ++i)
            {
                if (GetItem(i) == item)
                    return i;
            }
            return -1;
        }

        public bool IsPlaceholder(int slot)
        {
            return GetItem(slot) != null && GetItem(slot).Quantity() == 0;
        }

        public bool IsNonePlaceholder(int slot)
        {
            return GetItem(slot) != null && GetItem(slot).Quantity() > 0;
        }

        public void ClearItem(Item item)
        {
            if (Contains(item))
                ClearSlot(GetSlot(item));
        }

        public bool Contains(Item item)
        {
            return GetSlot(item) != -1;
        }

        public void ReplacePlaceholder(Item item)
        {
            for (int i = 0; i < SIZE; ++i)
            {
                if (IsPlaceholder(i) && item.IsSimilar(GetItem(i)))
                    SetSlot(i, item);
            }
        }

        public void ConvertToPlaceholder(Item item)
        {
            if (Contains(item))
            {
                Item placeholder = item.Virtual();
                if (placeholder == null)
                    return;

                for (int i = 0; i < SIZE; ++i)
                {
                    if (GetItem(i) == item)
                        SetSlot(i, placeholder);
                }
            }
        }

        public Item RandomNonePlaceholder()
        {
            var result = new List<Item>();
            for (int i = 0; i < SIZE; ++i)
            {
                if (GetItem(i) != null && !IsPlaceholder(i))
                    result.Add(GetItem(i));
            }

            return Rnd.Element(result);
        }

        private const string PLACEHOLDERS = "placeholders";
        private const string PLACEMENTS = "placements";

        /**
         * Placements array is used as order is preserved while bundling, but exact index is not, so if we
         * bundle both the placeholders (which preserves their order) and an array telling us where the placeholders are,
         * we can reconstruct them perfectly.
         */

        public void StorePlaceholders(Bundle bundle)
        {
            List<Item> placeholders = new List<Item>(SIZE);
            bool[] placements = new bool[SIZE];

            for (int i = 0; i < SIZE; ++i)
            {
                if (IsPlaceholder(i))
                {
                    placeholders.Add(GetItem(i));
                    placements[i] = true;
                }
            }
            bundle.Put(PLACEHOLDERS, placeholders);
            bundle.Put(PLACEMENTS, placements);
        }

        public void RestorePlaceholders(Bundle bundle)
        {
            var placeholders = bundle.GetCollection(PLACEHOLDERS);
            var placements = bundle.GetBooleanArray(PLACEMENTS);

            int i = 0;
            foreach (var item in placeholders)
            {
                while (!placements[i])
                    ++i;

                SetSlot(i, (Item)item);
                ++i;
            }
        }
    }
}