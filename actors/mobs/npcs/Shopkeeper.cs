using System.Linq;
using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.items.armor;
using spdd.journal;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.windows;

namespace spdd.actors.mobs.npcs
{
    public class Shopkeeper : NPC
    {
        public Shopkeeper()
        {
            spriteClass = typeof(ShopkeeperSprite);

            properties.Add(Property.IMMOVABLE);
        }

        public override bool Act()
        {
            if (Dungeon.level.heroFOV[pos])
            {
                Notes.Add(Notes.Landmark.SHOP);
            }

            sprite.TurnTo(pos, Dungeon.hero.pos);
            Spend(TICK);
            return base.Act();
        }

        public override void Damage(int dmg, object src)
        {
            Flee();
        }

        public override void Add(Buff buff)
        {
            Flee();
        }

        public virtual void Flee()
        {
            Destroy();

            Notes.Remove(Notes.Landmark.SHOP);

            sprite.KillAndErase();
            CellEmitter.Get(pos).Burst(ElmoParticle.Factory, 6);
        }

        public override void Destroy()
        {
            base.Destroy();
            foreach (var heap in Dungeon.level.heaps.Values.ToList())
            {
                if (heap.type == Heap.Type.FOR_SALE)
                {
                    CellEmitter.Get(heap.pos).Burst(ElmoParticle.Factory, 4);
                    heap.Destroy();     // Dungeon.level.heaps에서 제거됨 -> foreach의 ToList()를 사용한 이유
                }
            }
        }

        public override bool Reset()
        {
            return true;
        }

        //shopkeepers are greedy!
        public static int SellPrice(Item item)
        {
            return item.Value() * 5 * (Dungeon.depth / 5 + 1);
        }

        public static WndBag Sell()
        {
            return GameScene.SelectItem(itemSelector,
                WndBag.Mode.FOR_SALE,
                Messages.Get(typeof(Shopkeeper), "sell"));
        }

        public static bool WillBuyItem(Item item)
        {
            if (item.Value() < 0)
                return false;

            // [FIXED]
            //if (item.unique && !item.stackable)
            //    return false;
            // ex) CeremonialCandle : unique && stackable
            if (item.unique)
                return false;

            if (item is Armor && ((Armor)item).CheckSeal() != null)
                return false;

            if (item.IsEquipped(Dungeon.hero) && item.cursed)
                return false;

            return true;
        }

        private static WndBag.IListener itemSelector = new ShopkeeperListener();

        class ShopkeeperListener : WndBag.IListener
        {
            public void OnSelect(Item item)
            {
                if (item == null)
                    return;

                var parentWnd = Shopkeeper.Sell();
                GameScene.Show(new WndTradeItem(item, parentWnd));
            }
        }

        public override bool Interact(Character c)
        {
            if (c != Dungeon.hero)
                return true;

            Sell();
            return true;
        }
    }
}