using watabou.gltextures;
using watabou.noosa;
using watabou.utils;
using spdd.actors.hero;

namespace spdd.sprites
{
    public class HeroSprite : CharSprite
    {
        private const int FRAME_WIDTH = 12;
        private const int FRAME_HEIGHT = 15;

        private const int RUN_FRAMERATE = 20;

        private static TextureFilm tiers;

        private Animation fly;
        private Animation read;

        public HeroSprite()
        {
            Texture(Dungeon.hero.heroClass.Spritesheet());
            UpdateArmor();

            Link(Dungeon.hero);

            if (ch.IsAlive())
                Idle();
            else
                Die();
        }

        public void UpdateArmor()
        {
            var film = new TextureFilm(Tiers(), Dungeon.hero.Tier(), FRAME_WIDTH, FRAME_HEIGHT);

            idle = new Animation(1, true);
            idle.Frames(film, 0, 0, 0, 1, 0, 0, 1, 1);

            run = new Animation(RUN_FRAMERATE, true);
            run.Frames(film, 2, 3, 4, 5, 6, 7);

            die = new Animation(20, false);
            die.Frames(film, 8, 9, 10, 11, 12, 11);

            attack = new Animation(15, false);
            attack.Frames(film, 13, 14, 15, 0);

            zap = attack.Clone();

            operate = new Animation(8, false);
            operate.Frames(film, 16, 17, 16, 17);

            fly = new Animation(1, true);
            fly.Frames(film, 18);

            read = new Animation(20, false);
            read.Frames(film, 19, 20, 20, 20, 20, 20, 20, 20, 20, 19);

            if (Dungeon.hero.IsAlive())
                Idle();
            else
                Die();
        }

        public override void Place(int p)
        {
            base.Place(p);
            Camera.main.PanTo(Center(), 5.0f);
        }

        public override void Move(int from, int to)
        {
            base.Move(from, to);

            if (ch.flying)
                Play(fly);

            Camera.main.PanFollow(this, 20.0f);
        }

        public override void Jump(int from, int to, ICallback callback)
        {
            base.Jump(from, to, callback);
            Play(fly);
        }

        public void Read()
        {
            var callback = new ActionCallback();
            callback.action = () =>
            {
                Idle();
                ch.OnOperateComplete();
            };

            animCallback = callback;
            Play(read);
        }

        public override void BloodBurstA(PointF from, int damage)
        {
            //Does nothing.

            /*
             * This is both for visual clarity, and also for content ratings regarding violence
             * towards human characters. The heroes are the only human or human-like characters which
             * participate in combat, so removing all blood associated with them is a simple way to
             * reduce the violence rating of the game.
             */
        }

        public override void Update()
        {
            sleeping = ch.IsAlive() && ((Hero)ch).resting;

            base.Update();
        }

        public void Sprint(float speed)
        {
            run.delay = 1f / speed / RUN_FRAMERATE;
        }

        public static TextureFilm Tiers()
        {
            if (tiers == null)
            {
                // Sprites for All classes are the same in size
                var texture = TextureCache.Get(Assets.Sprites.ROGUE);
                tiers = new TextureFilm(texture, texture.width, FRAME_HEIGHT);
            }

            return tiers;
        }

        public static Image Avatar(HeroClass cl, int armorTier)
        {
            RectF patch = Tiers().Get(armorTier);
            Image avatar = new Image(cl.Spritesheet());
            RectF frame = avatar.texture.UvRect(1, 0, FRAME_WIDTH, FRAME_HEIGHT);
            frame.Shift(patch.left, patch.top);
            avatar.Frame(frame);

            return avatar;
        }
    }
}