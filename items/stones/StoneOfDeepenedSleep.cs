using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.effects;

namespace spdd.items.stones
{
    public class StoneOfDeepenedSleep : Runestone
    {
        public StoneOfDeepenedSleep()
        {
            image = ItemSpriteSheet.STONE_SLEEP;
        }

        protected override void Activate(int cell)
        {
            foreach (int i in PathFinder.NEIGHBORS9)
            {
                CellEmitter.Get(cell + i).Start(Speck.Factory(Speck.NOTE), 0.1f, 2);

                var c = Actor.FindChar(cell + i);

                if (c != null)
                {
                    if (c is Mob && ((Mob)c).state == ((Mob)c).SLEEPING)
                    {
                        Buff.Affect<MagicalSleep>(c);
                    }
                }
            }

            Sample.Instance.Play(Assets.Sounds.LULLABY);
        }
    }
}