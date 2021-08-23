using watabou.noosa;
using spdd.sprites;

namespace spdd.effects
{
    public class DarkBlock : Gizmo
    {
        private CharSprite target;

        public DarkBlock(CharSprite target)
        {
            this.target = target;
        }

        public override void Update()
        {
            base.Update();

            target.Brightness(0.4f);
        }

        public void Lighten()
        {
            target.ResetColor();
            KillAndErase();
        }

        public static DarkBlock Darken(CharSprite sprite)
        {
            var darkBlock = new DarkBlock(sprite);
            if (sprite.parent != null)
                sprite.parent.Add(darkBlock);

            return darkBlock;
        }
    }
}