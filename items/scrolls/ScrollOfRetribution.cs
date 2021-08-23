using System;
using System.Linq;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.sprites;
using spdd.scenes;

namespace spdd.items.scrolls
{
    public class ScrollOfRetribution : Scroll
    {
        public ScrollOfRetribution()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_RETRIB;
        }

        public override void DoRead()
        {
            GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));

            //scales from 0x to 1x power, maxing at ~10% HP
            float hpPercent = (curUser.HT - curUser.HP) / (float)(curUser.HT);
            float power = Math.Min(4f, 4.45f * hpPercent);

            Sample.Instance.Play(Assets.Sounds.BLAST);

            foreach (Mob mob in Dungeon.level.mobs.ToArray())
            {
                if (Dungeon.level.heroFOV[mob.pos])
                {
                    //deals 10%HT, plus 0-90%HP based on scaling
                    mob.Damage((int)Math.Round(mob.HT / 10f + (mob.HP * power * 0.225f), MidpointRounding.AwayFromZero), this);
                    if (mob.IsAlive())
                    {
                        Buff.Prolong<Blindness>(mob, Blindness.DURATION);
                    }
                }
            }

            Buff.Prolong<Weakness>(curUser, Weakness.DURATION);
            Buff.Prolong<Blindness>(curUser, Blindness.DURATION);
            Dungeon.Observe();

            SetKnown();

            ReadAnimation();
        }

        public override int Value()
        {
            return IsKnown() ? 40 * quantity : base.Value();
        }
    }
}