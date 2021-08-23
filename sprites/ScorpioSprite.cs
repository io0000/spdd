using watabou.noosa;
using watabou.utils;
using spdd.items;

namespace spdd.sprites
{
    public class ScorpioSprite : MobSprite
    {
        private int cellToAttack;

        public ScorpioSprite() : base()
        {
            Texture(Assets.Sprites.SCORPIO);

            var frames = new TextureFilm(texture, 18, 17);

            idle = new Animation(12, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 1, 2, 1, 2);

            run = new Animation(8, true);
            run.Frames(frames, 5, 5, 6, 6);

            attack = new Animation(15, false);
            attack.Frames(frames, 0, 3, 4);

            zap = attack.Clone();

            die = new Animation(12, false);
            die.Frames(frames, 0, 7, 8, 9, 10);

            Play(idle);
        }

        public override Color Blood()
        {
            return new Color(0x44, 0xFF, 0x22, 0xFF);
        }

        public override void Attack(int cell)
        {
            if (!Dungeon.level.Adjacent(cell, ch.pos))
            {
                cellToAttack = cell;
                TurnTo(ch.pos, cell);
                Play(zap);
            }
            else
            {
                base.Attack(cell);
            }
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == zap)
            {
                Idle();

                var spr = parent.Recycle<MissileSprite>();

                var callback = new ActionCallback();
                callback.action = () => ch.OnAttackComplete();

                spr.Reset(this,
                    cellToAttack,
                    new ScorpioShot(),
                    callback);
            }
            else
            {
                base.OnComplete(anim);
            }
        }

        public class ScorpioShot : Item
        {
            public ScorpioShot()
            {
                image = ItemSpriteSheet.FISHING_SPEAR;
            }
        }
    }
}