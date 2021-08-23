using watabou.utils;
using watabou.noosa;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.mobs.npcs;
using spdd.items.food;
using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Monk : Mob
    {
        public Monk()
        {
            spriteClass = typeof(MonkSprite);

            HP = HT = 70;
            defenseSkill = 30;

            EXP = 11;
            maxLvl = 21;

            loot = new Food();
            lootChance = 0.083f;

            properties.Add(Property.UNDEAD);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(12, 25);
        }

        public override int AttackSkill(Character target)
        {
            return 30;
        }

        protected override float AttackDelay()
        {
            return base.AttackDelay() * 0.5f;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 2);
        }

        public override void RollToDropLoot()
        {
            Imp.Quest.Process(this);

            base.RollToDropLoot();
        }

        protected float focusCooldown = 0;

        public override void Die(object cause)
        {
            Imp.Quest.Process(this);

            base.Die(cause);
        }

        public override bool Act()
        {
            var result = base.Act();
            if (FindBuff<Focus>() == null && state == HUNTING && focusCooldown <= 0)
            {
                Buff.Affect<Focus>(this);
            }
            return result;
        }

        public override void Spend(float time)
        {
            focusCooldown -= time;
            base.Spend(time);
        }

        public override void Move(int step)
        {
            // moving reduces cooldown by an additional 0.67, giving a total reduction of 1.67f.
            // basically monks will become focused notably faster if you kite them.
            focusCooldown -= 0.67f;
            base.Move(step);
        }

        public override int DefenseSkill(Character enemy)
        {
            if (FindBuff<Focus>() != null && paralysed == 0 && state != SLEEPING)
                return INFINITE_EVASION;

            return base.DefenseSkill(enemy);
        }

        public override string DefenseVerb()
        {
            Focus f = FindBuff<Focus>();
            if (f == null)
            {
                return base.DefenseVerb();
            }
            else
            {
                f.Detach();
                Sample.Instance.Play(Assets.Sounds.HIT_PARRY, 1, Rnd.Float(0.96f, 1.05f));
                focusCooldown = Rnd.NormalFloat(6, 7);
                return Messages.Get(this, "parried");
            }
        }

        private const string FOCUS_COOLDOWN = "focus_cooldown";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(FOCUS_COOLDOWN, focusCooldown);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            focusCooldown = bundle.GetInt(FOCUS_COOLDOWN);
        }

        [SPDStatic]
        public class Focus : Buff
        {
            public Focus()
            {
                type = BuffType.POSITIVE;
                announced = true;
            }

            public override int Icon()
            {
                return BuffIndicator.MIND_VISION;
            }

            public override void TintIcon(Image icon)
            {
                icon.Hardlight(0.25f, 1.5f, 1f);
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc");
            }
        }
    }
}