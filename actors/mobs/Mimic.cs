using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.items;
using spdd.items.artifacts;
using spdd.utils;
using spdd.effects;
using spdd.plants;
using spdd.actors.buffs;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Mimic : Mob
    {
        private int level;

        public Mimic()
        {
            spriteClass = typeof(MimicSprite);

            properties.Add(Property.DEMONIC);

            EXP = 0;

            //mimics are neutral when hidden
            alignment = Alignment.NEUTRAL;
            state = PASSIVE;
        }

        public List<Item> items;

        private const string LEVEL = "level";
        private const string ITEMS = "items";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            if (items != null)
                bundle.Put(ITEMS, items);
            bundle.Put(LEVEL, level);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            if (bundle.Contains(ITEMS))
            {
                items = new List<Item>();
                foreach (var item in bundle.GetCollection(ITEMS))
                {
                    items.Add((Item)item);
                }
            }

            level = bundle.GetInt(LEVEL);
            AdjustStats(level);

            base.RestoreFromBundle(bundle);

            if (state != PASSIVE && alignment == Alignment.NEUTRAL)
            {
                alignment = Alignment.ENEMY;
            }
        }

        public override void Add(Buff buff)
        {
            base.Add(buff);
            if (buff.type == Buff.BuffType.NEGATIVE && alignment == Alignment.NEUTRAL)
            {
                alignment = Alignment.ENEMY;
                StopHiding();
                if (sprite != null)
                    sprite.Idle();
            }
        }

        public override string Name()
        {
            if (alignment == Alignment.NEUTRAL)
            {
                return Messages.Get(typeof(Heap), "chest");
            }
            else
            {
                return base.Name();
            }
        }

        public override string Description()
        {
            if (alignment == Alignment.NEUTRAL)
            {
                return Messages.Get(typeof(Heap), "chest_desc") + 
                    "\n\n" +
                    Messages.Get(this, "hidden_hint");
            }
            else
            {
                return base.Description();
            }
        }

        public override bool Act()
        {
            if (alignment == Alignment.NEUTRAL && state != PASSIVE)
            {
                alignment = Alignment.ENEMY;
                GLog.Warning(Messages.Get(this, "reveal"));
                CellEmitter.Get(pos).Burst(Speck.Factory(Speck.STAR), 10);
                Sample.Instance.Play(Assets.Sounds.MIMIC);
            }
            return base.Act();
        }

        public override CharSprite GetSprite()
        {
            var sprite = (MimicSprite)base.GetSprite();
            if (alignment == Alignment.NEUTRAL)
                sprite.HideMimic();
            return sprite;
        }

        public override bool Interact(Character c)
        {
            if (alignment != Alignment.NEUTRAL || c != Dungeon.hero)
            {
                return base.Interact(c);
            }

            StopHiding();

            Dungeon.hero.Busy();
            Dungeon.hero.sprite.Operate(pos);
            if (Dungeon.hero.invisible <= 0 &&
                Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>() == null &&
                Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>() == null)
            {
                return DoAttack(Dungeon.hero);
            }
            else
            {
                sprite.Idle();
                alignment = Alignment.ENEMY;
                Dungeon.hero.SpendAndNext(1f);
                return true;
            }
        }

        public override void OnAttackComplete()
        {
            base.OnAttackComplete();

            if (alignment == Alignment.NEUTRAL)
            {
                alignment = Alignment.ENEMY;
                Dungeon.hero.SpendAndNext(1f);
            }
        }

        public override void Damage(int dmg, object src)
        {
            if (state == PASSIVE)
            {
                alignment = Alignment.ENEMY;
                StopHiding();
            }
            base.Damage(dmg, src);
        }

        public virtual void StopHiding()
        {
            state = HUNTING;
            if (Actor.Chars().Contains(this) && Dungeon.level.heroFOV[pos])
            {
                enemy = Dungeon.hero;
                target = Dungeon.hero.pos;
                enemySeen = true;
                GLog.Warning(Messages.Get(this, "reveal"));
                CellEmitter.Get(pos).Burst(Speck.Factory(Speck.STAR), 10);
                Sample.Instance.Play(Assets.Sounds.MIMIC);
            }
        }

        public override int DamageRoll()
        {
            if (alignment == Alignment.NEUTRAL)
            {
                return Rnd.NormalIntRange(2 + 2 * level, 2 + 2 * level);
            }
            else
            {
                return Rnd.NormalIntRange(1 + level, 2 + 2 * level);
            }
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 1 + level / 2);
        }

        public override void Beckon(int cell)
        {
            // Do nothing
        }

        public override int AttackSkill(Character target)
        {
            if (target != null && alignment == Alignment.NEUTRAL && target.invisible <= 0)
            {
                return INFINITE_ACCURACY;
            }
            else
            {
                return 6 + level;
            }
        }

        public virtual void SetLevel(int level)
        {
            this.level = level;
            AdjustStats(level);
        }

        public void AdjustStats(int level)
        {
            HP = HT = (1 + level) * 6;
            defenseSkill = 2 + level / 2;

            enemySeen = true;
        }

        public override void RollToDropLoot()
        {
            if (items != null)
            {
                foreach (Item item in items)
                {
                    Dungeon.level.Drop(item, pos).sprite.Drop();
                }
                items = null;
            }
            base.RollToDropLoot();
        }

        public override float SpawningWeight()
        {
            return 0f;
        }

        public override bool Reset()
        {
            if (state != PASSIVE)
                state = WANDERING;
            return true;
        }

        public static Mimic SpawnAt(int pos, Item item)
        {
            var list = new List<Item>();
            list.Add(item);

            return SpawnAt(pos, list, typeof(Mimic));
        }

        public static Mimic SpawnAt(int pos, Item item, Type mimicType)
        {
            var list = new List<Item>();
            list.Add(item);

            return SpawnAt(pos, list, mimicType);
        }

        public static Mimic SpawnAt(int pos, List<Item> items)
        {
            return SpawnAt(pos, items, typeof(Mimic));
        }

        public static Mimic SpawnAt(int pos, List<Item> items, Type mimicType)
        {
            Mimic m;

            if (mimicType == typeof(GoldenMimic))
            {
                m = new GoldenMimic();
            }
            else if (mimicType == typeof(CrystalMimic))
            {
                m = new CrystalMimic();
            }
            else
            {
                m = new Mimic();
            }

            m.items = new List<Item>(items);
            m.SetLevel(Dungeon.depth);
            m.pos = pos;

            //generate an extra reward for killing the mimic
            m.GeneratePrize();

            return m;
        }

        protected virtual void GeneratePrize()
        {
            Item reward = null;
            do
            {
                switch (Rnd.Int(5))
                {
                    case 0:
                        reward = new Gold().Random();
                        break;
                    case 1:
                        reward = Generator.RandomMissile();
                        break;
                    case 2:
                        reward = Generator.RandomArmor();
                        break;
                    case 3:
                        reward = Generator.RandomWeapon();
                        break;
                    case 4:
                        reward = Generator.Random(Generator.Category.RING);
                        break;
                }
            } while (reward == null || Challenges.IsItemBlocked(reward));

            items.Add(reward);
        }
    }
}