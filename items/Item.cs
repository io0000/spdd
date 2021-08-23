using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.items.bags;
using spdd.journal;
using spdd.mechanics;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;
using spdd.utils;

namespace spdd.items
{
    public class Item : IBundlable
    {
        private const string TXT_TO_STRING_LVL = "%s %+d";
        private const string TXT_TO_STRING_X = "%s x%d";

        public const float TIME_TO_THROW = 1.0f;
        public const float TIME_TO_PICK_UP = 1.0f;
        public const float TIME_TO_DROP = 1.0f;

        public const string AC_DROP = "DROP";
        public const string AC_THROW = "THROW";

        public string defaultAction;
        public bool usesTargeting;

        //TODO should these be private and accessed through methods?
        public int image;
        public int icon = -1; //used as an identifier for items with randomized images

        public bool stackable;
        public int quantity = 1;
        public bool dropsDownHeap;

        private int level;

        public bool levelKnown;

        public bool cursed;
        public bool cursedKnown;

        // Unique items persist through revival
        public bool unique;

        // whether an item can be included in heroes remains
        public bool bones;

        //private static Comparator<Item> itemComparator = new Comparator<Item>() {	
        //    public int compare( Item lhs, Item rhs ) {
        //        return Generator.Category.order( lhs ) - Generator.Category.order( rhs );
        //    }
        //};

        public virtual List<string> Actions(Hero hero)
        {
            var actions = new List<string>();
            actions.Add(AC_DROP);
            actions.Add(AC_THROW);
            return actions;
        }

        public virtual bool DoPickUp(Hero hero)
        {
            if (Collect(hero.belongings.backpack))
            {
                GameScene.PickUp(this, hero.pos);
                Sample.Instance.Play(Assets.Sounds.ITEM);
                hero.SpendAndNext(TIME_TO_PICK_UP);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void DoDrop(Hero hero)
        {
            hero.SpendAndNext(TIME_TO_DROP);
            int pos = hero.pos;
            Dungeon.level.Drop(DetachAll(hero.belongings.backpack), pos).sprite.Drop(pos);
        }

        //resets an item's properties, to ensure consistency between runs
        public virtual void Reset()
        { }

        public virtual void DoThrow(Hero hero)
        {
            GameScene.SelectCell(thrower);
        }

        public virtual void Execute(Hero hero, string action)
        {
            GameScene.Cancel();
            curUser = hero;
            curItem = this;

            if (action.Equals(AC_DROP))
            {
                if (hero.belongings.backpack.Contains(this) || IsEquipped(hero))
                    DoDrop(hero);
            }
            else if (action.Equals(AC_THROW))
            {
                if (hero.belongings.backpack.Contains(this) || IsEquipped(hero))
                    DoThrow(hero);
            }
        }

        public void Execute(Hero hero)
        {
            Execute(hero, defaultAction);
        }

        public virtual void OnThrow(int cell)
        {
            var heap = Dungeon.level.Drop(this, cell);
            if (!heap.IsEmpty())
                heap.sprite.Drop(cell);
        }

        //takes two items and merges them (if possible)
        public virtual Item Merge(Item other)
        {
            if (IsSimilar(other))
            {
                quantity += other.quantity;
                other.quantity = 0;
            }
            return this;
        }

        public virtual bool Collect(Bag container)
        {
            if (quantity <= 0)
                return true;

            var items = container.items;

            foreach (var item in items)
            {
                if (item is Bag && ((Bag)item).CanHold(this))
                {
                    if (Collect((Bag)item))
                        return true;
                }
            }

            if (!container.CanHold(this))
            {
                GLog.Negative(Messages.Get(typeof(Item), "pack_full", container.Name()));
                return false;
            }

            if (items.Contains(this))
                return true;

            if (stackable)
            {
                foreach (var item in items)
                {
                    if (IsSimilar(item))
                    {
                        item.Merge(this);
                        Item.UpdateQuickslot();
                        return true;
                    }
                }
            }

            if (Dungeon.hero != null && Dungeon.hero.IsAlive())
                BadgesExtensions.ValidateItemLevelAquired(this);

            items.Add(this);
            Dungeon.quickslot.ReplacePlaceholder(this);
            UpdateQuickslot();
            //Collections.sort(items, itemComparator);
            items.Sort(delegate (Item lhs, Item rhs)
            {
                //return Generator.Category.Order(lhs) - Generator.Category.Order(rhs);
                int l = Generator.Order(lhs);
                int r = Generator.Order(rhs);
                return l - r;
            });

            return true;
        }

        public bool Collect()
        {
            return Collect(Dungeon.hero.belongings.backpack);
        }

        //returns a new item if the split was sucessful and there are now 2 items, otherwise null
        public virtual Item Split(int amount)
        {
            if (amount <= 0 || amount >= Quantity())
            {
                return null;
            }
            else
            {
                //pssh, who needs copy constructors?
                Item split = (Item)Reflection.NewInstance(GetType());

                if (split == null)
                    return null;

                Bundle copy = new Bundle();
                this.StoreInBundle(copy);
                split.RestoreFromBundle(copy);
                split.Quantity(amount);
                quantity -= amount;

                return split;
            }
        }

        public Item Detach(Bag container)
        {
            if (quantity <= 0)
            {
                return null;
            }
            else if (quantity == 1)
            {
                if (stackable)
                    Dungeon.quickslot.ConvertToPlaceholder(this);

                return DetachAll(container);
            }
            else
            {
                Item detached = Split(1);
                UpdateQuickslot();
                if (detached != null)
                    detached.OnDetach();
                return detached;
            }
        }

        public Item DetachAll(Bag container)
        {
            Dungeon.quickslot.ClearItem(this);
            UpdateQuickslot();

            foreach (var item in container.items)
            {
                if (item == this)
                {
                    container.items.Remove(this);
                    item.OnDetach();
                    container.GrabItems(); //try to put more items into the bag as it now has free space
                    return this;
                }
                else if (item is Bag)
                {
                    var bag = (Bag)item;
                    if (bag.Contains(this))
                        return DetachAll(bag);
                }
            }

            return this;
        }

        public virtual bool IsSimilar(Item item)
        {
            return level == item.level && GetType() == item.GetType();
        }

        protected virtual void OnDetach()
        { }

        //returns the true level of the item, only affected by modifiers which are persistent (e.g. curse infusion)
        // public int level()
        public virtual int GetLevel()
        {
            return level;
        }

        //returns the level of the item, after it may have been modified by temporary boosts/reductions
        //note that not all item properties should care about buffs/debuffs! (e.g. str requirement)
        public virtual int BuffedLvl()
        {
            if (Dungeon.hero.FindBuff<Degrade>() != null)
            {
                return actors.buffs.Degrade.ReduceLevel(GetLevel());
            }
            else
            {
                return GetLevel();
            }
        }

        // public void Level(int value)
        public virtual void SetLevel(int value)
        {
            level = value;

            UpdateQuickslot();
        }

        public virtual Item Upgrade()
        {
            ++level;

            UpdateQuickslot();

            return this;
        }

        public Item Upgrade(int n)
        {
            for (var i = 0; i < n; ++i)
                Upgrade();

            return this;
        }

        public virtual Item Degrade()
        {
            --level;

            return this;
        }

        public Item Degrade(int n)
        {
            for (var i = 0; i < n; ++i)
                Degrade();

            return this;
        }

        public virtual int VisiblyUpgraded()
        {
            return levelKnown ? GetLevel() : 0;
        }

        public virtual int BuffedVisiblyUpgraded()
        {
            return levelKnown ? BuffedLvl() : 0;
        }

        //public bool VisiblyCursed()
        //{
        //    return cursed && cursedKnown;
        //}

        public virtual bool IsUpgradable()
        {
            return true;
        }

        public virtual bool IsIdentified()
        {
            return levelKnown && cursedKnown;
        }

        public virtual bool IsEquipped(Hero hero)
        {
            return false;
        }

        public virtual Item Identify()
        {
            levelKnown = true;
            cursedKnown = true;

            if (Dungeon.hero != null && Dungeon.hero.IsAlive())
            {
                CatalogExtensions.SetSeen(GetType());
            }

            return this;
        }

        public virtual void OnHeroGainExp(float levelPercent, Hero hero)
        {
            //do nothing by default
        }

        public static void Evoke(Hero hero)
        {
            hero.sprite.Emitter().Burst(Speck.Factory(Speck.EVOKE), 5);
        }

        public override string ToString()
        {
            var name = Name();

            if (VisiblyUpgraded() != 0)
                name = Messages.Format(TXT_TO_STRING_LVL, name, VisiblyUpgraded());

            if (quantity > 1)
                name = Messages.Format(TXT_TO_STRING_X, name, quantity);

            return name;
        }

        public virtual string Name()
        {
            return TrueName();
        }

        public string TrueName()
        {
            return Messages.Get(this, "name");
        }

        public virtual int Image()
        {
            return image;
        }

        public virtual ItemSprite.Glowing Glowing()
        {
            return null;
        }

        public virtual Emitter Emitter()
        {
            return null;
        }

        public virtual string Info()
        {
            return Desc();
        }

        public virtual string Desc()
        {
            return Messages.Get(this, "desc");
        }

        public int Quantity()
        {
            return quantity;
        }

        public virtual Item Quantity(int value)
        {
            quantity = value;
            return this;
        }

        public virtual int Value()
        {
            return 0;
        }

        public Item Virtual()
        {
            Item item = (Item)Reflection.NewInstance(GetType());
            if (item == null)
                return null;

            item.quantity = 0;
            item.level = level;
            return item;
        }

        public virtual Item Random()
        {
            return this;
        }

        public virtual string Status()
        {
            return quantity != 1 ? quantity.ToString() : null;
        }

        public static void UpdateQuickslot()
        {
            QuickSlotButton.Refresh();
        }

        private const string QUANTITY = "quantity";
        private const string LEVEL = "level";
        private const string LEVEL_KNOWN = "levelKnown";
        private const string CURSED = "cursed";
        private const string CURSED_KNOWN = "cursedKnown";
        private const string QUICKSLOT = "quickslotpos";

        public virtual void StoreInBundle(Bundle bundle)
        {
            bundle.Put(QUANTITY, quantity);
            bundle.Put(LEVEL, level);
            bundle.Put(LEVEL_KNOWN, levelKnown);
            bundle.Put(CURSED, cursed);
            bundle.Put(CURSED_KNOWN, cursedKnown);
            if (Dungeon.quickslot.Contains(this))
            {
                bundle.Put(QUICKSLOT, Dungeon.quickslot.GetSlot(this));
            }
        }

        public virtual void RestoreFromBundle(Bundle bundle)
        {
            quantity = bundle.GetInt(QUANTITY);
            levelKnown = bundle.GetBoolean(LEVEL_KNOWN);
            cursedKnown = bundle.GetBoolean(CURSED_KNOWN);

            var level = bundle.GetInt(LEVEL);
            if (level > 0)
                Upgrade(level);
            else if (level < 0)
                Degrade(-level);

            cursed = bundle.GetBoolean(CURSED);

            //only want to populate slot on first load.
            if (Dungeon.hero == null)
            {
                if (bundle.Contains(QUICKSLOT))
                {
                    Dungeon.quickslot.SetSlot(bundle.GetInt(QUICKSLOT), this);
                }
            }
        }

        public virtual int ThrowPos(Hero user, int dst)
        {
            return new Ballistic(user.pos, dst, Ballistic.PROJECTILE).collisionPos;
        }

        public virtual void ThrowSound()
        {
            Sample.Instance.Play(Assets.Sounds.MISS, 0.6f, 0.6f, 1.5f);
        }

        public virtual void Cast(Hero user, int dst)
        {
            int cell = ThrowPos(user, dst);
            user.sprite.Zap(cell);
            user.Busy();

            ThrowSound();

            var enemy = Actor.FindChar(cell);
            QuickSlotButton.Target(enemy);

            float delay = CastDelay(user, dst);

            var callback = new ActionCallback();
            callback.action = () =>
            {
                Item.curUser = user;
                Detach(user.belongings.backpack).OnThrow(cell);
                user.SpendAndNext(delay);
            };

            if (enemy != null)
            {
                user.sprite.parent.Recycle<MissileSprite>().
                    Reset(user.sprite,
                            enemy.sprite,
                            this,
                            callback);
            }
            else
            {
                user.sprite.parent.Recycle<MissileSprite>().
                    Reset(user.sprite,
                            cell,
                            this,
                            callback);
            }
        }

        public virtual float CastDelay(Character user, int dst)
        {
            return TIME_TO_THROW;
        }

        public static Hero curUser;
        public static Item curItem;
        protected static CellSelector.IListener thrower = new ItemCellSelector();

        public class ItemCellSelector : CellSelector.IListener
        {
            public void OnSelect(int? target)
            {
                if (target != null)
                    Item.curItem.Cast(Item.curUser, target.Value);
            }

            public string Prompt()
            {
                return Messages.Get(typeof(Item), "prompt");
            }
        }
    }
}