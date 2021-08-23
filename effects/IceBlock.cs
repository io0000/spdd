using watabou.utils;
using watabou.noosa;
using watabou.noosa.audio;
using spdd.sprites;

namespace spdd.effects
{
    public class IceBlock : Gizmo
    {
        private float phase;

        private readonly CharSprite target;

        public IceBlock(CharSprite target)
        {
            this.target = target;
            phase = 0;
        }

        public override void Update()
        {
            base.Update();

            if ((phase += Game.elapsed * 2) < 1)
                target.Tint(0.83f, 1.17f, 1.33f, phase * 0.6f);
            else
                target.Tint(0.83f, 1.17f, 1.33f, 0.6f);
        }

        public void Melt()
        {
            target.ResetColor();
            KillAndErase();

            if (visible)
            {
                var c = new Color(0xB2, 0xD6, 0xFF, 0xFF);
                Splash.At(target.Center(), c, 5);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
            }
        }

        public static IceBlock Freeze(CharSprite sprite)
        {
            var iceBlock = new IceBlock(sprite);
            if (sprite.parent != null)
                sprite.parent.Add(iceBlock);

            return iceBlock;
        }
    }
}