using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.mobs;
using spdd.effects;

namespace spdd.sprites
{
    public class WarlockSprite : MobSprite
    {
        public WarlockSprite()
        {
            Texture(Assets.Sprites.WARLOCK);

            var frames = new TextureFilm(texture, 12, 15);

            idle = new Animation(2, true);
            idle.Frames(frames, 0, 0, 0, 1, 0, 0, 1, 1);

            run = new Animation(15, true);
            run.Frames(frames, 0, 2, 3, 4);

            attack = new Animation(12, false);
            attack.Frames(frames, 0, 5, 6);

            zap = attack.Clone();

            die = new Animation(15, false);
            die.Frames(frames, 0, 7, 8, 8, 9, 10);

            Play(idle);
        }

        public override void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);

            var callback = new ActionCallback();
            callback.action = () => ((Warlock)ch).OnZapComplete();

            MagicMissile.BoltFromChar(parent,
                    MagicMissile.SHADOW,
                    this,
                    cell,
                    callback);

            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == zap)
                Idle();

            base.OnComplete(anim);
        }
    }
}