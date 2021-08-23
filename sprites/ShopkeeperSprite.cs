using watabou.utils;
using watabou.noosa;
using watabou.noosa.particles;

namespace spdd.sprites
{
    public class ShopkeeperSprite : MobSprite
    {
        private PixelParticle coin;

        public ShopkeeperSprite()
        {
            Texture(Assets.Sprites.KEEPER);
            var film = new TextureFilm(texture, 14, 14);

            idle = new Animation(10, true);
            idle.Frames(film, 1, 1, 1, 1, 1, 0, 0, 0, 0);

            die = new Animation(20, false);
            die.Frames(film, 0);

            run = idle.Clone();

            attack = idle.Clone();

            Idle();
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim);

            if (visible && anim == idle)
            {
                if (coin == null)
                {
                    coin = new PixelParticle();
                    parent.Add(coin);
                }

                coin.Reset(x + (flipHorizontal ? 0 : 13), y + 7, new Color(0xFF, 0xFF, 0x00, 0xFF), 1, 0.5f);
                coin.speed.y = -40;
                coin.acc.y = +160;
            }
        }
    }
}