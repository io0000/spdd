using watabou.utils;
using watabou.noosa;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.mobs;
using spdd.effects;

namespace spdd.sprites
{
    //TODO improvements here
    public class SpinnerSprite : MobSprite
    {
        public SpinnerSprite()
        {
            perspectiveRaise = 0.0f;

            Texture(Assets.Sprites.SPINNER);

            var frames = new TextureFilm(texture, 16, 16);

            idle = new Animation(10, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 1, 0, 1);

            run = new Animation(15, true);
            run.Frames(frames, 0, 2, 0, 3);

            attack = new Animation(12, false);
            attack.Frames(frames, 0, 4, 5, 0);

            zap = attack.Clone();

            die = new Animation(12, false);
            die.Frames(frames, 6, 7, 8, 9);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);
            if (parent != null)
                parent.SendToBack(this);
            renderShadow = false;
        }

        public override void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);

            var callback = new ActionCallback();
            callback.action = () => ((Spinner)ch).ShootWeb();

            MagicMissile.BoltFromChar(parent,
                    MagicMissile.MAGIC_MISSILE,
                    this,
                    cell,
                    callback);

            Sample.Instance.Play(Assets.Sounds.MISS);
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == zap)
            {
                Play(run);
            }
            base.OnComplete(anim);
        }

        public override Color Blood()
        {
            // BF E5 B8 FF
            return new Color(0xBF, 0xE5, 0xB8, 0xFF);
        }
    }
}