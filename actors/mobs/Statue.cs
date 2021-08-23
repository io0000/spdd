using watabou.utils;
using spdd.items;
using spdd.items.weapon;
using spdd.items.weapon.enchantments;
using spdd.sprites;
using spdd.journal;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Statue : Mob
    {
        public Statue()
        {
            InitInstance();

            spriteClass = typeof(StatueSprite);

            EXP = 0;
            state = PASSIVE;

            properties.Add(Property.INORGANIC);

            do
            {
                weapon = (Weapon)Generator.Random(Generator.Category.WEAPON);
            }
            while (weapon.cursed);

            weapon.Enchant(items.weapon.Weapon.Enchantment.Random());

            HP = HT = 15 + Dungeon.depth * 5;
            defenseSkill = 4 + Dungeon.depth;
        }

        protected Weapon weapon;

        private const string Weapon = "weapon";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(Weapon, weapon);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            weapon = (Weapon)bundle.Get(Weapon);
        }

        public override bool Act()
        {
            if (Dungeon.level.heroFOV[pos])
                Notes.Add(Notes.Landmark.STATUE);

            return base.Act();
        }

        public override int DamageRoll()
        {
            return weapon.DamageRoll(this);
        }

        public override int AttackSkill(Character target)
        {
            return (int)((9 + Dungeon.depth) * weapon.AccuracyFactor(this));
        }

        protected override float AttackDelay()
        {
            return base.AttackDelay() * weapon.SpeedFactor(this);
        }

        protected override bool CanAttack(Character enemy)
        {
            return base.CanAttack(enemy) || weapon.CanReach(this, enemy.pos);
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, Dungeon.depth + weapon.DefenseFactor(this));
        }

        public override void Damage(int dmg, object src)
        {
            if (state == PASSIVE)
                state = HUNTING;

            base.Damage(dmg, src);
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);
            damage = weapon.Proc(this, enemy, damage);
            if (!enemy.IsAlive() && enemy == Dungeon.hero)
            {
                Dungeon.Fail(GetType());
                GLog.Negative(Messages.Capitalize(Messages.Get(typeof(Character), "kill", Name())));
            }
            return damage;
        }

        public override void Beckon(int cell)
        {
            // Do nothing
        }

        public override void Die(object cause)
        {
            weapon.Identify();
            Dungeon.level.Drop(weapon, pos).sprite.Drop();
            base.Die(cause);
        }

        public override void Destroy()
        {
            Notes.Remove(Notes.Landmark.STATUE);
            base.Destroy();
        }

        public override float SpawningWeight()
        {
            return 0f;
        }

        public override bool Reset()
        {
            state = PASSIVE;
            return true;
        }

        public override string Description()
        {
            return Messages.Get(this, "desc", weapon.Name());
        }

        private void InitInstance()
        {
            resistances.Add(typeof(Grim));
        }

        public static Statue Random()
        {
            if (Rnd.Int(10) == 0 && !Dungeon.IsChallenged(Challenges.NO_ARMOR))
            {
                return new ArmoredStatue();
            }
            else
            {
                return new Statue();
            }
        }
    }
}