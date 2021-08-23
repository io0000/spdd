using System.Linq;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.effects;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfTerror : Scroll
    {
        public ScrollOfTerror()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_TERROR;
        }

        public override void DoRead()
        {
            new Flare(5, 32).Color(new Color(0xFF, 0x00, 0x00, 0xFF), true).Show(curUser.sprite, 2f);
            Sample.Instance.Play(Assets.Sounds.READ);

            int count = 0;
            Mob affected = null;
            foreach (var mob in Dungeon.level.mobs.ToArray())
            {
                if (mob.alignment != Character.Alignment.ALLY && Dungeon.level.heroFOV[mob.pos])
                {
                    var terror = Buff.Affect<Terror>(mob, Terror.DURATION);
                    terror.obj = curUser.Id();

                    if (mob.FindBuff<Terror>() != null)
                    {
                        ++count;
                        affected = mob;
                    }
                }
            }

            switch (count)
            {
                case 0:
                    GLog.Information(Messages.Get(this, "none"));
                    break;
                case 1:
                    GLog.Information(Messages.Get(this, "one", affected.Name()));
                    break;
                default:
                    GLog.Information(Messages.Get(this, "many"));
                    break;
            }
            SetKnown();

            ReadAnimation();
        }

        public override int Value()
        {
            return IsKnown() ? 40 * quantity : base.Value();
        }
    }
}