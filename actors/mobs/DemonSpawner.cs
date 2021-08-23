using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.buffs;
using spdd.utils;
using spdd.sprites;
using spdd.scenes;
using spdd.items.potions;
using spdd.effects;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class DemonSpawner : Mob
    {
        public DemonSpawner()
        {
            InitInstance();

            spriteClass = typeof(SpawnerSprite);

            HP = HT = 120;
            defenseSkill = 0;

            EXP = 25;
            maxLvl = 29;

            state = PASSIVE;

            loot = typeof(PotionOfHealing);
            lootChance = 1f;

            properties.Add(Property.IMMOVABLE);
            properties.Add(Property.MINIBOSS);
            properties.Add(Property.DEMONIC);
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 12);
        }

        public override void Beckon(int cell)
        {
            //do nothing
        }

        public override bool Reset()
        {
            return true;
        }

        private float spawnCooldown;

        public bool spawnRecorded;

        public override bool Act()
        {
            if (!spawnRecorded)
            {
                ++Statistics.spawnersAlive;
                spawnRecorded = true;
            }

            --spawnCooldown;
            if (spawnCooldown <= 0)
            {
                List<int> candidates = new List<int>();
                foreach (int n in PathFinder.NEIGHBORS8)
                {
                    if (Dungeon.level.passable[pos + n] && Actor.FindChar(pos + n) == null)
                    {
                        candidates.Add(pos + n);
                    }
                }

                if (candidates.Count > 0)
                {
                    RipperDemon spawn = new RipperDemon();

                    spawn.pos = Rnd.Element(candidates);
                    spawn.state = spawn.HUNTING;

                    Dungeon.level.OccupyCell(spawn);

                    GameScene.Add(spawn, 1);
                    if (sprite.visible)
                        Actor.AddDelayed(new Pushing(spawn, pos, spawn.pos), -1);

                    spawnCooldown += 60;
                    if (Dungeon.depth > 21)
                    {
                        //60/53.33/46.67/40 turns to spawn on floor 21/22/23/24
                        spawnCooldown -= Math.Min(20, (Dungeon.depth - 21) * 6.67f);
                    }
                }
            }

            return base.Act();
        }

        public override void Damage(int dmg, object src)
        {
            if (dmg >= 20)
            {
                //takes 20/21/22/23/24/25/26/27/28/29/30 dmg
                // at   20/22/25/29/34/40/47/55/64/74/85 incoming dmg
                dmg = 19 + (int)(Math.Sqrt(8 * (dmg - 19) + 1) - 1) / 2;
            }
            spawnCooldown -= dmg;
            base.Damage(dmg, src);
        }

        public override void Die(object cause)
        {
            if (spawnRecorded)
            {
                --Statistics.spawnersAlive;
            }
            GLog.Highlight(Messages.Get(this, "on_death"));

            base.Die(cause);
        }

        public const string SPAWN_COOLDOWN = "spawn_cooldown";
        public const string SPAWN_RECORDED = "spawn_recorded";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(SPAWN_COOLDOWN, spawnCooldown);
            bundle.Put(SPAWN_RECORDED, spawnRecorded);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            spawnCooldown = bundle.GetFloat(SPAWN_COOLDOWN);
            spawnRecorded = bundle.GetBoolean(SPAWN_RECORDED);
        }

        private void InitInstance()
        {
            immunities.Add(typeof(Paralysis));
            immunities.Add(typeof(Amok));
            immunities.Add(typeof(Sleep));
            immunities.Add(typeof(Terror));
            immunities.Add(typeof(Vertigo));
        }
    }
}