using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.effects;
using spdd.actors.mobs;

namespace spdd.sprites
{
    public class DM201Sprite : MobSprite
    {
        public DM201Sprite()
        {
            Texture(Assets.Sprites.DM200);

            var frames = new TextureFilm(texture, 21, 18);

            int c = 12;

            idle = new Animation(2, true);
            idle.Frames(frames, c + 0, c + 1);

            run = idle.Clone();

            attack = new Animation(15, false);
            attack.Frames(frames, c + 4, c + 5, c + 6);

            zap = new Animation(15, false);
            zap.Frames(frames, c + 7, c + 8, c + 8, c + 7);

            die = new Animation(8, false);
            die.Frames(frames, c + 9, c + 10, c + 11);

            Play(idle);
        }

        public override void Place(int cell)
        {
            if (parent != null)
                parent.BringToFront(this);
            base.Place(cell);
        }

        public override void Die()
        {
            Emitter().Burst(Speck.Factory(Speck.WOOL), 8);
            base.Die();
        }

        public override void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);

            var callback = new ActionCallback();
            callback.action = () =>
            {
                Sample.Instance.Play(Assets.Sounds.PUFF);
                ((DM201)ch).OnZapComplete();
            };

            MagicMissile.BoltFromChar(parent,
                    MagicMissile.CORROSION,
                    this,
                    cell,
                    callback);
            Sample.Instance.Play(Assets.Sounds.MISS, 0.6f, 0.6f, 1.5f);
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == zap)
                Idle();

            base.OnComplete(anim);
        }
    }
}