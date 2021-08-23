using System.Collections.Generic;
using watabou.utils;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Bee : Mob
    {
        public Bee()
        {
            spriteClass = typeof(BeeSprite);

            viewDistance = 4;

            EXP = 0;

            flying = true;
            state = WANDERING;

            //only applicable when the bee is charmed with elixir of honeyed healing
            intelligentAlly = true;
        }

        private int level;

        //-1 refers to a pot that has gone missing.
        private int potPos;
        //-1 for no owner
        private int potHolder;

        private const string LEVEL = "level";
        private const string POTPOS = "potpos";
        private const string POTHOLDER = "potholder";
        private const string ALIGNMENT = "alignment";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEVEL, level);
            bundle.Put(POTPOS, potPos);
            bundle.Put(POTHOLDER, potHolder);
            bundle.Put(ALIGNMENT, alignment.ToString());    // enum
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            Spawn(bundle.GetInt(LEVEL));
            potPos = bundle.GetInt(POTPOS);
            potHolder = bundle.GetInt(POTHOLDER);
            if (bundle.Contains(ALIGNMENT))
                alignment = bundle.GetEnum<Alignment>(ALIGNMENT);
        }

        public void Spawn(int level)
        {
            this.level = level;

            HT = (2 + level) * 4;
            defenseSkill = 9 + level;
        }

        public void SetPotInfo(int potPos, Character potHolder)
        {
            this.potPos = potPos;
            if (potHolder == null)
                this.potHolder = -1;
            else
                this.potHolder = potHolder.Id();
        }

        public int PotPos()
        {
            return potPos;
        }

        public int PotHolderID()
        {
            return potHolder;
        }

        public override int AttackSkill(Character target)
        {
            return defenseSkill;
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(HT / 10, HT / 4);
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);
            if (enemy is Mob)
            {
                ((Mob)enemy).Aggro(this);
            }
            return damage;
        }

        public override void Add(Buff buff)
        {
            base.Add(buff);
            if (buff is Corruption)
            {
                intelligentAlly = false;
                SetPotInfo(-1, null);
            }
        }

        protected override Character ChooseEnemy()
        {
            //if the pot is no longer present, default to regular AI behaviour
            if (alignment == Alignment.ALLY || (potHolder == -1 && potPos == -1))
            {
                return base.ChooseEnemy();
            }
            //if something is holding the pot, target that
            else if (Actor.FindById(potHolder) != null)
            {
                return (Character)Actor.FindById(potHolder);
            }
            //if the pot is on the ground
            else
            {
                //try to find a new enemy in these circumstances
                if (enemy == null ||
                    !enemy.IsAlive() ||
                    !Actor.Chars().Contains(enemy) ||
                    state == WANDERING ||
                    Dungeon.level.Distance(enemy.pos, potPos) > 3 ||
                    (alignment == Alignment.ALLY && enemy.alignment == Alignment.ALLY) ||
                    (FindBuff<Amok>() == null && enemy.IsInvulnerable(GetType())))
                {
                    //find all mobs near the pot
                    List<Character> enemies = new List<Character>();
                    foreach (Mob mob in Dungeon.level.mobs)
                    {
                        if (!(mob == this) &&
                            Dungeon.level.Distance(mob.pos, potPos) <= 3 &&
                            mob.alignment != Alignment.NEUTRAL &&
                            !mob.IsInvulnerable(GetType()) &&
                            !(alignment == Alignment.ALLY && mob.alignment == Alignment.ALLY))
                        {
                            enemies.Add(mob);
                        }
                    }

                    if (enemies.Count > 0)
                    {
                        return Rnd.Element(enemies);
                    }
                    else
                    {
                        if (alignment != Alignment.ALLY && Dungeon.level.Distance(Dungeon.hero.pos, potPos) <= 3)
                        {
                            return Dungeon.hero;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    return enemy;
                }
            }
        }

        public override bool GetCloser(int target)
        {
            if (alignment == Alignment.ALLY && enemy == null && FindBuff<Corruption>() == null)
            {
                target = Dungeon.hero.pos;
            }
            else if (enemy != null && Actor.FindById(potHolder) == enemy)
            {
                target = enemy.pos;
            }
            else if (potPos != -1 && (state == WANDERING || Dungeon.level.Distance(target, potPos) > 3))
            {
                this.target = target = potPos;
            }

            return base.GetCloser(target);
        }

        public override string Description()
        {
            if (alignment == Alignment.ALLY && FindBuff<Corruption>() == null)
                return Messages.Get(this, "desc_honey");
            else
                return base.Description();
        }
    }
}