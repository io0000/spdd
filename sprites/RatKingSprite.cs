using System;
using watabou.noosa;
using watabou.utils;

namespace spdd.sprites
{
    public class RatKingSprite : MobSprite
    {
        public bool festive;

        public RatKingSprite()
        {
            //final Calendar calendar = Calendar.getInstance();
            ////once a year the rat king feels a bit festive!
            //festive = (calendar.get(Calendar.MONTH) == Calendar.DECEMBER
            //        && calendar.get(Calendar.WEEK_OF_MONTH) > 2);
            var now = DateTime.Now;
            festive = (now.Month == 12 && Utils.GetWeekOfMonth(now) > 2);

            int c = festive ? 8 : 0;

            Texture(Assets.Sprites.RATKING);

            var frames = new TextureFilm(texture, 16, 17);

            idle = new Animation(2, true);
            idle.Frames(frames, c + 0, c + 0, c + 0, c + 1);

            run = new Animation(10, true);
            run.Frames(frames, c + 2, c + 3, c + 4, c + 5, c + 6);

            attack = new Animation(15, false);
            attack.Frames(frames, c + 0);

            die = new Animation(10, false);
            die.Frames(frames, c + 0);

            Play(idle);
        }
    }
}