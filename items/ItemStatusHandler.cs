using System;
using System.Collections.Generic;
using watabou.utils;

namespace spdd.items
{
    public class ItemStatusHandler
    {
        private Type[] items;
        private Dictionary<Type, string> itemLabels;
        private Dictionary<string, int> labelImages;
        private HashSet<Type> known;

        public ItemStatusHandler(Type[] items, Dictionary<string, int> labelImages)
        {
            this.items = items;

            this.itemLabels = new Dictionary<Type, string>();
            this.labelImages = new Dictionary<string, int>(labelImages);
            known = new HashSet<Type>();

            List<string> labelsLeft = new List<string>(labelImages.Keys);

            foreach (var item in items)
            {
                int index = Rnd.Int(labelsLeft.Count);

                itemLabels.Add(item, labelsLeft[index]);
                labelsLeft.RemoveAt(index);
            }
        }

        public ItemStatusHandler(Type[] items, Dictionary<string, int> labelImages, Bundle bundle)
        {
            this.items = items;

            this.itemLabels = new Dictionary<Type, string>();
            this.labelImages = new Dictionary<string, int>(labelImages);
            known = new HashSet<Type>();

            List<string> allLabels = new List<string>(labelImages.Keys);

            Restore(bundle, allLabels);
        }

        private const string PFX_LABEL = "_label";
        private const string PFX_KNOWN = "_known";

        public void Save(Bundle bundle)
        {
            foreach (var t in items)
            {
                var itemName = t.ToString();
                bundle.Put(itemName + PFX_LABEL, itemLabels[t]);
                bundle.Put(itemName + PFX_KNOWN, known.Contains(t));
            }
        }

        public void SaveSelectively(Bundle bundle, List<Item> itemsToSave)
        {
            List<Type> items = new List<Type>(this.items);

            foreach (Item item in itemsToSave)
            {
                if (items.Contains(item.GetType()))
                {
                    var index = items.IndexOf(item.GetType());
                    var cls = items[index];
                    string itemName = cls.ToString();
                    bundle.Put(itemName + PFX_LABEL, itemLabels[cls]);
                    bundle.Put(itemName + PFX_KNOWN, known.Contains(cls));
                }
            }
        }

        public void SaveClassesSelectively(Bundle bundle, List<Type> clsToSave)
        {
            List<Type> items = new List<Type>(this.items);

            foreach (var cls in clsToSave)
            {
                if (items.Contains(cls))
                {
                    var index = items.IndexOf(cls);
                    Type toSave = items[index];
                    string itemName = toSave.ToString();
                    bundle.Put(itemName + PFX_LABEL, itemLabels[toSave]);
                    bundle.Put(itemName + PFX_KNOWN, known.Contains(toSave));
                }
            }
        }

        private void Restore(Bundle bundle, List<string> labelsLeft)
        {
            List<Type> unlabelled = new List<Type>();

            foreach (var item in items)
            {
                var itemName = item.ToString();

                if (bundle.Contains(itemName + PFX_LABEL))
                {
                    var label = bundle.GetString(itemName + PFX_LABEL);
                    itemLabels[item] = label;
                    labelsLeft.Remove(label);

                    if (bundle.GetBoolean(itemName + PFX_KNOWN))
                        known.Add(item);
                }
                else
                {
                    unlabelled.Add(item);
                }
            }

            foreach (Type item in unlabelled)
            {
                string itemName = item.ToString();

                int index = Rnd.Int(labelsLeft.Count);

                itemLabels.Add(item, labelsLeft[index]);
                labelsLeft.RemoveAt(index);

                if (bundle.Contains(itemName + PFX_KNOWN) && bundle.GetBoolean(itemName + PFX_KNOWN))
                {
                    known.Add(item);
                }
            }
        }

        //public boolean contains(T item)
        //{
        //    for (Class <? extends Item > i : items)
        //    {
        //        if (item.getClass().equals(i))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public bool Contains(Type itemCls)
        {
            foreach (Type i in items)
            {
                if (itemCls.Equals(i))
                {
                    return true;
                }
            }
            return false;
        }

        //public int image(T item)
        //{
        //    return labelImages.get(label(item));
        //}

        public int Image(Type itemCls)
        {
            return labelImages[Label(itemCls)];
        }

        //public string label(T item)
        //{
        //    return itemLabels.get(item.getClass());
        //}

        public string Label(Type itemCls)
        {
            return itemLabels[itemCls];
        }

        //public boolean isKnown(T item)
        //{
        //    return known.contains(item.getClass());
        //}

        public bool IsKnown(Type itemCls)
        {
            return known.Contains(itemCls);
        }

        //public void know(T item)
        //{
        //    known.add((Class <? extends T >)item.getClass());
        //}

        public void Know(Type itemCls)
        {
            known.Add(itemCls);
        }

        public HashSet<Type> Known()
        {
            return known;
        }

        public HashSet<Type> Unknown()
        {
            var result = new HashSet<Type>();

            foreach (var i in items)
                if (!known.Contains(i))
                    result.Add(i);

            return result;
        }
    }
}