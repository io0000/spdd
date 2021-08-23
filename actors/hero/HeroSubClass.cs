using watabou.utils;
using spdd.messages;

namespace spdd.actors.hero
{
    public enum HeroSubClass
    {
        NONE,
        GLADIATOR,
        BERSERKER,
        WARLOCK,
        BATTLEMAGE,
        ASSASSIN,
        FREERUNNER,
        SNIPER,
        WARDEN
    }

    public static class HeroSubClassExtensions
    {
        static string[] titles = {
            "",
            "gladiator",
            "berserker",
            "warlock",
            "battlemage",
            "assassin",
            "freerunner",
            "sniper",
            "warden"
        };

        public static string Title(this HeroSubClass subClass)
        {
            int index = (int)subClass;
            var title = titles[index];

            return Messages.Get(typeof(HeroSubClass), title);
        }

        public static string Desc(this HeroSubClass subClass)
        {
            int index = (int)subClass;
            var title = titles[index];

            return Messages.Get(typeof(HeroSubClass), title + "_desc");
        }

        private const string SUBCLASS = "subClass";

        public static void StoreInBundle(this HeroSubClass subClass, Bundle bundle)
        {
            bundle.Put(SUBCLASS, subClass.ToString());      // enum
        }

        public static HeroSubClass RestoreInBundle(Bundle bundle)
        {
            HeroSubClass type = bundle.GetEnum<HeroSubClass>(SUBCLASS);
            return type;
        }
    }
}