using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.effects;
using spdd.sprites;

namespace spdd.plants
{
    public class Blindweed : Plant
    {
        public Blindweed()
        {
            image = 11;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch != null)
            {
                if (ch is Hero && ((Hero)ch).subClass == HeroSubClass.WARDEN)
                {
                    Buff.Affect<Invisibility>(ch, Invisibility.DURATION / 2f);
                }
                else
                {
                    Buff.Prolong<Blindness>(ch, Blindness.DURATION);
                    Buff.Prolong<Cripple>(ch, Cripple.DURATION);
                    if (ch is Mob)
                    {
                        if (((Mob)ch).state == ((Mob)ch).HUNTING)
                            ((Mob)ch).state = ((Mob)ch).WANDERING;

                        ((Mob)ch).Beckon(Dungeon.level.RandomDestination(ch));
                    }
                }
            }

            if (Dungeon.level.heroFOV[pos])
            {
                CellEmitter.Get(pos).Burst(Speck.Factory(Speck.LIGHT), 4);
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_BLINDWEED;

                plantClass = typeof(Blindweed);
            }
        }
    }
}