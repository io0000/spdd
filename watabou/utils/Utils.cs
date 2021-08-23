using System;
using System.Globalization;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;

namespace watabou.utils
{
    public class Utils
    {
        static GregorianCalendar gc = new GregorianCalendar();

        static int GetWeekOfYear(DateTime time)
        {
            return gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        public static int GetWeekOfMonth(DateTime time)
        {
            DateTime first = new DateTime(time.Year, time.Month, 1);
            return GetWeekOfYear(time) - GetWeekOfYear(first) + 1;
        }

        public static bool CheckObjectType(object cause, Type t)
        {
            if (cause == null)
                return false;

            if (cause is Type && ((Type)cause) == t)
                return true;

            if (cause.GetType() == t)
                return true;

            return false;
        }

        // code from Dear_ImGui_Sample
        [Conditional("DEBUG")]
        public static void CheckGLError(string title)
        {
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Debug.Print($"{title}: {error}");
            }
        }
    }
}