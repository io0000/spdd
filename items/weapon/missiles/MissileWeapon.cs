using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.bags;
using spdd.items.rings;
using spdd.items.weapon.enchantments;
using spdd.messages;
using spdd.utils;

namespace spdd.items.weapon.missiles
{
    public class MissileWeapon : Weapon
    {
        public MissileWeapon()
        {
            stackable = true;
            levelKnown = true;

            bones = true;

            defaultAction = AC_THROW;
            usesTargeting = true;
        }

        protected bool sticky = true;

        protected const float MAX_DURABILITY = 100;
        public float durability = MAX_DURABILITY;
        protected float baseUses = 10;

        public bool holster;

        //used to reduce durability from the source weapon stack, rather than the one being thrown.
        protected MissileWeapon parent;

        public int tier;

        public override int Min()
        {
            return Math.Max(0, Min(BuffedLvl() + RingOfSharpshooting.LevelDamageBonus(Dungeon.hero)));
        }

        public override int Min(int lvl)
        {
            return 2 * tier +                      //base
                   (tier == 1 ? lvl : 2 * lvl);    //level scaling
        }

        public override int Max()
        {
            return Math.Max(0, Max(BuffedLvl() + RingOfSharpshooting.LevelDamageBonus(Dungeon.hero)));
        }

        public override int Max(int lvl)
        {
            return 5 * tier +                          //base
                   (tier == 1 ? 2 * lvl : tier * lvl); //level scaling
        }

        public override int STRReq(int lvl)
        {
            lvl = Math.Max(0, lvl);
            //strength req decreases at +1,+3,+6,+10,etc.
            return (7 + tier * 2) - (int)(Math.Sqrt(8 * lvl + 1) - 1) / 2;
        }

        //FIXME some logic here assumes the items are in the player's inventory. Might need to adjust
        public override Item Upgrade()
        {
            if (!bundleRestoring)
            {
                durability = MAX_DURABILITY;
                if (quantity > 1)
                {
                    var upgraded = (MissileWeapon)Split(1);
                    upgraded.parent = null;

                    upgraded = (MissileWeapon)upgraded.Upgrade();

                    //try to put the upgraded into inventory, if it didn't already merge
                    if (upgraded.Quantity() == 1 && !upgraded.Collect())
                    {
                        Dungeon.level.Drop(upgraded, Dungeon.hero.pos);
                    }
                    UpdateQuickslot();
                    return upgraded;
                }
                else
                {
                    base.Upgrade();

                    Item similar = Dungeon.hero.belongings.GetSimilar(this);
                    if (similar != null)
                    {
                        Detach(Dungeon.hero.belongings.backpack);
                        return similar.Merge(this);
                    }
                    UpdateQuickslot();
                    return this;
                }
            }
            else
            {
                return base.Upgrade();
            }
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            actions.Remove(AC_EQUIP);
            return actions;
        }

        public override bool Collect(Bag container)
        {
            if (container is MagicalHolster)
                holster = true;
            return base.Collect(container);
        }

        public override int ThrowPos(Hero user, int dst)
        {
            if (HasEnchant(typeof(Projecting), user) &&
                !Dungeon.level.solid[dst] &&
                Dungeon.level.Distance(user.pos, dst) <= 4)
            {
                return dst;
            }
            else
            {
                return base.ThrowPos(user, dst);
            }
        }

        public override void DoThrow(Hero hero)
        {
            parent = null; //reset parent before throwing, just incase
            base.DoThrow(hero);
        }

        public override void OnThrow(int cell)
        {
            var enemy = Actor.FindChar(cell);
            if (enemy == null || enemy == curUser)
            {
                parent = null;
                base.OnThrow(cell);
            }
            else
            {
                if (!curUser.Shoot(enemy, this))
                {
                    RangedMiss(cell);
                }
                else
                {
                    RangedHit(enemy, cell);
                }
            }
        }

        public override Item Random()
        {
            if (!stackable)
                return this;

            //2: 66.67% (2/3)
            //3: 26.67% (4/15)
            //4: 6.67%  (1/15)
            quantity = 2;
            if (Rnd.Int(3) == 0)
            {
                ++quantity;
                if (Rnd.Int(5) == 0)
                    ++quantity;
            }

            return this;
        }

        public override float CastDelay(Character user, int dst)
        {
            return SpeedFactor(user);
        }

        protected virtual void RangedHit(Character enemy, int cell)
        {
            DecrementDurability();
            if (durability > 0)
            {
                //attempt to stick the missile weapon to the enemy, just drop it if we can't.
                if (sticky &&
                    enemy != null &&
                    enemy.IsAlive() &&
                    enemy.FindBuff<Corruption>() == null)
                {
                    PinCushion p = Buff.Affect<PinCushion>(enemy);
                    if (p.target == enemy)
                    {
                        p.Stick(this);
                        return;
                    }
                }
                Dungeon.level.Drop(this, cell).sprite.Drop();
            }
        }

        protected virtual void RangedMiss(int cell)
        {
            parent = null;
            base.OnThrow(cell);
        }

        protected virtual float DurabilityPerUse()
        {
            float usages = baseUses * (float)Math.Pow(3, GetLevel());

            if (Dungeon.hero.heroClass == HeroClass.HUNTRESS)
                usages *= 1.5f;

            if (holster)
                usages *= MagicalHolster.HOLSTER_DURABILITY_FACTOR;

            usages *= RingOfSharpshooting.DurabilityMultiplier(Dungeon.hero);

            //at 100 uses, items just last forever.
            if (usages >= 100f)
                return 0;

            //add a tiny amount to account for rounding error for calculations like 1/3
            return (MAX_DURABILITY / usages) + 0.001f;
        }

        public void DecrementDurability()
        {
            //if this weapon was thrown from a source stack, degrade that stack.
            //unless a weapon is about to break, then break the one being thrown
            if (parent != null)
            {
                if (parent.durability <= parent.DurabilityPerUse())
                {
                    durability = 0;
                    parent.durability = MAX_DURABILITY;
                }
                else
                {
                    parent.durability -= parent.DurabilityPerUse();
                    if (parent.durability > 0 && parent.durability <= parent.DurabilityPerUse())
                    {
                        if (GetLevel() <= 0)
                            GLog.Warning(Messages.Get(this, "about_to_break"));
                        else
                            GLog.Negative(Messages.Get(this, "about_to_break"));
                    }
                }
                parent = null;
            }
            else
            {
                durability -= DurabilityPerUse();
                if (durability > 0 && durability <= DurabilityPerUse())
                {
                    if (GetLevel() <= 0)
                        GLog.Warning(Messages.Get(this, "about_to_break"));
                    else
                        GLog.Negative(Messages.Get(this, "about_to_break"));
                }
            }
        }

        public override int DamageRoll(Character owner)
        {
            int damage = augment.DamageFactor(base.DamageRoll(owner));

            if (owner is Hero)
            {
                int exStr = ((Hero)owner).GetSTR() - STRReq();
                if (exStr > 0)
                {
                    damage += Rnd.IntRange(0, exStr);
                }
            }

            return damage;
        }

        public override void Reset()
        {
            base.Reset();
            durability = MAX_DURABILITY;
        }

        public override Item Merge(Item other)
        {
            base.Merge(other);
            if (IsSimilar(other))
            {
                durability += ((MissileWeapon)other).durability;
                durability -= MAX_DURABILITY;
                while (durability <= 0)
                {
                    quantity -= 1;
                    durability += MAX_DURABILITY;
                }
            }
            return this;
        }

        public override Item Split(int amount)
        {
            bundleRestoring = true;
            Item split = base.Split(amount);
            bundleRestoring = false;

            //unless the thrown weapon will break, split off a max durability item and
            //have it reduce the durability of the main stack. Cleaner to the player this way
            if (split != null)
            {
                MissileWeapon m = (MissileWeapon)split;
                m.durability = MAX_DURABILITY;
                m.parent = this;
            }

            return split;
        }

        public override bool DoPickUp(Hero hero)
        {
            parent = null;
            return base.DoPickUp(hero);
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override string Info()
        {
            string info = Desc();

            int str = Dungeon.hero.GetSTR();
            int strReq = STRReq();

            info += "\n\n" + Messages.Get(typeof(MissileWeapon), "stats",
                    tier,
                    augment.DamageFactor(Min()),
                    augment.DamageFactor(Max()),
                    STRReq());

            if (strReq > str)
            {
                info += " " + Messages.Get(typeof(Weapon), "too_heavy");
            }
            else if (str > strReq)
            {
                info += " " + Messages.Get(typeof(Weapon), "excess_str", str - strReq);
            }

            if (enchantment != null && (cursedKnown || !enchantment.Curse()))
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "enchanted", enchantment.Name());
                info += " " + Messages.Get(enchantment, "desc");
            }

            if (cursed && IsEquipped(Dungeon.hero))
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "cursed_worn");
            }
            else if (cursedKnown && cursed)
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "cursed");
            }
            else if (!IsIdentified() && cursedKnown)
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "not_cursed");
            }

            info += "\n\n" + Messages.Get(typeof(MissileWeapon), "distance");

            info += "\n\n" + Messages.Get(this, "durability");

            if (DurabilityPerUse() > 0)
            {
                info += " " + Messages.Get(this, "uses_left",
                        (int)Math.Ceiling(durability / DurabilityPerUse()),
                        (int)Math.Ceiling(MAX_DURABILITY / DurabilityPerUse()));
            }
            else
            {
                info += " " + Messages.Get(this, "unlimited_uses");
            }

            return info;
        }

        public override int Value()
        {
            return 6 * tier * quantity * (GetLevel() + 1);
        }

        private const string DURABILITY = "durability";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(DURABILITY, durability);
        }

        private static bool bundleRestoring;

        public override void RestoreFromBundle(Bundle bundle)
        {
            bundleRestoring = true;
            base.RestoreFromBundle(bundle);
            bundleRestoring = false;
            durability = bundle.GetInt(DURABILITY);
        }
    }
}