using System.Collections.Generic;
using watabou.noosa.ui;
using watabou.utils;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;

namespace spdd.ui
{
    public class GameLog : Component, Signal<string>.IListener
    {
        private const int MAX_LINES = 3;

        //private static final Pattern PUNCTUATION = Pattern.compile( ".*[.,;?! ]$" );

        private RenderedTextBlock lastEntry;
        private Color lastColor;

        private static List<Entry> entries = new List<Entry>();

        public GameLog()
        {
            GLog.update.Replace(this);

            RecreateLines();
        }

        private static List<string> textsToAdd = new List<string>();

        public override void Update()
        {
            //foreach (string text in textsToAdd)
            for (int index = 0; index < textsToAdd.Count; ++index)
            {
                string text = textsToAdd[index];

                // length´Â groupÀÇ member
                if (length != entries.Count)
                {
                    Clear();
                    RecreateLines();
                }

                if (text.Equals(GLog.NEW_LINE))
                {
                    lastEntry = null;
                    continue;
                }

                Color color = CharSprite.DEFAULT;
                if (text.StartsWith(GLog.POSITIVE))
                {
                    text = text.Substring(GLog.POSITIVE.Length);
                    color = CharSprite.POSITIVE;
                }
                else if (text.StartsWith(GLog.NEGATIVE))
                {
                    text = text.Substring(GLog.NEGATIVE.Length);
                    color = CharSprite.NEGATIVE;
                }
                else if (text.StartsWith(GLog.WARNING))
                {
                    text = text.Substring(GLog.WARNING.Length);
                    color = CharSprite.WARNING;
                }
                else if (text.StartsWith(GLog.HIGHLIGHT))
                {
                    text = text.Substring(GLog.HIGHLIGHT.Length);
                    color = CharSprite.NEUTRAL;
                }

                if (lastEntry != null && color.i32value == lastColor.i32value && lastEntry.nLines < MAX_LINES)
                {
                    string lastMessage = lastEntry.Text();
                    lastEntry.Text(lastMessage.Length == 0 ? text : lastMessage + " " + text);

                    entries[entries.Count - 1].text = lastEntry.Text();
                }
                else
                {
                    lastEntry = PixelScene.RenderTextBlock(text, 6);
                    lastEntry.SetHightlighting(false);
                    lastEntry.Hardlight(color);
                    lastColor = color;
                    Add(lastEntry);

                    entries.Add(new Entry(text, color));
                }

                if (length > 0)
                {
                    int nLines;
                    do
                    {
                        nLines = 0;
                        for (int i = 0; i < length - 1; ++i)
                        {
                            nLines += ((RenderedTextBlock)members[i]).nLines;
                        }

                        if (nLines > MAX_LINES)
                        {
                            RenderedTextBlock r = ((RenderedTextBlock)members[0]);
                            Remove(r);
                            r.Destroy();

                            entries.RemoveAt(0);
                        }
                    }
                    while (nLines > MAX_LINES);

                    if (entries.Count == 0)
                        lastEntry = null;
                }
            }

            if (textsToAdd.Count > 0)
            {
                Layout();
                textsToAdd.Clear();
            }
            base.Update();
        }

        private void RecreateLines()
        {
            foreach (Entry entry in entries)
            {
                lastEntry = PixelScene.RenderTextBlock(entry.text, 6);
                lastEntry.Hardlight(lastColor = entry.color);
                Add(lastEntry);
            }
        }

        public void NewLine()
        {
            lastEntry = null;
        }

        // Signal<string>.IListener
        public bool OnSignal(string text)
        {
            textsToAdd.Add(text);
            return false;
        }

        protected override void Layout()
        {
            float pos = y;
            for (int i = members.Count - 1; i >= 0; --i)
            {
                var entry = (RenderedTextBlock)members[i];
                entry.MaxWidth((int)width);
                entry.SetPos(x, pos - entry.Height());
                pos -= entry.Height() + 2;
            }
        }

        public override void Destroy()
        {
            GLog.update.Remove(this);
            base.Destroy();
        }

        private class Entry
        {
            public string text;
            public Color color;

            public Entry(string text, Color color)
            {
                this.text = text;
                this.color = color;
            }
        }

        public static void Wipe()
        {
            entries.Clear();
        }
    }
}