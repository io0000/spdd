using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors.blobs;
using spdd.actors.hero;
using spdd.effects;
using spdd.ui;
using spdd.items;
using spdd.utils;
using spdd.items.bombs;
using spdd.items.artifacts;
using spdd.levels;
using spdd.mechanics;
using spdd.scenes;
using spdd.sprites;
using spdd.actors.buffs;
using spdd.effects.particles;
using spdd.messages;
using spdd.tiles;

namespace spdd.actors.mobs
{
    public class NewTengu : Mob
    {
        public NewTengu()
        {
            InitInstance();

            spriteClass = typeof(TenguSprite);

            HP = HT = 160;
            EXP = 20;
            defenseSkill = 15;

            HUNTING = new NewTenguHunting(this);

            flying = true; //doesn't literally fly, but he is fleet-of-foot enough to avoid hazards

            properties.Add(Property.BOSS);

            viewDistance = 12;
        }

        protected override void OnAdd()
        {
            //when he's removed and re-added to the fight, his time is always set to now.
            Spend(-Cooldown());
            base.OnAdd();
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(6, 12);
        }

        public override int AttackSkill(Character target)
        {
            if (Dungeon.level.Adjacent(pos, target.pos))
            {
                return 12;
            }
            else
            {
                return 18;
            }
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 5);
        }

        //Tengu is immune to debuffs and damage when removed from the level
        public override void Add(Buff buff)
        {
            if (Actor.Chars().Contains(this) || buff is Doom)
                base.Add(buff);
        }

        public override void Damage(int dmg, object src)
        {
            if (!Dungeon.level.mobs.Contains(this))
                return;

            NewPrisonBossLevel.State state = ((NewPrisonBossLevel)Dungeon.level).GetState();

            int hpBracket = 20;

            int beforeHitHP = HP;
            base.Damage(dmg, src);
            dmg = beforeHitHP - HP;

            //tengu cannot be hit through multiple brackets at a time
            if ((beforeHitHP / hpBracket - HP / hpBracket) >= 2)
            {
                HP = hpBracket * ((beforeHitHP / hpBracket) - 1) + 1;
            }

            LockedFloor lockedFloor = Dungeon.hero.FindBuff<LockedFloor>();
            if (lockedFloor != null)
            {
                int multiple = state == NewPrisonBossLevel.State.FIGHT_START ? 1 : 4;
                lockedFloor.AddTime(dmg * multiple);
            }

            //phase 2 of the fight is over
            if (HP == 0 && state == NewPrisonBossLevel.State.FIGHT_ARENA)
            {
                //let full attack action complete first
                Actor.Add(new NewTenguActor());
                return;
            }

            //phase 1 of the fight is over
            if (state == NewPrisonBossLevel.State.FIGHT_START && HP <= HT / 2)
            {
                HP = (HT / 2);
                Yell(Messages.Get(this, "interesting"));
                ((NewPrisonBossLevel)Dungeon.level).Progress();
                BossHealthBar.Bleed(true);

                //if tengu has lost a certain amount of hp, jump
            }
            else if (beforeHitHP / hpBracket != HP / hpBracket)
            {
                Jump();
            }
        }

        private class NewTenguActor : Actor
        {
            public NewTenguActor()
            {
                actPriority = VFX_PRIO;
            }

            public override bool Act()
            {
                Actor.Remove(this);
                ((NewPrisonBossLevel)Dungeon.level).Progress();
                return true;
            }
        }

        public override bool IsAlive()
        {
            return HP > 0 || Dungeon.level.mobs.Contains(this); //Tengu has special death rules, see prisonbosslevel.progress()
        }

        public override void Die(object cause)
        {
            if (Dungeon.hero.subClass == HeroSubClass.NONE)
                Dungeon.level.Drop(new TomeOfMastery(), pos).sprite.Drop();

            GameScene.BossSlain();
            base.Die(cause);

            BadgesExtensions.ValidateBossSlain();

            var beacon = Dungeon.hero.belongings.GetItem<items.artifacts.LloydsBeacon>();
            if (beacon != null)
                beacon.Upgrade();

            Yell(Messages.Get(this, "defeated"));
        }

        protected override bool CanAttack(Character enemy)
        {
            return new Ballistic(pos, enemy.pos, Ballistic.PROJECTILE).collisionPos == enemy.pos;
        }

        //tengu's attack is always visible
        protected override bool DoAttack(Character enemy)
        {
            sprite.Attack(enemy.pos);
            Spend(AttackDelay());
            return false;
        }

        private void Jump()
        {
            //in case tengu hasn't had a chance to act yet
            if (fieldOfView == null || fieldOfView.Length != Dungeon.level.Length())
            {
                fieldOfView = new bool[Dungeon.level.Length()];
                Dungeon.level.UpdateFieldOfView(this, fieldOfView);
            }

            if (enemy == null)
                enemy = ChooseEnemy();

            if (enemy == null)
                return;

            int newPos;
            if (Dungeon.level is NewPrisonBossLevel)
            {
                NewPrisonBossLevel level = (NewPrisonBossLevel)Dungeon.level;

                //if we're in phase 1, want to warp around within the room
                if (level.GetState() == NewPrisonBossLevel.State.FIGHT_START)
                {
                    level.CleanTenguCell();

                    int tries = 100;
                    do
                    {
                        newPos = level.RandomTenguCellPos();
                        --tries;
                    }
                    while (tries > 0 && (level.TrueDistance(newPos, enemy.pos) <= 3.5f ||
                                           level.TrueDistance(newPos, Dungeon.hero.pos) <= 3.5f ||
                                           Actor.FindChar(newPos) != null));

                    if (tries <= 0)
                        newPos = pos;

                    if (level.heroFOV[pos])
                        CellEmitter.Get(pos).Burst(Speck.Factory(Speck.WOOL), 6);

                    sprite.Move(pos, newPos);
                    Move(newPos);

                    if (level.heroFOV[newPos])
                        CellEmitter.Get(newPos).Burst(Speck.Factory(Speck.WOOL), 6);

                    Sample.Instance.Play(Assets.Sounds.PUFF);

                    float fill = 0.9f - 0.5f * ((HP - 80) / 80f);
                    level.PlaceTrapsInTenguCell(fill);
                }
                else
                {
                    //otherwise, jump in a larger possible area, as the room is bigger
                    int tries = 100;
                    do
                    {
                        newPos = Rnd.Int(level.Length());
                        --tries;
                    }
                    while (tries > 0 &&
                            (level.solid[newPos] ||
                                    level.Distance(newPos, enemy.pos) < 5 ||
                                    level.Distance(newPos, enemy.pos) > 7 ||
                                    level.Distance(newPos, Dungeon.hero.pos) < 5 ||
                                    level.Distance(newPos, Dungeon.hero.pos) > 7 ||
                                    level.Distance(newPos, pos) < 5 ||
                                    Actor.FindChar(newPos) != null ||
                                    level.heaps[newPos] != null));

                    if (tries <= 0)
                        newPos = pos;

                    if (level.heroFOV[pos])
                        CellEmitter.Get(pos).Burst(Speck.Factory(Speck.WOOL), 6);

                    sprite.Move(pos, newPos);
                    Move(newPos);

                    if (arenaJumps < 4)
                        ++arenaJumps;

                    if (level.heroFOV[newPos])
                        CellEmitter.Get(newPos).Burst(Speck.Factory(Speck.WOOL), 6);
                    Sample.Instance.Play(Assets.Sounds.PUFF);
                }

                //if we're on another type of level
            }
            else
            {
                Level level = Dungeon.level;

                newPos = level.RandomRespawnCell(this);

                if (level.heroFOV[pos])
                    CellEmitter.Get(pos).Burst(Speck.Factory(Speck.WOOL), 6);

                sprite.Move(pos, newPos);
                Move(newPos);

                if (level.heroFOV[newPos])
                    CellEmitter.Get(newPos).Burst(Speck.Factory(Speck.WOOL), 6);
                Sample.Instance.Play(Assets.Sounds.PUFF);
            }
        }

        public override void Notice()
        {
            base.Notice();
            if (!BossHealthBar.IsAssigned())
            {
                BossHealthBar.AssignBoss(this);
                if (HP <= HT / 2)
                    BossHealthBar.Bleed(true);

                if (HP == HT)
                {
                    Yell(Messages.Get(this, "notice_gotcha", Dungeon.hero.Name()));
                    foreach (var ch in Actor.Chars())
                    {
                        if (ch is DriedRose.GhostHero)
                            ((DriedRose.GhostHero)ch).SayBoss();
                    }
                }
                else
                {
                    Yell(Messages.Get(this, "notice_have", Dungeon.hero.Name()));
                }
            }
        }

        private void InitInstance()
        {
            immunities.Add(typeof(Blindness));
            immunities.Add(typeof(Terror));
        }

        private const string LAST_ABILITY = "last_ability";
        private const string ABILITIES_USED = "abilities_used";
        private const string ARENA_JUMPS = "arena_jumps";
        private const string ABILITY_COOLDOWN = "ability_cooldown";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LAST_ABILITY, lastAbility);
            bundle.Put(ABILITIES_USED, abilitiesUsed);
            bundle.Put(ARENA_JUMPS, arenaJumps);
            bundle.Put(ABILITY_COOLDOWN, abilityCooldown);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            lastAbility = bundle.GetInt(LAST_ABILITY);
            abilitiesUsed = bundle.GetInt(ABILITIES_USED);
            arenaJumps = bundle.GetInt(ARENA_JUMPS);
            abilityCooldown = bundle.GetInt(ABILITY_COOLDOWN);

            BossHealthBar.AssignBoss(this);
            if (HP <= HT / 2)
                BossHealthBar.Bleed(true);
        }

        //don't bother bundling this, as its purely cosmetic
        //private bool yelledCoward = false;

        //tengu is always hunting
        private class NewTenguHunting : Mob.Hunting
        {
            public NewTenguHunting(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                NewTengu t = (NewTengu)mob;
                return t.NewTenguHuntingAct(enemyInFOV, justAlerted);
            }
        }

        public bool NewTenguHuntingAct(bool enemyInFOV, bool justAlerted)
        {
            enemySeen = enemyInFOV;
            if (enemyInFOV && !IsCharmedBy(enemy) && CanAttack(enemy))
            {
                if (CanUseAbility())
                    return UseAbility();

                return DoAttack(enemy);
            }
            else
            {
                if (enemyInFOV)
                {
                    target = enemy.pos;
                }
                else
                {
                    ChooseEnemy();
                    if (enemy == null)
                    {
                        //if nothing else can be targeted, target hero
                        enemy = Dungeon.hero;
                    }
                    target = enemy.pos;
                }

                //if not charmed, attempt to use an ability, even if the enemy can't be seen
                if (CanUseAbility())
                {
                    return UseAbility();
                }

                Spend(TICK);
                return true;
            }
        }

        //*****************************************************************************************
        //***** Tengu abilities. These are expressed in game logic as buffs, blobs, and items *****
        //*****************************************************************************************

        //so that mobs can also use this
        private static Character throwingChar;

        private int lastAbility = -1;
        private int abilitiesUsed;
        private int arenaJumps;

        //starts at 2, so one turn and then first ability
        private int abilityCooldown = 2;

        private const int BOMB_ABILITY = 0;
        private const int FIRE_ABILITY = 1;
        private const int SHOCKER_ABILITY = 2;

        //expects to be called once per turn;
        public bool CanUseAbility()
        {
            if (HP > HT / 2)
                return false;

            if (abilitiesUsed >= TargetAbilityUses())
            {
                return false;
            }
            else
            {
                --abilityCooldown;

                if (TargetAbilityUses() - abilitiesUsed >= 4)
                {
                    //Very behind in ability uses, use one right away!
                    abilityCooldown = 0;
                }
                else if (TargetAbilityUses() - abilitiesUsed >= 3)
                {
                    //moderately behind in uses, use one every other action.
                    if (abilityCooldown == -1 || abilityCooldown > 1)
                        abilityCooldown = 1;
                }
                else
                {
                    //standard delay before ability use, 1-4 turns
                    if (abilityCooldown == -1)
                        abilityCooldown = Rnd.IntRange(1, 4);
                }

                if (abilityCooldown == 0)
                    return true;
                else
                    return false;
            }
        }

        private int TargetAbilityUses()
        {
            //1 base ability use, plus 2 uses per jump
            int targetAbilityUses = 1 + 2 * arenaJumps;

            //and ane extra 2 use for jumps 3 and 4
            targetAbilityUses += Math.Max(0, arenaJumps - 2);

            return targetAbilityUses;
        }

        public bool UseAbility()
        {
            bool abilityUsed = false;
            int abilityToUse = -1;

            while (!abilityUsed)
            {
                if (abilitiesUsed == 0)
                {
                    abilityToUse = BOMB_ABILITY;
                }
                else if (abilitiesUsed == 1)
                {
                    abilityToUse = SHOCKER_ABILITY;
                }
                else
                {
                    abilityToUse = Rnd.Int(3);
                }

                //If we roll the same ability as last time, 9/10 chance to reroll
                if (abilityToUse != lastAbility || Rnd.Int(10) == 0)
                {
                    switch (abilityToUse)
                    {
                        case BOMB_ABILITY:
                        default:
                            abilityUsed = ThrowBomb(this, enemy);
                            //if Tengu cannot use his bomb ability first, use fire instead.
                            if (abilitiesUsed == 0 && !abilityUsed)
                            {
                                abilityToUse = FIRE_ABILITY;
                                abilityUsed = ThrowFire(this, enemy);
                            }
                            break;
                        case FIRE_ABILITY:
                            abilityUsed = ThrowFire(this, enemy);
                            break;
                        case SHOCKER_ABILITY:
                            abilityUsed = ThrowShocker(this, enemy);
                            //if Tengu cannot use his shocker ability second, use fire instead.
                            if (abilitiesUsed == 1 && !abilityUsed)
                            {
                                abilityToUse = FIRE_ABILITY;
                                abilityUsed = ThrowFire(this, enemy);
                            }
                            break;
                    }
                }

            }

            //spend only 1 turn if seriously behind on ability uses
            if (TargetAbilityUses() - abilitiesUsed >= 4)
            {
                Spend(TICK);
            }
            else
            {
                Spend(2 * TICK);
            }

            lastAbility = abilityToUse;
            ++abilitiesUsed;
            return lastAbility == FIRE_ABILITY;
        }

        //******************
        //***Bomb Ability***
        //******************

        //returns true if bomb was thrown
        public static bool ThrowBomb(Character thrower, Character target)
        {
            int targetCell = -1;

            //Targets closest cell which is adjacent to target, and at least 3 tiles away
            foreach (int i in PathFinder.NEIGHBORS8)
            {
                int cell = target.pos + i;
                if (Dungeon.level.Distance(cell, thrower.pos) >= 3 && !Dungeon.level.solid[cell])
                {
                    if (targetCell == -1 ||
                           Dungeon.level.TrueDistance(cell, thrower.pos) < Dungeon.level.TrueDistance(targetCell, thrower.pos))
                    {
                        targetCell = cell;
                    }
                }
            }

            if (targetCell == -1)
            {
                return false;
            }

            int finalTargetCell = targetCell;
            throwingChar = thrower;
            BombAbility.BombItem item = new BombAbility.BombItem();
            thrower.sprite.Zap(finalTargetCell);
            ((MissileSprite)thrower.sprite.parent.Recycle<MissileSprite>()).
                Reset(thrower.sprite,
                        finalTargetCell,
                        item,
                        new NewTenguBombCallback(item, finalTargetCell, thrower));
            return true;
        }

        public class NewTenguBombCallback : ICallback
        {
            BombAbility.BombItem item;
            int finalTargetCell;
            Character thrower;

            public NewTenguBombCallback(BombAbility.BombItem item, int finalTargetCell, Character thrower)
            {
                this.item = item;
                this.finalTargetCell = finalTargetCell;
                this.thrower = thrower;
            }

            public void Call()
            {
                this.item.OnThrow(this.finalTargetCell);
                this.thrower.Next();
            }
        }

        [SPDStatic]
        public class BombAbility : Buff
        {
            public int bombPos;
            private int timer = 3;

            public override bool Act()
            {
                PointF p = DungeonTilemap.RaisedTileCenterToWorld(bombPos);
                if (timer == 3)
                {
                    FloatingText.Show(p.x, p.y, bombPos, "3...", CharSprite.NEUTRAL);
                    PathFinder.BuildDistanceMap(bombPos, BArray.Not(Dungeon.level.solid, null), 2);
                    for (int i = 0; i < PathFinder.distance.Length; ++i)
                    {
                        if (PathFinder.distance[i] < int.MaxValue)
                        {
                            GameScene.Add(Blob.Seed(i, 4, typeof(BombBlob)));
                        }
                    }
                }
                else if (timer == 2)
                {
                    FloatingText.Show(p.x, p.y, bombPos, "2...", CharSprite.WARNING);
                }
                else if (timer == 1)
                {
                    FloatingText.Show(p.x, p.y, bombPos, "1...", CharSprite.NEGATIVE);
                }
                else
                {
                    Heap h = Dungeon.level.heaps[bombPos];
                    if (h != null)
                    {
                        foreach (Item i in h.items.ToArray())
                        {
                            if (i is BombItem)
                                h.Remove(i);
                        }
                    }
                    Detach();
                    return true;
                }

                --timer;
                Spend(TICK);
                return true;
            }

            private const string BOMB_POS = "bomb_pos";
            private const string TIMER = "timer";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(BOMB_POS, bombPos);
                bundle.Put(TIMER, timer);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                bombPos = bundle.GetInt(BOMB_POS);
                timer = bundle.GetInt(TIMER);
            }

            [SPDStatic]
            public class BombBlob : Blob
            {
                public BombBlob()
                {
                    actPriority = BUFF_PRIO - 1;
                    alwaysVisible = true;
                }

                protected override void Evolve()
                {
                    bool exploded = false;

                    int cell;
                    for (int i = area.left; i < area.right; ++i)
                    {
                        for (int j = area.top; j < area.bottom; ++j)
                        {
                            cell = i + j * Dungeon.level.Width();
                            off[cell] = cur[cell] > 0 ? cur[cell] - 1 : 0;

                            if (off[cell] > 0)
                            {
                                volume += off[cell];
                            }

                            if (cur[cell] > 0 && off[cell] == 0)
                            {
                                var ch = Actor.FindChar(cell);
                                if (ch != null && !(ch is NewTengu))
                                {
                                    int dmg = Rnd.NormalIntRange(5 + Dungeon.depth, 10 + Dungeon.depth * 2);
                                    dmg -= ch.DrRoll();

                                    if (dmg > 0)
                                    {
                                        ch.Damage(dmg, typeof(Bomb));
                                    }

                                    if (ch == Dungeon.hero && !ch.IsAlive())
                                    {
                                        Dungeon.Fail(typeof(NewTengu));
                                    }
                                }

                                Heap h = Dungeon.level.heaps[cell];
                                if (h != null)
                                {
                                    foreach (Item it in h.items.ToArray())
                                    {
                                        if (it is BombItem)
                                        {
                                            h.Remove(it);
                                        }
                                    }
                                }

                                exploded = true;
                                CellEmitter.Center(cell).Burst(BlastParticle.Factory, 2);
                            }
                        }
                    }

                    if (exploded)
                    {
                        Sample.Instance.Play(Assets.Sounds.BLAST);
                    }

                } //  Evolve()

                public override void Use(BlobEmitter emitter)
                {
                    base.Use(emitter);

                    emitter.Pour(SmokeParticle.Factory, 0.25f);
                }

                public override string TileDesc()
                {
                    return Messages.Get(this, "desc");
                }
            }

            [SPDStatic]
            public class BombItem : Item
            {
                public BombItem()
                {
                    dropsDownHeap = true;
                    unique = true;

                    image = ItemSpriteSheet.TENGU_BOMB;
                }

                public override bool DoPickUp(Hero hero)
                {
                    GLog.Warning(Messages.Get(this, "cant_pickup"));
                    return false;
                }

                public override void OnThrow(int cell)
                {
                    base.OnThrow(cell);
                    if (throwingChar != null)
                    {
                        Buff.Append<BombAbility>(throwingChar).bombPos = cell;
                        throwingChar = null;
                    }
                    else
                    {
                        Buff.Append<BombAbility>(curUser).bombPos = cell;
                    }
                }

                public override Emitter Emitter()
                {
                    Emitter emitter = new Emitter();
                    emitter.Pos(7.5f, 3.5f);
                    emitter.fillTarget = false;
                    emitter.Pour(SmokeParticle.Spew, 0.05f);
                    return emitter;
                }
            }
        }

        //******************
        //***Fire Ability***
        //******************

        public static bool ThrowFire(Character thrower, Character target)
        {
            Ballistic aim = new Ballistic(thrower.pos, target.pos, Ballistic.WONT_STOP);

            for (int i = 0; i < PathFinder.CIRCLE8.Length; ++i)
            {
                if (aim.sourcePos + PathFinder.CIRCLE8[i] == aim.path[1])
                {
                    thrower.sprite.Zap(target.pos);
                    Buff.Append<NewTengu.FireAbility>(thrower).direction = i;

                    thrower.sprite.Emitter().Start(Speck.Factory(Speck.STEAM), .03f, 10);
                    return true;
                }
            }

            return false;
        }

        [SPDStatic]
        public class FireAbility : Buff
        {
            public int direction;
            private int[] curCells;

            HashSet<int> toCells = new HashSet<int>();

            public override bool Act()
            {
                toCells.Clear();

                if (curCells == null)
                {
                    curCells = new int[1];
                    curCells[0] = target.pos;
                    SpreadFromCell(curCells[0]);
                }
                else
                {
                    foreach (var c in curCells)
                    {
                        if (FireBlob.VolumeAt(c, typeof(FireBlob)) > 0)
                            SpreadFromCell(c);
                    }
                }

                foreach (var c in curCells)
                {
                    toCells.Remove(c);
                }

                if (toCells.Count == 0)
                {
                    Detach();
                }
                else
                {
                    curCells = new int[toCells.Count];
                    int i = 0;
                    foreach (var c in toCells)
                    {
                        GameScene.Add(Blob.Seed(c, 2, typeof(FireBlob)));
                        curCells[i] = c;
                        ++i;
                    }
                }

                Spend(TICK);
                return true;
            }

            private void SpreadFromCell(int cell)
            {
                if (!Dungeon.level.solid[cell + PathFinder.CIRCLE8[Left(direction)]])
                {
                    toCells.Add(cell + PathFinder.CIRCLE8[Left(direction)]);
                }
                if (!Dungeon.level.solid[cell + PathFinder.CIRCLE8[direction]])
                {
                    toCells.Add(cell + PathFinder.CIRCLE8[direction]);
                }
                if (!Dungeon.level.solid[cell + PathFinder.CIRCLE8[Right(direction)]])
                {
                    toCells.Add(cell + PathFinder.CIRCLE8[Right(direction)]);
                }
            }

            private int Left(int direction)
            {
                return direction == 0 ? 7 : direction - 1;
            }

            private int Right(int direction)
            {
                return direction == 7 ? 0 : direction + 1;
            }

            private const string DIRECTION = "direction";
            private const string CUR_CELLS = "cur_cells";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(DIRECTION, direction);
                bundle.Put(CUR_CELLS, curCells);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                direction = bundle.GetInt(DIRECTION);
                curCells = bundle.GetIntArray(CUR_CELLS);
            }

            [SPDStatic]
            public class FireBlob : Blob
            {
                public FireBlob()
                {
                    actPriority = BUFF_PRIO - 1;
                    alwaysVisible = true;
                }

                protected override void Evolve()
                {
                    bool observe = false;
                    bool burned = false;

                    int cell;
                    for (int i = area.left; i < area.right; ++i)
                    {
                        for (int j = area.top; j < area.bottom; ++j)
                        {
                            cell = i + j * Dungeon.level.Width();
                            off[cell] = cur[cell] > 0 ? cur[cell] - 1 : 0;

                            if (off[cell] > 0)
                            {
                                volume += off[cell];
                            }

                            if (cur[cell] > 0 && off[cell] == 0)
                            {
                                var ch = Actor.FindChar(cell);
                                if (ch != null && !ch.IsImmune(typeof(Fire)) && !(ch is NewTengu))
                                {
                                    Buff.Affect<Burning>(ch).Reignite(ch);
                                }

                                if (Dungeon.level.flamable[cell])
                                {
                                    Dungeon.level.Destroy(cell);

                                    observe = true;
                                    GameScene.UpdateMap(cell);
                                }

                                burned = true;
                                CellEmitter.Get(cell).Start(FlameParticle.Factory, 0.03f, 10);
                            }
                        }
                    }

                    if (observe)
                    {
                        Dungeon.Observe();
                    }

                    if (burned)
                    {
                        Sample.Instance.Play(Assets.Sounds.BURNING);
                    }
                }

                public override void Use(BlobEmitter emitter)
                {
                    base.Use(emitter);

                    emitter.Pour(Speck.Factory(Speck.STEAM), 0.2f);
                }

                public override string TileDesc()
                {
                    return Messages.Get(this, "desc");
                }
            }
        }

        //*********************
        //***Shocker Ability***
        //*********************

        //returns true if shocker was thrown
        public static bool ThrowShocker(Character thrower, Character target)
        {
            int targetCell = -1;

            //Targets closest cell which is adjacent to target, and not adjacent to thrower or another shocker
            foreach (int i in PathFinder.NEIGHBORS8)
            {
                int cell = target.pos + i;
                if (Dungeon.level.Distance(cell, thrower.pos) >= 2 && !Dungeon.level.solid[cell])
                {
                    bool validTarget = true;
                    foreach (var s in thrower.Buffs<ShockerAbility>())
                    {
                        if (Dungeon.level.Distance(cell, s.shockerPos) < 2)
                        {
                            validTarget = false;
                            break;
                        }
                    }
                    if (validTarget && Dungeon.level.TrueDistance(cell, thrower.pos) < Dungeon.level.TrueDistance(targetCell, thrower.pos))
                    {
                        targetCell = cell;
                    }
                }
            }

            if (targetCell == -1)
            {
                return false;
            }

            int finalTargetCell = targetCell;
            throwingChar = thrower;
            ShockerAbility.ShockerItem item = new ShockerAbility.ShockerItem();
            thrower.sprite.Zap(finalTargetCell);
            ((MissileSprite)thrower.sprite.parent.Recycle<MissileSprite>()).
                            Reset(thrower.sprite,
                                    finalTargetCell,
                                    item,
                                    new NewTenguMissileCallback(item, finalTargetCell, thrower));
            return true;
        }

        public class NewTenguMissileCallback : ICallback
        {
            ShockerAbility.ShockerItem item;
            int finalTargetCell;
            Character thrower;

            public NewTenguMissileCallback(ShockerAbility.ShockerItem item, int finalTargetCell, Character thrower)
            {
                this.item = item;
                this.finalTargetCell = finalTargetCell;
                this.thrower = thrower;
            }

            public void Call()
            {
                this.item.OnThrow(this.finalTargetCell);
                this.thrower.Next();
            }
        }

        [SPDStatic]
        public class ShockerAbility : Buff
        {
            public int shockerPos;
            private bool? shockingOrdinals;

            public override bool Act()
            {
                if (shockingOrdinals == null)
                {
                    shockingOrdinals = Rnd.Int(2) == 1;

                    Spreadblob();
                }
                else if (shockingOrdinals.Value)
                {
                    target.sprite.parent.Add(new Lightning(shockerPos - 1 - Dungeon.level.Width(), shockerPos + 1 + Dungeon.level.Width(), null));
                    target.sprite.parent.Add(new Lightning(shockerPos - 1 + Dungeon.level.Width(), shockerPos + 1 - Dungeon.level.Width(), null));

                    if (Dungeon.level.Distance(Dungeon.hero.pos, shockerPos) <= 1)
                    {
                        Sample.Instance.Play(Assets.Sounds.LIGHTNING);
                    }

                    shockingOrdinals = false;
                    Spreadblob();
                }
                else
                {
                    target.sprite.parent.Add(new Lightning(shockerPos - Dungeon.level.Width(), shockerPos + Dungeon.level.Width(), null));
                    target.sprite.parent.Add(new Lightning(shockerPos - 1, shockerPos + 1, null));

                    if (Dungeon.level.Distance(Dungeon.hero.pos, shockerPos) <= 1)
                    {
                        Sample.Instance.Play(Assets.Sounds.LIGHTNING);
                    }

                    shockingOrdinals = true;
                    Spreadblob();
                }

                Spend(TICK);
                return true;
            }

            private void Spreadblob()
            {
                GameScene.Add(Blob.Seed(shockerPos, 1, typeof(ShockerBlob)));
                for (int i = shockingOrdinals.HasValue ? 0 : 1; i < PathFinder.CIRCLE8.Length; i += 2)
                {
                    if (!Dungeon.level.solid[shockerPos + PathFinder.CIRCLE8[i]])
                    {
                        GameScene.Add(Blob.Seed(shockerPos + PathFinder.CIRCLE8[i], 2, typeof(ShockerBlob)));
                    }
                }
            }

            private const string SHOCKER_POS = "shocker_pos";
            private const string SHOCKING_ORDINALS = "shocking_ordinals";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(SHOCKER_POS, shockerPos);
                bundle.Put(SHOCKING_ORDINALS, shockingOrdinals ?? false);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                shockerPos = bundle.GetInt(SHOCKER_POS);
                shockingOrdinals = bundle.GetBoolean(SHOCKING_ORDINALS);
            }

            [SPDStatic]
            public class ShockerBlob : Blob
            {
                public ShockerBlob()
                {
                    actPriority = BUFF_PRIO - 1;
                    alwaysVisible = true;
                }

                protected override void Evolve()
                {
                    int cell;
                    for (int i = area.left; i < area.right; ++i)
                    {
                        for (int j = area.top; j < area.bottom; ++j)
                        {
                            cell = i + j * Dungeon.level.Width();

                            off[cell] = cur[cell] > 0 ? cur[cell] - 1 : 0;

                            if (off[cell] > 0)
                            {
                                volume += off[cell];
                            }

                            if (cur[cell] > 0 && off[cell] == 0)
                            {
                                var ch = Actor.FindChar(cell);
                                if (ch != null && !(ch is NewTengu))
                                {
                                    ch.Damage(2 + Dungeon.depth, new Electricity());

                                    if (ch == Dungeon.hero && !ch.IsAlive())
                                    {
                                        Dungeon.Fail(typeof(NewTengu));
                                    }
                                }
                            }
                        } // for
                    } // for
                }

                public override void Use(BlobEmitter emitter)
                {
                    base.Use(emitter);

                    emitter.Pour(SparkParticle.Static, 0.10f);
                }

                public override string TileDesc()
                {
                    return Messages.Get(this, "desc");
                }
            }

            [SPDStatic]
            public class ShockerItem : Item
            {
                public ShockerItem()
                {
                    dropsDownHeap = true;
                    unique = true;

                    image = ItemSpriteSheet.TENGU_SHOCKER;
                }

                public override bool DoPickUp(Hero hero)
                {
                    GLog.Warning(Messages.Get(this, "cant_pickup"));
                    return false;
                }

                public override void OnThrow(int cell)
                {
                    base.OnThrow(cell);
                    if (throwingChar != null)
                    {
                        Buff.Append<ShockerAbility>(throwingChar).shockerPos = cell;
                        throwingChar = null;
                    }
                    else
                    {
                        Buff.Append<ShockerAbility>(curUser).shockerPos = cell;
                    }
                }

                public override Emitter Emitter()
                {
                    Emitter emitter = new Emitter();
                    emitter.Pos(5, 5);
                    emitter.fillTarget = false;
                    emitter.Pour(SparkParticle.Factory, 0.1f);
                    return emitter;
                }
            }
        }
    }
}
