using System;
using watabou.noosa;
using spdd.actors;
using spdd.actors.mobs.npcs;
using spdd.actors.hero;

namespace spdd.sprites
{
    public class PrismaticSprite : MobSprite
    {
        private const int FRAME_WIDTH = 12;
        private const int FRAME_HEIGHT = 15;

        public PrismaticSprite()
        {
            Texture(Dungeon.hero.heroClass.Spritesheet());

            UpdateArmor(0);
            Idle();
        }

        public override void Link(Character ch)
        {
            base.Link(ch);
            UpdateArmor(((PrismaticImage)ch).armTier);
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

        public override void Update()
        {
            base.Update();

            if (flashTime <= 0)
            {
                float interval = (Game.timeTotal % 9) / 3f;
                Tint(interval > 2 ? interval - 2 : Math.Max(0, 1 - interval),
                        interval > 1 ? Math.Max(0, 2 - interval) : interval,
                        interval > 2 ? Math.Max(0, 3 - interval) : interval - 1, 0.5f);
            }
        }
    }
}