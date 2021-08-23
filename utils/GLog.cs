using watabou.utils;
using spdd.messages;

namespace spdd.utils
{
    public class GLog
    {
        public const string TAG = "GAME";

        public const string POSITIVE = "++ ";
        public const string NEGATIVE = "-- ";
        public const string WARNING = "** ";
        public const string HIGHLIGHT = "@@ ";

        public const string NEW_LINE = "\n";


        public static Signal<string> update = new Signal<string>();

        public static void NewLine()
        {
            update.Dispatch(NEW_LINE);
        }

        public static void Information(string text, params object[] args)
        {
            if (args.Length > 0)
                text = Messages.Format(text, args);

            DeviceCompat.Log(TAG, text);
            update.Dispatch(text);
        }

        public static void Positive(string text, params object[] args)
        {
            Information(POSITIVE + text, args);
        }

        public static void Negative(string text, params object[] args)
        {
            Information(NEGATIVE + text, args);
        }

        public static void Warning(string text, params object[] args)
        {
            Information(WARNING + text, args);
        }

        public static void Highlight(string text, params object[] args)
        {
            Information(HIGHLIGHT + text, args);
        }
    }
}