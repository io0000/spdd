using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.messages;
using spdd.utils;

namespace spdd.items
{
    public abstract class KindOfWeapon : EquipableItem
    {
        public const float TIME_TO_EQUIP = 1f;

        protected string hitSound = Assets.Sounds.HIT;
        protected float hitSoundPitch = 1f;

        public override bool IsEquipped(Hero hero)
        {
            return hero.belongings.weapon == this || hero.belongings.stashedWeapon == this;
        }

        public override bool DoEquip(Hero hero)
        {
            DetachAll(hero.belongings.backpack);

            if (hero.belongings.weapon == null || hero.belongings.weapon.DoUnequip(hero, true))
            {
                hero.belongings.weapon = this;
                Activate(hero);

                UpdateQuickslot();

                cursedKnown = true;
                if (cursed)
                {
                    EquipCursed(hero);
                    GLog.Negative(Messages.Get(typeof(KindOfWeapon), "equip_cursed"));
                }

                hero.SpendAndNext(TIME_TO_EQUIP);
                return true;
            }
            else
            {
                Collect(hero.belongings.backpack);
                return false;
            }
        }

        public override bool DoUnequip(Hero hero, bool collect, bool single)
        {
            if (base.DoUnequip(hero, collect, single))
            {
                hero.belongings.weapon = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual int Min()
        {
            return Min(BuffedLvl());
        }

        public virtual int Max()
        {
            return Max(BuffedLvl());
        }

        abstract public int Min(int lvl);
        abstract public int Max(int lvl);

        public virtual int DamageRoll(Character owner)
        {
            return Rnd.NormalIntRange(Min(), Max());
        }

        public virtual float AccuracyFactor(Character hero)
        {
            return 1f;
        }

        public virtual float SpeedFactor(Character hero)
        {
            return 1f;
        }

        public virtual int ReachFactor(Character owner)
        {
            return 1;
        }

        public bool CanReach(Character owner, int target)
        {
            int reachFactor = ReachFactor(owner);
            if (Dungeon.level.Distance(owner.pos, target) > reachFactor)
            {
                return false;
            }
            else
            {
                var passable = BArray.Not(Dungeon.level.solid, null);
                foreach (var ch in Actor.Chars())
                {
                    if (ch != owner)
                        passable[ch.pos] = false;
                }

                PathFinder.BuildDistanceMap(target, passable, reachFactor);

                return PathFinder.distance[owner.pos] <= reachFactor;
            }
        }

        public virtual int DefenseFactor(Character owner)
        {
            return 0;
        }

        public virtual int Proc(Character attacker, Character defender, int damage)
        {
            return damage;
        }

        public virtual void HitSound(float pitch)
        {
            Sample.Instance.Play(hitSound, 1, pitch * hitSoundPitch);
        }
    }
}