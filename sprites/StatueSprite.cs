using watabou.noosa;
using watabou.utils;

namespace spdd.sprites
{
    public class StatueSprite : MobSprite
    {
        public StatueSprite()
        {
            Texture(Assets.Sprites.STATUE);

            var frames = new TextureFilm(texture, 12, 15);

            idle = new Animation(2, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 1, 1);

            run = new Animation(15, true);
            run.Frames(frames, 2, 3, 4, 5, 6, 7);

            attack = new Animation(12, false);
            attack.Frames(frames, 8, 9, 10);

            die = new Animation(5, false);
            die.Frames(frames, 11, 12, 13, 14, 15, 15);

            Play(idle);
        }

        private static int[] tierFrames = { 0, 21, 32, 43, 54, 65 };

        public void SetArmor(int tier)
        {
            int c = tierFrames[(int)GameMath.Gate(0, tier, 5)];

            TextureFilm frames = new TextureFilm(texture, 12, 15);

            idle.Frames(frames, 0 + c, 0 + c, 0 + c, 0 + c, 0 + c, 1 + c, 1 + c);
            run.Frames(frames, 2 + c, 3 + c, 4 + c, 5 + c, 6 + c, 7 + c);
            attack.Frames(frames, 8 + c, 9 + c, 10 + c);
            //death animation is always armorless

            Play(idle, true);
        }

        public override Color Blood()
        {
            return new Color(0xcd, 0xcd, 0xb7, 0xFF);
        }
    }
}