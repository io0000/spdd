using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items.armor;
using spdd.items.artifacts;
using spdd.items.bombs;
using spdd.items.food;
using spdd.items.journal;
using spdd.items.potions;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.items.wands;
using spdd.journal;
using spdd.messages;
using spdd.sprites;

namespace spdd.items
{
    public class Heap : IBundlable
    {
        public enum Type
        {
            HEAP,
            FOR_SALE,
            CHEST,
            LOCKED_CHEST,
            CRYSTAL_CHEST,
            TOMB,
            SKELETON,
            REMAINS,
            //MIMIC //remains for pre-0.8.0 compatibility. There are converted to mimics on level load
        }

        public Type type = Type.HEAP;

        public int pos;

        public ItemSprite sprite;
        public bool seen;
        public bool haunted;

        public List<Item> items = new List<Item>();

        public void Open(Hero hero)
        {
            switch (type)
            {
                //case Type.MIMIC:
                //    type = Type.CHEST;
                //    break;
                case Type.TOMB:
                    Wraith.SpawnAround(hero.pos);
                    break;
                case Type.REMAINS:
                case Type.SKELETON:
                    CellEmitter.Center(pos).Start(Speck.Factory(Speck.RATTLE), 0.1f, 3);
                    break;
            }

            if (haunted)
            {
                if (Wraith.SpawnAt(pos) == null)
                {
                    hero.sprite.Emitter().Burst(ShadowParticle.Curse, 6);
                    hero.Damage(hero.HP / 2, this);
                }
                Sample.Instance.Play(Assets.Sounds.CURSED);
            }

            //if (type != Type.MIMIC)
            {
                type = Type.HEAP;
                List<Item> bonus = RingOfWealth.TryForBonusDrop(hero, 1);
                if (bonus != null && bonus.Count > 0)
                {
                    items.AddRange(bonus);
                    RingOfWealth.ShowFlareForBonusDrop(sprite);
                }
                sprite.Link();
                sprite.Drop();
            }
        }

        public Heap SetHauntedIfCursed()
        {
            foreach (Item item in items)
            {
                if (item.cursed)
                {
                    haunted = true;
                    item.cursedKnown = true;
                    break;
                }
            }
            return this;
        }

        public int Size()
        {
            return items.Count;
        }

        public Item PickUp()
        {
            if (items.Count == 0)
            {
                Destroy();
                return null;
            }

            var item = items[0];
            items.RemoveAt(0);

            if (items.Count == 0)
                Destroy();
            else if (sprite != null)
                sprite.View(this).Place(pos);

            return item;
        }

        public Item Peek()
        {
            //items.peek();
            if (items.Count > 0)
                return items[0];
            else
                return null;
        }

        public void Drop(Item item)
        {
            if (item.stackable && type != Type.FOR_SALE)
            {
                foreach (var i in items)
                {
                    if (i.IsSimilar(item))
                    {
                        item = i.Merge(item);
                        break;
                    }
                }

                items.Remove(item);
            }

            if (item.dropsDownHeap && type != Type.FOR_SALE)
                items.Add(item);
            else
                items.Insert(0, item);  // items.addFirst( item );

            if (sprite != null)
                sprite.View(this).Place(pos);
        }

        public void Replace(Item a, Item b)
        {
            var index = items.IndexOf(a);
            if (index == -1)
                return;
            //items.RemoveAt(index);
            //items.Insert(index, b);
            items[index] = b;
        }

        public void Remove(Item a)
        {
            items.Remove(a);
            if (items.Count == 0)
            {
                Destroy();
            }
            else if (sprite != null)
            {
                sprite.View(this).Place(pos);
            }
        }

        public void Burn()
        {
            if (type != Type.HEAP)
                return;

            var burnt = false;
            var evaporated = false;

            foreach (var item in items.ToArray())
            {
                if (item is Scroll && !item.unique)
                {
                    items.Remove(item);
                    burnt = true;
                }
                else if (item is Dewdrop)
                {
                    items.Remove(item);
                    evaporated = true;
                }
                else if (item is MysteryMeat)
                {
                    Replace(item, ChargrilledMeat.Cook((MysteryMeat)item));
                    burnt = true;
                }
                else if (item is Bomb)
                {
                    items.Remove(item);
                    ((Bomb)item).Explode(pos);
                    if (((Bomb)item).ExplodesDestructively())
                    {
                        //stop processing the burning, it will be replaced by the explosion.
                        return;
                    }
                    else
                    {
                        burnt = true;
                    }
                }
            }

            if (burnt || evaporated)
            {
                if (Dungeon.level.heroFOV[pos])
                {
                    if (burnt)
                        BurnFX(pos);
                    else
                        EvaporateFX(pos);
                }

                if (IsEmpty())
                    Destroy();
                else if (sprite != null)
                    sprite.View(this).Place(pos);
            }
        }

        //Note: should not be called to initiate an explosion, but rather by an explosion that is happening.
        public void Explode()
        {
            //breaks open most standard containers, mimics die.
            //if (type == Type.MIMIC || type == Type.CHEST || type == Type.SKELETON)
            if (type == Type.CHEST || type == Type.SKELETON)
            {
                type = Type.HEAP;
                sprite.Link();
                sprite.Drop();
                return;
            }

            if (type != Type.HEAP)
            {
                return;
            }
            else
            {
                foreach (Item item in items.ToArray())
                {
                    //unique items aren't affect by explosions
                    if (item.unique || (item is Armor && ((Armor)item).CheckSeal() != null))
                    {
                        continue;
                    }

                    if (item is Potion)
                    {
                        items.Remove(item);
                        ((Potion)item).Shatter(pos);
                    }
                    else if (item is Honeypot.ShatteredPot)
                    {
                        items.Remove(item);
                        ((Honeypot.ShatteredPot)item).DestroyPot(pos);
                    }
                    else if (item is Bomb)
                    {
                        items.Remove(item);
                        ((Bomb)item).Explode(pos);
                        if (((Bomb)item).ExplodesDestructively())
                        {
                            //stop processing current explosion, it will be replaced by the new one.
                            return;
                        }
                    }
                    else if (item.GetLevel() <= 0)
                    {
                        //upgraded items can endure the blast
                        items.Remove(item);
                    }
                }

                if (IsEmpty())
                {
                    Destroy();
                }
                else if (sprite != null)
                {
                    sprite.View(this).Place(pos);
                }
            }
        }

        public void Freeze()
        {
            if (type != Type.HEAP)
            {
                return;
            }

            bool frozen = false;
            foreach (Item item in items.ToArray())
            {
                if (item is MysteryMeat)
                {
                    Replace(item, FrozenCarpaccio.Cook((MysteryMeat)item));
                    frozen = true;
                }
                else if (item is Potion && !item.unique)
                {
                    items.Remove(item);
                    ((Potion)item).Shatter(pos);
                    frozen = true;
                }
                else if (item is Bomb)
                {
                    ((Bomb)item).fuse = null;
                    frozen = true;
                }
            }

            if (frozen)
            {
                if (IsEmpty())
                {
                    Destroy();
                }
                else if (sprite != null)
                {
                    sprite.View(this).Place(pos);
                }
            }
        }

        public static void BurnFX(int pos)
        {
            CellEmitter.Get(pos).Burst(ElmoParticle.Factory, 6);
            Sample.Instance.Play(Assets.Sounds.BURNING);
        }

        public static void EvaporateFX(int pos)
        {
            CellEmitter.Get(pos).Burst(Speck.Factory(Speck.STEAM), 5);
        }

        public bool IsEmpty()
        {
            return items == null || items.Count == 0;
        }

        public void Destroy()
        {
            Dungeon.level.heaps.Remove(pos);

            if (sprite != null)
                sprite.Kill();

            items.Clear();
        }

        public override string ToString()
        {
            switch (type)
            {
                case Type.FOR_SALE:
                    Item i = Peek();
                    return Messages.Get(this, "for_sale", Shopkeeper.SellPrice(i), i.ToString());
                case Type.CHEST:
                    //case Type.MIMIC:
                    return Messages.Get(this, "chest");
                case Type.LOCKED_CHEST:
                    return Messages.Get(this, "locked_chest");
                case Type.CRYSTAL_CHEST:
                    return Messages.Get(this, "crystal_chest");
                case Type.TOMB:
                    return Messages.Get(this, "tomb");
                case Type.SKELETON:
                    return Messages.Get(this, "skeleton");
                case Type.REMAINS:
                    return Messages.Get(this, "remains");
                default:
                    return Peek().ToString();
            }
        }

        public string Info()
        {
            switch (type)
            {
                case Type.CHEST:
                    //case Type.MIMIC:
                    return Messages.Get(this, "chest_desc");
                case Type.LOCKED_CHEST:
                    return Messages.Get(this, "locked_chest_desc");
                case Type.CRYSTAL_CHEST:
                    if (Peek() is Artifact)
                        return Messages.Get(this, "crystal_chest_desc", Messages.Get(this, "artifact"));
                    else if (Peek() is Wand)
                        return Messages.Get(this, "crystal_chest_desc", Messages.Get(this, "wand"));
                    else
                        return Messages.Get(this, "crystal_chest_desc", Messages.Get(this, "ring"));
                case Type.TOMB:
                    return Messages.Get(this, "tomb_desc");
                case Type.SKELETON:
                    return Messages.Get(this, "skeleton_desc");
                case Type.REMAINS:
                    return Messages.Get(this, "remains_desc");
                default:
                    return Peek().Info();
            }
        }

        private const string POS = "pos";
        private const string SEEN = "seen";
        private const string TYPE = "type";
        private const string ITEMS = "items";
        private const string HAUNTED = "haunted";

        public void RestoreFromBundle(Bundle bundle)
        {
            pos = bundle.GetInt(POS);
            seen = bundle.GetBoolean(SEEN);
            type = bundle.GetEnum<Type>(TYPE);

            items = new List<Item>();
            foreach (var item in bundle.GetCollection(ITEMS))
                items.Add((Item)item);

            //remove any document pages that either don't exist anymore or that the player already has
            foreach (var item in items.ToArray())
            {
                if (item is DocumentPage)
                {
                    var docPage = (DocumentPage)item;
                    var page = docPage.Page();
                    Document doc = docPage.GetDocument();
                    var pages = doc.Pages();

                    if (pages.Contains(page) || doc.HasPage(page))
                    {
                        items.Remove(item);
                    }
                }
            }

            haunted = bundle.GetBoolean(HAUNTED);
        }

        public void StoreInBundle(Bundle bundle)
        {
            bundle.Put(POS, pos);
            bundle.Put(SEEN, seen);
            bundle.Put(TYPE, type.ToString());  // enum
            bundle.Put(ITEMS, items);
            bundle.Put(HAUNTED, haunted);
        }
    }
}