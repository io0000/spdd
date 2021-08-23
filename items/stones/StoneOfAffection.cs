using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.effects;

namespace spdd.items.stones
{
    public class StoneOfAffection : Runestone
    {
        public StoneOfAffection()
        {
            image = ItemSpriteSheet.STONE_AFFECTION;
        }

        protected override void Activate(int cell)
        {
            foreach (int i in PathFinder.NEIGHBORS9)
            {
                CellEmitter.Center(cell + i).Start(Speck.Factory(Speck.HEART), 0.2f, 5);

                var ch = Actor.FindChar(cell + i);

                if (ch != null && ch.alignment == Character.Alignment.ENEMY)
                {
                    Buff.Prolong<Charm>(ch, Charm.DURATION).obj = curUser.Id();
                }
            }

            Sample.Instance.Play(Assets.Sounds.CHARMS);
        }
    }
}