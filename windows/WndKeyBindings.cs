using System;
using System.Collections.Generic;
using Microsoft.Collections.Extensions;
using watabou.input;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.windows
{
    public class WndKeyBindings : Window
    {
        private const int WIDTH = 120;

        private const int BTN_HEIGHT = 16;

        private static readonly int COL1_CENTER = WIDTH / 4;
        private static readonly int COL2_CENTER = 5 * WIDTH / 8;
        private static readonly int COL3_CENTER = 7 * WIDTH / 8;

        private Component bindingsList;
        private List<BindingItem> listItems = new List<BindingItem>();

        private OrderedDictionary<int, GameAction> changedBindings;

        public WndKeyBindings()
        {
            changedBindings = KeyBindings.GetAllBindings();

            RenderedTextBlock ttlAction = PixelScene.RenderTextBlock(Messages.Get(this, "ttl_action"), 9);
            ttlAction.SetPos(COL1_CENTER - ttlAction.Width() / 2, (BTN_HEIGHT - ttlAction.Height()) / 2);
            Add(ttlAction);

            ColorBlock ttlSep1 = new ColorBlock(1, BTN_HEIGHT, new Color(0x22, 0x22, 0x22, 0xFF));
            ttlSep1.x = WIDTH / 2;
            Add(ttlSep1);

            RenderedTextBlock ttlKey1 = PixelScene.RenderTextBlock(Messages.Get(this, "ttl_key1"), 9);
            ttlKey1.SetPos(COL2_CENTER - ttlKey1.Width() / 2, (BTN_HEIGHT - ttlKey1.Height()) / 2);
            Add(ttlKey1);

            ColorBlock ttlSep2 = new ColorBlock(1, BTN_HEIGHT, new Color(0x22, 0x22, 0x22, 0xFF));
            ttlSep2.x = 3 * WIDTH / 4;
            Add(ttlSep2);

            RenderedTextBlock ttlKey2 = PixelScene.RenderTextBlock(Messages.Get(this, "ttl_key2"), 9);
            ttlKey2.SetPos(COL3_CENTER - ttlKey2.Width() / 2, (BTN_HEIGHT - ttlKey2.Height()) / 2);
            Add(ttlKey2);

            ColorBlock ttlSep3 = new ColorBlock(WIDTH, 1, new Color(0x22, 0x22, 0x22, 0xFF));
            ttlSep3.y = BTN_HEIGHT;
            Add(ttlSep3);

            bindingsList = new Component();

            var scrollingList = new WndKeyBindingsScrollPane(this, bindingsList);

            Add(scrollingList);

            int y = 0;
            foreach (GameAction action in GameAction.AllActions())
            {
                //start at 1. No bindings for NONE
                if (action.Code() < 1)
                    continue;

                BindingItem item = new BindingItem(this, action);
                item.SetRect(0, y, WIDTH, BindingItem.HEIGHT);
                bindingsList.Add(item);
                listItems.Add(item);
                y += (int)item.Height();
            }
            bindingsList.SetSize(WIDTH, y + 1);

            Resize(WIDTH, Math.Min(BTN_HEIGHT * 3 + 3 + BindingItem.HEIGHT * listItems.Count, PixelScene.uiCamera.height - 20));

            var btnDefaults = new ActionRedButton(Messages.Get(this, "default"));
            btnDefaults.action = () =>
            {
                changedBindings = SPDAction.GetDefaults();
                foreach (var i in listItems)
                {
                    int key1 = 0;
                    int key2 = 0;
                    foreach (var pair in changedBindings)
                    {
                        int k = pair.Key;
                        if (pair.Value == i.gameAction)
                        {
                            if (key1 == 0)
                                key1 = k;
                            else
                                key2 = k;
                        }
                    }
                    i.UpdateBindings(key1, key2);
                }
            };

            btnDefaults.SetRect(0, height - BTN_HEIGHT * 2 - 1, WIDTH, BTN_HEIGHT);
            Add(btnDefaults);

            var btnConfirm = new ActionRedButton(Messages.Get(this, "confirm"));
            btnConfirm.action = () =>
            {
                KeyBindings.SetAllBindings(changedBindings);
                SPDAction.SaveBindings();
                Hide();
            };

            btnConfirm.SetRect(0, height - BTN_HEIGHT, WIDTH / 2, BTN_HEIGHT);
            Add(btnConfirm);

            var btnCancel = new ActionRedButton(Messages.Get(this, "cancel"));
            btnCancel.action = () =>
            {
                Hide(); //close and don't save
            };

            btnCancel.SetRect(WIDTH / 2 + 1, height - BTN_HEIGHT, WIDTH / 2 - 1, BTN_HEIGHT);
            Add(btnCancel);

            scrollingList.SetRect(0, BTN_HEIGHT + 1, WIDTH, btnDefaults.Top() - BTN_HEIGHT - 1);
        }

        private class WndKeyBindingsScrollPane : ScrollPane
        {
            private readonly WndKeyBindings wndKeyBindings;

            public WndKeyBindingsScrollPane(WndKeyBindings wndKeyBindings, Component bindingsList)
                : base(bindingsList)
            {
                this.wndKeyBindings = wndKeyBindings;
            }

            public override void OnClick(float x, float y)
            {
                foreach (BindingItem i in wndKeyBindings.listItems)
                {
                    if (i.OnClick(x, y))
                    {
                        break;
                    }
                }
            }
        }

        public override void OnBackPressed()
        {
            //do nothing, avoids accidental back presses which would lose progress.
        }

        private class BindingItem : Component
        {
            WndKeyBindings wndKeyBindings;
            internal const int HEIGHT = 12;

            internal static readonly Color CHANGED = TITLE_COLOR;
            internal static readonly Color DEFAULT = new Color(0xFF, 0xFF, 0xFF, 0xFF);
            internal static readonly Color UNBOUND = new Color(0x88, 0x88, 0x88, 0xFF);
            internal static readonly Color UNBOUND_CHANGED = new Color(0x88, 0x88, 0x22, 0xFF);

            internal GameAction gameAction;
            internal int key1;
            internal int key2;

            internal int origKey1;
            internal int origKey2;

            internal RenderedTextBlock actionName;
            internal RenderedTextBlock key1Name;
            internal RenderedTextBlock key2Name;

            internal ColorBlock sep1;
            internal ColorBlock sep2;
            internal ColorBlock sep3;

            public BindingItem(WndKeyBindings wndKeyBindings, GameAction action)
            {
                this.wndKeyBindings = wndKeyBindings;
                gameAction = action;

                actionName = PixelScene.RenderTextBlock(Messages.Get(typeof(WndKeyBindings), action.Name()), 6);
                actionName.SetHightlighting(false);
                Add(actionName);

                List<int> keys = KeyBindings.GetBoundKeysForAction(action);
                if (keys.Count == 0)
                {
                    origKey1 = key1 = 0;
                }
                else
                {
                    var k = keys[0];
                    keys.RemoveAt(0);
                    origKey1 = key1 = k;
                }

                if (keys.Count == 0)
                {
                    origKey2 = key2 = 0;
                }
                else
                {
                    var k = keys[0];
                    keys.RemoveAt(0);
                    origKey2 = key2 = k;
                }

                key1Name = PixelScene.RenderTextBlock(KeyBindings.GetKeyName(key1), 6);
                if (key1 == 0)
                    key1Name.Hardlight(UNBOUND);
                Add(key1Name);

                key2Name = PixelScene.RenderTextBlock(KeyBindings.GetKeyName(key2), 6);
                if (key2 == 0)
                    key2Name.Hardlight(UNBOUND);
                Add(key2Name);

                sep1 = new ColorBlock(1, 1, new Color(0x22, 0x22, 0x22, 0xFF));
                Add(sep1);

                sep2 = new ColorBlock(1, 1, new Color(0x22, 0x22, 0x22, 0xFF));
                Add(sep2);

                sep3 = new ColorBlock(1, 1, new Color(0x22, 0x22, 0x22, 0xFF));
                Add(sep3);
            }

            public void UpdateBindings(int first, int second)
            {
                if (first == 0)
                {
                    key1 = second;
                    key2 = first;
                }
                else
                {
                    key1 = first;
                    key2 = second;
                }

                key1Name.Text(KeyBindings.GetKeyName(key1));
                if (key1 != origKey1)
                    key1Name.Hardlight(key1 == 0 ? UNBOUND_CHANGED : CHANGED);
                else
                    key1Name.Hardlight(key1 == 0 ? UNBOUND : DEFAULT);

                key2Name.Text(KeyBindings.GetKeyName(key2));
                if (key2 != origKey2)
                    key2Name.Hardlight(key2 == 0 ? UNBOUND_CHANGED : CHANGED);
                else
                    key2Name.Hardlight(key2 == 0 ? UNBOUND : DEFAULT);

                Layout();
            }

            protected override void Layout()
            {
                base.Layout();

                actionName.SetPos(x, y + (Height() - actionName.Height()) / 2);
                key1Name.SetPos(x + Width() / 2 + 2, y + (Height() - key1Name.Height()) / 2);
                key2Name.SetPos(x + 3 * Width() / 4 + 2, y + (Height() - key2Name.Height()) / 2);

                sep1.Size(width, 1);
                sep1.x = x;
                sep1.y = Bottom();

                sep2.Size(1, height);
                sep2.x = key1Name.Left() - 2;
                sep2.y = y;

                sep3.Size(1, height);
                sep3.x = key2Name.Left() - 2;
                sep3.y = y;
            }

            public bool OnClick(float x, float y)
            {
                if (Inside(x, y))
                {
                    if (x >= this.x + 3 * Width() / 4 && key1 != 0)
                    {
                        //assigning second key
                        ShatteredPixelDungeonDash.Scene().AddToFront(new WndChangeBinding(wndKeyBindings, gameAction, this, false, key2, key1));
                    }
                    else if (x >= this.x + Width() / 2)
                    {
                        //assigning first key
                        ShatteredPixelDungeonDash.Scene().AddToFront(new WndChangeBinding(wndKeyBindings, gameAction, this, true, key1, key2));
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        } // BindingItem

        private class WndChangeBinding : Window
        {
            WndKeyBindings wndKeyBindings;
            private int curKeyCode;
            private int otherBoundKey;
            private int changedKeyCode = -1;

            private BindingItem changedAction;
            private RenderedTextBlock changedKey;
            private RenderedTextBlock warnErr;

            private ActionRedButton btnConfirm;

            public WndChangeBinding(WndKeyBindings wndKeyBindings, GameAction action, BindingItem listItem, bool firstKey, int curKeyCode, int otherBoundKey)
            {
                this.wndKeyBindings = wndKeyBindings;
                this.curKeyCode = curKeyCode;
                this.otherBoundKey = otherBoundKey;

                RenderedTextBlock desc = PixelScene.RenderTextBlock(Messages.Get(this, firstKey ? "desc_first" : "desc_second",
                            Messages.Get(typeof(WndKeyBindings), action.Name()),
                            KeyBindings.GetKeyName(curKeyCode)), 6);
                desc.MaxWidth(WIDTH);
                desc.SetRect(0, 0, WIDTH, desc.Height());
                Add(desc);

                RenderedTextBlock curBind;
                curBind = PixelScene.RenderTextBlock(Messages.Get(this, "desc_current", KeyBindings.GetKeyName(curKeyCode)), 6);
                curBind.MaxWidth(WIDTH);
                curBind.SetRect((WIDTH - curBind.Width()) / 2, desc.Bottom() + 6, WIDTH, curBind.Height());
                Add(curBind);

                changedKey = PixelScene.RenderTextBlock(6);
                changedKey.MaxWidth(WIDTH);
                changedKey.SetRect(0, curBind.Bottom() + 2, WIDTH, changedKey.Height());
                Add(changedKey);

                warnErr = PixelScene.RenderTextBlock(6);
                warnErr.MaxWidth(WIDTH);
                warnErr.SetRect(0, changedKey.Bottom() + 10, WIDTH, warnErr.Height());
                Add(warnErr);

                var btnUnbind = new ActionRedButton(Messages.Get(this, "unbind"), 9);
                btnUnbind.action = () => OnSignal(new KeyEvent(0, true));

                btnUnbind.SetRect(0, warnErr.Bottom() + 6, WIDTH, BTN_HEIGHT);
                Add(btnUnbind);

                btnConfirm = new ActionRedButton(Messages.Get(this, "confirm"), 9);
                btnConfirm.action = () =>
                {
                    var changedBindings = wndKeyBindings.changedBindings;

                    if (changedKeyCode != -1)
                    {
                        changedBindings.Remove(changedKeyCode);
                        changedBindings.Remove(listItem.key1);
                        changedBindings.Remove(listItem.key2);

                        if (firstKey)
                        {
                            if (changedKeyCode != 0) changedBindings.Add(changedKeyCode, action);
                            if (listItem.key2 != 0) changedBindings.Add(listItem.key2, action);
                            listItem.UpdateBindings(changedKeyCode, listItem.key2);
                        }
                        else
                        {
                            if (listItem.key1 != 0) changedBindings.Add(listItem.key1, action);
                            if (changedKeyCode != 0) changedBindings.Add(changedKeyCode, action);
                            listItem.UpdateBindings(listItem.key1, changedKeyCode);
                        }

                        if (changedAction != null)
                        {
                            if (changedAction.key1 != changedKeyCode)
                            {
                                changedAction.UpdateBindings(changedAction.key1, 0);
                            }
                            else if (changedAction.key2 != changedKeyCode)
                            {
                                changedAction.UpdateBindings(changedAction.key2, 0);
                            }
                            else
                            {
                                changedAction.UpdateBindings(0, 0);
                            }
                        }
                    }

                    Hide();
                };

                btnConfirm.SetRect(0, btnUnbind.Bottom() + 1, WIDTH / 2, BTN_HEIGHT);
                btnConfirm.Enable(false);
                Add(btnConfirm);

                var btnCancel = new ActionRedButton(Messages.Get(this, "cancel"), 9);
                btnCancel.action = () => Hide();

                btnCancel.SetRect(btnConfirm.Right() + 1, btnUnbind.Bottom() + 1, WIDTH / 2 - 1, BTN_HEIGHT);
                Add(btnCancel);

                Resize(WIDTH, (int)btnCancel.Bottom());
                KeyBindings.acceptUnbound = true;
            }

            public override bool OnSignal(KeyEvent ev)
            {
                if (ev.pressed)
                {
                    changedKey.Text(Messages.Get(this, "changed_bind", KeyBindings.GetKeyName(ev.code)));
                    changedKey.SetPos((WIDTH - changedKey.Width()) / 2, changedKey.Top());

                    changedKeyCode = ev.code;
                    changedAction = null;

                    if (ev.code != 0 && (ev.code == curKeyCode || ev.code == otherBoundKey))
                    {
                        warnErr.Text(Messages.Get(this, "error"));
                        warnErr.Hardlight(CharSprite.NEGATIVE);
                        btnConfirm.Enable(false);

                    }
                    else if (ev.code != 0 && wndKeyBindings.changedBindings.TryGetValue(changedKeyCode, out GameAction ga) != false)
                    {
                        foreach (BindingItem i in wndKeyBindings.listItems)
                        {
                            if (i.gameAction == ga)
                            {
                                changedAction = i;
                                break;
                            }
                        }
                        warnErr.Text(Messages.Get(this, "warning", Messages.Get(typeof(WndKeyBindings), ga.Name())));
                        warnErr.Hardlight(CharSprite.WARNING);
                        btnConfirm.Enable(true);

                    }
                    else
                    {
                        warnErr.Text(" ");
                        btnConfirm.Enable(true);
                    }
                }

                return true;
            }

            public override void Destroy()
            {
                base.Destroy();
                KeyBindings.acceptUnbound = false;
            }
        }
    }
}