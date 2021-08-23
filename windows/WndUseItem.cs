using System.Collections.Generic;
using spdd.items;
using spdd.messages;
using spdd.ui;

namespace spdd.windows
{
    public class WndUseItem : WndInfoItem
    {
        //only one wnduseitem can appear at a time
        private static WndUseItem instance;

        private const float BUTTON_HEIGHT = 16;

        private const float GAP = 2;

        public WndUseItem(WndBag owner, Item item)
            : base(item)
        {
            if (instance != null)
            {
                instance.Hide();
            }
            instance = this;

            float y = height + GAP;

            if (Dungeon.hero.IsAlive())
            {
                List<RedButton> buttons = new List<RedButton>();
                foreach (var action in item.Actions(Dungeon.hero))
                {
                    var act = action;
                    var btn = new ActionRedButton(Messages.Get(item, "ac_" + action), 8);
                    btn.action = () =>
                    {
                        Hide();
                        if (owner != null && owner.parent != null)
                            owner.Hide();
                        if (Dungeon.hero.IsAlive())
                            item.Execute(Dungeon.hero, act);
                    };
                    btn.SetSize(btn.ReqWidth(), BUTTON_HEIGHT);
                    buttons.Add(btn);
                    Add(btn);

                    if (action.Equals(item.defaultAction))
                        btn.TextColor(TITLE_COLOR);
                }
                y = LayoutButtons(buttons, width, y);
            }

            Resize(width, (int)y);
        }

        private static float LayoutButtons(List<RedButton> buttons, float width, float y)
        {
            List<RedButton> curRow = new List<RedButton>();
            float widthLeftThisRow = width;

            while (buttons.Count > 0)
            {
                RedButton btn = buttons[0];

                widthLeftThisRow -= btn.Width();
                if (curRow.Count == 0)
                {
                    curRow.Add(btn);
                    buttons.Remove(btn);
                }
                else
                {
                    widthLeftThisRow -= 1;
                    if (widthLeftThisRow >= 0)
                    {
                        curRow.Add(btn);
                        buttons.Remove(btn);
                    }
                }

                //layout current row. Currently forces a max of 3 buttons but can work with more
                if (buttons.Count == 0 || widthLeftThisRow <= 0 || curRow.Count >= 3)
                {
                    //re-use this variable for laying out the buttons
                    widthLeftThisRow = width - (curRow.Count - 1);
                    foreach (var b in curRow)
                    {
                        widthLeftThisRow -= b.Width();
                    }

                    //while we still have space in this row, find the shortest button(s) and extend them
                    while (widthLeftThisRow > 0)
                    {
                        List<RedButton> shortest = new List<RedButton>();
                        RedButton secondShortest = null;

                        foreach (RedButton b in curRow)
                        {
                            if (shortest.Count == 0)
                            {
                                shortest.Add(b);
                            }
                            else
                            {
                                if (b.Width() < shortest[0].Width())
                                {
                                    secondShortest = shortest[0];
                                    shortest.Clear();
                                    shortest.Add(b);
                                }
                                else if (b.Width() == shortest[0].Width())
                                {
                                    shortest.Add(b);
                                }
                                else if (secondShortest == null || secondShortest.Width() > b.Width())
                                {
                                    secondShortest = b;
                                }
                            }
                        }

                        float widthToGrow;

                        if (secondShortest == null)
                        {
                            widthToGrow = widthLeftThisRow / shortest.Count;
                            widthLeftThisRow = 0;
                        }
                        else
                        {
                            widthToGrow = secondShortest.Width() - shortest[0].Width();
                            if ((widthToGrow * shortest.Count) >= widthLeftThisRow)
                            {
                                widthToGrow = widthLeftThisRow / shortest.Count;
                                widthLeftThisRow = 0;
                            }
                            else
                            {
                                widthLeftThisRow -= widthToGrow * shortest.Count;
                            }
                        }

                        foreach (RedButton toGrow in shortest)
                        {
                            toGrow.SetRect(0, 0, toGrow.Width() + widthToGrow, toGrow.Height());
                        }
                    }

                    //finally set positions
                    float x = 0;
                    foreach (var b in curRow)
                    {
                        b.SetRect(x, y, b.Width(), b.Height());
                        x += b.Width() + 1;
                    }

                    //move to next line and reset variables
                    y += BUTTON_HEIGHT + 1;
                    widthLeftThisRow = width;
                    curRow.Clear();
                }
            }

            return y - 1;
        }

        public override void Hide()
        {
            base.Hide();
            if (instance == this)
                instance = null;
        }
    }
}