using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.buffs;
using spdd.items.potions;
using spdd.sprites;
using spdd.messages;
using spdd.utils;

namespace spdd.plants
{
    public class Dreamfoil : Plant
    {
        public Dreamfoil()
        {
            image = 7;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch != null)
            {
                if (ch is Mob)
                {
                    Buff.Affect<MagicalSleep>(ch);
                }
                else if (ch is Hero)
                {
                    GLog.Information(Messages.Get(this, "refreshed"));
                    PotionOfHealing.Cure(ch);

                    if (((Hero)ch).subClass == HeroSubClass.WARDEN)
                    {
                        Buff.Affect<BlobImmunity>(ch, BlobImmunity.DURATION / 2f);
                    }
                }
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_DREAMFOIL;

                plantClass = typeof(Dreamfoil);
            }
        }
    }
}