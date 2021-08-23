using watabou.noosa;

namespace spdd.sprites
{
    public class GhoulSprite : MobSprite
    {
        private Animation crumple;

        public GhoulSprite()
        {
            Texture(Assets.Sprites.GHOUL);

            var frames = new TextureFilm(texture, 12, 14);

            idle = new Animation(2, true);
            idle.Frames(frames, 0, 0, 0, 1);

            run = new Animation(12, true);
            run.Frames(frames, 2, 3, 4, 5, 6, 7);

            attack = new Animation(12, false);
            attack.Frames(frames, 0, 8, 9);

            crumple = new Animation(15, false);
            crumple.Frames(frames, 0, 10, 11, 12);

            die = new Animation(15, false);
            die.Frames(frames, 0, 10, 11, 12, 13);

            Play(idle);
        }

        public void Crumple()
        {
            HideEmo();
            Play(crumple);
        }

        public override void Die()
        {
            if (curAnim == crumple)
            {
                //causes the sprite to not rise then fall again when dieing.
                die.frames[0] = die.frames[1] = die.frames[2] = die.frames[3];
            }
            base.Die();
        }
    }
}