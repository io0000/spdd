using watabou.utils;
using watabou.input;
using spdd.items;

namespace spdd.ui
{
    public class LootIndicator : Tag
    {
        private ItemSlotLoot slot;

        private Item lastItem;
        private int lastQuantity;

        public LootIndicator()
            : base(new Color(0x1F, 0x75, 0xCC, 0xFF))
        {
            SetSize(24, 24);

            visible = false;
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();

            slot = new ItemSlotLoot();
            slot.ShowExtraInfo(false);
            Add(slot);
        }

        private class ItemSlotLoot : ItemSlot
        {
            protected override void OnClick()
            {
                if (Dungeon.hero.Handle(Dungeon.hero.pos))
                    Dungeon.hero.Next();
            }

            public override GameAction KeyAction()
            {
                return SPDAction.TAG_LOOT;
            }
        }

        protected override void Layout()
        {
            base.Layout();

            slot.SetRect(x + 2, y + 3, width - 3, height - 6);
        }

        public override void Update()
        {
            if (Dungeon.hero.ready)
            {
                Heap heap = Dungeon.level.heaps[Dungeon.hero.pos];
                if (heap != null)
                {
                    Item item =
                        //heap.type == Heap.Type.CHEST || heap.type == Heap.Type.MIMIC ? ItemSlot.CHEST :
                        heap.type == Heap.Type.CHEST ? ItemSlot.CHEST :
                        heap.type == Heap.Type.LOCKED_CHEST ? ItemSlot.LOCKED_CHEST :
                        heap.type == Heap.Type.CRYSTAL_CHEST ? ItemSlot.CRYSTAL_CHEST :
                        heap.type == Heap.Type.TOMB ? ItemSlot.TOMB :
                        heap.type == Heap.Type.SKELETON ? ItemSlot.SKELETON :
                        heap.type == Heap.Type.REMAINS ? ItemSlot.REMAINS :
                        heap.Peek();

                    if (item != lastItem || item.Quantity() != lastQuantity)
                    {
                        lastItem = item;
                        lastQuantity = item.Quantity();

                        slot.Item(item);
                        Flash();
                    }
                    visible = true;
                }
                else
                {
                    lastItem = null;
                    visible = false;
                }
            }

            slot.Enable(visible && Dungeon.hero.ready);

            base.Update();
        }
    }
}