using watabou.noosa;

namespace spdd
{
    public class Chrome
    {
        public enum Type
        {
            TOAST,
            TOAST_TR,
            WINDOW,
            WINDOW_SILVER,
            RED_BUTTON,
            GREY_BUTTON,
            GREY_BUTTON_TR,
            TAG,
            GEM,
            SCROLL,
            TAB_SET,
            TAB_SELECTED,
            TAB_UNSELECTED
        }

        public static NinePatch Get(Type type)
        {
            string Asset = Assets.Interfaces.CHROME;
            switch (type)
            {
                case Type.WINDOW:
                    return new NinePatch(Asset, 0, 0, 20, 20, 6);
                case Type.WINDOW_SILVER:
                    return new NinePatch(Asset, 86, 0, 22, 22, 7);
                case Type.TOAST:
                    return new NinePatch(Asset, 22, 0, 18, 18, 5);
                case Type.TOAST_TR:
                    return new NinePatch(Asset, 40, 0, 18, 18, 5);
                case Type.RED_BUTTON:
                    return new NinePatch(Asset, 58, 0, 6, 6, 2);
                case Type.GREY_BUTTON:
                    return new NinePatch(Asset, 58, 6, 6, 6, 2);
                case Type.TAG:
                    return new NinePatch(Asset, 22, 18, 16, 14, 3);
                case Type.GEM:
                    return new NinePatch(Asset, 0, 32, 32, 32, 13);
                case Type.GREY_BUTTON_TR:
                    return new NinePatch(Asset, 53, 20, 9, 9, 5);
                case Type.SCROLL:
                    return new NinePatch(Asset, 32, 32, 32, 32, 5, 11, 5, 11);
                case Type.TAB_SET:
                    return new NinePatch(Asset, 64, 0, 20, 20, 6);
                case Type.TAB_SELECTED:
                    return new NinePatch(Asset, 65, 22, 8, 13, 3, 7, 3, 5);
                case Type.TAB_UNSELECTED:
                    return new NinePatch(Asset, 75, 22, 8, 13, 3, 7, 3, 5);
                default:
                    return null;
            }
        }
    }
}