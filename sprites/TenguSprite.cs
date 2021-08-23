using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.items;
using spdd.scenes;

namespace spdd.sprites
{
    public class TenguSprite : MobSprite
    {
        public TenguSprite()
        {
            Texture(Assets.Sprites.TENGU);

            var frames = new TextureFilm(texture, 14, 16);

            idle = new Animation(2, true);
            idle.Frames(frames, 0, 0, 0, 1);

            run = new Animation(15, false);
            run.Frames(frames, 2, 3, 4, 5, 0);

            attack = new Animation(15, false);
            attack.Frames(frames, 6, 7, 7, 0);

            zap = attack.Clone();

            die = new Animation(8, false);
            die.Frames(frames, 8, 9, 10, 10, 10, 10, 10, 10);

            Play(run.Clone());
        }

        public override void Idle()
        {
            isMoving = false;
            base.Idle();
        }

        public override void Move(int from, int to)
        {
            Place(to);

            Play(run);
            TurnTo(from, to);

            isMoving = true;

            if (Dungeon.level.water[to])
                GameScene.Ripple(to);
        }

        public override void Attack(int cell)
        {
            if (!Dungeon.level.Adjacent(cell, ch.pos))
            {
                var spr = parent.Recycle<MissileSprite>();

                var callback = new ActionCallback();
                callback.action = () => ch.OnAttackComplete();

                spr.Reset(this, cell, new TenguShuriken(), callback);

                Play(zap);
                TurnTo(ch.pos, cell);
            }
            else
            {
                base.Attack(cell);
            }
        }

        public override void OnComplete(Animation anim)
        {
            if (anim == run)
            {
                isMoving = false;
                Idle();

                // notifyAll();
                Actor.InputMoveDone(ch);
            }
            else
            {
                base.OnComplete(anim);
            }
        }

        [SPDStatic]
        public class TenguShuriken : Item
        {
            public TenguShuriken()
            {
                image = ItemSpriteSheet.SHURIKEN;
            }
        }
    }
}