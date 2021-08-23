using System;
using spdd.actors.hero;
using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.items.artifacts;
using spdd.messages;
using spdd.ui;

namespace spdd.windows
{
    public class WndTradeItem : WndInfoItem
    {
        private const float GAP = 2;
        private const int BTN_HEIGHT = 16;

        private WndBag owner;

        //selling
        public WndTradeItem(Item item, WndBag owner)
            : base(item)
        {
            this.owner = owner;

            float pos = height;

            if (item.Quantity() == 1)
            {
                var btnSell = new ActionRedButton(Messages.Get(this, "sell", item.Value()));
                btnSell.action = () =>
                {
                    Sell(item);
                    Hide();
                };
                btnSell.SetRect(0, pos + GAP, width, BTN_HEIGHT);
                Add(btnSell);

                pos = btnSell.Bottom();
            }
            else
            {
                int priceAll = item.Value();
                var btnSell1 = new ActionRedButton(Messages.Get(this, "sell_1", priceAll / item.Quantity()));
                btnSell1.action = () =>
                {
                    SellOne(item);
                    Hide();
                };

                btnSell1.SetRect(0, pos + GAP, width, BTN_HEIGHT);
                Add(btnSell1);

                var btnSellAll = new ActionRedButton(Messages.Get(this, "sell_all", priceAll));
                btnSellAll.action = () =>
                {
                    Sell(item);
                    Hide();
                };
                btnSellAll.SetRect(0, btnSell1.Bottom() + 1, width, BTN_HEIGHT);
                Add(btnSellAll);

                pos = btnSellAll.Bottom();
            }

            Resize(width, (int)pos);
        }

        public WndTradeItem(Heap heap)
            : base(heap)
        {
            Item item = heap.Peek();

            float pos = height;

            int price = Shopkeeper.SellPrice(item);

            var btnBuy = new ActionRedButton(Messages.Get(this, "buy", price));
            btnBuy.action = () =>
            {
                Hide();
                Buy(heap);
            };
            btnBuy.SetRect(0, pos + GAP, width, BTN_HEIGHT);
            btnBuy.Enable(price <= Dungeon.gold);
            Add(btnBuy);

            pos = btnBuy.Bottom();

            var thievery = Dungeon.hero.FindBuff<MasterThievesArmband.Thievery>();
            if (thievery != null && !thievery.IsCursed())
            {
                float chance = thievery.StealChance(price);
                var btnSteal = new ActionRedButton(Messages.Get(this, "steal", Math.Min(100, (int)(chance * 100))));
                btnSteal.action = () =>
                {
                    if (thievery.Steal(price))
                    {
                        Hero hero = Dungeon.hero;
                        Item item1 = heap.PickUp();
                        Hide();

                        if (!item1.DoPickUp(hero))
                        {
                            Dungeon.level.Drop(item1, heap.pos).sprite.Drop();
                        }
                    }
                    else
                    {
                        foreach (var mob in Dungeon.level.mobs)
                        {
                            if (mob is Shopkeeper)
                            {
                                mob.Yell(Messages.Get(mob, "thief"));
                                ((Shopkeeper)mob).Flee();
                                break;
                            }
                        }
                        Hide();
                    }
                };
                btnSteal.SetRect(0, pos + 1, width, BTN_HEIGHT);
                Add(btnSteal);

                pos = btnSteal.Bottom();
            }

            Resize(width, (int)pos);
        }

        public override void Hide()
        {
            base.Hide();

            if (owner != null)
            {
                owner.Hide();
                Shopkeeper.Sell();
            }
        }

        private void Sell(Item item)
        {
            Hero hero = Dungeon.hero;

            if (item.IsEquipped(hero) && !((EquipableItem)item).DoUnequip(hero, false))
            {
                return;
            }
            item.DetachAll(hero.belongings.backpack);

            new Gold(item.Value()).DoPickUp(hero);

            //selling items in the sell interface doesn't spend time
            hero.Spend(-hero.Cooldown());
        }

        private void SellOne(Item item)
        {
            if (item.Quantity() <= 1)
            {
                Sell(item);
            }
            else
            {
                Hero hero = Dungeon.hero;

                item = item.Detach(hero.belongings.backpack);

                new Gold(item.Value()).DoPickUp(hero);

                //selling items in the sell interface doesn't spend time
                hero.Spend(-hero.Cooldown());
            }
        }

        private void Buy(Heap heap)
        {
            Item item = heap.PickUp();
            if (item == null)
                return;

            int price = Shopkeeper.SellPrice(item);
            Dungeon.gold -= price;

            if (!item.DoPickUp(Dungeon.hero))
            {
                Dungeon.level.Drop(item, heap.pos).sprite.Drop();
            }
        }
    }
}