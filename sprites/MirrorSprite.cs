using watabou.noosa;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs.npcs;

namespace spdd.sprites
{
    public class MirrorSprite : MobSprite
    {
        private const int FRAME_WIDTH = 12;
        private const int FRAME_HEIGHT = 15;

        public MirrorSprite()
        {
            Texture(Dungeon.hero.heroClass.Spritesheet());
            UpdateArmor(0);
            Idle();
        }

        public override void Link(Character ch)
        {
            base.Link(ch);
            UpdateArmor(((MirrorImage)ch).armTier);
        }

        public virtual void UpdateArmor(int tier)
        {
            var film = new TextureFilm(HeroSprite.Tiers(), tier, FRAME_WIDTH, FRAME_HEIGHT);

            idle = new Animation(1, true);
            idle.Frames(film, 0, 0, 0, 1, 0, 0, 1, 1);

            run = new Animation(20, true);
            run.Frames(film, 2, 3, 4, 5, 6, 7);

            die = new Animation(20, false);
            die.Frames(film, 0);

            attack = new Animation(15, false);
            attack.Frames(film, 13, 14, 15, 0);

            Idle();
        }
    }
}