using watabou.noosa;
using spdd.effects.particles;

namespace spdd.sprites
{
    public class GuardSprite : MobSprite
    {
        public GuardSprite()
        {
            Texture(Assets.Sprites.GUARD);

            var frames = new TextureFilm(texture, 12, 16);

            idle = new Animation(2, true);
            idle.Frames(frames, 0, 0, 0, 1, 0, 0, 1, 1);

            run = new Animation(15, true);
            run.Frames(frames, 2, 3, 4, 5, 6, 7);

            attack = new Animation(12, false);
            attack.Frames(frames, 8, 9, 10);

            die = new Animation(8, false);
            die.Frames(frames, 11, 12, 13, 14);

            Play(idle);
        }

        public override void Play(Animation anim)
        {
            if (anim == die)
            {
                Emitter().Burst(ShadowParticle.Up, 4);
            }
            base.Play(anim);
        }
    }
}