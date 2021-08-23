using System.Linq;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.effects;
using spdd.sprites;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfPetrification : ExoticScroll
    {
        public ScrollOfPetrification()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_PETRIF;
        }

        public override void DoRead()
        {
            new Flare(5, 32).Color(new Color(0xFF, 0x00, 0x00, 0xFF), true).Show(curUser.sprite, 2f);
            Sample.Instance.Play(Assets.Sounds.READ);

            foreach (Mob mob in Dungeon.level.mobs.ToArray())
            {
                if (mob.alignment != Character.Alignment.ALLY && Dungeon.level.heroFOV[mob.pos])
                {
                    Buff.Affect<Paralysis>(mob, Paralysis.DURATION);
                }
            }

            SetKnown();

            ReadAnimation();
        }
    }
}