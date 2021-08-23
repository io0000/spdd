using watabou.noosa;
using spdd.effects;
using spdd.effects.particles;

namespace spdd.sprites
{
    public class SuccubusSprite : MobSprite
    {
        public SuccubusSprite()
        {
            Texture(Assets.Sprites.SUCCUBUS);

            var frames = new TextureFilm(texture, 12, 15);

            idle = new Animation(8, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 2, 2, 2, 1);

            run = new Animation(15, true);
            run.Frames(frames, 3, 4, 5, 6, 7, 8);

            attack = new Animation(12, false);
            attack.Frames(frames, 9, 10, 11);

            die = new Animation(10, false);
            die.Frames(frames, 12);

            Play(idle);
        }

        public override void Die()
        {
            base.Die();
            Emitter().Burst(Speck.Factory(Speck.HEART), 6);
            Emitter().Burst(ShadowParticle.Up, 8);
        }
    }
}