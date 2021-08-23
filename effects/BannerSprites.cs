using watabou.noosa;

namespace spdd.effects
{
    public class BannerSprites
    {
        public enum Type
        {
            PIXEL_DUNGEON,
            BOSS_SLAIN,
            GAME_OVER,
            SELECT_YOUR_HERO,
            PIXEL_DUNGEON_SIGNS,
        }

        public static Image Get(Type type)
        {
            var icon = new Image(Assets.Interfaces.BANNERS);
            switch (type)
            {
                case Type.PIXEL_DUNGEON:
                    icon.Frame(icon.texture.UvRect(0, 0, 132, 90));
                    break;
                case Type.BOSS_SLAIN:
                    icon.Frame(icon.texture.UvRect(0, 90, 128, 125));
                    break;
                case Type.GAME_OVER:
                    icon.Frame(icon.texture.UvRect(0, 125, 128, 160));
                    break;
                case Type.SELECT_YOUR_HERO:
                    icon.Frame(icon.texture.UvRect(0, 160, 128, 181));
                    break;
                case Type.PIXEL_DUNGEON_SIGNS:
                    icon.Frame(icon.texture.UvRect(132, 0, 256, 90));
                    break;
            }
            return icon;
        }
    }
}