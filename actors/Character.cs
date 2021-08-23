using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.items;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.items.rings;
using spdd.items.wands;
using spdd.items.stones;
using spdd.items.armor.glyphs;
using spdd.items.weapon.missiles;
using spdd.items.weapon.missiles.darts;
using spdd.items.weapon.enchantments;
using spdd.levels;
using spdd.levels.features;
using spdd.levels.traps;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.actors
{
    public abstract class Character : Actor
    {
        public int pos;

        public CharSprite sprite;

        public int HT;
        public int HP;

        public float baseSpeed = 1;
        protected PathFinder.Path path;

        public int paralysed;
        public bool rooted;
        public bool flying;
        public int invisible;

        public enum Alignment
        {
            ENEMY,
            NEUTRAL,
            ALLY
        }

        public Alignment alignment;

        public int viewDistance = 8;

        public bool[] fieldOfView = null;

        private readonly List<Buff> buffs = new List<Buff>();

        public override bool Act()
        {
            if (fieldOfView == null || fieldOfView.Length != Dungeon.level.Length())
                fieldOfView = new bool[Dungeon.level.Length()];

            Dungeon.level.UpdateFieldOfView(this, fieldOfView);

            //throw any items that are on top of an immovable char
            if (properties.Contains(Property.IMMOVABLE))
                ThrowItems();

            return false;
        }

        protected void ThrowItems()
        {
            Heap heap = Dungeon.level.heaps[pos];
            if (heap != null && heap.type == Heap.Type.HEAP)
            {
                int n;
                do
                {
                    n = pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                } while (!Dungeon.level.passable[n] && !Dungeon.level.avoid[n]);
                Dungeon.level.Drop(heap.PickUp(), n).sprite.Drop(pos);
            }
        }

        public virtual string Name()
        {
            return Messages.Get(this, "name");
        }

        public virtual bool CanInteract(Character c)
        {
            return Dungeon.level.Adjacent(pos, c.pos);
        }

        //swaps places by default
        public virtual bool Interact(Character c)
        {
            //can't spawn places if one char has restricted movement
            if (rooted || c.rooted || FindBuff<Vertigo>() != null || c.FindBuff<Vertigo>() != null)
                return true;

            //don't allow char to swap onto hazard unless they're flying
            //you can swap onto a hazard though, as you're not the one instigating the swap
            if (!Dungeon.level.passable[pos] && !c.flying)
                return true;

            //can't swap into a space without room
            if (properties.Contains(Property.LARGE) && !Dungeon.level.openSpace[c.pos] 
                || c.properties.Contains(Property.LARGE) && !Dungeon.level.openSpace[pos])
                return true;

            int curPos = pos;

            MoveSprite(pos, Dungeon.hero.pos);
            Move(Dungeon.hero.pos);

            Dungeon.hero.sprite.Move(Dungeon.hero.pos, curPos);
            Dungeon.hero.Move(curPos);

            Dungeon.hero.Spend(1 / Dungeon.hero.Speed());
            Dungeon.hero.Busy();

            return true;
        }

        protected bool MoveSprite(int from, int to)
        {
            if (sprite.IsVisible() && (Dungeon.level.heroFOV[from] || Dungeon.level.heroFOV[to]))
            {
                sprite.Move(from, to);
                return true;
            }
            else
            {
                sprite.TurnTo(from, to);
                sprite.Place(to);
                return true;
            }
        }

        public virtual void HitSound(float pitch)
        {
            Sample.Instance.Play(Assets.Sounds.HIT, 1, pitch);
        }

        public virtual bool BlockSound(float pitch)
        {
            return false;
        }

        private const string POS = "pos";
        public const string TAG_HP = "HP";
        public const string TAG_HT = "HT";
        public const string TAG_SHLD = "SHLD";
        private const string BUFFS = "buffs";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);

            bundle.Put(POS, pos);
            bundle.Put(TAG_HP, HP);
            bundle.Put(TAG_HT, HT);
            bundle.Put(BUFFS, buffs);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            pos = bundle.GetInt(POS);
            HP = bundle.GetInt(TAG_HP);
            HT = bundle.GetInt(TAG_HT);

            foreach (var b in bundle.GetCollection(BUFFS))
            {
                if (b != null)
                    ((Buff)b).AttachTo(this);
            }
        }

        public virtual bool Attack(Character enemy)
        {
            if (enemy == null)
                return false;

            bool visibleFight = Dungeon.level.heroFOV[pos] || Dungeon.level.heroFOV[enemy.pos];

            if (enemy.IsInvulnerable(GetType()))
            {
                if (visibleFight)
                {
                    enemy.sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "invulnerable"));

                    Sample.Instance.Play(Assets.Sounds.HIT_PARRY, 1f, Rnd.Float(0.96f, 1.05f));
                }

                return false;
            }
            else if (Hit(this, enemy, false))
            {
                int dr = enemy.DrRoll();

                if (this is Hero)
                {
                    Hero h = (Hero)this;

                    if (h.belongings.weapon is MissileWeapon &&
                        h.subClass == HeroSubClass.SNIPER &&
                        !Dungeon.level.Adjacent(h.pos, enemy.pos))
                    {
                        dr = 0;
                    }
                }

                int dmg = 0;

                Preparation prep = FindBuff<Preparation>();
                if (prep != null)
                    dmg = prep.DamageRoll(this);
                else
                    dmg = DamageRoll();

                int effectiveDamage = enemy.DefenseProc(this, dmg);
                effectiveDamage = Math.Max(effectiveDamage - dr, 0);

                if (enemy.FindBuff<Vulnerable>() != null)
                    effectiveDamage = (int)(effectiveDamage * 1.33f);

                effectiveDamage = AttackProc(enemy, effectiveDamage);

                if (visibleFight)
                {
                    if (effectiveDamage > 0 || !enemy.BlockSound(Rnd.Float(0.96f, 1.05f)))
                        HitSound(Rnd.Float(0.87f, 1.15f));
                }

                // If the enemy is already dead, interrupt the attack.
                // This matters as defence procs can sometimes inflict self-damage, such as armor glyphs.
                if (!enemy.IsAlive())
                    return true;

                enemy.Damage(effectiveDamage, this);

                if (FindBuff<FireImbue>() != null)
                    FindBuff<FireImbue>().Proc(enemy);
                if (FindBuff<EarthImbue>() != null)
                    FindBuff<EarthImbue>().Proc(enemy);
                if (FindBuff<FrostImbue>() != null)
                    FindBuff<FrostImbue>().Proc(enemy);

                if (enemy.IsAlive() && prep != null && prep.CanKO(enemy))
                {
                    enemy.HP = 0;
                    if (!enemy.IsAlive())
                    {
                        enemy.Die(this);
                    }
                    else
                    {
                        //helps with triggering any on-damage effects that need to activate
                        enemy.Damage(-1, this);
                    }
                    enemy.sprite.ShowStatus(CharSprite.NEGATIVE, Messages.Get(typeof(Preparation), "assassinated"));
                }

                enemy.sprite.BloodBurstA(sprite.Center(), effectiveDamage);
                enemy.sprite.Flash();

                if (!enemy.IsAlive() && visibleFight)
                {
                    if (enemy == Dungeon.hero)
                    {
                        if (this == Dungeon.hero)
                            return true;

                        Dungeon.Fail(GetType());

                        GLog.Negative(Messages.Capitalize(Messages.Get(typeof(Character), "kill", Name())));
                    }
                    else if (this == Dungeon.hero)
                    {
                        GLog.Information(Messages.Capitalize(Messages.Get(typeof(Character), "defeat", enemy.Name())));
                    }
                }

                return true;
            }
            else
            {
                if (visibleFight)
                {
                    string defense = enemy.DefenseVerb();
                    enemy.sprite.ShowStatus(CharSprite.NEUTRAL, defense);

                    //TODO enemy.defenseSound? currently miss plays for monks/crab even when the parry
                    Sample.Instance.Play(Assets.Sounds.MISS);
                }

                return false;
            }
        }

        public const int INFINITE_ACCURACY = 1_000_000;
        public const int INFINITE_EVASION = 1_000_000;

        public static bool Hit(Character attacker, Character defender, bool magic)
        {
            float acuStat = attacker.AttackSkill(defender);
            float defStat = defender.DefenseSkill(attacker);

            //if accuracy or evasion are large enough, treat them as infinite.
            //note that infinite evasion beats infinite accuracy
            if (defStat >= INFINITE_EVASION)
                return false;
            else if (acuStat >= INFINITE_ACCURACY)
                return true;

            float acuRoll = Rnd.Float(acuStat);
            if (attacker.FindBuff<Bless>() != null)
                acuRoll *= 1.25f;
            if (attacker.FindBuff<Hex>() != null)
                acuRoll *= 0.8f;

            float defRoll = Rnd.Float(defStat);
            if (defender.FindBuff<Bless>() != null)
                defRoll *= 1.25f;
            if (defender.FindBuff<Hex>() != null)
                defRoll *= 0.8f;

            return (magic ? acuRoll * 2 : acuRoll) >= defRoll;
        }

        public virtual int AttackSkill(Character target)
        {
            return 0;
        }

        public virtual int DefenseSkill(Character enemy)
        {
            return 0;
        }

        public virtual string DefenseVerb()
        {
            return Messages.Get(this, "def_verb");
        }

        public virtual int DrRoll()
        {
            return 0;
        }

        public virtual int DamageRoll()
        {
            return 1;
        }

        public virtual int AttackProc(Character enemy, int damage)
        {
            if (FindBuff<Weakness>() != null)
                damage = (int)(damage * 0.67f);

            return damage;
        }

        public virtual int DefenseProc(Character enemy, int damage)
        {
            return damage;
        }

        public virtual float Speed()
        {
            float speed = baseSpeed;

            if (FindBuff<Cripple>() != null)
                speed /= 2.0f;
            if (FindBuff<Stamina>() != null)
                speed *= 1.5f;
            if (FindBuff<Adrenaline>() != null)
                speed *= 2.0f;
            if (FindBuff<Haste>() != null)
                speed *= 3.0f;

            return speed;
        }

        //used so that buffs(Shieldbuff.class) isn't called every time unnecessarily
        private int cachedShield;
        public bool needsShieldUpdate = true;

        public int Shielding()
        {
            if (!needsShieldUpdate)
                return cachedShield;

            cachedShield = 0;
            foreach (var s in Buffs<ShieldBuff>())
                cachedShield += s.Shielding();

            needsShieldUpdate = false;
            return cachedShield;
        }

        public virtual void Damage(int dmg, object src)
        {
            if (!IsAlive() || dmg < 0)
                return;

            if (IsInvulnerable(src.GetType()))
            {
                sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "invulnerable"));
                return;
            }

            if (!(src is LifeLink) && FindBuff<LifeLink>() != null)
            {
                var links = Buffs<LifeLink>();
                foreach (var link in links.ToArray())
                {
                    if (Actor.FindById(link.obj) == null)
                    {
                        links.Remove(link);
                        link.Detach();
                    }
                }

                dmg = (int)Math.Ceiling(dmg / (float)(links.Count + 1));
                foreach (var link in links)
                {
                    var ch = (Character)Actor.FindById(link.obj);
                    ch.Damage(dmg, link);
                    if (!ch.IsAlive())
                        link.Detach();
                }
            }

            Terror t = FindBuff<Terror>();
            if (t != null)
                t.Recover();

            Charm c = FindBuff<Charm>();
            if (c != null)
                c.Recover();

            if (FindBuff<Frost>() != null)
                Buff.Detach<Frost>(this);

            if (FindBuff<MagicalSleep>() != null)
                Buff.Detach<MagicalSleep>(this);

            if (FindBuff<Doom>() != null && !IsImmune(typeof(Doom)))
                dmg *= 2;

            var srcClass = src.GetType();
            if (IsImmune(srcClass))
                dmg = 0;
            else
                dmg = (int)Math.Round(dmg * Resist(srcClass), MidpointRounding.AwayFromZero);

            if (AntiMagic.RESISTS.Contains(srcClass) && FindBuff<ArcaneArmor>() != null)
            {
                dmg -= Rnd.NormalIntRange(0, FindBuff<ArcaneArmor>().Level());
                if (dmg < 0)
                    dmg = 0;
            }

            if (FindBuff<Paralysis>() != null)
                FindBuff<Paralysis>().ProcessDamage(dmg);

            int shielded = dmg;
            //FIXME: when I add proper damage properties, should add an IGNORES_SHIELDS property to use here.
            if (!(src is Hunger))
            {
                foreach (ShieldBuff s in Buffs<ShieldBuff>())
                {
                    dmg = s.AbsorbDamage(dmg);
                    if (dmg == 0) 
                        break;
                }
            }

            shielded -= dmg;
            HP -= dmg;

            if (sprite != null)
            {
                int ds = dmg + shielded;
                sprite.ShowStatus(HP > HT / 2 ?
                                CharSprite.WARNING :
                                CharSprite.NEGATIVE,
                                ds.ToString());
            }

            if (HP < 0) 
                HP = 0;

            if (!IsAlive())
                Die(src);
        }

        public virtual void Destroy()
        {
            HP = 0;
            Actor.Remove(this);
        }

        public virtual void Die(object src)
        {
            Destroy();

            if (Utils.CheckObjectType(src, typeof(Chasm)) == false)
                sprite.Die();
        }

        public virtual bool IsAlive()
        { 
            return HP > 0; 
        }

        public override void Spend(float time)
        {
            var timeScale = 1f;

            if (FindBuff<Slow>() != null)
                timeScale *= 0.5f;
            else if (FindBuff<Chill>() != null)
                timeScale *= FindBuff<Chill>().SpeedFactor();

            if (FindBuff<Speed>() != null)
                timeScale *= 2.0f;

            base.Spend(time / timeScale);
        }

        public List<Buff> Buffs()
        {
            return new List<Buff>(buffs);
        }

        //returns all buffs assignable from the given buff class
        public List<T> Buffs<T>() where T : Buff
        {
            List<T> filtered = new List<T>();

            Type c = typeof(T);

            foreach (var b in buffs)
            {
                // if (c.isInstance( b ))
                if (c.IsAssignableFrom(b.GetType()))
                {
                    filtered.Add((T)b);
                }
            }

            return filtered;
        }

        //returns an instance of the specific buff class, if it exists. Not just assignable
        public T FindBuff<T>() where T : Buff
        {
            foreach (var b in buffs)
            {
                if (b.GetType() == typeof(T))
                {
                    return (T)b;
                }
            }

            return default(T);
        }

        public bool IsCharmedBy(Character ch)
        {
            int chID = ch.Id();
            foreach (Buff b in buffs)
            {
                if (b is Charm && ((Charm)b).obj == chID)
                    return true;
            }

            return false;
        }

        public virtual void Add(Buff buff)
        {
            if (buffs.Contains(buff) == false)
                buffs.Add(buff);
            Actor.Add(buff);

            if (sprite != null && buff.announced)
            {
                switch (buff.type)
                {
                    case Buff.BuffType.POSITIVE:
                        sprite.ShowStatus(CharSprite.POSITIVE, buff.ToString());
                        break;
                    case Buff.BuffType.NEGATIVE:
                        sprite.ShowStatus(CharSprite.NEGATIVE, buff.ToString());
                        break;
                    case Buff.BuffType.NEUTRAL:
                    default:
                        sprite.ShowStatus(CharSprite.NEUTRAL, buff.ToString());
                        break;
                }
            }
        }

        public virtual void Remove(Buff buff)
        {
            buffs.Remove(buff);
            Actor.Remove(buff);
        }

        //public synchronized void remove(Class<? extends Buff> buffClass)
        //{
        //    for (Buff buff : buffs(buffClass))
        //    {
        //        remove(buff);
        //    }
        //}

        protected override void OnRemove()
        {
            foreach (var buff in buffs.ToArray())
                buff.Detach();
        }

        public virtual void UpdateSpriteState()
        {
            foreach (var buff in buffs)
            {
                buff.Fx(true);
            }
        }

        public virtual float Stealth()
        {
            return 0;
        }

        public virtual void Move(int step)
        {
            if (Dungeon.level.Adjacent(step, pos) && FindBuff<Vertigo>() != null)
            {
                sprite.InterruptMotion();
                int newPos = pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                if (!(Dungeon.level.passable[newPos] || Dungeon.level.avoid[newPos]) || 
                    (properties.Contains(Property.LARGE) && !Dungeon.level.openSpace[pos]) || 
                    Actor.FindChar(newPos) != null)
                {
                    return;
                }
                else
                {
                    sprite.Move(pos, newPos);
                    step = newPos;
                }
            }

            if (Dungeon.level.map[pos] == Terrain.OPEN_DOOR)
                Door.Leave(pos);

            pos = step;

            if (this != Dungeon.hero)
                sprite.visible = Dungeon.level.heroFOV[pos];

            Dungeon.level.OccupyCell(this);
        }

        public int Distance(Character other)
        {
            return Dungeon.level.Distance(pos, other.pos);
        }

        public virtual void OnMotionComplete()
        {
            //Does nothing by default
            //The main actor thread already accounts for motion,
            // so calling next() here isn't necessary (see Actor.process)
        }

        public virtual void OnAttackComplete()
        {
            Next();
        }

        public virtual void OnOperateComplete()
        {
            Next();
        }

        protected HashSet<Type> resistances = new HashSet<Type>();

        //returns percent effectiveness after resistances
        //TODO currently resistances reduce effectiveness by a static 50%, and do not stack.
        public float Resist(Type effect)
        {
            HashSet<Type> resists = new HashSet<Type>(resistances);

            foreach (Property p in Properties())
            {
                resists.UnionWith(p.Resistances());
            }

            foreach (Buff b in Buffs())
            {
                resists.UnionWith(b.Resistances());
            }
            
            float result = 1.0f;
            foreach (var c in resists)
            {
                if (c.IsAssignableFrom(effect))
                    result *= 0.5f;
            }
            
            return result * RingOfElements.Resist(this, effect);
        }

        protected HashSet<Type> immunities = new HashSet<Type>();

        public virtual bool IsImmune(Type effect)
        {
            HashSet<Type> immunes = new HashSet<Type>(immunities);

            foreach (Property p in Properties())
            {
                immunes.UnionWith(p.Immunities());
            }

            foreach (Buff b in Buffs())
            {
                immunes.UnionWith(b.Immunities());
            }

            foreach (var c in immunes)
            {
                if (c.IsAssignableFrom(effect))
                    return true;
            }

            return false;
        }

        //similar to isImmune, but only factors in damage.
        //Is used in AI decision-making
        public virtual bool IsInvulnerable(Type effect)
        {
            return false;
        }

        protected HashSet<Property> properties = new HashSet<Property>();

        public HashSet<Property> Properties()
        {
            return new HashSet<Property>(properties);
        }

        public class Property
        {
            public static readonly Property BOSS = new Property();
            public static readonly Property MINIBOSS = new Property();
            public static readonly Property UNDEAD = new Property();
            public static readonly Property DEMONIC = new Property();
            public static readonly Property INORGANIC = new Property();
            public static readonly Property BLOB_IMMUNE = new Property();
            public static readonly Property FIERY = new Property();
            public static readonly Property ICY = new Property();
            public static readonly Property ACIDIC = new Property();
            public static readonly Property ELECTRIC = new Property();
            public static readonly Property LARGE = new Property();
            public static readonly Property IMMOVABLE = new Property();

            public HashSet<Type> resistances = new HashSet<Type>();
            public HashSet<Type> immunities = new HashSet<Type>();

            public HashSet<Type> Resistances()
            {
                return new HashSet<Type>(resistances);
            }

            public HashSet<Type> Immunities()
            {
                return new HashSet<Type>(immunities);
            }

            static Property()
            {
                {
                    BOSS.resistances.Add(typeof(Grim));
                    BOSS.resistances.Add(typeof(GrimTrap));
                    BOSS.resistances.Add(typeof(ScrollOfRetribution));
                    BOSS.resistances.Add(typeof(ScrollOfPsionicBlast));

                    BOSS.immunities.Add(typeof(Corruption));
                    BOSS.immunities.Add(typeof(StoneOfAggression.Aggression));
                }

                {
                    MINIBOSS.immunities.Add(typeof(Corruption));
                }

                {
                    INORGANIC.immunities.Add(typeof(Bleeding));
                    INORGANIC.immunities.Add(typeof(blobs.ToxicGas));
                    INORGANIC.immunities.Add(typeof(Poison));
                }

                {
                    BLOB_IMMUNE.immunities.Add(typeof(blobs.Blob));
                }

                {
                    FIERY.resistances.Add(typeof(WandOfFireblast));
                    FIERY.resistances.Add(typeof(Elemental.FireElemental));

                    FIERY.immunities.Add(typeof(Burning));
                    FIERY.immunities.Add(typeof(Blazing));
                }

                {
                    ICY.resistances.Add(typeof(WandOfFrost));
                    ICY.resistances.Add(typeof(Elemental.FrostElemental));

                    ICY.immunities.Add(typeof(Frost));
                    ICY.immunities.Add(typeof(Chill));
                }

                {
                    ACIDIC.resistances.Add(typeof(Corrosion));

                    ACIDIC.immunities.Add(typeof(Ooze));
                }

                {
                    ELECTRIC.resistances.Add(typeof(WandOfLightning));
                    ELECTRIC.resistances.Add(typeof(Shocking));
                    ELECTRIC.resistances.Add(typeof(Potential));
                    ELECTRIC.resistances.Add(typeof(blobs.Electricity));
                    ELECTRIC.resistances.Add(typeof(ShockingDart));
                    ELECTRIC.resistances.Add(typeof(Elemental.ShockElemental));
                }
            }
        }
        
        public static bool HasProp(Character ch, Property p)
        {
            return (ch != null && ch.properties.Contains(p));
        }
    }
}