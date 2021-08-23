using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using watabou.utils;
using watabou.noosa.audio;
using spdd.scenes;
using spdd.messages;

namespace spdd
{
    public class SPDSettings
    {
        private static JObject data = new JObject();
        public const string DEFAULT_PREFS_FILE = "settings.json";

        public static void Load()
        {
            try
            {
                var str = FileUtils.Read(DEFAULT_PREFS_FILE);
                data = JObject.Parse(str);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static bool Contains(string key)
        {
            return data.ContainsKey(key);
        }

        public static int GetInt(string key, int defValue)
        {
            return GetInt(key, defValue, int.MinValue, int.MaxValue);
        }

        public static int GetInt(string key, int defValue, int min, int max)
        {
            try
            {
                var token = data.GetValue(key);
                if (token == null)
                    return defValue;

                int value = token.ToObject<int>();

                if (value < min || value > max)
                {
                    value = GameMath.Clamp(value, min, max);
                    Put(key, value);
                    return value;
                }
                else
                {
                    return value;
                }
            }
            catch (Exception)
            {
                //ShatteredPixelDungeon.reportException(e);
                Put(key, defValue);
                return defValue;
            }
        }

        public static long GetLong(string key, long defValue)
        {
            return GetLong(key, defValue, long.MinValue, long.MaxValue);
        }

        public static long GetLong(string key, long defValue, long min, long max)
        {
            try
            {
                var token = data.GetValue(key);
                if (token == null)
                    return defValue;

                long value = token.ToObject<long>();

                if (value < min || value > max)
                {
                    long val = GameMath.Clamp(min, value, max);
                    Put(key, val);
                    return val;
                }
                else
                {
                    return value;
                }
            }
            catch (Exception)
            {
                //ShatteredPixelDungeon.reportException(e);
                Put(key, defValue);
                return defValue;
            }
        }

        public static bool GetBoolean(string key, bool defValue)
        {
            try
            {
                var token = data.GetValue(key);
                if (token == null)
                    return defValue;

                bool value = token.ToObject<bool>();
                return value;
            }
            catch (Exception)
            {
                //ShatteredPixelDungeon.reportException(e);
                Put(key, defValue);
                return defValue;
            }
        }

        public static string GetString(string key, string defValue)
        {
            try
            {
                var token = data.GetValue(key);
                if (token == null)
                    return defValue;

                string value = token.ToObject<string>();
                if (string.IsNullOrEmpty(value))
                    return defValue;

                return value;
            }
            catch (Exception)
            {
                //ShatteredPixelDungeon.reportException(e);
                Put(key, defValue);
                return defValue;
            }
        }

        public static void Put(string key, int value)
        {
            data[key] = value;
            Flush();
        }

        public static void Put(string key, long value)
        {
            data[key] = value;
            Flush();
        }

        public static void Put(string key, bool value)
        {
            data[key] = value;
            Flush();
        }

        public static void Put(string key, string value)
        {
            data[key] = value;
            Flush();
        }

        public static void Flush()
        {
            FileUtils.Write(DEFAULT_PREFS_FILE, data.ToString());
        }
    

        //Version info

        public const string KEY_VERSION = "version";

        public static void Version(int value)
        {
            Put(KEY_VERSION, value);
        }

        public static int Version()
        {
            return GetInt(KEY_VERSION, 0);
        }

        //Graphics

        public const string KEY_FULLSCREEN = "fullscreen";
        public const string KEY_LANDSCAPE = "landscape";
        public const string KEY_POWER_SAVER = "power_saver";
        public const string KEY_SCALE = "scale";
        public const string KEY_ZOOM = "zoom";
        public const string KEY_BRIGHTNESS = "brightness";
        public const string KEY_GRID = "visual_grid";

        public static void Fullscreen(bool value)
        {
            Put(KEY_FULLSCREEN, value);

            ShatteredPixelDungeonDash.UpdateSystemUI();
        }

        public static bool Fullscreen()
        {
            return GetBoolean(KEY_FULLSCREEN, false);
        }

        public static void Landscape(bool value)
        {
            Put(KEY_LANDSCAPE, value);
            ((ShatteredPixelDungeonDash)ShatteredPixelDungeonDash.instance).UpdateDisplaySize();
        }

        //can return null because we need to directly handle the case of landscape not being set
        // as there are different defaults for different devices
        public static bool? Landscape()
        {
            if (Contains(KEY_LANDSCAPE))
            {
                return GetBoolean(KEY_LANDSCAPE, false);
            }
            else
            {
                return null;
            }
        }

        public static void PowerSaver(bool value)
        {
            Put(KEY_POWER_SAVER, value);
            ((ShatteredPixelDungeonDash)ShatteredPixelDungeonDash.instance).UpdateDisplaySize();
        }

        public static bool PowerSaver()
        {
            return GetBoolean(KEY_POWER_SAVER, false);
        }

        public static void Scale(int value)
        {
            Put(KEY_SCALE, value);
        }

        public static int Scale()
        {
            return GetInt(KEY_SCALE, 0);
        }

        public static void Zoom(int value)
        {
            Put(KEY_ZOOM, value);
        }

        public static int Zoom()
        {
            return GetInt(KEY_ZOOM, 0);
        }

        public static void Brightness(int value)
        {
            Put(KEY_BRIGHTNESS, value);
            GameScene.UpdateFog();
        }

        public static int Brightness()
        {
            return GetInt(KEY_BRIGHTNESS, 0, -1, 1);
        }

        public static void VisualGrid(int value)
        {
            Put(KEY_GRID, value);
            GameScene.UpdateMap();
        }

        public static int VisualGrid()
        {
            return GetInt(KEY_GRID, 0, -1, 2);
        }

        //Interface

        public const string KEY_QUICKSLOTS = "quickslots";
        public const string KEY_FLIPTOOLBAR = "flipped_ui";
        public const string KEY_FLIPTAGS = "flip_tags";
        public const string KEY_BARMODE = "toolbar_mode";

        //public static void QuickSlots(int value)
        //{
        //    Put(KEY_QUICKSLOTS, value);
        //}
        //
        //public static int QuickSlots()
        //{
        //    return GetInt(KEY_QUICKSLOTS, 4, 0, 4);
        //}

        public static void FlipToolbar(bool value)
        {
            Put(KEY_FLIPTOOLBAR, value);
        }

        public static bool FlipToolbar()
        {
            return GetBoolean(KEY_FLIPTOOLBAR, false);
        }

        public static void FlipTags(bool value)
        {
            Put(KEY_FLIPTAGS, value);
        }

        public static bool FlipTags()
        {
            return GetBoolean(KEY_FLIPTAGS, false);
        }

        public static void ToolbarMode(string value)
        {
            Put(KEY_BARMODE, value);
        }

        public static string ToolbarMode()
        {
            return GetString(KEY_BARMODE, PixelScene.Landscape() ? "GROUP" : "SPLIT");
        }

        //Game State

        public const string KEY_LAST_CLASS = "last_class";
        public const string KEY_CHALLENGES = "challenges";
        public const string KEY_INTRO = "intro";

        //public const string KEY_SUPPORT_NAGGED = "support_nagged";

        public static void Intro(bool value)
        {
            Put(KEY_INTRO, value);
        }

        public static bool Intro()
        {
            return GetBoolean(KEY_INTRO, true);
        }

        public static void LastClass(int value)
        {
            Put(KEY_LAST_CLASS, value);
        }

        public static int LastClass()
        {
            return GetInt(KEY_LAST_CLASS, 0, 0, 3);
        }

        public static void Challenges(int value)
        {
            Put(KEY_CHALLENGES, value);
        }

        public static int Challenges()
        {
            return GetInt(KEY_CHALLENGES, 0, 0, spdd.Challenges.MAX_VALUE);
        }

        //public static void SupportNagged(bool value)
        //{
        //    Put(KEY_SUPPORT_NAGGED, value);
        //}
        //
        //public static bool SupportNagged()
        //{
        //    return GetBoolean(KEY_SUPPORT_NAGGED, false);
        //}

        //Audio

        public const string KEY_MUSIC = "music";
        public const string KEY_MUSIC_VOL = "music_vol";
        public const string KEY_SOUND_FX = "soundfx";
        public const string KEY_SFX_VOL = "sfx_vol";

        public static void Music(bool value)
        {
            watabou.noosa.audio.Music.Instance.Enable(value);
            Put(KEY_MUSIC, value);
        }

        public static bool Music()
        {
            return GetBoolean(KEY_MUSIC, true);
        }

        public static void MusicVol(int value)
        {
            watabou.noosa.audio.Music.Instance.Volume(value * value / 100f);
            Put(KEY_MUSIC_VOL, value);
        }

        public static int MusicVol()
        {
            return GetInt(KEY_MUSIC_VOL, 10, 0, 10);
        }

        public static void SoundFx(bool value)
        {
            Sample.Instance.Enable(value);
            Put(KEY_SOUND_FX, value);
        }

        public static bool SoundFx()
        {
            return GetBoolean(KEY_SOUND_FX, true);
        }

        public static void SFXVol(int value)
        {
            Sample.Instance.Volume(value * value / 100f);
            Put(KEY_SFX_VOL, value);
        }

        public static int SFXVol()
        {
            return GetInt(KEY_SFX_VOL, 10, 0, 10);
        }

        //Languages and Font

        public const string KEY_LANG = "language";
        public const string KEY_SYSTEMFONT = "system_font";

        public static void Language(Languages lang)
        {
            Put(KEY_LANG, lang.Code());
        }

        public static Languages Language()
        {
            string code = GetString(KEY_LANG, null);
            if (code == null)
            {
                return Languages.MatchLocale(CultureInfo.CurrentCulture);
            }
            else
            {
                return Languages.MatchCode(code);
            }
        }

        public static void SystemFont(bool value)
        {
            Put(KEY_SYSTEMFONT, value);
        }

        public static bool SystemFont()
        {
            return GetBoolean(KEY_SYSTEMFONT,
                    (Language() == Languages.KOREAN || Language() == Languages.CHINESE || Language() == Languages.JAPANESE));
        }

        //Connectivity

        public const string KEY_NEWS = "news";
        public const string KEY_UPDATES = "updates";
        public const string KEY_WIFI = "wifi";

        public const string KEY_NEWS_LAST_READ = "news_last_read";

        public static void News(bool value)
        {
            Put(KEY_NEWS, value);
        }

        public static bool News()
        {
            return GetBoolean(KEY_NEWS, true);
        }

        public static void Updates(bool value)
        {
            Put(KEY_UPDATES, value);
        }

        public static bool Updates()
        {
            return GetBoolean(KEY_UPDATES, true);
        }

        public static void WiFi(bool value)
        {
            Put(KEY_WIFI, value);
        }

        public static bool WiFi()
        {
            return GetBoolean(KEY_WIFI, true);
        }

        public static void NewsLastRead(long lastRead)
        {
            Put(KEY_NEWS_LAST_READ, lastRead);
        }

        public static long NewsLastRead()
        {
            return GetLong(KEY_NEWS_LAST_READ, 0);
        }

        //Window management (desktop only atm)

        public const string KEY_WINDOW_WIDTH = "window_width";
        public const string KEY_WINDOW_HEIGHT = "window_height";
        public const string KEY_WINDOW_MAXIMIZED = "window_maximized";

        public static void WindowResolution(Point p)
        {
            Put(KEY_WINDOW_WIDTH, p.x);
            Put(KEY_WINDOW_HEIGHT, p.y);
        }

        public static Point WindowResolution()
        {
            return new Point(
                    GetInt(KEY_WINDOW_WIDTH, 960, 480, int.MaxValue),
                    GetInt(KEY_WINDOW_HEIGHT, 640, 320, int.MaxValue)
            );
        }

        public static void WindowMaximized(bool value)
        {
            Put(KEY_WINDOW_MAXIMIZED, value);
        }

        public static bool WindowMaximized()
        {
            return GetBoolean(KEY_WINDOW_MAXIMIZED, false);
        }
    }
}
