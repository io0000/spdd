using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.items;
using spdd.items.armor;
using spdd.items.potions;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.journal;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.windows
{
    public class WndJournal : WndTabbed
    {
        public const int WIDTH_P = 126;
        public const int HEIGHT_P = 180;

        public const int WIDTH_L = 200;
        public const int HEIGHT_L = 130;

        private const int ITEM_HEIGHT = 18;

        private GuideTab guideTab;
        private AlchemyTab alchemyTab;
        private NotesTab notesTab;
        private CatalogTab catalogTab;

        public static int last_index = 0;

        public WndJournal()
        {
            int width = PixelScene.Landscape() ? WIDTH_L : WIDTH_P;
            int height = PixelScene.Landscape() ? HEIGHT_L : HEIGHT_P;

            Resize(width, height);

            guideTab = new GuideTab();
            Add(guideTab);
            guideTab.SetRect(0, 0, width, height);
            guideTab.UpdateList();

            alchemyTab = new AlchemyTab();
            Add(alchemyTab);
            alchemyTab.SetRect(0, 0, width, height);

            notesTab = new NotesTab();
            Add(notesTab);
            notesTab.SetRect(0, 0, width, height);
            notesTab.UpdateList();

            catalogTab = new CatalogTab();
            Add(catalogTab);
            catalogTab.SetRect(0, 0, width, height);
            catalogTab.UpdateList();

            Tab[] tabs = new Tab[]
            {
                new WndJournalIconTab(this, new ItemSprite(ItemSpriteSheet.GUIDE_PAGE, null)),
                new WndJournalIconTab(this, new ItemSprite(ItemSpriteSheet.ALCH_PAGE, null)),
                new WndJournalIconTab(this, Icons.DEPTH.Get()),
                new WndJournalIconTab(this, new ItemSprite(ItemSpriteSheet.WEAPON_HOLDER, null))
            };

            ((WndJournalIconTab)tabs[0]).action = (value) =>
            {
                guideTab.active = guideTab.visible = value;
                if (value)
                    last_index = 0;
            };

            ((WndJournalIconTab)tabs[1]).action = (value) =>
            {
                alchemyTab.active = alchemyTab.visible = value;
                if (value)
                    last_index = 1;
            };

            ((WndJournalIconTab)tabs[2]).action = (value) =>
            {
                notesTab.active = notesTab.visible = value;
                if (value)
                    last_index = 2;
            };

            ((WndJournalIconTab)tabs[3]).action = (value) =>
            {
                catalogTab.active = catalogTab.visible = value;
                if (value)
                    last_index = 3;
            };

            foreach (Tab tab in tabs)
            {
                Add(tab);
            }

            LayoutTabs();

            Select(last_index);
        }

        private class WndJournalIconTab : IconTab
        {
            internal Action<bool> action;

            public WndJournalIconTab(WndTabbed owner, Image icon)
                : base(owner, icon)
            { }

            public override void Select(bool value)
            {
                base.Select(value);
                action(value);
            }
        }

        private class ListItem : Component
        {
            protected internal RenderedTextBlock label;
            protected internal BitmapText depth;
            protected internal ColorBlock line;
            protected internal Image icon;

            public ListItem(Image icon, string text)
                : this(icon, text, -1)
            { }

            public ListItem(Image icon, string text, int d)
            {
                this.icon.Copy(icon);

                label.Text(text);

                if (d >= 0)
                {
                    depth.Text(d.ToString());
                    depth.Measure();

                    if (d == Dungeon.depth)
                    {
                        label.Hardlight(TITLE_COLOR);
                        depth.Hardlight(TITLE_COLOR);
                    }
                }
            }

            protected override void CreateChildren()
            {
                label = PixelScene.RenderTextBlock(7);
                Add(label);

                icon = new Image();
                Add(icon);

                depth = new BitmapText(PixelScene.pixelFont);
                Add(depth);

                line = new ColorBlock(1, 1, new Color(0x22, 0x22, 0x22, 0xFF));
                Add(line);
            }

            protected override void Layout()
            {
                icon.y = y + 1 + (Height() - 1 - icon.Height()) / 2f;
                icon.x = x + (16 - icon.Width()) / 2f;
                PixelScene.Align(icon);

                depth.x = icon.x + (icon.width - depth.Width()) / 2f;
                depth.y = icon.y + (icon.height - depth.Height()) / 2f + 1;
                PixelScene.Align(depth);

                line.Size(width, 1);
                line.x = 0;
                line.y = y;

                label.MaxWidth((int)(width - 16 - 1));
                label.SetPos(17, y + 1 + (Height() - label.Height()) / 2f);
                PixelScene.Align(label);
            }
        }  // ListItem

        private class GuideTab : Component
        {
            private ScrollPane list;
            private List<GuideItem> pages = new List<GuideItem>();

            protected override void CreateChildren()
            {
                list = new GuideTabScrollPane(new Component());
                ((GuideTabScrollPane)list).tab = this;
                Add(list);
            }

            private class GuideTabScrollPane : ScrollPane
            {
                internal GuideTab tab;
                public GuideTabScrollPane(Component content)
                    : base(content)
                { }

                public override void OnClick(float x, float y)
                {
                    foreach (var page in tab.pages)
                    {
                        if (page.OnClick(x, y))
                            break;
                    }
                }
            }

            protected override void Layout()
            {
                base.Layout();
                list.SetRect(0, 0, width, height);
            }

            public void UpdateList()
            {
                Component content = list.Content();

                float pos = 0;

                ColorBlock line = new ColorBlock(Width(), 1, new Color(0x22, 0x22, 0x22, 0xFF));
                line.y = pos;
                content.Add(line);

                RenderedTextBlock title = PixelScene.RenderTextBlock(Document.ADVENTURERS_GUIDE.Title(), 9);
                title.Hardlight(TITLE_COLOR);
                title.MaxWidth((int)Width() - 2);
                title.SetPos((Width() - title.Width()) / 2f, pos + 1 + ((ITEM_HEIGHT) - title.Height()) / 2f);
                PixelScene.Align(title);
                content.Add(title);

                pos += Math.Max(ITEM_HEIGHT, title.Height());

                foreach (string page in Document.ADVENTURERS_GUIDE.Pages())
                {
                    GuideItem item = new GuideItem(page);

                    item.SetRect(0, pos, Width(), ITEM_HEIGHT);
                    content.Add(item);

                    pos += item.Height();
                    pages.Add(item);
                }

                content.SetSize(Width(), pos);
                list.SetSize(list.Width(), list.Height());
            }

            private class GuideItem : ListItem
            {
                private bool found = false;
                private string page;

                public GuideItem(string page)
                    : base(IconForPage(page), Messages.TitleCase(Document.ADVENTURERS_GUIDE.PageTitle(page)))
                {
                    this.page = page;
                    found = Document.ADVENTURERS_GUIDE.HasPage(page);

                    if (!found)
                    {
                        icon.Hardlight(0.5f, 0.5f, 0.5f);
                        label.Text(Messages.TitleCase(Messages.Get(this, "missing")));
                        label.Hardlight(new Color(0x99, 0x99, 0x99, 0xFF));
                    }
                }

                public bool OnClick(float x, float y)
                {
                    if (Inside(x, y) && found)
                    {
                        GameScene.Show(new WndStory(IconForPage(page),
                                Document.ADVENTURERS_GUIDE.PageTitle(page),
                                Document.ADVENTURERS_GUIDE.PageBody(page)));
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                //TODO might just want this to be part of the Document class
                private static Image IconForPage(string page)
                {
                    if (!Document.ADVENTURERS_GUIDE.HasPage(page))
                    {
                        return new ItemSprite(ItemSpriteSheet.GUIDE_PAGE);
                    }

                    switch (page)
                    {
                        case Document.GUIDE_INTRO_PAGE:
                        default:
                            return new ItemSprite(ItemSpriteSheet.MASTERY);
                        case "Identifying":
                            return new ItemSprite(ItemSpriteSheet.SCROLL_ISAZ);
                        case Document.GUIDE_SEARCH_PAGE:
                            return new ItemSprite(ItemSpriteSheet.LOCKED_CHEST);
                        case "Strength":
                            return new ItemSprite(ItemSpriteSheet.ARMOR_SCALE);
                        case "Food":
                            return new ItemSprite(ItemSpriteSheet.PASTY);
                        case "Levelling":
                            return new ItemSprite(ItemSpriteSheet.POTION_MAGENTA);
                        case "Surprise_Attacks":
                            return new ItemSprite(ItemSpriteSheet.ASSASSINS_BLADE);
                        case "Dieing":
                            return new ItemSprite(ItemSpriteSheet.ANKH);
                        case "Looting":
                            return new ItemSprite(ItemSpriteSheet.CRYSTAL_KEY);
                        case "Magic":
                            return new ItemSprite(ItemSpriteSheet.WAND_LIGHTNING);
                    }
                }
            }  // GuideItem
        }  // GuideTab

        public class AlchemyTab : Component
        {
            private ActionRedButton[] pageButtons;
            private const int NUM_BUTTONS = 9;

            private static readonly int[] spriteIndexes = { 10, 12, 7, 8, 9, 11, 13, 14, 15 };

            private static int currentPageIdx = -1;

            private IconTitle title;
            private RenderedTextBlock body;

            private ScrollPane list;
            private List<QuickRecipe> recipes = new List<QuickRecipe>();

            protected override void CreateChildren()
            {
                pageButtons = new ActionRedButton[NUM_BUTTONS];
                for (int i = 0; i < NUM_BUTTONS; ++i)
                {
                    int idx = i;

                    pageButtons[i] = new ActionRedButton("");
                    pageButtons[i].action = () =>
                    {
                        currentPageIdx = idx;
                        UpdateList();
                    };

                    if (Document.ALCHEMY_GUIDE.HasPage(i))
                    {
                        pageButtons[i].Icon(new ItemSprite(ItemSpriteSheet.SOMETHING + spriteIndexes[i], null));
                    }
                    else
                    {
                        pageButtons[i].Icon(new ItemSprite(ItemSpriteSheet.SOMETHING, null));
                        pageButtons[i].Enable(false);
                    }
                    Add(pageButtons[i]);
                }

                title = new IconTitle();
                title.Icon(new ItemSprite(ItemSpriteSheet.ALCH_PAGE));
                title.visible = false;

                body = PixelScene.RenderTextBlock(6);

                list = new ScrollPane(new Component());
                Add(list);
            }

            protected override void Layout()
            {
                base.Layout();

                if (PixelScene.Landscape())
                {
                    float buttonWidth = Width() / pageButtons.Length;
                    for (int i = 0; i < NUM_BUTTONS; ++i)
                    {
                        pageButtons[i].SetRect(i * buttonWidth, 0, buttonWidth, ITEM_HEIGHT);
                        PixelScene.Align(pageButtons[i]);
                    }
                }
                else
                {
                    //for first row
                    float buttonWidth = Width() / 4;
                    float y = 0;
                    float x = 0;
                    for (int i = 0; i < NUM_BUTTONS; ++i)
                    {
                        pageButtons[i].SetRect(x, y, buttonWidth, ITEM_HEIGHT);
                        PixelScene.Align(pageButtons[i]);
                        x += buttonWidth;
                        if (i == 3)
                        {
                            y += ITEM_HEIGHT;
                            x = 0;
                            buttonWidth = Width() / 5;
                        }
                    }
                }

                list.SetRect(0, pageButtons[NUM_BUTTONS - 1].Bottom() + 1, width,
                        height - pageButtons[NUM_BUTTONS - 1].Bottom() - 1);

                UpdateList();
            }

            public void UpdateList()
            {
                for (int i = 0; i < NUM_BUTTONS; ++i)
                {
                    if (i == currentPageIdx)
                    {
                        pageButtons[i].Icon().SetColor(TITLE_COLOR);
                    }
                    else
                    {
                        pageButtons[i].Icon().ResetColor();
                    }
                }

                if (currentPageIdx == -1)
                {
                    return;
                }

                foreach (QuickRecipe r in recipes)
                {
                    if (r != null)
                    {
                        r.KillAndErase();
                        r.Destroy();
                    }
                }
                recipes.Clear();

                Component content = list.Content();

                content.Clear();

                title.visible = true;
                title.Label(Document.ALCHEMY_GUIDE.PageTitle(currentPageIdx));
                title.SetRect(0, 0, Width(), 10);
                content.Add(title);

                body.MaxWidth((int)Width());
                body.Text(Document.ALCHEMY_GUIDE.PageBody(currentPageIdx));
                body.SetPos(0, title.Bottom());
                content.Add(body);

                List<QuickRecipe> toAdd = QuickRecipe.GetRecipes(currentPageIdx);

                float left;
                float top = body.Bottom() + 1;
                int w;
                List<QuickRecipe> toAddThisRow = new List<QuickRecipe>();
                while (toAdd.Count > 0)
                {
                    if (toAdd[0] == null)
                    {
                        toAdd.RemoveAt(0);
                        top += 6;
                    }

                    w = 0;
                    while (toAdd.Count > 0 &&
                        toAdd[0] != null &&
                        w + toAdd[0].Width() <= Width())
                    {
                        var toRemove = toAdd[0];
                        toAdd.RemoveAt(0);
                        toAddThisRow.Add(toRemove);
                        w += (int)(toAddThisRow[0].Width());
                    }

                    float spacing = (Width() - w) / (toAddThisRow.Count + 1);
                    left = spacing;
                    while (toAddThisRow.Count > 0)
                    {
                        var r = toAddThisRow[0];
                        toAddThisRow.RemoveAt(0);
                        r.SetPos(left, top);
                        left += r.Width() + spacing;
                        if (toAddThisRow.Count > 0)
                        {
                            ColorBlock spacer = new ColorBlock(1, 16, new Color(0x22, 0x22, 0x22, 0xFF));
                            spacer.y = top;
                            spacer.x = left - spacing / 2 - 0.5f;
                            PixelScene.Align(spacer);
                            content.Add(spacer);
                        }
                        recipes.Add(r);
                        content.Add(r);
                    }

                    if (toAdd.Count > 0 && toAdd[0] == null)
                    {
                        toAdd.RemoveAt(0);
                    }

                    if (toAdd.Count > 0 && toAdd[0] != null)
                    {
                        ColorBlock spacer = new ColorBlock(Width(), 1, new Color(0x22, 0x22, 0x22, 0xFF));
                        spacer.y = top + 16;
                        spacer.x = 0;
                        content.Add(spacer);
                    }
                    top += 17;
                    toAddThisRow.Clear();
                }
                top -= 1;
                content.SetSize(Width(), top);
                list.SetSize(list.Width(), list.Height());
                list.ScrollTo(0, 0);
            }
        }  // AlchemyTab

        private class NotesTab : Component
        {
            private ScrollPane list;

            protected override void CreateChildren()
            {
                list = new ScrollPane(new Component());
                Add(list);
            }

            protected override void Layout()
            {
                base.Layout();
                list.SetRect(0, 0, width, height);
            }

            public void UpdateList()
            {
                Component content = list.Content();

                float pos = 0;

                //Keys
                //List<Notes.KeyRecord> keys = Notes.GetRecords<Notes.KeyRecord>();
                //
                List<Notes.KeyRecord> keys = Notes.GetKeyRecords();
                if (keys.Count > 0)
                {
                    ColorBlock line = new ColorBlock(Width(), 1, new Color(0x22, 0x22, 0x22, 0xFF));
                    line.y = pos;
                    content.Add(line);

                    RenderedTextBlock title = PixelScene.RenderTextBlock(Messages.Get(this, "keys"), 9);
                    title.Hardlight(TITLE_COLOR);
                    title.MaxWidth((int)Width() - 2);
                    title.SetPos((Width() - title.Width()) / 2f, pos + 1 + ((ITEM_HEIGHT) - title.Height()) / 2f);
                    PixelScene.Align(title);
                    content.Add(title);

                    pos += Math.Max(ITEM_HEIGHT, title.Height());
                }

                foreach (Notes.Record rec in keys)
                {
                    ListItem item = new ListItem(Icons.DEPTH.Get(),
                            Messages.TitleCase(rec.Desc()), rec.Depth());
                    item.SetRect(0, pos, Width(), ITEM_HEIGHT);
                    content.Add(item);

                    pos += item.Height();
                }

                //Landmarks
                //List<Notes.LandmarkRecord> landmarks = Notes.GetRecords<Notes.LandmarkRecord>();
                List<Notes.LandmarkRecord> landmarks = Notes.GetLandmarkRecords();
                if (landmarks.Count > 0)
                {
                    ColorBlock line = new ColorBlock(Width(), 1, new Color(0x22, 0x22, 0x22, 0xFF));
                    line.y = pos;
                    content.Add(line);

                    RenderedTextBlock title = PixelScene.RenderTextBlock(Messages.Get(this, "landmarks"), 9);
                    title.Hardlight(TITLE_COLOR);
                    title.MaxWidth((int)Width() - 2);
                    title.SetPos((Width() - title.Width()) / 2f, pos + 1 + ((ITEM_HEIGHT) - title.Height()) / 2f);
                    PixelScene.Align(title);
                    content.Add(title);

                    pos += Math.Max(ITEM_HEIGHT, title.Height());
                }
                foreach (Notes.Record rec in landmarks)
                {
                    ListItem item = new ListItem(Icons.DEPTH.Get(),
                            Messages.TitleCase(rec.Desc()), rec.Depth());
                    item.SetRect(0, pos, Width(), ITEM_HEIGHT);
                    content.Add(item);

                    pos += item.Height();
                }

                content.SetSize(Width(), pos);
                list.SetSize(list.Width(), list.Height());
            }
        }  // NotesTab

        private class CatalogTab : Component
        {
            private ActionRedButton[] itemButtons;
            private const int NUM_BUTTONS = 7;

            private static int currentItemIdx = 0;

            //sprite locations
            private const int WEAPON_IDX = 0;
            private const int ARMOR_IDX = 1;
            private const int WAND_IDX = 2;
            private const int RING_IDX = 3;
            private const int ARTIF_IDX = 4;
            private const int POTION_IDX = 5;
            private const int SCROLL_IDX = 6;

            private static readonly int[] spriteIndexes = { 1, 2, 4, 5, 6, 9, 11 };

            private ScrollPane paneList;

            private List<CatalogItem> items = new List<CatalogItem>();

            protected override void CreateChildren()
            {
                itemButtons = new ActionRedButton[NUM_BUTTONS];
                for (int i = 0; i < NUM_BUTTONS; ++i)
                {
                    int idx = i;

                    itemButtons[i] = new ActionRedButton("");
                    itemButtons[i].action = () =>
                    {
                        currentItemIdx = idx;
                        UpdateList();
                    };

                    itemButtons[i].Icon(new ItemSprite(ItemSpriteSheet.SOMETHING + spriteIndexes[i], null));
                    Add(itemButtons[i]);
                }

                paneList = new CatalogTabScollPane(new Component());
                ((CatalogTabScollPane)paneList).tab = this;
                Add(paneList);
            }

            private class CatalogTabScollPane : ScrollPane
            {
                internal CatalogTab tab;

                public CatalogTabScollPane(Component component)
                    : base(component)
                { }

                public override void OnClick(float x, float y)
                {
                    foreach (var item in tab.items)
                    {
                        if (item.OnClick(x, y))
                        {
                            break;
                        }
                    }
                }
            }

            protected override void Layout()
            {
                base.Layout();

                int perRow = NUM_BUTTONS;
                float buttonWidth = Width() / perRow;

                for (int i = 0; i < NUM_BUTTONS; ++i)
                {
                    itemButtons[i].SetRect((i % perRow) * (buttonWidth), (i / perRow) * (ITEM_HEIGHT),
                            buttonWidth, ITEM_HEIGHT);
                    PixelScene.Align(itemButtons[i]);
                }

                paneList.SetRect(0,
                        itemButtons[NUM_BUTTONS - 1].Bottom() + 1, width,
                        height - itemButtons[NUM_BUTTONS - 1].Bottom() - 1);
            }

            public void UpdateList()
            {
                items.Clear();

                for (int i = 0; i < NUM_BUTTONS; ++i)
                {
                    if (i == currentItemIdx)
                    {
                        itemButtons[i].Icon().SetColor(TITLE_COLOR);
                    }
                    else
                    {
                        itemButtons[i].Icon().ResetColor();
                    }
                }

                Component content = paneList.Content();
                content.Clear();
                paneList.ScrollTo(0, 0);

                List<Type> itemClasses;
                Dictionary<Type, bool> known = new Dictionary<Type, bool>();
                if (currentItemIdx == WEAPON_IDX)
                {
                    itemClasses = new List<Type>(Catalog.WEAPONS.Items());
                    foreach (Type cls in itemClasses)
                        known.Add(cls, true);
                }
                else if (currentItemIdx == ARMOR_IDX)
                {
                    itemClasses = new List<Type>(Catalog.ARMOR.Items());
                    foreach (Type cls in itemClasses)
                        known.Add(cls, true);
                }
                else if (currentItemIdx == WAND_IDX)
                {
                    itemClasses = new List<Type>(Catalog.WANDS.Items());
                    foreach (Type cls in itemClasses)
                        known.Add(cls, true);
                }
                else if (currentItemIdx == RING_IDX)
                {
                    itemClasses = new List<Type>(Catalog.RINGS.Items());
                    foreach (Type cls in itemClasses)
                        known.Add(cls, Ring.GetKnown().Contains(cls));
                }
                else if (currentItemIdx == ARTIF_IDX)
                {
                    itemClasses = new List<Type>(Catalog.ARTIFACTS.Items());
                    foreach (Type cls in itemClasses)
                        known.Add(cls, true);
                }
                else if (currentItemIdx == POTION_IDX)
                {
                    itemClasses = new List<Type>(Catalog.POTIONS.Items());
                    foreach (Type cls in itemClasses)
                        known.Add(cls, Potion.GetKnown().Contains(cls));
                }
                else if (currentItemIdx == SCROLL_IDX)
                {
                    itemClasses = new List<Type>(Catalog.SCROLLS.Items());
                    foreach (Type cls in itemClasses)
                        known.Add(cls, Scroll.GetKnown().Contains(cls));
                }
                else
                {
                    itemClasses = new List<Type>();
                }

                ItemComparator comparator = new ItemComparator();
                comparator.known = known;
                itemClasses.Sort(comparator);

                float pos = 0;
                foreach (Type itemClass in itemClasses)
                {
                    CatalogItem item = new CatalogItem((Item)Reflection.NewInstance(itemClass), known[itemClass], CatalogExtensions.IsSeen(itemClass));
                    item.SetRect(0, pos, width, ITEM_HEIGHT);
                    content.Add(item);
                    items.Add(item);

                    pos += item.Height();
                }

                content.SetSize(width, pos);
                paneList.SetSize(paneList.Width(), paneList.Height());
            }

            public class ItemComparator : IComparer<Type>
            {
                internal Dictionary<Type, bool> known;

                public int Compare(Type a, Type b)
                {
                    int result = 0;

                    //specifically known items appear first, then seen items, then unknown items.
                    if (known[a] && CatalogExtensions.IsSeen(a))
                        result -= 2;
                    if (known[b] && CatalogExtensions.IsSeen(b))
                        result += 2;
                    if (CatalogExtensions.IsSeen(a))
                        --result;
                    if (CatalogExtensions.IsSeen(b))
                        ++result;

                    return result;
                }
            }

            private class CatalogItem : ListItem
            {
                private Item item;
                private bool seen;

                public CatalogItem(Item item, bool IDed, bool seen)
                    : base(new ItemSprite(item), Messages.TitleCase(item.TrueName()))
                {
                    this.item = item;
                    this.seen = seen;

                    if (seen && !IDed)
                    {
                        if (item is Ring)
                        {
                            ((Ring)item).Anonymize();
                        }
                        else if (item is Potion)
                        {
                            ((Potion)item).Anonymize();
                        }
                        else if (item is Scroll)
                        {
                            ((Scroll)item).Anonymize();
                        }
                    }

                    if (!seen)
                    {
                        icon.Copy(new ItemSprite(ItemSpriteSheet.SOMETHING + spriteIndexes[currentItemIdx], null));
                        label.Text("???");
                        label.Hardlight(new Color(0x99, 0x99, 0x99, 0xFF));
                    }
                    else if (!IDed)
                    {
                        icon.Copy(new ItemSprite(ItemSpriteSheet.SOMETHING + spriteIndexes[currentItemIdx], null));
                        label.Hardlight(new Color(0xCC, 0xCC, 0xCC, 0xFF));
                    }
                }

                public bool OnClick(float x, float y)
                {
                    if (Inside(x, y) && seen)
                    {
                        if (item is ClassArmor)
                        {
                            GameScene.Show(new WndTitledMessage(new Image(icon),
                                    Messages.TitleCase(item.TrueName()), item.Desc()));
                        }
                        else
                        {
                            GameScene.Show(new WndTitledMessage(new Image(icon),
                                    Messages.TitleCase(item.TrueName()), item.Info()));
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}