using System;
using watabou.utils;
using spdd.ui;
using spdd.utils;
using spdd.effects;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Bleeding : Buff
    {
        public Bleeding()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public float level;

        private const string LEVEL = "level";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEVEL, level);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            level = bundle.GetFloat(LEVEL);
        }

        public void Set(int level)
        {
            this.level = Math.Max(this.level, level);
        }

        public override int Icon()
        {
            return BuffIndicator.BLEEDING;
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override bool Act()
        {
            if (target.IsAlive())
            {
                level = Rnd.NormalFloat(level / 2f, level);
                int dmg = (int)Math.Round(level, MidpointRounding.AwayFromZero);

                if (dmg > 0)
                {
                    target.Damage((int)level, this);
                    if (target.sprite.visible)
                    {
                        Splash.At(target.sprite.Center(),
                            -PointF.PI / 2,
                            PointF.PI / 6,
                            target.sprite.Blood(),
                            (int)Math.Min(10 * dmg / target.HT, 10));
                    }

                    if (target == Dungeon.hero && !target.IsAlive())
                    {
                        Dungeon.Fail(GetType());
                        GLog.Negative(Messages.Get(this, "ondeath"));
                    }

                    Spend(TICK);
                }
                else
                {
                    Detach();
                }
            }
            else
            {
                Detach();
            }

            return true;
        }

        public override string HeroMessage()
        {
            return Messages.Get(this, "heromsg");
        }

        public override string Desc()
        {
            // desc에 해당하는 문자열이 %d format을 사용하니까 (int)로 캐스팅
            return Messages.Get(this, "desc", (int)Math.Round(level, MidpointRounding.AwayFromZero));
        }
    }
}