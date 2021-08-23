using System;
using watabou.input;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.tiles;
using spdd.windows;

namespace spdd.ui
{
    public class Toolbar : Component
    {
        private Tool btnWait;
        private Tool btnSearch;
        private Tool btnInventory;
        private QuickslotTool[] btnQuick;

        private PickedUpItem pickedUp;

        private bool lastEnabled = true;
        public bool examining;

        private static Toolbar instance;

        public enum Mode
        {
            SPLIT,
            GROUP,
            CENTER
        }

        public Toolbar()
        {
            instance = this;

            height = btnInventory.Height();
        }

        protected override void CreateChildren()
        {
            btnQuick = new QuickslotTool[4];

            Add(btnQuick[3] = new QuickslotTool(64, 0, 22, 24, 3));
            Add(btnQuick[2] = new QuickslotTool(64, 0, 22, 24, 2));
            Add(btnQuick[1] = new QuickslotTool(64, 0, 22, 24, 1));
            Add(btnQuick[0] = new QuickslotTool(64, 0, 22, 24, 0));

            Add(btnWait = new WaitTool(24, 0, 20, 26));
            ((WaitTool)btnWait).toolbar = this;

            var restButton = new RestButton();
            restButton.toolbar = this;
            Add(restButton);

            Add(btnSearch = new SearchTool(44, 0, 20, 26));
            ((SearchTool)btnSearch).toolbar = this;

            Add(btnInventory = new BtnInventory(0, 0, 24, 26));

            Add(pickedUp = new PickedUpItem());
        }

        private class WaitTool : Tool
        {
            internal Toolbar toolbar;
            public WaitTool(int x, int y, int width, int height)
                : base(x, y, width, height)
            { }

            protected override void OnClick()
            {
                toolbar.examining = false;
                Dungeon.hero.Rest(false);
            }

            public override GameAction KeyAction()
            {
                return SPDAction.WAIT;
            }

            protected override bool OnLongClick()
            {
                toolbar.examining = false;
                Dungeon.hero.Rest(true);
                return true;
            }
        }

        private class RestButton : Button
        {
            internal Toolbar toolbar;

            protected override void OnClick()
            {
                toolbar.examining = false;
                Dungeon.hero.Rest(true);
            }

            public override GameAction KeyAction()
            {
                return SPDAction.REST;
            }
        }

        private class SearchTool : Tool
        {
            internal Toolbar toolbar;
            public SearchTool(int x, int y, int width, int height)
                : base(x, y, width, height)
            { }

            protected override void OnClick()
            {
                if (!toolbar.examining)
                {
                    GameScene.SelectCell(informer);
                    toolbar.examining = true;
                }
                else
                {
                    informer.OnSelect(null);
                    Dungeon.hero.Search(true);
                }
            }

            public override GameAction KeyAction()
            {
                return SPDAction.SEARCH;
            }

            protected override bool OnLongClick()
            {
                Dungeon.hero.Search(true);
                return true;
            }
        }

        private class BtnInventory : Tool
        {
            internal GoldIndicator gold;

            public BtnInventory(int x, int y, int width, int height)
                : base(x, y, width, height)
            { }

            protected override void OnClick()
            {
                GameScene.Show(new WndBag(Dungeon.hero.belongings.backpack, null, WndBag.Mode.ALL, null));
            }

            public override GameAction KeyAction()
            {
                return SPDAction.INVENTORY;
            }

            protected override bool OnLongClick()
            {
                WndJournal.last_index = 3; //catalog page
                GameScene.Show(new WndJournal());
                return true;
            }

            protected override void CreateChildren()
            {
                base.CreateChildren();
                gold = new GoldIndicator();
                Add(gold);
            }

            protected override void Layout()
            {
                base.Layout();
                gold.Fill(this);
            }
        }

        protected override void Layout()
        {
            for (int i = 0; i <= 3; ++i)
            {
                if (i == 0 && !SPDSettings.FlipToolbar() ||
                    i == 3 && SPDSettings.FlipToolbar())
                {
                    btnQuick[i].Border(0, 2);
                    btnQuick[i].Frame(106, 0, 19, 24);
                }
                else if (i == 0 && SPDSettings.FlipToolbar() ||
                      i == 3 && !SPDSettings.FlipToolbar())
                {
                    btnQuick[i].Border(2, 1);
                    btnQuick[i].Frame(86, 0, 20, 24);
                }
                else
                {
                    btnQuick[i].Border(0, 1);
                    btnQuick[i].Frame(88, 0, 18, 24);
                }
            }

            float right = width;
            switch (Enum.Parse(typeof(Mode), SPDSettings.ToolbarMode()))
            {
                case Mode.SPLIT:
                    btnWait.SetPos(x, y);
                    btnSearch.SetPos(btnWait.Right(), y);

                    btnInventory.SetPos(right - btnInventory.Width(), y);

                    btnQuick[0].SetPos(btnInventory.Left() - btnQuick[0].Width(), y + 2);
                    btnQuick[1].SetPos(btnQuick[0].Left() - btnQuick[1].Width(), y + 2);
                    btnQuick[2].SetPos(btnQuick[1].Left() - btnQuick[2].Width(), y + 2);
                    btnQuick[3].SetPos(btnQuick[2].Left() - btnQuick[3].Width(), y + 2);

                    //center the quickslots if they
                    if (btnQuick[3].Left() < btnSearch.Right())
                    {
                        float diff = (float)Math.Round(btnSearch.Right() - btnQuick[3].Left(), MidpointRounding.AwayFromZero) / 2;
                        for (int i = 0; i < 4; ++i)
                        {
                            btnQuick[i].SetPos(btnQuick[i].Left() + diff, btnQuick[i].Top());
                        }
                    }
                    break;

                //center = group but.. well.. centered, so all we need to do is pre-emptively set the right side further in.
                case Mode.CENTER:
                    float toolbarWidth = btnWait.Width() + btnSearch.Width() + btnInventory.Width();
                    foreach (Button slot in btnQuick)
                    {
                        if (slot.visible) toolbarWidth += slot.Width();
                    }
                    right = (width + toolbarWidth) / 2;
                    goto case Mode.GROUP;

                case Mode.GROUP:
                    btnWait.SetPos(right - btnWait.Width(), y);
                    btnSearch.SetPos(btnWait.Left() - btnSearch.Width(), y);
                    btnInventory.SetPos(btnSearch.Left() - btnInventory.Width(), y);

                    btnQuick[0].SetPos(btnInventory.Left() - btnQuick[0].Width(), y + 2);
                    btnQuick[1].SetPos(btnQuick[0].Left() - btnQuick[1].Width(), y + 2);
                    btnQuick[2].SetPos(btnQuick[1].Left() - btnQuick[2].Width(), y + 2);
                    btnQuick[3].SetPos(btnQuick[2].Left() - btnQuick[3].Width(), y + 2);

                    if (btnQuick[3].Left() < 0)
                    {
                        float diff = (float)(-Math.Round(btnQuick[3].Left(), MidpointRounding.AwayFromZero) / 2);
                        for (int i = 0; i < 4; ++i)
                        {
                            btnQuick[i].SetPos(btnQuick[i].Left() + diff, btnQuick[i].Top());
                        }
                    }
                    break;
            }

            right = width;

            if (SPDSettings.FlipToolbar())
            {
                btnWait.SetPos((right - btnWait.Right()), y);
                btnSearch.SetPos((right - btnSearch.Right()), y);
                btnInventory.SetPos((right - btnInventory.Right()), y);

                for (int i = 0; i <= 3; ++i)
                {
                    btnQuick[i].SetPos(right - btnQuick[i].Right(), y + 2);
                }
            }
        }

        public static void UpdateLayout()
        {
            if (instance != null)
                instance.Layout();
        }

        public override void Update()
        {
            base.Update();

            if (lastEnabled != (Dungeon.hero.ready && Dungeon.hero.IsAlive()))
            {
                lastEnabled = !lastEnabled;

                foreach (Gizmo tool in members)
                {
                    if (tool is Tool)
                    {
                        ((Tool)tool).Enable(lastEnabled);
                    }
                }
            }

            if (!Dungeon.hero.IsAlive())
                btnInventory.Enable(true);
        }

        public void Pickup(Item item, int cell)
        {
            pickedUp.Reset(item,
                cell,
                btnInventory.CenterX(),
                btnInventory.CenterY());
        }

        internal static readonly CellSelector.IListener informer = new ToolbarCellSelectorListener();

        class ToolbarCellSelectorListener : CellSelector.IListener
        {
            public void OnSelect(int? cell)
            {
                instance.examining = false;
                GameScene.ExamineCell(cell);
            }

            public string Prompt()
            {
                return Messages.Get(typeof(Toolbar), "examine_prompt");
            }
        }

        private class Tool : Button
        {
            private static Color BGCOLOR = new Color(0x7B, 0x80, 0x73, 0xFF);

            //private Image base;
            private Image baseImage;

            public Tool(int x, int y, int width, int height)
            {
                hotArea.blockWhenInactive = true;
                Frame(x, y, width, height);
            }

            public void Frame(int x, int y, int width, int height)
            {
                baseImage.Frame(x, y, width, height);

                this.width = width;
                this.height = height;
            }

            protected override void CreateChildren()
            {
                base.CreateChildren();

                baseImage = new Image(Assets.Interfaces.TOOLBAR);
                Add(baseImage);
            }

            protected override void Layout()
            {
                base.Layout();

                baseImage.x = x;
                baseImage.y = y;
            }

            protected override void OnPointerDown()
            {
                baseImage.Brightness(1.4f);
            }

            protected override void OnPointerUp()
            {
                if (active)
                    baseImage.ResetColor();
                else
                    baseImage.Tint(BGCOLOR, 0.7f);
            }

            public virtual void Enable(bool value)
            {
                if (value == active)
                    return;

                if (value)
                    baseImage.ResetColor();
                else
                    baseImage.Tint(BGCOLOR, 0.7f);

                active = value;
            }
        }

        private class QuickslotTool : Tool
        {
            private QuickSlotButton slot;
            private int borderLeft = 2;
            private int borderRight = 2;

            public QuickslotTool(int x, int y, int width, int height, int slotNum)
                : base(x, y, width, height)
            {
                slot = new QuickSlotButton(slotNum);
                Add(slot);
            }

            public void Border(int left, int right)
            {
                borderLeft = left;
                borderRight = right;
                Layout();
            }

            protected override void Layout()
            {
                base.Layout();
                slot.SetRect(x + borderLeft, y + 2, width - borderLeft - borderRight, height - 4);
            }

            public override void Enable(bool value)
            {
                base.Enable(value);
                slot.Enable(value);
            }
        }

        public class PickedUpItem : ItemSprite
        {
            private const float DURATION = 0.5f;

            private float startScale;
            private float startX, startY;
            private float endX, endY;
            private float left;

            public PickedUpItem()
            {
                OriginToCenter();

                active = visible = false;
            }

            public void Reset(Item item, int cell, float endX, float endY)
            {
                View(item);

                active = visible = true;

                PointF tile = DungeonTerrainTilemap.RaisedTileCenterToWorld(cell);
                Point screen = Camera.main.CameraToScreen(tile.x, tile.y);
                PointF start = GetCamera().ScreenToCamera(screen.x, screen.y);

                x = this.startX = start.x - ItemSprite.SIZE / 2;
                y = this.startY = start.y - ItemSprite.SIZE / 2;

                this.endX = endX - ItemSprite.SIZE / 2;
                this.endY = endY - ItemSprite.SIZE / 2;
                left = DURATION;

                scale.Set(startScale = Camera.main.zoom / GetCamera().zoom);
            }

            public override void Update()
            {
                base.Update();

                if ((left -= Game.elapsed) <= 0)
                {
                    visible = active = false;

                    if (emitter != null)
                        emitter.on = false;
                }
                else
                {
                    float p = left / DURATION;
                    scale.Set(startScale * (float)Math.Sqrt(p));

                    x = startX * p + endX * (1 - p);
                    y = startY * p + endY * (1 - p);
                }
            }
        }
    }
}