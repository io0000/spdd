using watabou.input;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.actors;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.windows;

namespace spdd.ui
{
    public class QuickSlotButton : Button, WndBag.IListener
    {
        private static QuickSlotButton[] instance = new QuickSlotButton[4];
        private int slotNum;

        private ItemSlot slot;

        private static Image crossB;
        private static Image crossM;

        private static bool targeting;
        public static Character lastTarget;

        public QuickSlotButton(int slotNum)
        {
            this.slotNum = slotNum;
            Item(Select(slotNum));

            instance[slotNum] = this;
        }

        public override void Destroy()
        {
            base.Destroy();
            Reset();
        }

        public static void Reset()
        {
            instance = new QuickSlotButton[4];

            lastTarget = null;
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();

            slot = new ItemSlotQuick(this);
            slot.ShowExtraInfo(false);
            Add(slot);

            crossB = Icons.TARGET.Get();
            crossB.visible = false;
            Add(crossB);

            crossM = new Image();
            crossM.Copy(crossB);
        }

        class ItemSlotQuick : ItemSlot
        {
            private QuickSlotButton quickslotButton;

            public ItemSlotQuick(QuickSlotButton quickSlot)
            {
                quickslotButton = quickSlot;
            }

            protected override void OnClick()
            {
                quickslotButton.OnItemSlotClick();
            }

            public override GameAction KeyAction()
            {
                return quickslotButton.KeyAction();
            }

            protected override bool OnLongClick()
            {
                return quickslotButton.OnLongClick();
            }

            protected override void OnPointerDown()
            {
                sprite.Lightness(0.7f);
            }

            protected override void OnPointerUp()
            {
                sprite.ResetColor();
            }
        }

        public void OnItemSlotClick()
        {
            Item item = Select(slotNum);

            if (targeting)
            {
                int cell = AutoAim(lastTarget, item);

                if (cell != -1)
                {
                    GameScene.HandleCell(cell);
                }
                else
                {
                    //couldn't auto-aim, just target the position and hope for the best.
                    GameScene.HandleCell(lastTarget.pos);
                }
            }
            else
            {
                if (item.usesTargeting)
                    UseTargeting();
                item.Execute(Dungeon.hero);
            }
        }

        protected override void Layout()
        {
            base.Layout();

            slot.Fill(this);

            crossB.x = x + (width - crossB.width) / 2;
            crossB.y = y + (height - crossB.height) / 2;
            PixelScene.Align(crossB);
        }

        public override void Update()
        {
            base.Update();
            if (targeting && lastTarget != null && lastTarget.sprite != null)
            {
                crossM.Point(lastTarget.sprite.Center(crossM));
            }
        }

        public override GameAction KeyAction()
        {
            switch (slotNum)
            {
                case 0:
                    return SPDAction.QUICKSLOT_1;
                case 1:
                    return SPDAction.QUICKSLOT_2;
                case 2:
                    return SPDAction.QUICKSLOT_3;
                case 3:
                    return SPDAction.QUICKSLOT_4;
                default:
                    return base.KeyAction();
            }
        }

        protected override void OnClick()
        {
            GameScene.SelectItem(this, WndBag.Mode.QUICKSLOT, Messages.Get(this, "select_item"));
        }

        protected override bool OnLongClick()
        {
            GameScene.SelectItem(this, WndBag.Mode.QUICKSLOT, Messages.Get(this, "select_item"));
            return true;
        }

        private static Item Select(int slotNum)
        {
            return Dungeon.quickslot.GetItem(slotNum);
        }

        // WndBag.IListener interface
        public void OnSelect(Item item)
        {
            if (item != null)
            {
                Dungeon.quickslot.SetSlot(slotNum, item);
                Refresh();
            }
        }

        public void Item(Item item)
        {
            slot.Item(item);
            EnableSlot();
        }

        public void Enable(bool value)
        {
            active = value;
            if (value)
                EnableSlot();
            else
                slot.Enable(false);
        }

        private void EnableSlot()
        {
            slot.Enable(Dungeon.quickslot.IsNonePlaceholder(slotNum));
        }

        private void UseTargeting()
        {
            if (lastTarget != null &&
                Actor.Chars().Contains(lastTarget) &&
                lastTarget.IsAlive() &&
                Dungeon.level.heroFOV[lastTarget.pos])
            {
                targeting = true;
                CharSprite sprite = lastTarget.sprite;

                sprite.parent.AddToFront(crossM);
                crossM.Point(sprite.Center(crossM));

                crossB.Point(slot.sprite.Center(crossB));
                crossB.visible = true;
            }
            else
            {
                lastTarget = null;
                targeting = false;
            }
        }

        public static int AutoAim(Character target)
        {
            //will use generic projectile logic if no item is specified
            return AutoAim(target, new Item());
        }

        //FIXME: this is currently very expensive, should either optimize ballistica or this, or both
        public static int AutoAim(Character target, Item item)
        {
            //first try to directly target
            if (item.ThrowPos(Dungeon.hero, target.pos) == target.pos)
            {
                return target.pos;
            }

            //Otherwise pick nearby tiles to try and 'angle' the shot, auto-aim basically.
            PathFinder.BuildDistanceMap(target.pos, BArray.Not(new bool[Dungeon.level.Length()], null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue &&
                    item.ThrowPos(Dungeon.hero, i) == target.pos)
                {
                    return i;
                }
            }

            //couldn't find a cell, give up.
            return -1;
        }

        public static void Refresh()
        {
            for (int i = 0; i < instance.Length; ++i)
            {
                if (instance[i] != null)
                {
                    instance[i].Item(Select(i));
                }
            }
        }

        public static void Target(Character target)
        {
            if (target != null && target.alignment != Character.Alignment.ALLY)
            {
                lastTarget = target;

                TargetHealthIndicator.instance.Target(target);
            }
        }

        public static void Cancel()
        {
            if (targeting)
            {
                crossB.visible = false;
                crossM.Remove();    // 부모에게서 제거
                targeting = false;
            }
        }
    }
}