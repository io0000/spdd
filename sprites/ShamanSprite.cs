using watabou.noosa;
using watabou.utils;
using spdd.actors.mobs;
using spdd.effects;

namespace spdd.sprites
{
    public abstract class ShamanSprite : MobSprite
    {
        protected int boltType;
        protected abstract int TexOffset();

        public ShamanSprite()
        {
            int c = TexOffset();

            Texture(Assets.Sprites.SHAMAN);

            var frames = new TextureFilm(texture, 12, 15);

            idle = new Animation(2, true);
            idle.Frames(frames, c + 0, c + 0, c + 0, c + 1, c + 0, c + 0, c + 1, c + 1);

            run = new Animation(12, true);
            run.Frames(frames, c + 4, c + 5, c + 6, c + 7);

            attack = new Animation(12, false);
            attack.Frames(frames, c + 2, c + 3, c + 0);

            zap = attack.Clone();

            die = new Animation(12, false);
            die.Frames(frames, c + 8, c + 9, c + 10);

            Play(idle);
        }

        public override void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);

            var callback = new ActionCallback();
            callback.action = () => ((Shaman)ch).OnZapComplete();

            MagicMissile.BoltFromChar(parent,
                    boltType,
                    this,
                    cell,
                    callback);
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == zap)
            {
                Idle();
            }
            base.OnComplete(anim);
        }

        public class Red : ShamanSprite
        {
            public Red()
            {
                boltType = MagicMissile.SHAMAN_RED;
            }

            protected override int TexOffset()
            {
                return 0;
            }
        }

        public class Blue : ShamanSprite
        {
            public Blue()
            {
                boltType = MagicMissile.SHAMAN_BLUE;
            }

            protected override int TexOffset()
            {
                return 21;
            }
        }

        public class Purple : ShamanSprite
        {
            public Purple()
            {
                boltType = MagicMissile.SHAMAN_PURPLE;
            }

            protected override int TexOffset()
            {
                return 42;
            }
        }
    }
}