using watabou.utils;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.blobs;

namespace spdd.items.potions.exotic
{
    public class PotionOfSnapFreeze : ExoticPotion
    {
        public PotionOfSnapFreeze()
        {
            icon = ItemSpriteSheet.Icons.POTION_SNAPFREEZ;
        }

        public override void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                SetKnown();

                Splash(cell);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
            }

            Fire fire = (Fire)Dungeon.level.GetBlob(typeof(Fire));

            foreach (int offset in PathFinder.NEIGHBORS9)
            {
                if (!Dungeon.level.solid[cell + offset])
                {
                    Freezing.Affect(cell + offset, fire);

                    var ch = Actor.FindChar(cell + offset);
                    if (ch != null)
                    {
                        Buff.Affect<Roots>(ch, Roots.DURATION * 2f);
                    }
                }
            }
        }
    }
}
