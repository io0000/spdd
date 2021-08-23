using watabou.noosa;
using watabou.utils;
using spdd.items.weapon.missiles.darts;

namespace spdd.sprites
{
    public class GnollTricksterSprite : MobSprite
    {
        private Animation cast;

        public GnollTricksterSprite()
        {
            Texture(Assets.Sprites.GNOLL);

            var frames = new TextureFilm(texture, 12, 15);

            idle = new Animation(2, true);
            idle.Frames(frames, 21, 21, 21, 22, 21, 21, 22, 22);

            run = new Animation(12, true);
            run.Frames(frames, 25, 26, 27, 28);

            attack = new Animation(12, false);
            attack.Frames(frames, 23, 24, 21);

            cast = attack.Clone();

            die = new Animation(12, false);
            die.Frames(frames, 29, 30, 31);

            Play(idle);
        }

        public override void Attack(int cell)
        {
            if (!Dungeon.level.Adjacent(cell, ch.pos))
            {
                var callback = new ActionCallback();
                callback.action = () =>
                {
                    ch.OnAttackComplete();
                };

                parent.Recycle<MissileSprite>().
                    Reset(this, cell, new ParalyticDart(), callback);

                Play(cast);
                TurnTo(ch.pos, cell);
            }
            else
            {
                base.Attack(cell);
            }
        }
    }
}