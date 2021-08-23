using watabou.noosa;
using spdd.actors;
using spdd.actors.mobs.npcs;
using spdd.effects;

namespace spdd.sprites
{
    public class ImpSprite : MobSprite
    {
        public ImpSprite()
        {
            Texture(Assets.Sprites.IMP);

            var frames = new TextureFilm(texture, 12, 14);

            idle = new Animation(10, true);
            idle.Frames(frames,
                0, 1, 2, 3, 0, 1, 2, 3, 0, 0, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
                0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 3, 0, 0, 0, 4, 4, 4, 4, 4, 4, 4, 4, 0, 0, 0, 4, 4, 4, 4, 4, 4, 4, 4);

            run = new Animation(20, true);
            run.Frames(frames, 0);

            die = new Animation(10, false);
            die.Frames(frames, 0, 3, 2, 1, 0, 3, 2, 1, 0);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            if (ch is Imp)
                Alpha(0.4f);
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == die)
            {
                Emitter().Burst(Speck.Factory(Speck.WOOL), 15);
                KillAndErase();
            }
            else
            {
                base.OnComplete(anim);
            }
        }
    }
}