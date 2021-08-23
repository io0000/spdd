using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class SleepDart : TippedDart
    {
        public SleepDart()
        {
            image = ItemSpriteSheet.SLEEP_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            //need to delay this so damage from the dart doesn't break the sleep
            (new SleepDartBuff(defender)).AttachTo(defender);

            return base.Proc(attacker, defender, damage);
        }

        private class SleepDartBuff : FlavourBuff
        {
            private Character defender;

            public SleepDartBuff(Character defender)
            {
                this.defender = defender;

                actPriority = VFX_PRIO;
            }

            public override bool Act()
            {
                Buff.Affect<Sleep>(defender);
                return base.Act();
            }
        }
    }
}