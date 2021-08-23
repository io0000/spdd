using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;
using spdd.levels.features;
using spdd.items.armor.glyphs;
using spdd.effects;

namespace spdd.actors.mobs.npcs
{
    public class PrismaticImage : NPC
    {
        public PrismaticImage()
        {
            InitInstance();

            spriteClass = typeof(PrismaticSprite);

            HP = HT = 8;
            defenseSkill = 1;

            alignment = Alignment.ALLY;
            intelligentAlly = true;
            state = HUNTING;

            WANDERING = new PrismaticWandering(this);

            //before other mobs
            actPriority = MOB_PRIO + 1;
        }

        private Hero hero;
        private int heroID;
        public int armTier;

        private int deathTimer = -1;

        public override bool Act()
        {
            if (!IsAlive())
            {
                --deathTimer;

                if (deathTimer > 0)
                {
                    sprite.Alpha((deathTimer + 3) / 8f);
                    Spend(TICK);
                }
                else
                {
                    Destroy();
                    sprite.Die();
                }
                return true;
            }

            if (deathTimer != -1)
            {
                if (paralysed == 0)
                    sprite.Remove(CharSprite.State.PARALYSED);
                deathTimer = -1;
                sprite.ResetColor();
            }

            if (hero == null)
            {
                hero = (Hero)Actor.FindById(heroID);
                if (hero == null)
                {
                    Destroy();
                    sprite.Die();
                    return true;
                }
            }

            if (hero.Tier() != armTier)
            {
                armTier = hero.Tier();
                ((PrismaticSprite)sprite).UpdateArmor(armTier);
            }

            return base.Act();
        }

        public override void Die(object cause)
        {
            if (deathTimer == -1)
            {
                if (Utils.CheckObjectType(cause, typeof(Chasm)))
                {
                    base.Die(cause);
                }
            }
            else
            {
                deathTimer = 5;
                sprite.Add(CharSprite.State.PARALYSED);
            }
        }

        private const string HEROID = "hero_id";
        private const string TIMER = "timer";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(HEROID, heroID);
            bundle.Put(TIMER, deathTimer);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            heroID = bundle.GetInt(HEROID);
            deathTimer = bundle.GetInt(TIMER);
        }

        public void Duplicate(Hero hero, int HP)
        {
            this.hero = hero;
            heroID = this.hero.Id();
            this.HP = HP;
            HT = PrismaticGuard.MaxHP(hero);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(1 + hero.lvl / 8, 4 + hero.lvl / 2);
        }

        public override int AttackSkill(Character target)
        {
            return hero.AttackSkill(target);
        }

        public override int DefenseSkill(Character enemy)
        {
            if (hero != null)
            {
                int baseEvasion = 4 + hero.lvl;
                int heroEvasion = hero.DefenseSkill(enemy);

                //if the hero has more/less evasion, 50% of it is applied
                return base.DefenseSkill(enemy) * (baseEvasion + heroEvasion) / 2;
            }
            else
            {
                return 0;
            }
        }

        public override int DrRoll()
        {
            if (hero != null)
            {
                return hero.DrRoll();
            }
            else
            {
                return 0;
            }
        }

        public override int DefenseProc(Character enemy, int damage)
        {
            damage = base.DefenseProc(enemy, damage);
            if (hero.belongings.armor != null)
            {
                return hero.belongings.armor.Proc(enemy, this, damage);
            }
            else
            {
                return damage;
            }
        }

        public override void Damage(int dmg, object src)
        {
            //TODO improve this when I have proper damage source logic
            if (hero.belongings.armor != null &&
                hero.belongings.armor.HasGlyph(typeof(AntiMagic), this) &&
                AntiMagic.RESISTS.Contains(src.GetType()))
            {
                dmg -= AntiMagic.DrRoll(hero.belongings.armor.BuffedLvl());
            }

            base.Damage(dmg, src);
        }

        public override float Speed()
        {
            if (hero.belongings.armor != null)
            {
                return hero.belongings.armor.SpeedFactor(this, base.Speed());
            }

            return base.Speed();
        }

        public override int AttackProc(Character enemy, int damage)
        {
            if (enemy is Mob)
            {
                ((Mob)enemy).Aggro(this);
            }

            return base.AttackProc(enemy, damage);
        }

        public override CharSprite GetSprite()
        {
            var s = base.GetSprite();

            hero = (Hero)Actor.FindById(heroID);
            if (hero != null)
            {
                armTier = hero.Tier();
            }

            ((PrismaticSprite)s).UpdateArmor(armTier);
            return s;
        }

        public override bool IsImmune(Type effect)
        {
            if (effect == typeof(Burning) &&
                hero != null &&
                hero.belongings.armor != null &&
                hero.belongings.armor.HasGlyph(typeof(Brimstone), this))
            {
                return true;
            }

            return base.IsImmune(effect);
        }

        private void InitInstance()
        {
            immunities.Add(typeof(ToxicGas));
            immunities.Add(typeof(CorrosiveGas));
            immunities.Add(typeof(Burning));
            immunities.Add(typeof(Corruption));
        }

        public class PrismaticWandering : Mob.Wandering
        {
            public PrismaticWandering(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                PrismaticImage pi = (PrismaticImage)mob;

                if (!enemyInFOV)
                {
                    Buff.Affect<PrismaticGuard>(pi.hero).Set(pi.HP);
                    pi.Destroy();
                    CellEmitter.Get(pi.pos).Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);
                    pi.sprite.Die();
                    Sample.Instance.Play(Assets.Sounds.TELEPORT);
                    return true;
                }
                else
                {
                    return base.Act(enemyInFOV, justAlerted);
                }
            }
        }
    }
}