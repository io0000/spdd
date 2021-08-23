using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.effects;
using spdd.actors.mobs;

namespace spdd.sprites
{
    public class DM200Sprite : MobSprite
    {
        public DM200Sprite()
        {
            Texture(Assets.Sprites.DM200);

            var frames = new TextureFilm(texture, 21, 18);

            idle = new Animation(10, true);
            idle.Frames(frames, 0, 1);

            run = new Animation(10, true);
            run.Frames(frames, 2, 3);

            attack = new Animation(15, false);
            attack.Frames(frames, 4, 5, 6);

            zap = new Animation(15, false);
            zap.Frames(frames, 7, 8, 8, 7);

            die = new Animation(8, false);
            die.Frames(frames, 9, 10, 11);

            Play(idle);
        }

        public override void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);

            var callback = new ActionCallback();
            callback.action = () => ((DM200)ch).OnZapComplete();

            MagicMissile.BoltFromChar(parent,
                    MagicMissile.TOXIC_VENT,
                    this,
                    cell,
                    callback);
            Sample.Instance.Play(Assets.Sounds.GAS);
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

        public override void OnComplete(Animation anim)
        {
            if (anim == zap)
            {
                Idle();
            }
            base.OnComplete(anim);
        }
    }
}