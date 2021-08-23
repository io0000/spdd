using watabou.noosa;
using spdd.actors.hero;
using spdd.scenes;

namespace spdd.ui
{
    public enum Icons
    {
        //button icons
        CHECKED,
        UNCHECKED,
        INFO,
        CHALLENGE_OFF,
        CHALLENGE_ON,
        PREFS,
        LANGS,
        EXIT,
        CLOSE,
        ARROW,
        DISPLAY,
        DATA,
        AUDIO,

        //ingame UI icons
        SKULL,
        BUSY,
        COMPASS,
        SLEEP,
        ALERT,
        LOST,
        TARGET,
        BACKPACK,
        SEED_POUCH,
        SCROLL_HOLDER,
        POTION_BANDOLIER,
        WAND_HOLSTER,

        //hero & rankings icons
        DEPTH,
        WARRIOR,
        MAGE,
        ROGUE,
        HUNTRESS,

        //main menu icons
        ENTER,
        GOLD,
        RANKINGS,
        BADGES,
        NEWS,
        CHANGES,
        SHPX,

        //misc icons
        LIBGDX,
        WATA,
        WARNING,

        //32x32 icons for credits
        ALEKS,
        CHARLIE,
        CUBE_CODE,
        PURIGRO,
        ARCNOR
    }

    public static class IconsExtensions
    {
        public static Image Get(this Icons type)
        {
            var icon = new Image(Assets.Interfaces.ICONS);
            switch (type)
            {
                case Icons.CHECKED:
                    icon.Frame(icon.texture.UvRect(0, 0, 12, 12));
                    break;
                case Icons.UNCHECKED:
                    icon.Frame(icon.texture.UvRect(16, 0, 28, 12));
                    break;
                case Icons.INFO:
                    icon.Frame(icon.texture.UvRect(32, 0, 46, 14));
                    break;
                case Icons.CHALLENGE_ON:
                    icon.Frame(icon.texture.UvRect(48, 0, 62, 12));
                    break;
                case Icons.CHALLENGE_OFF:
                    icon.Frame(icon.texture.UvRect(64, 0, 78, 12));
                    break;
                case Icons.PREFS:
                    icon.Frame(icon.texture.UvRect(80, 0, 94, 14));
                    break;
                case Icons.LANGS:
                    icon.Frame(icon.texture.UvRect(96, 0, 110, 11));
                    break;
                case Icons.EXIT:
                    icon.Frame(icon.texture.UvRect(112, 0, 127, 11));
                    break;
                case Icons.CLOSE:
                    icon.Frame(icon.texture.UvRect(0, 16, 11, 27));
                    break;
                case Icons.ARROW:
                    icon.Frame(icon.texture.UvRect(16, 16, 27, 27));
                    break;
                case Icons.DISPLAY:
                    icon.Frame(icon.texture.UvRect(32, 16, 45, 32));
                    break;
                //TODO UI icon?
                case Icons.DATA:
                    icon.Frame(icon.texture.UvRect(48, 16, 64, 31));
                    break;
                case Icons.AUDIO:
                    icon.Frame(icon.texture.UvRect(64, 16, 78, 30));
                    break;
                case Icons.SKULL:
                    icon.Frame(icon.texture.UvRect(0, 32, 8, 40));
                    break;
                case Icons.BUSY:
                    icon.Frame(icon.texture.UvRect(8, 32, 16, 40));
                    break;
                case Icons.COMPASS:
                    icon.Frame(icon.texture.UvRect(0, 40, 7, 45));
                    break;
                case Icons.SLEEP:
                    icon.Frame(icon.texture.UvRect(16, 32, 25, 40));
                    break;
                case Icons.ALERT:
                    icon.Frame(icon.texture.UvRect(16, 40, 24, 48));
                    break;
                case Icons.LOST:
                    icon.Frame(icon.texture.UvRect(24, 40, 32, 48));
                    break;
                case Icons.TARGET:
                    icon.Frame(icon.texture.UvRect(32, 32, 48, 48));
                    break;
                case Icons.BACKPACK:
                    icon.Frame(icon.texture.UvRect(48, 32, 58, 42));
                    break;
                case Icons.SCROLL_HOLDER:
                    icon.Frame(icon.texture.UvRect(58, 32, 68, 42));
                    break;
                case Icons.SEED_POUCH:
                    icon.Frame(icon.texture.UvRect(68, 32, 78, 42));
                    break;
                case Icons.WAND_HOLSTER:
                    icon.Frame(icon.texture.UvRect(78, 32, 88, 42));
                    break;
                case Icons.POTION_BANDOLIER:
                    icon.Frame(icon.texture.UvRect(88, 32, 98, 42));
                    break;

                case Icons.DEPTH:
                    icon.Frame(icon.texture.UvRect(0, 48, 13, 64));
                    break;
                case Icons.WARRIOR:
                    icon.Frame(icon.texture.UvRect(16, 48, 25, 63));
                    break;
                case Icons.MAGE:
                    icon.Frame(icon.texture.UvRect(32, 48, 47, 62));
                    break;
                case Icons.ROGUE:
                    icon.Frame(icon.texture.UvRect(48, 48, 57, 63));
                    break;
                case Icons.HUNTRESS:
                    icon.Frame(icon.texture.UvRect(64, 48, 80, 64));
                    break;

                case Icons.ENTER:
                    icon.Frame(icon.texture.UvRect(0, 64, 16, 80));
                    break;
                case Icons.RANKINGS:
                    icon.Frame(icon.texture.UvRect(17, 64, 34, 80));
                    break;
                case Icons.BADGES:
                    icon.Frame(icon.texture.UvRect(34, 64, 50, 80));
                    break;
                case Icons.NEWS:
                    icon.Frame(icon.texture.UvRect(51, 64, 67, 79));
                    break;
                case Icons.CHANGES:
                    icon.Frame(icon.texture.UvRect(68, 64, 83, 79));
                    break;
                case Icons.SHPX:
                    icon.Frame(icon.texture.UvRect(85, 64, 101, 80));
                    break;
                case Icons.GOLD:
                    icon.Frame(icon.texture.UvRect(102, 64, 119, 80));
                    break;

                case Icons.LIBGDX:
                    icon.Frame(icon.texture.UvRect(0, 81, 16, 94));
                    break;
                case Icons.WATA:
                    icon.Frame(icon.texture.UvRect(17, 81, 34, 93));
                    break;
                case Icons.WARNING:
                    icon.Frame(icon.texture.UvRect(34, 81, 48, 95));
                    break;

                //32*32 icons are scaled down to match game's size
                case Icons.ALEKS:
                    icon.Frame(icon.texture.UvRect(0, 96, 32, 128));
                    icon.scale.Set(PixelScene.Align(0.49f));
                    break;
                case Icons.CHARLIE:
                    icon.Frame(icon.texture.UvRect(32, 96, 64, 128));
                    icon.scale.Set(PixelScene.Align(0.49f));
                    break;
                case Icons.ARCNOR:
                    icon.Frame(icon.texture.UvRect(64, 96, 96, 128));
                    icon.scale.Set(PixelScene.Align(0.49f));
                    break;
                case Icons.PURIGRO:
                    icon.Frame(icon.texture.UvRect(96, 96, 128, 128));
                    icon.scale.Set(PixelScene.Align(0.49f));
                    break;
                case Icons.CUBE_CODE:
                    icon.Frame(icon.texture.UvRect(101, 32, 128, 62));
                    icon.scale.Set(PixelScene.Align(0.49f));
                    break;

            }
            return icon;
        }

        public static Image Get(HeroClass cl)
        {
            switch (cl)
            {
                case HeroClass.WARRIOR:
                    return Icons.WARRIOR.Get();
                case HeroClass.MAGE:
                    return Icons.MAGE.Get();
                case HeroClass.ROGUE:
                    return Icons.ROGUE.Get();
                case HeroClass.HUNTRESS:
                    return Icons.HUNTRESS.Get();
                default:
                    return null;
            }
        }
    }
}