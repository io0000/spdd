using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.blobs;
using spdd.scenes;
using spdd.effects;
using spdd.items.quest;
using spdd.items.wands;
using spdd.items.scrolls;
using spdd.items.potions;
using spdd.items.weapon.enchantments;
using spdd.sprites;
using spdd.mechanics;
using spdd.actors.buffs;

namespace spdd.actors.mobs
{
    public abstract class Elemental : Mob
    {
        public Elemental()
        {
            HP = HT = 60;
            defenseSkill = 20;

            EXP = 10;
            maxLvl = 20;

            flying = true;
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(16, 26);
        }

        public override int AttackSkill(Character target)
        {
            return 25;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 5);
        }

        private int rangedCooldown = Rnd.NormalIntRange(3, 5);

        public override bool Act()
        {
            if (state == HUNTING)
                --rangedCooldown;

            return base.Act();
        }

        protected override bool CanAttack(Character enemy)
        {
            if (rangedCooldown <= 0)
            {
                return new Ballistic(pos, enemy.pos, Ballistic.MAGIC_BOLT).collisionPos == enemy.pos;
            }
            else
            {
                return base.CanAttack(enemy);
            }
        }

        protected override bool DoAttack(Character enemy)
        {
            if (Dungeon.level.Adjacent(pos, enemy.pos))
            {
                return base.DoAttack(enemy);
            }
            else
            {
                if (sprite != null && (sprite.visible || enemy.sprite.visible))
                {
                    sprite.Zap(enemy.pos);
                    return false;
                }
                else
                {
                    Zap();
                    return true;
                }
            }
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);
            MeleeProc(enemy, damage);

            return damage;
        }

        private void Zap()
        {
            Spend(1f);

            if (Hit(this, enemy, true))
            {
                RangedProc(enemy);
            }
            else
            {
                enemy.sprite.ShowStatus(CharSprite.NEUTRAL, enemy.DefenseVerb());
            }

            rangedCooldown = Rnd.NormalIntRange(3, 5);
        }

        public void OnZapComplete()
        {
            Zap();
            Next();
        }

        public override void Add(Buff buff)
        {
            if (harmfulBuffs.Contains(buff.GetType()))
            {
                Damage(Rnd.NormalIntRange(HT / 2, HT * 3 / 5), buff);
            }
            else
            {
                base.Add(buff);
            }
        }

        protected abstract void MeleeProc(Character enemy, int damage);
        protected abstract void RangedProc(Character enemy);

        protected List<Type> harmfulBuffs = new List<Type>();  //Class<? extends Buff>

        private const string COOLDOWN = "cooldown";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(COOLDOWN, rangedCooldown);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            if (bundle.Contains(COOLDOWN))
            {
                rangedCooldown = bundle.GetInt(COOLDOWN);
            }
        }

        [SPDStatic]
        public class FireElemental : Elemental
        {
            public FireElemental()
            {
                spriteClass = typeof(ElementalSprite.Fire);

                loot = new PotionOfLiquidFlame();
                lootChance = 1 / 8f;

                properties.Add(Property.FIERY);

                harmfulBuffs.Add(typeof(Frost));
                harmfulBuffs.Add(typeof(Chill));
            }

            protected override void MeleeProc(Character enemy, int damage)
            {
                if (Rnd.Int(2) == 0 && !Dungeon.level.water[enemy.pos])
                {
                    Buff.Affect<Burning>(enemy).Reignite(enemy);
                    Splash.At(enemy.sprite.Center(), sprite.Blood(), 5);
                }
            }

            protected override void RangedProc(Character enemy)
            {
                if (!Dungeon.level.water[enemy.pos])
                    Buff.Affect<Burning>(enemy).Reignite(enemy, 4.0f);

                Splash.At(enemy.sprite.Center(), sprite.Blood(), 5);
            }
        }

        //used in wandmaker quest
        [SPDStatic]
        public class NewbornFireElemental : FireElemental
        {
            public NewbornFireElemental()
            {
                spriteClass = typeof(ElementalSprite.NewbornFire);

                HT = 60;
                HP = HT / 2; //32

                defenseSkill = 12;

                EXP = 7;

                loot = new Embers();
                lootChance = 1f;

                properties.Add(Property.MINIBOSS);
            }

            public override bool Reset()
            {
                return true;
            }
        }

        [SPDStatic]
        public class FrostElemental : Elemental
        {
            public FrostElemental()
            {
                spriteClass = typeof(ElementalSprite.Frost);

                loot = new PotionOfFrost();
                lootChance = 1 / 8f;

                properties.Add(Property.ICY);

                harmfulBuffs.Add(typeof(Burning));
            }

            protected override void MeleeProc(Character enemy, int damage)
            {
                if (Rnd.Int(3) == 0 || Dungeon.level.water[enemy.pos])
                {
                    Freezing.Freeze(enemy.pos);
                    Splash.At(enemy.sprite.Center(), sprite.Blood(), 5);
                }
            }

            protected override void RangedProc(Character enemy)
            {
                Freezing.Freeze(enemy.pos);
                Splash.At(enemy.sprite.Center(), sprite.Blood(), 5);
            }
        }

        [SPDStatic]
        public class ShockElemental : Elemental
        {
            public ShockElemental()
            {
                spriteClass = typeof(ElementalSprite.Shock);

                loot = new ScrollOfRecharging();
                lootChance = 1 / 4f;

                properties.Add(Property.ELECTRIC);
            }

            protected override void MeleeProc(Character enemy, int damage)
            {
                List<Character> affected = new List<Character>();
                List<Lightning.Arc> arcs = new List<Lightning.Arc>();
                Shocking.Arc(this, enemy, 2, affected, arcs);

                if (!Dungeon.level.water[enemy.pos])
                    affected.Remove(enemy);

                foreach (Character ch in affected)
                {
                    ch.Damage((int)Math.Round(damage * 0.4f, MidpointRounding.AwayFromZero), this);
                }

                sprite.parent.AddToFront(new Lightning(arcs, null));
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);
            }

            protected override void RangedProc(Character enemy)
            {
                Buff.Affect<Blindness>(enemy, Blindness.DURATION);
                if (enemy == Dungeon.hero)
                    GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
            }
        }

        [SPDStatic]
        public class ChaosElemental : Elemental
        {
            public ChaosElemental()
            {
                spriteClass = typeof(ElementalSprite.Chaos);

                loot = new ScrollOfTransmutation();
                lootChance = 1f;
            }
            protected override void MeleeProc(Character enemy, int damage)
            {
                CursedWand.CursedEffect(null, this, enemy);
            }

            protected override void RangedProc(Character enemy)
            {
                CursedWand.CursedEffect(null, this, enemy);
            }
        }

        public static Type Random()
        {
            if (Rnd.Int(50) == 0)
            {
                return typeof(Elemental.ChaosElemental);
            }

            float roll = Rnd.Float();
            if (roll < 0.4f)
            {
                return typeof(FireElemental);
            }
            else if (roll < 0.8f)
            {
                return typeof(FrostElemental);
            }
            else
            {
                return typeof(ShockElemental);
            }
        }
    }
}