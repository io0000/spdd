using System;
using System.Linq;
using watabou.utils;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.scenes;
using spdd.utils;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.messages;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfPsionicBlast : ExoticScroll
    {
        public ScrollOfPsionicBlast()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_PSIBLAST;
        }

        public override void DoRead()
        {
            GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));

            Sample.Instance.Play(Assets.Sounds.BLAST);

            int targets = 0;
            foreach (Mob mob in Dungeon.level.mobs.ToArray())
            {
                if (Dungeon.level.heroFOV[mob.pos])
                {
                    ++targets;
                    mob.Damage((int)Math.Round(mob.HT / 2f + mob.HP / 2f, MidpointRounding.AwayFromZero), this);
                    if (mob.IsAlive())
                    {
                        Buff.Prolong<Blindness>(mob, Blindness.DURATION);
                    }
                }
            }

            var dmg = (int)Math.Max(0, Math.Round(curUser.HT * (0.5f * (float)Math.Pow(0.9, targets)), MidpointRounding.AwayFromZero));
            curUser.Damage(dmg, this);

            if (curUser.IsAlive())
            {
                Buff.Prolong<Blindness>(curUser, Blindness.DURATION);
                Buff.Prolong<Weakness>(curUser, Weakness.DURATION * 5f);
                Dungeon.Observe();
                ReadAnimation();
            }
            else
            {
                Dungeon.Fail(GetType());
                GLog.Negative(Messages.Get(this, "ondeath"));
            }

            SetKnown();
        }
    }
}