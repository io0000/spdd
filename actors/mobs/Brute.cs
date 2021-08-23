using watabou.utils;
using spdd.ui;
using spdd.items;
using spdd.sprites;
using spdd.levels.features;
using spdd.messages;
using spdd.actors.buffs;

namespace spdd.actors.mobs
{
    public class Brute : Mob
    {
        public Brute()
        {
            spriteClass = typeof(BruteSprite);

            HP = HT = 40;
            defenseSkill = 15;

            EXP = 8;
            maxLvl = 16;

            loot = typeof(Gold);
            lootChance = 0.5f;
        }

        protected bool hasRaged;

        public override int DamageRoll()
        {
            return FindBuff<BruteRage>() != null ?
                Rnd.NormalIntRange(15, 40) :
                Rnd.NormalIntRange(5, 25);
        }

        public override int AttackSkill(Character target)
        {
            return 20;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 8);
        }

        public override void Die(object cause)
        {
            base.Die(cause);

            if (Utils.CheckObjectType(cause, typeof(Chasm)))
                hasRaged = true; //don't let enrage trigger for chasm deaths
        }

        public override bool IsAlive()
        {
            if (HP > 0)
            {
                return true;
            }
            else
            {
                if (!hasRaged)
                    TriggerEnrage();

                return !(Buffs<BruteRage>().Count == 0);
            }
        }

        protected virtual void TriggerEnrage()
        {
            Buff.Affect<BruteRage>(this).SetShield(HT / 2 + 4);
            if (Dungeon.level.heroFOV[pos])
                sprite.ShowStatus(CharSprite.NEGATIVE, Messages.Get(this, "enraged"));

            Spend(TICK);
            hasRaged = true;
        }

        private const string HAS_RAGED = "has_raged";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(HAS_RAGED, hasRaged);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            hasRaged = bundle.GetBoolean(HAS_RAGED);
        }

        [SPDStatic]
        public class BruteRage : ShieldBuff
        {
            public BruteRage()
            {
                InitInstance();

                type = BuffType.POSITIVE;
            }

            public override bool Act()
            {
                if (target.HP > 0)
                {
                    Detach();
                    return true;
                }

                AbsorbDamage(4);

                if (Shielding() <= 0)
                    target.Die(null);

                Spend(TICK);
                return true;
            }

            public override int Icon()
            {
                return BuffIndicator.FURY;
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", Shielding());
            }

            private void InitInstance()
            {
                immunities.Add(typeof(Terror));
            }
        } // BruteRage
    } // Brute
}