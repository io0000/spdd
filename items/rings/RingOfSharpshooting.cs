using System;
using System.Globalization;
using spdd.actors;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.rings
{
    public class RingOfSharpshooting : Ring
    {
        public RingOfSharpshooting()
        {
            icon = ItemSpriteSheet.Icons.RING_SHARPSHOOT;
        }

        public override string StatsInfo()
        {
            if (IsIdentified())
            {
                var value = 100f * (Math.Pow(1.2, SoloBonus()) - 1f);
                return Messages.Get(this, "stats", SoloBuffedBonus(), value.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                var value = 20f;
                return Messages.Get(this, "typical_stats", 1, value.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        public override RingBuff Buff()
        {
            return new Aim(this);
        }

        public static int LevelDamageBonus(Character target)
        {
            return GetBuffedBonus<Aim>(target);
        }

        public static float DurabilityMultiplier(Character target)
        {
            // Degrade감쇠를 적용하면 이 함수의 return값이 작아짐
            // -> 유저에게 유리함 
            // -> Degrade의도에 부함되지 않음 그래서 GetBonus 함수 사용한 것으로 추정
            return (float)(Math.Pow(1.2, GetBonus<Aim>(target)));
        }

        public class Aim : RingBuff
        {
            public Aim(Ring ring)
                : base(ring)
            { }
        }
    }
}