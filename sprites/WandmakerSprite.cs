using watabou.noosa;
using watabou.noosa.audio;
using spdd.actors;
using spdd.effects.particles;

namespace spdd.sprites
{
    public class WandmakerSprite : MobSprite
    {
        public WandmakerSprite()
        {
            Texture(Assets.Sprites.MAKER);

            var frames = new TextureFilm(texture, 12, 14);

            idle = new Animation(10, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 3, 3, 3, 3, 3, 2, 1);

            run = new Animation(20, true);
            run.Frames(frames, 0);

            die = new Animation(20, false);
            die.Frames(frames, 0);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);
            Add(State.SHIELDED);
        }

        public override void Die()
        {
            base.Die();

            Remove(State.SHIELDED);
            Emitter().Start(ElmoParticle.Factory, 0.03f, 60);

            if (visible)
                Sample.Instance.Play(Assets.Sounds.BURNING);
        }
    }
}