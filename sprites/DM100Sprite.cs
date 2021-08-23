using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.effects;
using spdd.actors;
using spdd.actors.mobs;

namespace spdd.sprites
{
    public class DM100Sprite : MobSprite
    {
        public DM100Sprite()
        {
            Texture(Assets.Sprites.DM100);

            var frames = new TextureFilm(texture, 16, 14);

            idle = new Animation(1, true);
            idle.Frames(frames, 0, 1);

            run = new Animation(12, true);
            run.Frames(frames, 6, 7, 8, 9);

            attack = new Animation(12, false);
            attack.Frames(frames, 2, 3, 4, 0);

            zap = new Animation(8, false);
            zap.Frames(frames, 5, 5, 1);

            die = new Animation(12, false);
            die.Frames(frames, 10, 11, 12, 13, 14, 15);

            Play(idle);
        }

        public override void Zap(int pos)
        {
            var enemy = Actor.FindChar(pos);

            //shoot lightning from eye, not sprite center.
            PointF origin = Center();
            if (flipHorizontal)
            {
                origin.y -= 6 * scale.y;
                origin.x -= 1 * scale.x;
            }
            else
            {
                origin.y -= 8 * scale.y;
                origin.x += 1 * scale.x;
            }
            if (enemy != null)
            {
                parent.Add(new Lightning(origin, enemy.sprite.DestinationCenter(), (DM100)ch));
            }
            else
            {
                parent.Add(new Lightning(origin, pos, (DM100)ch));
            }
            Sample.Instance.Play(Assets.Sounds.LIGHTNING);

            TurnTo(ch.pos, pos);
            Flash();
            Play(zap);
        }

        public override void Die()
        {
            Emitter().Burst(Speck.Factory(Speck.WOOL), 5);
            base.Die();
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == zap)
                Idle();

            base.OnComplete(anim);
        }
    }
}