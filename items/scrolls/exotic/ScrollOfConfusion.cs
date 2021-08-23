using System.Linq;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.effects;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.mobs;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfConfusion : ExoticScroll
    {
        public ScrollOfConfusion()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_CONFUSION;
        }

        public override void DoRead()
        {
            foreach (Mob mob in Dungeon.level.mobs.ToArray())
            {
                if (mob.alignment != Character.Alignment.ALLY && Dungeon.level.heroFOV[mob.pos])
                {
                    Buff.Prolong<Vertigo>(mob, Vertigo.DURATION);
                    Buff.Prolong<Blindness>(mob, Blindness.DURATION);
                }
            }

            SetKnown();

            curUser.sprite.CenterEmitter().Start(Speck.Factory(Speck.SCREAM), 0.3f, 3);
            Sample.Instance.Play(Assets.Sounds.READ);

            ReadAnimation();
        }
    }
}