using watabou.noosa;
using watabou.noosa.audio;
using spdd.actors.mobs;

namespace spdd.sprites
{
    public class NecromancerSprite : MobSprite
    {
        private Animation charging;

        public NecromancerSprite()
        {
            Texture(Assets.Sprites.NECRO);

            var frames = new TextureFilm(texture, 16, 16);

            idle = new Animation(1, true);
            idle.Frames(frames, 0, 0, 0, 1, 0, 0, 0, 0, 1);

            run = new Animation(8, true);
            run.Frames(frames, 0, 0, 0, 2, 3, 4);

            zap = new Animation(10, false);
            zap.Frames(frames, 5, 6, 7, 8);

            charging = new Animation(5, true);
            charging.Frames(frames, 7, 8);

            die = new Animation(10, false);
            die.Frames(frames, 9, 10, 11, 12);

            attack = zap.Clone();

            Play(idle);
        }

        public void Charge()
        {
            Play(charging);
        }

        public override void Zap(int cell)
        {
            base.Zap(cell);
            if (visible && ch is Necromancer && ((Necromancer)ch).Summoning)
                Sample.Instance.Play(Assets.Sounds.CHARGEUP, 1.0f, 0.8f);
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim);

            if (anim == zap)
            {
                if (ch is Necromancer)
                {
                    if (((Necromancer)ch).Summoning)
                    {
                        Charge();
                    }
                    else
                    {
                        ((Necromancer)ch).OnZapComplete();
                        Idle();
                    }
                }
                else
                {
                    Idle();
                }
            }
        }
    }
}