using System;
using System.Linq;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.items;
using spdd.items.rings;
using spdd.items.artifacts;
using spdd.items.weapon.missiles;
using spdd.items.weapon;
using spdd.items.weapon.enchantments;
using spdd.items.weapon.missiles.darts;
using spdd.items.stones;
using spdd.sprites;
using spdd.levels;
using spdd.levels.features;
using spdd.utils;
using spdd.plants;
using spdd.messages;

namespace spdd.actors.mobs
{
    public abstract class Mob : Character
    {
        protected Mob()
        {
            actPriority = MOB_PRIO;

            alignment = Alignment.ENEMY;

            SLEEPING = new Sleeping(this);
            HUNTING = new Hunting(this);
            WANDERING = new Wandering(this);
            FLEEING = new Fleeing(this);
            PASSIVE = new Passive(this);
            state = SLEEPING;
        }

        public IAiState SLEEPING;
        public IAiState HUNTING;
        public IAiState WANDERING;
        public IAiState FLEEING;
        public IAiState PASSIVE;
        public IAiState state;

        public Type spriteClass;

        protected int target = -1;

        protected int defenseSkill;

        public int EXP = 1;
        public int maxLvl = Hero.MAX_LEVEL;

        protected Character enemy;
        protected bool enemySeen;
        protected bool alerted;

        protected const float TIME_TO_WAKE_UP = 1.0f;

        private const string STATE = "state";
        private const string SEEN = "seen";
        private const string TARGET = "target";
        private const string MAX_LVL = "max_lvl";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);

            if (state == SLEEPING)
                bundle.Put(STATE, Sleeping.TAG);
            else if (state == WANDERING)
                bundle.Put(STATE, Wandering.TAG);
            else if (state == HUNTING)
                bundle.Put(STATE, Hunting.TAG);
            else if (state == FLEEING)
                bundle.Put(STATE, Fleeing.TAG);
            else if (state == PASSIVE)
                bundle.Put(STATE, Passive.TAG);

            bundle.Put(SEEN, enemySeen);
            bundle.Put(TARGET, target);
            bundle.Put(MAX_LVL, maxLvl);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            var state = bundle.GetString(STATE);
            if (state.Equals(Sleeping.TAG))
                this.state = SLEEPING;
            else if (state.Equals(Wandering.TAG))
                this.state = WANDERING;
            else if (state.Equals(Hunting.TAG))
                this.state = HUNTING;
            else if (state.Equals(Fleeing.TAG))
                this.state = FLEEING;
            else if (state.Equals(Passive.TAG))
                this.state = PASSIVE;

            enemySeen = bundle.GetBoolean(SEEN);
            target = bundle.GetInt(TARGET);
            if (bundle.Contains(MAX_LVL))
                maxLvl = bundle.GetInt(MAX_LVL);
        }

        public virtual CharSprite GetSprite()
        {
            return (CharSprite)Reflection.NewInstance(spriteClass);
        }

        public override bool Act()
        {
            base.Act();

            var justAlerted = alerted;
            alerted = false;

            if (justAlerted)
            {
                sprite.ShowAlert();
            }
            else
            {
                sprite.HideAlert();
                sprite.HideLost();
            }

            if (paralysed > 0)
            {
                enemySeen = false;
                Spend(TICK);
                return true;
            }

            if (FindBuff<Terror>() != null)
                state = FLEEING;

            enemy = ChooseEnemy();

            bool enemyInFOV = enemy != null &&
                enemy.IsAlive() &&
                fieldOfView[enemy.pos] &&
                enemy.invisible <= 0;

            return state.Act(enemyInFOV, justAlerted);
        }

        //FIXME this is sort of a band-aid correction for allies needing more intelligent behaviour
        protected bool intelligentAlly;

        protected virtual Character ChooseEnemy()
        {
            Terror terror = FindBuff<Terror>();
            if (terror != null)
            {
                var source = (Character)Actor.FindById(terror.obj);
                if (source != null)
                    return source;
            }

            //if we are an enemy, and have no target or current target isn't affected by aggression
            //then auto-prioritize a target that is affected by aggression, even another enemy
            if (alignment == Alignment.ENEMY &&
                (enemy == null || enemy.FindBuff<StoneOfAggression.Aggression>() == null))
            {
                foreach (var ch in Actor.Chars())
                {
                    if (ch != this &&
                        fieldOfView[ch.pos] &&
                        ch.FindBuff<StoneOfAggression.Aggression>() != null)
                    {
                        return ch;
                    }
                }
            }

            //find a new enemy if..
            bool newEnemy = false;
            //we have no enemy, or the current one is dead/missing
            if (enemy == null || !enemy.IsAlive() || !Actor.Chars().Contains(enemy) || state == WANDERING)
            {
                newEnemy = true;
            }
            //We are an ally, and current _enemy is another ally.
            else if (alignment == Alignment.ALLY && enemy.alignment == Alignment.ALLY)
            {
                newEnemy = true;
            }
            //We are amoked and current _enemy is the hero
            else if (FindBuff<Amok>() != null && enemy == Dungeon.hero)
            {
                newEnemy = true;
            }
            //We are charmed and current _enemy is what charmed us
            else if (FindBuff<Charm>() != null && FindBuff<Charm>().obj == enemy.Id())
            {
                newEnemy = true;
            }
            //we aren't amoked, current _enemy is invulnerable to us, and that _enemy isn't affect by aggression
            else if (FindBuff<Amok>() != null && enemy.IsInvulnerable(GetType()) && enemy.FindBuff<StoneOfAggression.Aggression>() == null)
            {
                newEnemy = true;
            }

            if (newEnemy)
            {
                HashSet<Character> enemies = new HashSet<Character>();

                //if the mob is amoked...
                if (FindBuff<Amok>() != null)
                {
                    foreach (var mob in Dungeon.level.mobs)
                    {
                        if (mob.alignment == Alignment.ENEMY &&
                            mob != this &&
                            fieldOfView[mob.pos] &&
                            mob.invisible <= 0)
                        {
                            enemies.Add(mob);
                        }
                    }

                    if (enemies.Count == 0)
                    {
                        foreach (var mob in Dungeon.level.mobs)
                        {
                            if (mob.alignment == Alignment.ALLY &&
                                mob != this &&
                                fieldOfView[mob.pos] &&
                                mob.invisible <= 0)
                            {
                                enemies.Add(mob);
                            }
                        }

                        if (enemies.Count == 0)
                        {
                            //try to find the hero third
                            if (fieldOfView[Dungeon.hero.pos] &&
                                Dungeon.hero.invisible <= 0)
                            {
                                enemies.Add(Dungeon.hero);
                            }
                        }
                    }
                }
                //if the mob is an ally...
                else if (alignment == Alignment.ALLY)
                {
                    //look for hostile mobs to attack
                    foreach (var mob in Dungeon.level.mobs)
                    {
                        if (mob.alignment == Alignment.ENEMY &&
                            fieldOfView[mob.pos] &&
                            mob.invisible <= 0 &&
                            !mob.IsInvulnerable(GetType()))
                        {
                            //intelligent allies do not target mobs which are passive, wandering, or asleep
                            if (!intelligentAlly ||
                                (mob.state != mob.SLEEPING && mob.state != mob.PASSIVE && mob.state != mob.WANDERING))
                            {
                                enemies.Add(mob);
                            }
                        }
                    }
                }
                //if the mob is an enemy...
                else if (alignment == Alignment.ENEMY)
                {
                    //look for ally mobs to attack
                    foreach (var mob in Dungeon.level.mobs)
                    {
                        if (mob.alignment == Alignment.ALLY && fieldOfView[mob.pos] && mob.invisible <= 0 && !mob.IsInvulnerable(GetType()))
                            enemies.Add(mob);
                    }

                    //and look for the hero
                    if (fieldOfView[Dungeon.hero.pos] && Dungeon.hero.invisible <= 0 && !Dungeon.hero.IsInvulnerable(GetType()))
                    {
                        enemies.Add(Dungeon.hero);
                    }
                }

                var charm = FindBuff<Charm>();
                if (charm != null)
                {
                    var source = (Character)Actor.FindById(charm.obj);
                    if (source != null && enemies.Contains(source) && enemies.Count > 1)
                    {
                        enemies.Remove(source);
                    }
                }

                //neutral characters in particular do not choose enemies.
                if (enemies.Count == 0)
                {
                    return null;
                }
                else
                {
                    //go after the closest potential enemy, preferring the hero if two are equidistant
                    Character closest = null;
                    foreach (var curr in enemies)
                    {
                        int distance1 = Dungeon.level.Distance(pos, curr.pos);
                        int distance2 = 0;
                        if (closest != null)
                            distance2 = Dungeon.level.Distance(pos, closest.pos);

                        if (closest == null ||
                            distance1 < distance2 ||
                            (distance1 == distance2 && curr == Dungeon.hero))
                        {
                            closest = curr;
                        }
                    }
                    return closest;
                }
            } // if (newEnemy)
            else
            {
                return enemy;
            }
        }

        public override void Add(Buff buff)
        {
            base.Add(buff);

            if (buff is Amok || buff is Corruption)
            {
                state = HUNTING;
            }
            else if (buff is Terror)
            {
                state = FLEEING;
            }
            else if (buff is Sleep)
            {
                state = SLEEPING;
                Postpone(Sleep.SWS);
            }
        }

        public override void Remove(Buff buff)
        {
            base.Remove(buff);

            if (buff is Terror)
            {
                if (enemySeen)
                {
                    sprite.ShowStatus(CharSprite.NEGATIVE, Messages.Get(this, "rage"));
                    state = HUNTING;
                }
                else
                {
                    state = WANDERING;
                }
            }
        }

        protected virtual bool CanAttack(Character enemy)
        {
            return Dungeon.level.Adjacent(pos, enemy.pos);
        }

        public virtual bool GetCloser(int target)
        {
            if (rooted || target == pos)
                return false;

            int step = -1;

            if (Dungeon.level.Adjacent(pos, target))
            {
                path = null;

                if (Actor.FindChar(target) == null &&
                    (Dungeon.level.passable[target] || (flying && Dungeon.level.avoid[target])) &&
                    (!Character.HasProp(this, Character.Property.LARGE) || Dungeon.level.openSpace[target]))
                {
                    step = target;
                }
            }
            else
            {
                bool newPath = false;
                //scrap the current path if it's empty, no longer connects to the current location
                //or if it's extremely inefficient and checking again may result in a much better path
                if (path == null ||
                    path.Count == 0 ||
                    !Dungeon.level.Adjacent(pos, path[0]) ||
                    path.Count > 2 * Dungeon.level.Distance(pos, target))
                {
                    newPath = true;
                }
                else if (path[path.Count - 1] != target)
                {
                    //if the new target is adjacent to the end of the path, adjust for that
                    //rather than scrapping the whole path.
                    if (Dungeon.level.Adjacent(target, path[path.Count - 1]))
                    {
                        int last = path[path.Count - 1];
                        path.RemoveAt(path.Count - 1);

                        if (path.Count == 0)
                        {
                            if (Dungeon.level.Adjacent(target, pos))
                            {
                                //shorten for a closer one
                                path.Add(target);
                            }
                            else
                            {
                                //extend the path for a further target
                                path.Add(last);
                                path.Add(target);
                            }
                        }
                        else
                        {
                            //if the new target is simply 1 earlier in the path shorten the path
                            if (path[path.Count - 1] == target)
                            {
                                // ÀÇµµÀû
                            }
                            //if the new target is closer/same, need to modify end of path
                            else if (Dungeon.level.Adjacent(target, path[path.Count - 1]))
                            {
                                path.Add(target);
                            }
                            //if the new target is further away, need to extend the path
                            else
                            {
                                path.Add(last);
                                path.Add(target);
                            }
                        }
                    }
                    else
                    {
                        newPath = true;
                    }
                }

                //checks if the next cell along the current path can be stepped into
                if (!newPath)
                {
                    int nextCell = path[0];
                    path.RemoveAt(0);

                    if (!Dungeon.level.passable[nextCell] ||
                        (!flying && Dungeon.level.avoid[nextCell]) ||
                        (Character.HasProp(this, Character.Property.LARGE) && !Dungeon.level.openSpace[nextCell]) ||
                        Actor.FindChar(nextCell) != null)
                    {
                        newPath = true;
                        //If the next cell on the path can't be moved into, see if there is another cell that could replace it
                        if (path.Count > 0)
                        {
                            foreach (int i in PathFinder.NEIGHBORS8)
                            {
                                if (Dungeon.level.Adjacent(pos, nextCell + i) && Dungeon.level.Adjacent(nextCell + i, path[0]))
                                {
                                    if (Dungeon.level.passable[nextCell + i] &&
                                        (flying || !Dungeon.level.avoid[nextCell + i]) &&
                                        (!Character.HasProp(this, Character.Property.LARGE) || Dungeon.level.openSpace[nextCell + i]) &&
                                        Actor.FindChar(nextCell + i) == null)
                                    {
                                        path.Insert(0, nextCell + i);
                                        newPath = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        path.Insert(0, nextCell);
                    }
                }

                //generate a new path
                if (newPath)
                {
                    //If we aren't hunting, always take a full path
                    PathFinder.Path full = Dungeon.FindPath(this, target, Dungeon.level.passable, fieldOfView, true);
                    if (state != HUNTING)
                    {
                        path = full;
                    }
                    else
                    {
                        //otherwise, check if other characters are forcing us to take a very slow route
                        // and don't try to go around them yet in response, basically assume their blockage is temporary
                        PathFinder.Path ignoreChars = Dungeon.FindPath(this, target, Dungeon.level.passable, fieldOfView, false);
                        if (ignoreChars != null && (full == null || full.Count > 2 * ignoreChars.Count))
                        {
                            //check if first cell of shorter path is valid. If it is, use new shorter path. Otherwise do nothing and wait.
                            path = ignoreChars;
                            if (!Dungeon.level.passable[ignoreChars[0]] ||
                                (!flying && Dungeon.level.avoid[ignoreChars[0]]) ||
                                (Character.HasProp(this, Character.Property.LARGE) && !Dungeon.level.openSpace[ignoreChars[0]]) ||
                                Actor.FindChar(ignoreChars[0]) != null)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            path = full;
                        }
                    }
                }

                if (path != null)
                {
                    step = path[0];
                    path.RemoveAt(0);
                }
                else
                {
                    return false;
                }
            }

            if (step == -1)
                return false;

            Move(step);
            return true;
        }

        public virtual bool GetFurther(int target)
        {
            if (rooted || target == pos)
                return false;

            var step = Dungeon.Flee(this, target, Dungeon.level.passable, fieldOfView, true);
            if (step != -1)
            {
                Move(step);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UpdateSpriteState()
        {
            base.UpdateSpriteState();
            if (Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>() != null ||
                Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>() != null)
                sprite.Add(CharSprite.State.PARALYSED);
        }

        protected virtual float AttackDelay()
        {
            float delay = 1f;
            if (FindBuff<Adrenaline>() != null)
                delay /= 1.5f;
            return delay;
        }

        protected virtual bool DoAttack(Character enemy)
        {
            if (sprite != null && (sprite.visible || enemy.sprite.visible))
            {
                sprite.Attack(enemy.pos);
                Spend(AttackDelay());
                return false;
            }
            else
            {
                Attack(enemy);
                Spend(AttackDelay());
                return true;
            }
        }

        public override void OnAttackComplete()
        {
            Attack(enemy);
            base.OnAttackComplete();
        }

        public override int DefenseSkill(Character enemy)
        {
            if (!SurprisedBy(enemy) &&
                paralysed == 0 &&
                !(alignment == Alignment.ALLY && enemy == Dungeon.hero))
            {
                return this.defenseSkill;
            }
            else
            {
                return 0;
            }
        }

        protected bool hitWithRanged;

        public override int DefenseProc(Character enemy, int damage)
        {
            if (enemy is Hero && ((Hero)enemy).belongings.weapon is MissileWeapon)
                hitWithRanged = true;

            if (SurprisedBy(enemy))
            {
                ++Statistics.sneakAttacks;
                BadgesExtensions.ValidateRogueUnlock();

                //TODO this is somewhat messy, it would be nicer to not have to manually handle delays here
                // playing the strong hit sound might work best as another property of weapon?
                if (Dungeon.hero.belongings.weapon is SpiritBow.SpiritArrow ||
                    Dungeon.hero.belongings.weapon is Dart)
                {
                    Sample.Instance.PlayDelayed(Assets.Sounds.HIT_STRONG, 0.125f);
                }
                else
                {
                    Sample.Instance.Play(Assets.Sounds.HIT_STRONG);
                }

                if (enemy.FindBuff<Preparation>() != null)
                {
                    Wound.Hit(this);
                }
                else
                {
                    Surprise.Hit(this);
                }
            }

            //if attacked by something else than current target, and that thing is closer, switch targets
            if (this.enemy == null ||
                (enemy != this.enemy && (Dungeon.level.Distance(pos, enemy.pos) < Dungeon.level.Distance(pos, this.enemy.pos))))
            {
                Aggro(enemy);
                target = enemy.pos;
            }

            if (FindBuff<SoulMark>() != null)
            {
                int restoration = Math.Min(damage, HP);

                //physical damage that doesn't come from the hero is less effective
                if (enemy != Dungeon.hero)
                    restoration = (int)Math.Round(restoration * 0.4f, MidpointRounding.AwayFromZero);

                Buff.Affect<Hunger>(Dungeon.hero).Satisfy(restoration);
                Dungeon.hero.HP = (int)Math.Ceiling(Math.Min(Dungeon.hero.HT, Dungeon.hero.HP + (restoration * 0.4f)));
                Dungeon.hero.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);
            }

            return damage;
        }

        public virtual bool SurprisedBy(Character enemy)
        {
            return enemy == Dungeon.hero &&
                (enemy.invisible > 0 || !enemySeen) &&
                ((Hero)enemy).CanSurpriseAttack();
        }

        public virtual void Aggro(Character ch)
        {
            enemy = ch;
            if (state != PASSIVE)
                state = HUNTING;
        }

        public bool IsTargeting(Character ch)
        {
            return enemy == ch;
        }

        public override void Damage(int dmg, object src)
        {
            if (state == SLEEPING)
                state = WANDERING;

            if (state != HUNTING)
                alerted = true;

            base.Damage(dmg, src);
        }

        public override void Destroy()
        {
            base.Destroy();

            Dungeon.level.mobs.Remove(this);

            if (Dungeon.hero.IsAlive())
            {
                if (alignment == Alignment.ENEMY)
                {
                    ++Statistics.enemiesSlain;
                    BadgesExtensions.ValidateMonstersSlain();
                    Statistics.qualifiedForNoKilling = false;

                    int exp = Dungeon.hero.lvl <= maxLvl ? EXP : 0;
                    if (exp > 0)
                    {
                        Dungeon.hero.sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "exp", exp));
                    }
                    Dungeon.hero.EarnExp(exp, GetType());
                }
            }
        }

        public override void Die(object cause)
        {
            if (hitWithRanged)
            {
                ++Statistics.thrownAssists;
                BadgesExtensions.ValidateHuntressUnlock();
            }

            if (Utils.CheckObjectType(cause, typeof(Chasm)))
            {
                //50% chance to round up, 50% to round down
                if (EXP % 2 == 1)
                    EXP += Rnd.Int(2);
                EXP /= 2;
            }

            if (alignment == Alignment.ENEMY)
                RollToDropLoot();

            if (Dungeon.hero.IsAlive() && !Dungeon.level.heroFOV[pos])
                GLog.Information(Messages.Get(this, "died"));

            base.Die(cause);
        }

        public virtual void RollToDropLoot()
        {
            if (Dungeon.hero.lvl > maxLvl + 2)
                return;

            float lootChance = this.lootChance;
            lootChance *= RingOfWealth.DropChanceMultiplier(Dungeon.hero);

            if (Rnd.Float() < lootChance)
            {
                Item loot = CreateLoot();
                if (loot != null)
                    Dungeon.level.Drop(loot, pos).sprite.Drop();
            }

            //ring of wealth logic
            if (Ring.GetBuffedBonus<RingOfWealth.Wealth>(Dungeon.hero) > 0)
            {
                int rolls = 1;
                if (properties.Contains(Property.BOSS))
                    rolls = 15;
                else if (properties.Contains(Property.MINIBOSS))
                    rolls = 5;

                List<Item> bonus = RingOfWealth.TryForBonusDrop(Dungeon.hero, rolls);
                if (bonus != null && bonus.Count > 0)
                {
                    foreach (Item b in bonus)
                        Dungeon.level.Drop(b, pos).sprite.Drop();
                    RingOfWealth.ShowFlareForBonusDrop(sprite);
                }
            }

            //lucky enchant logic
            if (Dungeon.hero.lvl <= maxLvl && FindBuff<Lucky.LuckProc>() != null)
            {
                Dungeon.level.Drop(Lucky.GenLoot(), pos).sprite.Drop();
                Lucky.ShowFlare(sprite);
            }
        }

        protected object loot;
        protected float lootChance;

        public virtual Item CreateLoot()
        {
            Item item;

            if (loot is Generator.Category)
                item = Generator.Random((Generator.Category)loot);
            else if (loot is Type)
                item = Generator.Random((Type)loot);
            else
                item = (Item)loot;

            return item;
        }

        //how many mobs this one should count as when determining spawning totals
        public virtual float SpawningWeight()
        {
            return 1;
        }

        public virtual bool Reset()
        {
            return false;
        }

        public virtual void Beckon(int cell)
        {
            Notice();

            if (state != HUNTING && state != FLEEING)
                state = WANDERING;

            target = cell;
        }

        public virtual string Description()
        {
            return Messages.Get(this, "desc");
        }

        public virtual void Notice()
        {
            sprite.ShowAlert();
        }

        public void Yell(string str)
        {
            GLog.NewLine();
            GLog.Negative("%s: \"%s\" ", Messages.TitleCase(Name()), str);
        }

        ////returns true when a mob sees the hero, and is currently targeting them.
        //public bool FocusingHero()
        //{
        //    return enemySeen && (target == Dungeon.hero.pos);
        //}

        public interface IAiState
        {
            bool Act(bool enemyInFOV, bool justAlerted);
        }

        public class Sleeping : IAiState
        {
            protected readonly Mob mob;
            public const string TAG = "SLEEPING";

            public Sleeping(Mob mob)
            {
                this.mob = mob;
            }

            public virtual bool Act(bool enemyInFOV, bool justAlerted)
            {
                return mob.SleppingAct(enemyInFOV, justAlerted);
            }
        }

        public bool SleppingAct(bool enemyInFOV, bool justAlerted)
        {
            var enemy = this.enemy;

            if (enemyInFOV && Rnd.Int(Distance(enemy) + (int)enemy.Stealth()) < 1)
            {
                enemySeen = true;

                Notice();
                state = HUNTING;
                target = this.enemy.pos;

                if (alignment == Alignment.ENEMY && Dungeon.IsChallenged(Challenges.SWARM_INTELLIGENCE))
                {
                    foreach (var mob in Dungeon.level.mobs)
                    {
                        if (mob.paralysed <= 0 &&
                            Dungeon.level.Distance(pos, mob.pos) <= 8 && //TODO base on pathfinder distance instead?
                            mob.state != mob.HUNTING)
                        {
                            mob.Beckon(target);
                        }
                    }
                }

                Spend(TIME_TO_WAKE_UP);
            }
            else
            {
                enemySeen = false;

                Spend(TICK);
            }

            return true;
        }

        public class Wandering : IAiState
        {
            protected readonly Mob mob;
            public const string TAG = "WANDERING";

            public Wandering(Mob mob)
            {
                this.mob = mob;
            }

            public virtual bool Act(bool enemyInFOV, bool justAlerted)
            {
                if (enemyInFOV && (justAlerted || Rnd.Float(mob.Distance(mob.enemy) / 2f + mob.enemy.Stealth()) < 1))
                {
                    return NoticeEnemy();
                }
                else
                {
                    return ContinueWandering();
                }
            }

            private bool NoticeEnemy()
            {
                return mob.NoticeEnemy();
            }

            protected virtual bool ContinueWandering()
            {
                return mob.ContinueWandering();
            }
        }

        private bool NoticeEnemy()
        {
            enemySeen = true;

            Notice();
            alerted = true;
            state = HUNTING;
            target = enemy.pos;

            if (alignment == Alignment.ENEMY && Dungeon.IsChallenged(Challenges.SWARM_INTELLIGENCE))
            {
                foreach (var mob in Dungeon.level.mobs)
                {
                    if (mob.paralysed <= 0 &&
                        Dungeon.level.Distance(pos, mob.pos) <= 8 && //TODO base on pathfinder distance instead?
                        mob.state != mob.HUNTING)
                    {
                        mob.Beckon(target);
                    }
                }
            }

            return true;
        }

        protected bool ContinueWandering()
        {
            enemySeen = false;

            int oldPos = pos;

            if (target != -1 && GetCloser(target))
            {
                Spend(1 / Speed());
                return MoveSprite(oldPos, pos);
            }
            else
            {
                target = Dungeon.level.RandomDestination(this);
                Spend(TICK);
            }

            return true;
        }

        public class Hunting : IAiState
        {
            protected readonly Mob mob;
            public const string TAG = "HUNTING";

            public Hunting(Mob mob)
            {
                this.mob = mob;
            }

            public virtual bool Act(bool enemyInFOV, bool justAlerted)
            {
                return mob.HuntingAct(enemyInFOV, justAlerted);
            }
        }

        public bool HuntingAct(bool enemyInFOV, bool justAlerted)
        {
            enemySeen = enemyInFOV;

            if (enemyInFOV && !IsCharmedBy(enemy) && CanAttack(enemy))
            {
                target = enemy.pos;
                return DoAttack(enemy);
            }
            else
            {
                if (enemyInFOV)
                {
                    target = enemy.pos;
                }
                else if (enemy == null)
                {
                    state = WANDERING;
                    target = Dungeon.level.RandomDestination(this);
                    return true;
                }

                var oldPos = pos;

                if (target != -1 && GetCloser(target))
                {
                    Spend(1 / Speed());
                    return MoveSprite(oldPos, pos);
                }
                else
                {
                    //if moving towards an enemy isn't possible, try to switch targets to another enemy that is closer
                    var newEnemy = ChooseEnemy();
                    if (newEnemy != null && enemy != newEnemy)
                    {
                        enemy = newEnemy;
                        return HuntingAct(enemyInFOV, justAlerted);
                    }

                    Spend(TICK);
                    if (!enemyInFOV)
                    {
                        sprite.ShowLost();
                        state = WANDERING;
                        target = Dungeon.level.RandomDestination(this);
                    }
                    return true;
                }
            }
        }

        public class Fleeing : IAiState
        {
            protected readonly Mob mob;
            public const string TAG = "FLEEING";

            public Fleeing(Mob mob)
            {
                this.mob = mob;
            }

            public bool Act(bool enemyInFOV, bool justAlerted)
            {
                return mob.FleeingAct(enemyInFOV, justAlerted, this);
            }

            public virtual void NowhereToRun()
            { }
        }

        public bool FleeingAct(bool enemyInFOV, bool justAlerted, Fleeing fleeing)
        {
            enemySeen = enemyInFOV;
            //loses target when 0-dist rolls a 6 or greater.
            if (enemy == null || !enemyInFOV && 1 + Rnd.Int(Dungeon.level.Distance(pos, target)) >= 6)
            {
                target = -1;
            }
            //if enemy isn't in FOV, keep running from their previous position.
            else if (enemyInFOV)
            {
                target = enemy.pos;
            }

            int oldPos = pos;
            if (target != -1 && GetFurther(target))
            {
                Spend(1 / Speed());
                return MoveSprite(oldPos, pos);
            }
            else
            {
                Spend(TICK);
                fleeing.NowhereToRun();
            }
            return true;
        }

        public class Passive : IAiState
        {
            protected readonly Mob mob;
            public const string TAG = "PASSIVE";

            public Passive(Mob mob)
            {
                this.mob = mob;
            }

            public bool Act(bool enemyInFOV, bool justAlerted)
            {
                return mob.PassiveAct(enemyInFOV, justAlerted);
            }
        }

        public bool PassiveAct(bool enemyInFOV, bool justAlerted)
        {
            enemySeen = enemyInFOV;

            Spend(TICK);

            return true;
        }

        private static List<Mob> heldAllies = new List<Mob>();

        public static void HoldAllies(Level level)
        {
            heldAllies.Clear();

            foreach (Mob mob in level.mobs.ToArray())
            {
                //preserve the ghost no matter where they are
                if (mob is DriedRose.GhostHero)
                {
                    ((DriedRose.GhostHero)mob).ClearDefensingPos();
                    level.mobs.Remove(mob);
                    heldAllies.Add(mob);
                }
                //preserve intelligent allies if they are near the hero
                else if (mob.alignment == Alignment.ALLY &&
                    mob.intelligentAlly &&
                    Dungeon.level.Distance(Dungeon.hero.pos, mob.pos) <= 3)
                {
                    level.mobs.Remove(mob);
                    heldAllies.Add(mob);
                }
            }
        }

        public static void RestoreAllies(Level level, int pos)
        {
            if (heldAllies.Count != 0)
            {
                List<int> candidatePositions = new List<int>();
                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    if (!Dungeon.level.solid[i + pos] && level.FindMob(i + pos) == null)
                        candidatePositions.Add(i + pos);
                }
                Rnd.Shuffle(candidatePositions);

                foreach (Mob ally in heldAllies)
                {
                    level.mobs.Add(ally);
                    ally.state = ally.WANDERING;

                    if (candidatePositions.Count > 0)
                    {
                        ally.pos = candidatePositions[0];
                        candidatePositions.RemoveAt(0);
                    }
                    else
                    {
                        ally.pos = pos;
                    }
                }
            }

            heldAllies.Clear();
        }

        public static void ClearHeldAllies()
        {
            heldAllies.Clear();
        }
    }
}