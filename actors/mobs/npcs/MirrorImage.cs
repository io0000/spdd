using watabou.utils;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.mobs.npcs
{
    public class MirrorImage : NPC
    {
        public MirrorImage()
        {
            InitInstance();

            spriteClass = typeof(MirrorSprite);

            HP = HT = 1;
            defenseSkill = 1;

            alignment = Alignment.ALLY;
            state = HUNTING;

            //before other mobs
            actPriority = MOB_PRIO + 1;
        }

        private Hero hero;
        private int heroID;
        public int armTier;

        public override bool Act()
        {
            if (hero == null)
            {
                hero = (Hero)Actor.FindById(heroID);
                if (hero == null)
                {
                    Die(null);
                    sprite.KillAndErase();
                    return true;
                }
            }

            if (hero.Tier() != armTier)
            {
                armTier = hero.Tier();
                ((MirrorSprite)sprite).UpdateArmor(armTier);
            }

            return base.Act();
        }

        private const string HEROID = "hero_id";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(HEROID, heroID);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            heroID = bundle.GetInt(HEROID);
        }

        public void Duplicate(Hero hero)
        {
            this.hero = hero;
            heroID = this.hero.Id();
            Buff.Affect<MirrorInvis>(this, 32767); //Short.MAX_VALUE);
        }

        public override int DamageRoll()
        {
            int damage;
            if (hero.belongings.weapon != null)
            {
                damage = hero.belongings.weapon.DamageRoll(this);
            }
            else
            {
                damage = hero.DamageRoll(); //handles ring of force
            }
            return (damage + 1) / 2; //half hero damage, rounded up
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

        protected override float AttackDelay()
        {
            return hero.AttackDelay(); //handles ring of furor
        }

        protected override bool CanAttack(Character enemy)
        {
            return base.CanAttack(enemy) ||
                (hero.belongings.weapon != null && hero.belongings.weapon.CanReach(this, enemy.pos));
        }

        public override int DrRoll()
        {
            if (hero != null && hero.belongings.weapon != null)
            {
                return Rnd.NormalIntRange(0, hero.belongings.weapon.DefenseFactor(this) / 2);
            }
            else
            {
                return 0;
            }
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);

            var buff = FindBuff<MirrorInvis>();
            if (buff != null)
                buff.Detach();

            if (enemy is Mob)
                ((Mob)enemy).Aggro(this);

            if (hero.belongings.weapon != null)
            {
                damage = hero.belongings.weapon.Proc(this, enemy, damage);
                if (!enemy.IsAlive() && enemy == Dungeon.hero)
                {
                    Dungeon.Fail(GetType());
                    GLog.Negative(Messages.Capitalize(Messages.Get(typeof(Character), "kill", Name())));
                }
                return damage;
            }
            else
            {
                return damage;
            }
        }

        public override CharSprite GetSprite()
        {
            CharSprite s = base.GetSprite();

            hero = (Hero)Actor.FindById(heroID);
            if (hero != null)
                armTier = hero.Tier();

            ((MirrorSprite)s).UpdateArmor(armTier);
            return s;
        }

        private void InitInstance()
        {
            immunities.Add(typeof(ToxicGas));
            immunities.Add(typeof(CorrosiveGas));
            immunities.Add(typeof(Burning));
            immunities.Add(typeof(Corruption));
    	}

        [SPDStatic]
        public class MirrorInvis : Invisibility
        {
            public MirrorInvis()
            {
                announced = false;
            }

            public override int Icon()
            {
                return BuffIndicator.NONE;
            }
        }
    }
}