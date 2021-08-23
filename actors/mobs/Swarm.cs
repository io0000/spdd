using System.Collections.Generic;
using watabou.utils;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items;
using spdd.items.potions;
using spdd.scenes;
using spdd.sprites;
using spdd.levels;
using spdd.levels.features;

namespace spdd.actors.mobs
{
    public class Swarm : Mob
    {
        public Swarm()
        {
            spriteClass = typeof(SwarmSprite);

            HP = HT = 50;
            defenseSkill = 5;

            EXP = 3;
            maxLvl = 9;

            flying = true;

            loot = new PotionOfHealing();
            lootChance = 0.1667f; //by default, see rollToDropLoot()
        }

        private const float SplitDelay = 1f;

        int generation;

        private const string GENERATION = "generation";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(GENERATION, generation);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            generation = bundle.GetInt(GENERATION);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(1, 4);
        }

        public override int DefenseProc(Character enemy, int damage)
        {
            if (HP < damage + 2)
                return damage;

            List<int> candidates = new List<int>();
            var solid = Dungeon.level.solid;

            int[] neighbors = { pos + 1, pos - 1, pos + Dungeon.level.Width(), pos - Dungeon.level.Width() };
            foreach (int n in neighbors)
            {
                if (!solid[n] && Actor.FindChar(n) == null)
                {
                    candidates.Add(n);
                }
            }

            if (candidates.Count > 0)
            {
                Swarm clone = Split();
                clone.HP = (HP - damage) / 2;
                clone.pos = Rnd.Element(candidates);
                clone.state = clone.HUNTING;

                if (Dungeon.level.map[clone.pos] == Terrain.DOOR)
                    Door.Enter(clone.pos);

                GameScene.Add(clone, SplitDelay);
                AddDelayed(new Pushing(clone, pos, clone.pos), -1);

                HP -= clone.HP;
            }

            return base.DefenseProc(enemy, damage);
        }

        public override int AttackSkill(Character target)
        {
            return 10;
        }

        private Swarm Split()
        {
            Swarm clone = new Swarm();
            clone.generation = this.generation + 1;
            clone.EXP = 0;
            if (FindBuff<Burning>() != null)
            {
                Buff.Affect<Burning>(clone).Reignite(clone);
            }
            if (FindBuff<Poison>() != null)
            {
                Buff.Affect<Poison>(clone).Set(2);
            }
            if (FindBuff<Corruption>() != null)
            {
                Buff.Affect<Corruption>(clone);
            }
            return clone;
        }

        public override void RollToDropLoot()
        {
            lootChance = 1f / (6 * (generation + 1));
            lootChance *= (5f - Dungeon.LimitedDrops.SWARM_HP.count) / 5f;
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.SWARM_HP.count;
            return base.CreateLoot();
        }
    }
}