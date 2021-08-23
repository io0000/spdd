using System;
using System.Collections;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.scenes;
using spdd.windows;

namespace spdd.items.bags
{
    public class Bag : Item, IEnumerable
    {
        public const string AC_OPEN = "OPEN";

        public Bag()
        {
            image = 11;

            defaultAction = AC_OPEN;

            unique = true;
        }

        public Character owner;
        public List<Item> items = new List<Item>();

        public virtual int Capacity()
        {
            return 20; // default container size
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_OPEN))
                GameScene.Show(new WndBag(this, null, WndBag.Mode.ALL, null));
        }

        public override bool Collect(Bag container)
        {
            GrabItems(container);

            if (base.Collect(container))
            {
                owner = container.owner;

                BadgesExtensions.ValidateAllBagsBought(this);

                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnDetach()
        {
            owner = null;
            foreach (var item in items)
                Dungeon.quickslot.ClearItem(item);
            UpdateQuickslot();
        }

        public void GrabItems()
        {
            if (owner != null &&
                owner is Hero &&
                this != ((Hero)owner).belongings.backpack)
            {
                GrabItems(((Hero)owner).belongings.backpack);
            }
        }

        public void GrabItems(Bag container)
        {
            foreach (Item item in container.items.ToArray())
            {
                if (CanHold(item))
                {
                    int slot = Dungeon.quickslot.GetSlot(item);
                    item.DetachAll(container);

                    if (!item.Collect(this))
                        item.Collect(container);

                    if (slot != -1)
                        Dungeon.quickslot.SetSlot(slot, item);
                }
            }
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public void Clear()
        {
            items.Clear();
        }

        public void Resurrect()
        {
            foreach (Item item in items.ToArray())
            {
                if (!item.unique)
                    items.Remove(item);
            }
        }

        private const string ITEMS = "inventory";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(ITEMS, items);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            foreach (var item in bundle.GetCollection(ITEMS))
            {
                if (item != null)
                    ((Item)item).Collect(this);
            }
        }

        public bool Contains(Item item)
        {
            foreach (var i in items)
            {
                if (i == item)
                    return true;

                if (i is Bag &&
                    ((Bag)i).Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool CanHold(Item item)
        {
            if (items.Contains(item) ||
                item is Bag ||
                items.Count < Capacity())
            {
                return true;
            }
            else if (item.stackable)
            {
                foreach (Item i in items)
                {
                    if (item.IsSimilar(i))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public BagEnumerator GetEnumerator()
        {
            return new BagEnumerator(this);
        }
    }

    public class BagEnumerator : IEnumerator
    {
        private List<Item> Items;
        private int index = 0;
        private BagEnumerator nested;

        public BagEnumerator(Bag bag)
        {
            Items = bag.items;
        }

        public bool HasNext()
        {
            return (index < Items.Count);
        }

        public bool MoveNext()
        {
            if (nested != null)
            {
                // index < Items.Count 검사 이유
                // bag에 더 이상 아이템이 없어도 상위 bag에 아이템 있는지 검사 
                return nested.MoveNext() || index < Items.Count;
            }
            else
            {
                return (index < Items.Count);
            }
        }

        public Item Current
        {
            get
            {
                if (nested != null && nested.HasNext())
                {
                    return nested.Current;
                }
                else
                {
                    nested = null;

                    try
                    {
                        Item item = Items[index++];
                        if (item is Bag)
                            nested = ((Bag)item).GetEnumerator();

                        return item;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
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
    }
}