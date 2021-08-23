using System;
using System.IO;
using Microsoft.Collections.Extensions;
using watabou.utils;
using watabou.input;

namespace spdd
{
    public class SPDAction : GameAction
    {
        protected SPDAction(string name)
            : base(name)
        {}

        //--New references to existing actions from GameAction
        public new static readonly GameAction NONE = GameAction.NONE;
        public new static readonly GameAction BACK = GameAction.BACK;
        //--

        public static readonly GameAction HERO_INFO = new SPDAction("hero_info");
        public static readonly GameAction JOURNAL = new SPDAction("journal");

        public static readonly GameAction WAIT = new SPDAction("wait");
        public static readonly GameAction SEARCH = new SPDAction("search");
        public static readonly GameAction REST = new SPDAction("rest");

        public static readonly GameAction INVENTORY = new SPDAction("inventory");
        public static readonly GameAction QUICKSLOT_1 = new SPDAction("quickslot_1");
        public static readonly GameAction QUICKSLOT_2 = new SPDAction("quickslot_2");
        public static readonly GameAction QUICKSLOT_3 = new SPDAction("quickslot_3");
        public static readonly GameAction QUICKSLOT_4 = new SPDAction("quickslot_4");

        public static readonly GameAction TAG_ATTACK = new SPDAction("tag_attack");
        public static readonly GameAction TAG_DANGER = new SPDAction("tag_danger");
        public static readonly GameAction TAG_ACTION = new SPDAction("tag_action");
        public static readonly GameAction TAG_LOOT = new SPDAction("tag_loot");
        public static readonly GameAction TAG_RESUME = new SPDAction("tag_resume");

        public static readonly GameAction ZOOM_IN = new SPDAction("zoom_in");
        public static readonly GameAction ZOOM_OUT = new SPDAction("zoom_out");

        public static readonly GameAction N = new SPDAction("n");
        public static readonly GameAction E = new SPDAction("e");
        public static readonly GameAction S = new SPDAction("s");
        public static readonly GameAction W = new SPDAction("w");
        public static readonly GameAction NE = new SPDAction("ne");
        public static readonly GameAction SE = new SPDAction("se");
        public static readonly GameAction SW = new SPDAction("sw");
        public static readonly GameAction NW = new SPDAction("nw");

        private static readonly OrderedDictionary<int, GameAction> defaultBindings = new OrderedDictionary<int, GameAction>();

        static SPDAction()
        {
            defaultBindings.Add(Keys.ESCAPE, SPDAction.BACK);
            defaultBindings.Add(Keys.BACKSPACE, SPDAction.BACK);

            defaultBindings.Add(Keys.H, SPDAction.HERO_INFO);
            defaultBindings.Add(Keys.J, SPDAction.JOURNAL);

            defaultBindings.Add(Keys.SPACE, SPDAction.WAIT);
            defaultBindings.Add(Keys.S, SPDAction.SEARCH);
            defaultBindings.Add(Keys.Z, SPDAction.REST);

            defaultBindings.Add(Keys.I, SPDAction.INVENTORY);
            defaultBindings.Add(Keys.Q, SPDAction.QUICKSLOT_1);
            defaultBindings.Add(Keys.W, SPDAction.QUICKSLOT_2);
            defaultBindings.Add(Keys.E, SPDAction.QUICKSLOT_3);
            defaultBindings.Add(Keys.R, SPDAction.QUICKSLOT_4);

            defaultBindings.Add(Keys.A, SPDAction.TAG_ATTACK);
            defaultBindings.Add(Keys.TAB, SPDAction.TAG_DANGER);
            defaultBindings.Add(Keys.D, SPDAction.TAG_ACTION);
            defaultBindings.Add(Keys.ENTER, SPDAction.TAG_LOOT);
            defaultBindings.Add(Keys.T, SPDAction.TAG_RESUME);

            defaultBindings.Add(Keys.PLUS, SPDAction.ZOOM_IN);
            defaultBindings.Add(Keys.EQUALS, SPDAction.ZOOM_IN);
            defaultBindings.Add(Keys.MINUS, SPDAction.ZOOM_OUT);

            defaultBindings.Add(Keys.UP, SPDAction.N);
            defaultBindings.Add(Keys.RIGHT, SPDAction.E);
            defaultBindings.Add(Keys.DOWN, SPDAction.S);
            defaultBindings.Add(Keys.LEFT, SPDAction.W);

            defaultBindings.Add(Keys.NUMPAD_5, SPDAction.WAIT);
            defaultBindings.Add(Keys.NUMPAD_8, SPDAction.N);
            defaultBindings.Add(Keys.NUMPAD_9, SPDAction.NE);
            defaultBindings.Add(Keys.NUMPAD_6, SPDAction.E);
            defaultBindings.Add(Keys.NUMPAD_3, SPDAction.SE);
            defaultBindings.Add(Keys.NUMPAD_2, SPDAction.S);
            defaultBindings.Add(Keys.NUMPAD_1, SPDAction.SW);
            defaultBindings.Add(Keys.NUMPAD_4, SPDAction.W);
            defaultBindings.Add(Keys.NUMPAD_7, SPDAction.NW);

            InitAndroidDevices();
        }

        public static OrderedDictionary<int, GameAction> GetDefaults()
        {
            return new OrderedDictionary<int, GameAction>(defaultBindings);
        }

        //hard bindings for android devices
        static void InitAndroidDevices()
        {
            KeyBindings.AddHardBinding(Keys.BACK, SPDAction.BACK);
            KeyBindings.AddHardBinding(Keys.MENU, SPDAction.INVENTORY);
        }

        //we only save/loads keys which differ from the default configuration.
        private const string BINDINGS_FILE = "keybinds.dat";

        public static void LoadBindings()
        {
            if (KeyBindings.GetAllBindings().Count > 0)
                return;

            try
            {
                Bundle b = FileUtils.BundleFromFile(BINDINGS_FILE);

                Bundle firstKeys = b.GetBundle("first_keys");
                Bundle secondKeys = b.GetBundle("second_keys");

                OrderedDictionary<int, GameAction> defaults = GetDefaults();
                OrderedDictionary<int, GameAction> custom = new OrderedDictionary<int, GameAction>();

                foreach (GameAction a in AllActions())
                {
                    if (firstKeys.Contains(a.Name()))
                    {
                        if (firstKeys.GetInt(a.Name()) == 0)
                        {
                            foreach (int i in defaults.Keys)
                            {
                                if (defaults[i] == a)
                                {
                                    defaults.Remove(i);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            custom.Add(firstKeys.GetInt(a.Name()), a);
                            defaults.Remove(firstKeys.GetInt(a.Name()));
                        }
                    }

                    //we store any custom second keys in defaults for the moment to preserve order
                    //incase the 2nd key is custom but the first one isn't
                    if (secondKeys.Contains(a.Name()))
                    {
                        if (secondKeys.GetInt(a.Name()) == 0)
                        {
                            int last = 0;
                            foreach (int i in defaults.Keys)
                            {
                                if (defaults[i] == a)
                                {
                                    last = i;
                                }
                            }
                            defaults.Remove(last);
                        }
                        else
                        {
                            defaults.Remove(secondKeys.GetInt(a.Name()));
                            defaults.Add(secondKeys.GetInt(a.Name()), a);
                        }
                    }
                }

                //now merge them and store
                foreach (var pair in defaults)
                {
                    int i = pair.Key;
                    if (i != 0)
                    {
                        custom.Add(i, pair.Value);
                    }
                }

                KeyBindings.SetAllBindings(custom);
            }
            catch (Exception )
            {
                KeyBindings.SetAllBindings(GetDefaults());
            }
        }

        public static void SaveBindings()
        {
            Bundle b = new Bundle();

            Bundle firstKeys = new Bundle();
            Bundle secondKeys = new Bundle();

            foreach (GameAction a in AllActions())
            {
                int firstCur = 0;
                int secondCur = 0;
                int firstDef = 0;
                int secondDef = 0;

                foreach (var pair in defaultBindings)
                {
                    int i = pair.Key;
                    if (pair.Value == a)
                    {
                        if (firstDef == 0)
                        {
                            firstDef = i;
                        }
                        else
                        {
                            secondDef = i;
                        }
                    }
                }

                OrderedDictionary<int, GameAction> curBindings = KeyBindings.GetAllBindings();
                foreach (var pair in curBindings)
                {
                    int i = pair.Key;
                    if (pair.Value == a)
                    {
                        if (firstCur == 0)
                        {
                            firstCur = i;
                        }
                        else
                        {
                            secondCur = i;
                        }
                    }
                }

                if (firstCur != firstDef)
                {
                    firstKeys.Put(a.Name(), firstCur);
                }
                if (secondCur != secondDef)
                {
                    secondKeys.Put(a.Name(), secondCur);
                }
            }

            b.Put("first_keys", firstKeys);
            b.Put("second_keys", secondKeys);

            try
            {
                FileUtils.BundleToFile(BINDINGS_FILE, b);
            }
            catch (IOException e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
            }
        }
    }
}
