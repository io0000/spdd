using System.Linq;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfRage : Scroll
    {
        public ScrollOfRage()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_RAGE;
        }

        public override void DoRead()
        {
            foreach (var mob in Dungeon.level.mobs.ToArray())
            {
                mob.Beckon(curUser.pos);
                if (mob.alignment != Character.Alignment.ALLY && Dungeon.level.heroFOV[mob.pos])
                {
                    Buff.Prolong<Amok>(mob, 5f);
                }
            }

            GLog.Warning(Messages.Get(this, "roar"));
            SetKnown();

            curUser.sprite.CenterEmitter().Start(Speck.Factory(Speck.SCREAM), 0.3f, 3);
            Sample.Instance.Play(Assets.Sounds.CHALLENGE);

            ReadAnimation();
        }

        public override int Value()
        {
            return IsKnown() ? 40 * quantity : base.Value();
        }
    }
}