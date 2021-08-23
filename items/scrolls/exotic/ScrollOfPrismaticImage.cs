using System.Linq;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.effects;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfPrismaticImage : ExoticScroll
    {
        public ScrollOfPrismaticImage()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_PRISIMG;
        }

        public override void DoRead()
        {
            bool found = false;
            foreach (Mob m in Dungeon.level.mobs.ToArray())
            {
                if (m is PrismaticImage)
                {
                    found = true;
                    m.HP = m.HT;
                    m.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 4);
                }
            }

            if (!found)
            {
                Buff.Affect<PrismaticGuard>(curUser).Set(PrismaticGuard.MaxHP(curUser));
            }

            SetKnown();

            Sample.Instance.Play(Assets.Sounds.READ);

            ReadAnimation();
        }
    }
}