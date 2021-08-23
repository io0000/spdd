using System.Collections.Generic;
using Microsoft.Collections.Extensions;

namespace watabou.input
{
    //FIXME at lot of the logic here, in WndKeyBindings, and SPDAction is fairly messy
    // should see about doing some refactoring to clean this up
    public class KeyBindings
    {
        private static OrderedDictionary<int, GameAction> bindings = new OrderedDictionary<int, GameAction>();

        public static OrderedDictionary<int, GameAction> GetAllBindings()
        {
            return new OrderedDictionary<int, GameAction>(bindings);
        }

        public static void SetAllBindings(OrderedDictionary<int, GameAction> newBindings)
        {
            bindings = new OrderedDictionary<int, GameAction>(newBindings);
        }

        //these are special keybinding that are not user-configurable
        private static OrderedDictionary<int, GameAction> hardBindings = new OrderedDictionary<int, GameAction>();

        public static void AddHardBinding(int keyCode, GameAction action)
        {
            //hardBindings[keyCode] = action;

            int index = hardBindings.IndexOf(keyCode);
            if (index >= 0)
            {
                hardBindings[index] = action;
            }
            else
            {
                hardBindings.Add(keyCode, action);
            }
        }

        public static bool acceptUnbound = false;

        public static bool IsKeyBound(int keyCode)
        {
            if (keyCode <= 0 || keyCode > 255)
            {
                return false;
            }

            //return acceptUnbound || bindings.ContainsKey(keyCode) || hardBindings.ContainsKey(keyCode);
            if (acceptUnbound)
                return true;

            int index;

            index = bindings.IndexOf(keyCode);
            if (index >= 0)
                return true;
            index = hardBindings.IndexOf(keyCode);
            if (index >= 0)
                return true;

            return false;
        }

        public static GameAction GetActionForKey(KeyEvent ev)
        {
            int index = 0;
            index = bindings.IndexOf(ev.code);

            if (index >= 0)
                return bindings[index];

            index = hardBindings.IndexOf(ev.code);
            if (index >= 0)
                return hardBindings[index];

            return GameAction.NONE;
        }

        public static List<int> GetBoundKeysForAction(GameAction action)
        {
            List<int> result = new List<int>();
            foreach (var pair in bindings)
            {
                int i = pair.Key;
                var act = pair.Value;
                if (act == action)
                {
                    result.Add(i);
                }
            }
            return result;
        }

        public static string GetKeyName(int keyCode)
        {
            if (keyCode == Keys.UNKNOWN)
            {
                return "None";
            }
            else if (keyCode == Keys.PLUS)
            {
                return "+";
            }
            else if (keyCode == Keys.BACKSPACE)
            {
                return "Backspace";
            }
            else if (keyCode == Keys.FORWARD_DEL)
            {
                return "Delete";
            }
            else
            {
                return Keys.ToString(keyCode);
            }
        }
    }
}

