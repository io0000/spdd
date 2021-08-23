using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

namespace spdd.messages
{
    /*
        Simple wrapper class for libGDX I18NBundles.

        The core idea here is that each string resource's key is a combination of the class definition and a local value.
        An object or static method would usually call this with an object/class reference (usually its own) and a local key.
        This means that an object can just ask for "name" rather than, say, "items.weapon.enchantments.death.name"
    */
    public class Messages
    {
        private static List<I18NBundle> bundles;
        private static Languages lang = Languages.ENGLISH;

        public static Languages Lang()
        {
            return lang;
        }

        /**
         * Setup Methods
         */

        private static string[] prop_files = new string[]{
            Assets.Messages.ACTORS,
            Assets.Messages.ITEMS,
            Assets.Messages.JOURNAL,
            Assets.Messages.LEVELS,
            Assets.Messages.MISC,
            Assets.Messages.PLANTS,
            Assets.Messages.SCENES,
            Assets.Messages.UI,
            Assets.Messages.WINDOWS
        };

        static Messages()
        {
            Setup(SPDSettings.Language());
        }

        public static void Setup(Languages lang)
        {
            ////seeing as missing keys are part of our process, this is faster than throwing an exception
            //I18NBundle.setExceptionOnMissingKey(false);
            //
            bundles = new List<I18NBundle>();
            Messages.lang = lang;
            CultureInfo locale = new CultureInfo(lang.Code());

            foreach (string file in prop_files)
            {
                //bundles.Add(I18NBundle.CreateBundle(Gdx.files.internal (file), locale));
                var fh = new FileHandle(file);
                bundles.Add(I18NBundle.CreateBundle(fh, locale));
            }
        }

        /**
         * Resource grabbing methods
         */

        //public static string Get(string key, params object[] args)
        //{
        //    return Get(null, key, args);
        //}

        public static string Get(object o, string k, params object[] args)
        {
            return Get(o.GetType(), k, args);
        }

        public static string Get(Type c, string k, params object[] args)
        {
            string key;
            if (c != null)
            {
                key = c.FullName.Replace("spdd.", "");
                key = key.Replace(".Character", ".Char");   // .properties 파일에는 Character class이름이 Char로 되어 있음
                key = key.Replace('+', '$');                // sub class - java와 c# 표기가 다름
                key += "." + k;
            }
            else
            {
                key = k;
            }

            // https://stackoverflow.com/questions/6225808/string-tolower-and-string-tolowerinvariant
            string value = GetFromBundle(key.ToLowerInvariant());
            if (value != null)
            {
                if (args.Length > 0)
                    return Format(value, args);
                else
                    return value;
            }
            else
            {
                //this is so child classes can inherit properties from their parents.
                //in cases where text is commonly grabbed as a utility from classes that aren't mean to be instantiated
                //(e.g. flavourbuff.dispTurns()) using .class directly is probably smarter to prevent unnecessary recursive calls.
                if (c != null && c.BaseType != null)
                {
                    return Get(c.BaseType, k, args);
                }
                else
                {
                    Debug.Assert(false);
                    return "!!!NO TEXT FOUND!!!";
                }
            }
        }

        private static string GetFromBundle(string key)
        {
            string result;
            foreach (I18NBundle b in bundles)
            {
                result = b.Get(key);
                //if it isn't the return string for no key found, return it
                //if (result.Length != key.Length + 6 || !result.Contains(key))
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static string Format(string format, params object[] args)
        {
            return clojure.lang.Printf.Format(format, args);
        }

        public static string Capitalize(string str)
        {
            //if (str.length() == 0)  return str;
            //else                    return Character.toTitleCase( str.charAt( 0 ) ) + str.substring( 1 );

            if (!string.IsNullOrEmpty(str))
            {
                var ti = CultureInfo.CurrentCulture.TextInfo;
                str = ti.ToTitleCase(str.Substring(0, 1)) + str.Substring(1);
            }
            return str;
        }

        public static string TitleCase(string str)
        {
            //English capitalizes every word except for a few exceptions
            if (lang == Languages.ENGLISH)
            {
                //String result = "";
                ////split by any unicode space character
                //for (String word : str.split("(?<=\\p{Zs})"))
                //{
                //    if (noCaps.contains(word.trim().toLowerCase(Locale.ENGLISH).replaceAll(":|[0-9]", "")))
                //    {
                //        result += word;
                //    }
                //    else
                //    {
                //        result += capitalize(word);
                //    }
                //}
                ////first character is always capitalized.
                //return capitalize(result);
                var ti = CultureInfo.CurrentCulture.TextInfo;
                return ti.ToTitleCase(str);
            }

            //Otherwise, use sentence case
            return Capitalize(str);
        }
    }
}