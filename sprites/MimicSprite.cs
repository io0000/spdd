using watabou.noosa;
using spdd.actors;

namespace spdd.sprites
{
    public class MimicSprite : MobSprite
    {
        private Animation hiding;

        protected virtual int TexOffset()
        {
            return 0;
        }

        public MimicSprite()
        {
            //adjust shadow slightly to account for 1 empty bottom pixel (used for border while hiding)
            perspectiveRaise = 5 / 16f; //5 pixels
            shadowWidth = 1f;
            shadowOffset = -0.4f;

            int c = TexOffset();

            Texture(Assets.Sprites.MIMIC);

            var frames = new TextureFilm(texture, 16, 16);

            hiding = new Animation(1, true);
            hiding.Frames(frames, 0 + c, 0 + c, 0 + c, 0 + c, 0 + c, 1 + c);

            idle = new Animation(5, true);
            idle.Frames(frames, 2 + c, 2 + c, 2 + c, 3 + c, 3 + c);

            run = new Animation(10, true);
            run.Frames(frames, 2 + c, 3 + c, 4 + c, 5 + c, 5 + c, 4 + c, 3 + c);

            attack = new Animation(10, false);
            attack.Frames(frames, 2 + c, 6 + c, 7 + c, 8 + c);

            die = new Animation(5, false);
            die.Frames(frames, 9 + c, 10 + c, 11 + c);

            Play(idle);
        }

        public override void LinkVisuals(Character ch)
        {
            base.LinkVisuals(ch);
            if (ch.alignment == Character.Alignment.NEUTRAL)
            {
                HideMimic();
            }
        }

        public void HideMimic()
        {
            Play(hiding);
            HideSleep();
        }

        public override void ShowSleep()
        {
            if (curAnim == hiding)
            {
                return;
            }
            base.ShowSleep();
        }

        public class Golden : MimicSprite
        {
            protected override int TexOffset()
            {
                return 16;
            }
        }

        public class Crystal : MimicSprite
        {
            protected override int TexOffset()
            {
                return 32;
            }
        }
    }
}