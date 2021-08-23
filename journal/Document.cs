using System.Collections.Generic;
using Microsoft.Collections.Extensions;
using watabou.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.journal
{
    public class Document
    {
        public static Document ADVENTURERS_GUIDE = new Document(ItemSpriteSheet.GUIDE_PAGE, "ADVENTURERS_GUIDE");
        public static Document ALCHEMY_GUIDE = new Document(ItemSpriteSheet.ALCH_PAGE, "ALCHEMY_GUIDE");

        public static IEnumerable<Document> Values()
        {
            yield return ADVENTURERS_GUIDE;
            yield return ALCHEMY_GUIDE;
        }

        private OrderedDictionary<string, bool> pages = new OrderedDictionary<string, bool>();
        private int pageSprite;
        private string name;

        public Document(int sprite, string name)
        {
            pageSprite = sprite;
            this.name = name;
        }

        public List<string> Pages()
        {
            return new List<string>(pages.Keys);
        }

        public bool AddPage(string page)
        {
            if (pages.ContainsKey(page) && !pages[page])
            {
                pages[page] = true;
                Journal.saveNeeded = true;
                return true;
            }
            return false;
        }

        public bool HasPage(string page)
        {
            return pages.ContainsKey(page) && pages[page];
        }

        public bool HasPage(int pageIdx)
        {
            var pages = Pages();
            return HasPage(pages[pageIdx]);
        }

        public int PageSprite()
        {
            return pageSprite;
        }

        private string Name()
        {
            return name;
        }

        public string Title()
        {
            return Messages.Get(this, Name() + ".title");
        }

        public string PageTitle(string page)
        {
            return Messages.Get(this, Name() + "." + page + ".title");
        }

        public string PageTitle(int pageIdx)
        {
            var pages = Pages();
            var page = pages[pageIdx];
            return PageTitle(page);
        }

        public string PageBody(string page)
        {
            return Messages.Get(this, Name() + "." + page + ".body");
        }

        public string PageBody(int pageIdx)
        {
            var pages = Pages();
            var page = pages[pageIdx];
            return PageBody(page);
        }

        public const string GUIDE_INTRO_PAGE = "Intro";
        public const string GUIDE_SEARCH_PAGE = "Examining_and_Searching";

        static Document()
        {
            ADVENTURERS_GUIDE.pages.Add(GUIDE_INTRO_PAGE, DeviceCompat.IsDebug());
            ADVENTURERS_GUIDE.pages.Add("Identifying", DeviceCompat.IsDebug());
            ADVENTURERS_GUIDE.pages.Add(GUIDE_SEARCH_PAGE, DeviceCompat.IsDebug());
            ADVENTURERS_GUIDE.pages.Add("Strength", DeviceCompat.IsDebug());
            ADVENTURERS_GUIDE.pages.Add("Food", DeviceCompat.IsDebug());
            ADVENTURERS_GUIDE.pages.Add("Levelling", DeviceCompat.IsDebug());
            ADVENTURERS_GUIDE.pages.Add("Surprise_Attacks", DeviceCompat.IsDebug());
            ADVENTURERS_GUIDE.pages.Add("Dieing", DeviceCompat.IsDebug());
            ADVENTURERS_GUIDE.pages.Add("Looting", DeviceCompat.IsDebug());
            ADVENTURERS_GUIDE.pages.Add("Magic", DeviceCompat.IsDebug());

            //sewers
            ALCHEMY_GUIDE.pages.Add("Potions", DeviceCompat.IsDebug());
            ALCHEMY_GUIDE.pages.Add("Stones", DeviceCompat.IsDebug());
            ALCHEMY_GUIDE.pages.Add("Energy_Food", DeviceCompat.IsDebug());
            ALCHEMY_GUIDE.pages.Add("Bombs", DeviceCompat.IsDebug());
            //ALCHEMY_GUIDE.pages.Add("Darts",              DeviceCompat.IsDebug());

            //prison
            ALCHEMY_GUIDE.pages.Add("Exotic_Potions", DeviceCompat.IsDebug());
            ALCHEMY_GUIDE.pages.Add("Exotic_Scrolls", DeviceCompat.IsDebug());

            //caves
            ALCHEMY_GUIDE.pages.Add("Catalysts", DeviceCompat.IsDebug());
            ALCHEMY_GUIDE.pages.Add("Brews_Elixirs", DeviceCompat.IsDebug());
            ALCHEMY_GUIDE.pages.Add("Spells", DeviceCompat.IsDebug());
        }

        private const string DOCUMENTS = "documents";

        public static void Store(Bundle bundle)
        {
            Bundle docBundle = new Bundle();

            foreach (Document doc in Values())
            {
                List<string> pages = new List<string>();
                foreach (var page in doc.Pages())   // pages.Keys
                {
                    if (doc.pages[page])
                    {
                        pages.Add(page);
                    }
                }
                if (pages.Count > 0)
                {
                    docBundle.Put(doc.Name(), pages.ToArray());
                }
            }

            bundle.Put(DOCUMENTS, docBundle);
        }

        public static void Restore(Bundle bundle)
        {
            if (!bundle.Contains(DOCUMENTS))
                return;

            Bundle docBundle = bundle.GetBundle(DOCUMENTS);

            foreach (Document doc in Values())
            {
                if (docBundle.Contains(doc.Name()))
                {
                    List<string> pages = new List<string>();
                    foreach (var str in docBundle.GetStringArray(doc.Name()))
                        pages.Add(str);

                    foreach (string page in pages)
                    {
                        if (doc.pages.ContainsKey(page))
                        {
                            doc.pages[page] = true;
                        }
                    }
                }
            }
        }
    }
}

