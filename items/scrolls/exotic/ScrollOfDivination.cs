using System.Linq;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.effects;
using spdd.items.potions;
using spdd.items.rings;
using spdd.scenes;
using spdd.ui;
using spdd.windows;
using spdd.utils;
using spdd.messages;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfDivination : ExoticScroll
    {
        public ScrollOfDivination()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_DIVINATE;
        }

        public override void DoRead()
        {
            curUser.sprite.parent.Add(new Identification(curUser.sprite.Center().Offset(0, -16)));

            Sample.Instance.Play(Assets.Sounds.READ);

            var potions = Potion.GetUnknown();
            var scrolls = Scroll.GetUnknown();
            var rings = Ring.GetUnknown();

            int total = potions.Count + scrolls.Count + rings.Count;

            if (total == 0)
            {
                GLog.Negative(Messages.Get(this, "nothing_left"));
                return;
            }

            var IDed = new List<Item>();
            int left = 4;

            float[] baseProbs = new float[] { 3, 3, 3 };
            float[] probs = (float[])baseProbs.Clone();

            while (left > 0 && total > 0)
            {
                switch (Rnd.Chances(probs))
                {
                    default:
                        probs = (float[])baseProbs.Clone();
                        continue;
                    case 0:
                        if (potions.Count == 0)
                        {
                            probs[0] = 0;
                            continue;
                        }
                        --probs[0];
                        Potion p = (Potion)Reflection.NewInstance(Rnd.Element(potions.ToArray()));
                        p.SetKnown();
                        IDed.Add(p);
                        potions.Remove(p.GetType());
                        break;
                    case 1:
                        if (scrolls.Count == 0)
                        {
                            probs[1] = 0;
                            continue;
                        }
                        --probs[1];
                        Scroll s = (Scroll)Reflection.NewInstance(Rnd.Element(scrolls.ToArray()));
                        s.SetKnown();
                        IDed.Add(s);
                        scrolls.Remove(s.GetType());
                        break;
                    case 2:
                        if (rings.Count == 0)
                        {
                            probs[2] = 0;
                            continue;
                        }
                        --probs[2];
                        Ring r = (Ring)Reflection.NewInstance(Rnd.Element(rings.ToArray()));
                        r.SetKnown();
                        IDed.Add(r);
                        rings.Remove(r.GetType());
                        break;
                }
                --left;
                --total;
            }

            GameScene.Show(new WndDivination(this, IDed));

            ReadAnimation();
            SetKnown();
        }

        private class WndDivination : Window
        {
            private const int WIDTH = 120;

            public WndDivination(ScrollOfDivination scroll, List<Item> IDed)
            {
                IconTitle cur = new IconTitle(new ItemSprite(scroll),
                        Messages.TitleCase(Messages.Get(typeof(ScrollOfDivination), "name")));
                cur.SetRect(0, 0, WIDTH, 0);
                Add(cur);

                RenderedTextBlock msg = PixelScene.RenderTextBlock(Messages.Get(this, "desc"), 6);
                msg.MaxWidth(120);
                msg.SetPos(0, cur.Bottom() + 2);
                Add(msg);

                float pos = msg.Bottom() + 10;

                foreach (Item i in IDed)
                {
                    cur = new IconTitle(i);
                    cur.SetRect(0, pos, WIDTH, 0);
                    Add(cur);
                    pos = cur.Bottom() + 2;
                }

                Resize(WIDTH, (int)pos);
            }
        }
    }
}