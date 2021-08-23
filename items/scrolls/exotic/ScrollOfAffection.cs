using System.Linq;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.effects;
using spdd.actors.buffs;
using spdd.actors.mobs;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfAffection : ExoticScroll
    {
        public ScrollOfAffection()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_AFFECTION;
        }

        public override void DoRead()
        {
            curUser.sprite.CenterEmitter().Start(Speck.Factory(Speck.HEART), 0.2f, 5);
            Sample.Instance.Play(Assets.Sounds.CHARMS);

            foreach (Mob mob in Dungeon.level.mobs.ToArray())
            {
                if (Dungeon.level.heroFOV[mob.pos])
                {
                    Buff.Affect<Charm>(mob, Charm.DURATION * 2f).obj = curUser.Id();
                    mob.sprite.CenterEmitter().Start(Speck.Factory(Speck.HEART), 0.2f, 5);
                }
            }

            //GLog.i( Messages.get(this, "sooth") );

            SetKnown();

            ReadAnimation();
        }
    }
}