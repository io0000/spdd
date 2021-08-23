using watabou.noosa;

namespace spdd.effects
{
    public class Effects
    {
        public enum Type
        {
            RIPPLE,
            LIGHTNING,
            WOUND,
            EXCLAMATION,
            CHAIN,
            DEATH_RAY,
            LIGHT_RAY,
            HEALTH_RAY
        }

        public static Image Get(Type type)
        {
            var icon = new Image(Assets.Effects.EFFECTS);
            switch (type)
            {
                case Type.RIPPLE:
                    icon.Frame(icon.texture.UvRect(0, 0, 16, 16));
                    break;
                case Type.LIGHTNING:
                    icon.Frame(icon.texture.UvRect(16, 0, 32, 8));
                    break;
                case Type.WOUND:
                    icon.Frame(icon.texture.UvRect(16, 8, 32, 16));
                    break;
                case Type.EXCLAMATION:
                    icon.Frame(icon.texture.UvRect(0, 16, 6, 25));
                    break;
                case Type.CHAIN:
                    icon.Frame(icon.texture.UvRect(6, 16, 11, 22));
                    break;
                case Type.DEATH_RAY:
                    icon.Frame(icon.texture.UvRect(16, 16, 32, 24));
                    break;
                case Type.LIGHT_RAY:
                    icon.Frame(icon.texture.UvRect(16, 23, 32, 31));
                    break;
                case Type.HEALTH_RAY:
                    icon.Frame(icon.texture.UvRect(16, 30, 32, 38));
                    break;
            }
            return icon;
        }
    }
}