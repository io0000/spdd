using System;
using System.Linq;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items.artifacts;
using spdd.mechanics;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.ui;
using spdd.effects.particles;
using spdd.messages;
using spdd.tiles;

namespace spdd.actors.mobs
{
    public class YogDzewa : Mob
    {
        public YogDzewa()
        {
            InitInstance1();
            InitInstance2();
            InitInstance3();

            spriteClass = typeof(YogSprite);

            HP = HT = 1000;

            EXP = 50;

            //so that allies can attack it. States are never actually used.
            state = HUNTING;

            properties.Add(Property.BOSS);
            properties.Add(Property.IMMOVABLE);
            properties.Add(Property.DEMONIC);
        }

        private int phase;

        private float abilityCooldown;
        private const int MIN_ABILITY_CD = 10;
        private const int MAX_ABILITY_CD = 15;

        private float summonCooldown;
        private const int MIN_SUMMON_CD = 10;
        private const int MAX_SUMMON_CD = 15;

        private List<Type> fistSummons = new List<Type>();

        private void InitInstance1()
        {
            Rnd.PushGenerator(Dungeon.SeedCurDepth());
            fistSummons.Add(Rnd.Int(2) == 0 ? typeof(YogFist.BurningFist) : typeof(YogFist.SoiledFist));
            fistSummons.Add(Rnd.Int(2) == 0 ? typeof(YogFist.RottingFist) : typeof(YogFist.RustedFist));
            fistSummons.Add(Rnd.Int(2) == 0 ? typeof(YogFist.BrightFist) : typeof(YogFist.DarkFist));
            Rnd.Shuffle(fistSummons);
            Rnd.PopGenerator();
        }

        private const int SUMMON_DECK_SIZE = 4;
        private List<Type> regularSummons = new List<Type>();

        private void InitInstance2()
        {
            for (int i = 0; i < SUMMON_DECK_SIZE; ++i)
            {
                if (i >= Statistics.spawnersAlive)
                {
                    regularSummons.Add(typeof(Larva));
                }
                else
                {
                    regularSummons.Add(typeof(YogRipper));
                }
            }
            Rnd.Shuffle(regularSummons);
        }

        private List<int> targetedCells = new List<int>();

        public override bool Act()
        {
            //char logic
            if (fieldOfView == null || fieldOfView.Length != Dungeon.level.Length())
            {
                fieldOfView = new bool[Dungeon.level.Length()];
            }
            Dungeon.level.UpdateFieldOfView(this, fieldOfView);

            ThrowItems();

            //mob logic
            enemy = ChooseEnemy();

            enemySeen = enemy != null && enemy.IsAlive() && fieldOfView[enemy.pos] && enemy.invisible <= 0;
            //end of char/mob logic

            if (phase == 0)
            {
                if (Dungeon.hero.viewDistance >= Dungeon.level.Distance(pos, Dungeon.hero.pos))
                {
                    Dungeon.Observe();
                }
                if (Dungeon.level.heroFOV[pos])
                {
                    Notice();
                }
            }

            if (phase == 4 && FindFist() == null)
            {
                Yell(Messages.Get(this, "hope"));
                summonCooldown = -15; //summon a burst of minions!
                phase = 5;
            }

            if (phase == 0)
            {
                Spend(TICK);
                return true;
            }
            else
            {
                bool terrainAffected = false;
                HashSet<Character> affected = new HashSet<Character>();
                //delay fire on a rooted hero
                if (!Dungeon.hero.rooted)
                {
                    foreach (int i in targetedCells)
                    {
                        Ballistic b = new Ballistic(pos, i, Ballistic.WONT_STOP);
                        //shoot beams
                        sprite.parent.Add(new Beam.DeathRay(sprite.Center(), DungeonTilemap.RaisedTileCenterToWorld(b.collisionPos)));
                        foreach (int p in b.path)
                        {
                            var ch = Actor.FindChar(p);
                            if (ch != null && (ch.alignment != alignment || ch is Bee))
                            {
                                affected.Add(ch);
                            }
                            if (Dungeon.level.flamable[p])
                            {
                                Dungeon.level.Destroy(p);
                                GameScene.UpdateMap(p);
                                terrainAffected = true;
                            }
                        }
                    }

                    if (terrainAffected)
                        Dungeon.Observe();

                    foreach (var ch in affected)
                    {
                        ch.Damage(Rnd.NormalIntRange(20, 30), new Eye.DeathGaze());

                        if (Dungeon.level.heroFOV[pos])
                        {
                            ch.sprite.Flash();
                            CellEmitter.Center(pos).Burst(PurpleParticle.Burst, Rnd.IntRange(1, 2));
                        }

                        if (!ch.IsAlive() && ch == Dungeon.hero)
                        {
                            Dungeon.Fail(GetType());
                            GLog.Negative(Messages.Get(typeof(Character), "kill", Name()));
                        }
                    }
                    targetedCells.Clear();
                }

                if (abilityCooldown <= 0)
                {
                    int beams = 1 + (HT - HP) / 400;
                    HashSet<int> affectedCells = new HashSet<int>();
                    for (int i = 0; i < beams; ++i)
                    {
                        int targetPos = Dungeon.hero.pos;
                        if (i != 0)
                        {
                            do
                            {
                                targetPos = Dungeon.hero.pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                            }
                            while (Dungeon.level.TrueDistance(pos, Dungeon.hero.pos) > Dungeon.level.TrueDistance(pos, targetPos));
                        }
                        targetedCells.Add(targetPos);
                        Ballistic b = new Ballistic(pos, targetPos, Ballistic.WONT_STOP);
                        //affectedCells.addAll(b.path);
                        foreach (int value in b.path)
                            affectedCells.Add(value);
                    }

                    //remove one beam if multiple shots would cause every cell next to the hero to be targeted
                    bool allAdjTargeted = true;
                    foreach (int i in PathFinder.NEIGHBORS9)
                    {
                        if (!affectedCells.Contains(Dungeon.hero.pos + i) && Dungeon.level.passable[Dungeon.hero.pos + i])
                        {
                            allAdjTargeted = false;
                            break;
                        }
                    }
                    if (allAdjTargeted)
                    {
                        targetedCells.Remove(targetedCells.Count - 1);
                    }
                    foreach (int i in targetedCells)
                    {
                        Ballistic b = new Ballistic(pos, i, Ballistic.WONT_STOP);
                        foreach (int p in b.path)
                        {
                            sprite.parent.Add(new TargetedCell(p, new Color(0xFF, 0x00, 0x00, 0xFF)));
                            affectedCells.Add(p);
                        }
                    }

                    //don't want to overly punish players with slow move or attack speed
                    Spend(GameMath.Gate(TICK, Dungeon.hero.Cooldown(), 3 * TICK));
                    Dungeon.hero.Interrupt();

                    abilityCooldown += Rnd.NormalFloat(MIN_ABILITY_CD, MAX_ABILITY_CD);
                    abilityCooldown -= (phase - 1);
                }
                else
                {
                    Spend(TICK);
                }

                while (summonCooldown <= 0)
                {
                    Type cls = regularSummons[0];
                    regularSummons.RemoveAt(0);
                    Mob summon = (Mob)Reflection.NewInstance(cls);
                    regularSummons.Add(cls);

                    int spawnPos = -1;
                    foreach (int i in PathFinder.NEIGHBORS8)
                    {
                        if (Actor.FindChar(pos + i) == null)
                        {
                            if (spawnPos == -1 || Dungeon.level.TrueDistance(Dungeon.hero.pos, spawnPos) > Dungeon.level.TrueDistance(Dungeon.hero.pos, pos + i))
                            {
                                spawnPos = pos + i;
                            }
                        }
                    }

                    if (spawnPos != -1)
                    {
                        summon.pos = spawnPos;
                        GameScene.Add(summon);
                        Actor.AddDelayed(new Pushing(summon, pos, summon.pos), -1);
                        summon.Beckon(Dungeon.hero.pos);

                        summonCooldown += Rnd.NormalFloat(MIN_SUMMON_CD, MAX_SUMMON_CD);
                        summonCooldown -= (phase - 1);
                        if (FindFist() != null)
                        {
                            summonCooldown += MIN_SUMMON_CD - (phase - 1);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (summonCooldown > 0)
                --summonCooldown;
            if (abilityCooldown > 0)
                --abilityCooldown;

            //extra fast abilities and summons at the final 100 HP
            if (phase == 5 && abilityCooldown > 2)
            {
                abilityCooldown = 2;
            }
            if (phase == 5 && summonCooldown > 3)
            {
                summonCooldown = 3;
            }

            return true;
        }

        public override bool IsAlive()
        {
            return base.IsAlive() || phase != 5;
        }

        public override bool IsInvulnerable(Type effect)
        {
            return phase == 0 || FindFist() != null;
        }

        public override void Damage(int dmg, object src)
        {
            int preHP = HP;
            base.Damage(dmg, src);

            if (phase == 0 || FindFist() != null)
                return;

            if (phase < 4)
            {
                HP = Math.Max(HP, HT - 300 * phase);
            }
            else if (phase == 4)
            {
                HP = Math.Max(HP, 100);
            }
            int dmgTaken = preHP - HP;

            if (dmgTaken > 0)
            {
                abilityCooldown -= dmgTaken / 10f;
                summonCooldown -= dmgTaken / 10f;
            }

            if (phase < 4 && HP <= HT - 300 * phase)
            {
                Dungeon.level.viewDistance = Math.Max(1, Dungeon.level.viewDistance - 1);
                if (Dungeon.hero.FindBuff<Light>() == null)
                {
                    Dungeon.hero.viewDistance = Dungeon.level.viewDistance;
                }
                Dungeon.Observe();
                GLog.Negative(Messages.Get(this, "darkness"));
                sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "invulnerable"));

                YogFist fist = (YogFist)Reflection.NewInstance(fistSummons[0]);
                fistSummons.RemoveAt(0);

                fist.pos = Dungeon.level.exit;

                CellEmitter.Get(Dungeon.level.exit - 1).Burst(ShadowParticle.Up, 25);
                CellEmitter.Get(Dungeon.level.exit).Burst(ShadowParticle.Up, 100);
                CellEmitter.Get(Dungeon.level.exit + 1).Burst(ShadowParticle.Up, 25);

                if (abilityCooldown < 5) abilityCooldown = 5;
                if (summonCooldown < 5) summonCooldown = 5;

                int targetPos = Dungeon.level.exit + Dungeon.level.Width();
                if (Actor.FindChar(targetPos) == null)
                {
                    fist.pos = targetPos;
                }
                else if (Actor.FindChar(targetPos - 1) == null)
                {
                    fist.pos = targetPos - 1;
                }
                else if (Actor.FindChar(targetPos + 1) == null)
                {
                    fist.pos = targetPos + 1;
                }

                GameScene.Add(fist, 4);
                Actor.AddDelayed(new Pushing(fist, Dungeon.level.exit, fist.pos), -1);
                ++phase;
            }

            LockedFloor lockedFloor = Dungeon.hero.FindBuff<LockedFloor>();
            if (lockedFloor != null)
                lockedFloor.AddTime(dmgTaken);
        }

        private YogFist FindFist()
        {
            foreach (var c in Actor.Chars())
            {
                if (c is YogFist)
                {
                    return (YogFist)c;
                }
            }
            return null;
        }

        public override void Beckon(int cell)
        { }

        public override void Aggro(Character ch)
        {
            foreach (Mob mob in Dungeon.level.mobs.ToArray())
            {
                if (Dungeon.level.Distance(pos, mob.pos) <= 4 &&
                        (mob is Larva || mob is RipperDemon))
                {
                    mob.Aggro(ch);
                }
            }
        }

        public override void Die(object cause)
        {
            foreach (var mob in Dungeon.level.mobs.ToArray())
            {
                if (mob is Larva || mob is RipperDemon)
                    mob.Die(cause);
            }

            Dungeon.level.viewDistance = 4;
            if (Dungeon.hero.FindBuff<Light>() == null)
                Dungeon.hero.viewDistance = Dungeon.level.viewDistance;

            GameScene.BossSlain();
            Dungeon.level.Unseal();
            base.Die(cause);

            Yell(Messages.Get(this, "defeated"));
        }

        public override void Notice()
        {
            base.Notice();
            if (!BossHealthBar.IsAssigned())
            {
                BossHealthBar.AssignBoss(this);
                Yell(Messages.Get(this, "notice"));
                foreach (var ch in Actor.Chars())
                {
                    if (ch is DriedRose.GhostHero)
                        ((DriedRose.GhostHero)ch).SayBoss();
                }

                if (phase == 0)
                {
                    phase = 1;
                    summonCooldown = Rnd.NormalFloat(MIN_SUMMON_CD, MAX_SUMMON_CD);
                    abilityCooldown = Rnd.NormalFloat(MIN_ABILITY_CD, MAX_ABILITY_CD);
                }
            }
        }

        public override string Description()
        {
            string desc = base.Description();

            if (Statistics.spawnersAlive > 0)
            {
                desc += "\n\n" + Messages.Get(this, "desc_spawners");
            }

            return desc;
        }

        private void InitInstance3()
        {
            immunities.Add(typeof(Terror));
            immunities.Add(typeof(Amok));
            immunities.Add(typeof(Charm));
            immunities.Add(typeof(Sleep));
            immunities.Add(typeof(Vertigo));
            immunities.Add(typeof(Frost));
            immunities.Add(typeof(buffs.Paralysis));
        }

        private const string PHASE = "phase";

        private const string ABILITY_CD = "ability_cd";
        private const string SUMMON_CD = "summon_cd";

        private const string FIST_SUMMONS = "fist_summons";
        private const string REGULAR_SUMMONS = "regular_summons";

        private const string TARGETED_CELLS = "targeted_cells";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(PHASE, phase);

            bundle.Put(ABILITY_CD, abilityCooldown);
            bundle.Put(SUMMON_CD, summonCooldown);

            bundle.Put(FIST_SUMMONS, fistSummons.ToArray());
            bundle.Put(REGULAR_SUMMONS, regularSummons.ToArray());

            int[] bundleArr = new int[targetedCells.Count];
            for (int i = 0; i < targetedCells.Count; ++i)
            {
                bundleArr[i] = targetedCells[i];
            }
            bundle.Put(TARGETED_CELLS, bundleArr);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            phase = bundle.GetInt(PHASE);
            if (phase != 0)
                BossHealthBar.AssignBoss(this);

            abilityCooldown = bundle.GetFloat(ABILITY_CD);
            summonCooldown = bundle.GetFloat(SUMMON_CD);

            fistSummons.Clear();
            var arr1 = bundle.GetClassArray(FIST_SUMMONS);
            fistSummons = arr1.ToList();

            regularSummons.Clear();
            var arr2 = bundle.GetClassArray(REGULAR_SUMMONS);
            regularSummons = arr2.ToList();

            foreach (int i in bundle.GetIntArray(TARGETED_CELLS))
            {
                targetedCells.Add(i);
            }
        }

        [SPDStatic]
        public class Larva : Mob
        {
            public Larva()
            {
                spriteClass = typeof(LarvaSprite);

                HP = HT = 20;
                defenseSkill = 20;
                viewDistance = Light.DISTANCE;

                EXP = 5;
                maxLvl = -2;

                properties.Add(Property.DEMONIC);
            }

            public override int AttackSkill(Character target)
            {
                return 30;
            }

            public override int DamageRoll()
            {
                return Rnd.NormalIntRange(15, 25);
            }

            public override int DrRoll()
            {
                return Rnd.NormalIntRange(0, 4);
            }

        }

        //used so death to yog's ripper demons have their own rankings description and are more aggro
        [SPDStatic]
        public class YogRipper : RipperDemon
        { }
    }
}