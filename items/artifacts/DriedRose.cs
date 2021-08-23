using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.items.weapon;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.items.armor.glyphs;
using spdd.items.armor;
using spdd.items.weapon.melee;
using spdd.items.rings;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.actors.blobs;
using spdd.actors.mobs.npcs;
using spdd.sprites;
using spdd.utils;
using spdd.scenes;
using spdd.effects;
using spdd.effects.particles;
using spdd.windows;
using spdd.ui;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class DriedRose : Artifact
    {
        public DriedRose()
        {
            image = ItemSpriteSheet.ARTIFACT_ROSE1;

            levelCap = 10;

            charge = 100;
            chargeCap = 100;

            defaultAction = AC_SUMMON;

            ghostDirector = new GhostDirector(this);
        }

        private bool talkedTo;
        private bool firstSummon;

        private GhostHero ghost;
        private int ghostID;

        private MeleeWeapon weapon;
        private Armor armor;

        public int droppedPetals;

        public const string AC_SUMMON = "SUMMON";
        public const string AC_DIRECT = "DIRECT";
        public const string AC_OUTFIT = "OUTFIT";

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            if (!Ghost.Quest.Completed())
            {
                actions.Remove(AC_EQUIP);
                return actions;
            }
            if (IsEquipped(hero) && charge == chargeCap && !cursed && ghostID == 0)
                actions.Add(AC_SUMMON);

            if (ghostID != 0)
                actions.Add(AC_DIRECT);

            if (IsIdentified() && !cursed)
                actions.Add(AC_OUTFIT);

            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_SUMMON))
            {
                if (!Ghost.Quest.Completed()) 
                    GameScene.Show(new WndUseItem(null, this));
                else if (ghost != null) 
                    GLog.Information(Messages.Get(this, "spawned"));
                else if (!IsEquipped(hero)) 
                    GLog.Information(Messages.Get(typeof(Artifact), "need_to_equip"));
                else if (charge != chargeCap) 
                    GLog.Information(Messages.Get(this, "no_charge"));
                else if (cursed) 
                    GLog.Information(Messages.Get(this, "cursed"));
                else
                {
                    List<int> spawnPoints = new List<int>();
                    for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
                    {
                        int p = hero.pos + PathFinder.NEIGHBORS8[i];
                        if (Actor.FindChar(p) == null && (Dungeon.level.passable[p] || Dungeon.level.avoid[p]))
                            spawnPoints.Add(p);
                    }

                    if (spawnPoints.Count > 0)
                    {
                        ghost = new GhostHero(this);
                        ghostID = ghost.Id();
                        ghost.pos = Rnd.Element(spawnPoints);

                        GameScene.Add(ghost, 1f);
                        Dungeon.level.OccupyCell(ghost);

                        CellEmitter.Get(ghost.pos).Start(ShaftParticle.Factory, 0.3f, 4);
                        CellEmitter.Get(ghost.pos).Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);

                        hero.Spend(1f);
                        hero.Busy();
                        hero.sprite.Operate(hero.pos);

                        if (!firstSummon)
                        {
                            ghost.Yell(Messages.Get(typeof(GhostHero), "hello", Dungeon.hero.Name()));
                            Sample.Instance.Play(Assets.Sounds.GHOST);
                            firstSummon = true;
                        }
                        else
                        {
                            if (BossHealthBar.IsAssigned())
                                ghost.SayBoss();
                            else
                                ghost.SayAppeared();
                        }

                        charge = 0;
                        partialCharge = 0;
                        UpdateQuickslot();
                    }
                    else
                    {
                        GLog.Information(Messages.Get(this, "no_space"));
                    }
                }
            }
            else if (action.Equals(AC_DIRECT))
            {
                if (ghost == null && ghostID != 0)
                {
                    Actor a = Actor.FindById(ghostID);
                    if (a != null)
                        ghost = (GhostHero)a;
                    else
                        ghostID = 0;
                }
                if (ghost != null)
                    GameScene.SelectCell(ghostDirector);
            }
            else if (action.Equals(AC_OUTFIT))
            {
                GameScene.Show(new WndGhostHero(this));
            }
        }

        public int GhostStrength()
        {
            return 13 + GetLevel() / 2;
        }

        public override string Desc()
        {
            if (!Ghost.Quest.Completed() && !IsIdentified())
                return Messages.Get(this, "desc_no_quest");

            string desc = base.Desc();

            if (IsEquipped(Dungeon.hero))
            {
                if (!cursed)
                {
                    if (GetLevel() < levelCap)
                        desc += "\n\n" + Messages.Get(this, "desc_hint");
                }
                else
                {
                    desc += "\n\n" + Messages.Get(this, "desc_cursed");
                }
            }

            if (weapon != null || armor != null)
            {
                desc += "\n";

                if (weapon != null)
                    desc += "\n" + Messages.Get(this, "desc_weapon", weapon.ToString());

                if (armor != null)
                    desc += "\n" + Messages.Get(this, "desc_armor", armor.ToString());
            }

            return desc;
        }

        public override int Value()
        {
            if (weapon != null)
                return -1;

            if (armor != null)
                return -1;

            return base.Value();
        }

        public override string Status()
        {
            if (ghost == null && ghostID != 0)
            {
                try
                {
                    Actor a = Actor.FindById(ghostID);
                    if (a != null)
                        ghost = (GhostHero)a;
                    else
                        ghostID = 0;
                }
                catch (Exception e)
                {
                    ShatteredPixelDungeonDash.ReportException(e);
                    ghostID = 0;
                }
            }

            if (ghost == null)
                return base.Status();
            else
                return (int)((ghost.HP + partialCharge) * 100) / ghost.HT + "%";
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new RoseRecharge(this);
        }

        public override void Charge(Hero target)
        {
            if (ghost == null)
            {
                if (charge < chargeCap)
                {
                    charge += 4;
                    if (charge >= chargeCap)
                    {
                        charge = chargeCap;
                        partialCharge = 0;
                        GLog.Positive(Messages.Get(typeof(DriedRose), "charged"));
                    }
                    UpdateQuickslot();
                }
            }
            else
            {
                ghost.HP = Math.Min(ghost.HT, ghost.HP + 1 + GetLevel() / 3);
                UpdateQuickslot();
            }
        }

        public override Item Upgrade()
        {
            if (GetLevel() >= 9)
                image = ItemSpriteSheet.ARTIFACT_ROSE3;
            else if (GetLevel() >= 4)
                image = ItemSpriteSheet.ARTIFACT_ROSE2;

            //For upgrade transferring via well of transmutation
            droppedPetals = Math.Max(GetLevel(), droppedPetals);

            if (ghost != null)
            {
                ghost.UpdateRose();
            }

            return base.Upgrade();
        }

        public Weapon GhostWeapon()
        {
            return weapon;
        }

        public Armor GhostArmor()
        {
            return armor;
        }

        private const string TALKEDTO = "talkedto";
        private const string FIRSTSUMMON = "firstsummon";
        private const string GHOSTID = "ghostID";
        private const string PETALS = "petals";

        private const string WEAPON = "weapon";
        private const string ARMOR = "armor";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);

            bundle.Put(TALKEDTO, talkedTo);
            bundle.Put(FIRSTSUMMON, firstSummon);
            bundle.Put(GHOSTID, ghostID);
            bundle.Put(PETALS, droppedPetals);

            if (weapon != null)
                bundle.Put(WEAPON, weapon);
            if (armor != null)
                bundle.Put(ARMOR, armor);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            talkedTo = bundle.GetBoolean(TALKEDTO);
            firstSummon = bundle.GetBoolean(FIRSTSUMMON);
            ghostID = bundle.GetInt(GHOSTID);
            droppedPetals = bundle.GetInt(PETALS);

            if (ghostID != 0)
                defaultAction = AC_DIRECT;

            if (bundle.Contains(WEAPON))
                weapon = (MeleeWeapon)bundle.Get(WEAPON);
            if (bundle.Contains(ARMOR))
                armor = (Armor)bundle.Get(ARMOR);
        }

        public class RoseRecharge : ArtifactBuff
        {
            public RoseRecharge(Artifact artifact)
                : base(artifact)
            { }

            public override bool Act()
            {
                var dr = (DriedRose)artifact;
                return dr.RoseRechargeAct(this);
            }
        }

        public bool RoseRechargeAct(ArtifactBuff buff)
        {
            var target = buff.target;
            buff.Spend(Actor.TICK);

            if (ghost == null && ghostID != 0)
            {
                Actor a = Actor.FindById(ghostID);
                if (a != null)
                    ghost = (GhostHero)a;
                else
                    ghostID = 0;
            }

            LockedFloor lockedFloor;
            //rose does not charge while ghost hero is alive
            if (ghost != null)
            {
                defaultAction = AC_DIRECT;

                //heals to full over 1000 turns
                lockedFloor = target.FindBuff<LockedFloor>();
                if (ghost.HP < ghost.HT && (lockedFloor == null || lockedFloor.RegenOn()))
                {
                    partialCharge += (ghost.HT / 1000f) * RingOfEnergy.ArtifactChargeMultiplier(target);
                    UpdateQuickslot();

                    if (partialCharge > 1)
                    {
                        ++ghost.HP;
                        --partialCharge;
                    }
                }
                else
                {
                    partialCharge = 0;
                }

                return true;
            }
            else
            {
                defaultAction = AC_SUMMON;
            }

            lockedFloor = target.FindBuff<LockedFloor>();
            if (charge < chargeCap && !cursed && (lockedFloor == null || lockedFloor.RegenOn()))
            {
                //500 turns to a full charge
                partialCharge += (1 / 5f * RingOfEnergy.ArtifactChargeMultiplier(target));
                if (partialCharge > 1)
                {
                    ++charge;
                    --partialCharge;
                    if (charge == chargeCap)
                    {
                        partialCharge = 0f;
                        GLog.Positive(Messages.Get(typeof(DriedRose), "charged"));
                    }
                }
            }
            else if (cursed && Rnd.Int(100) == 0)
            {
                List<int> spawnPoints = new List<int>();

                for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
                {
                    int p = target.pos + PathFinder.NEIGHBORS8[i];
                    if (Actor.FindChar(p) == null && (Dungeon.level.passable[p] || Dungeon.level.avoid[p]))
                    {
                        spawnPoints.Add(p);
                    }
                }

                if (spawnPoints.Count > 0)
                {
                    Wraith.SpawnAt(Rnd.Element(spawnPoints));
                    Sample.Instance.Play(Assets.Sounds.CURSED);
                }
            }

            UpdateQuickslot();

            return true;
        }

        public GhostDirector ghostDirector;

        public class GhostDirector : CellSelector.IListener
        {
            DriedRose driedRose;

            public GhostDirector(DriedRose driedRose)
            {
                this.driedRose = driedRose;
            }

            public void OnSelect(int? t)
            {
                if (t == null)
                    return;
                int cell = t.Value;

                var ghost = driedRose.ghost;

                Sample.Instance.Play(Assets.Sounds.GHOST);

                if (!Dungeon.level.heroFOV[cell] ||
                    Actor.FindChar(cell) == null ||
                    (Actor.FindChar(cell) != Dungeon.hero && Actor.FindChar(cell).alignment != Character.Alignment.ENEMY))
                {
                    ghost.Yell(Messages.Get(ghost, "directed_position_" + Rnd.IntRange(1, 5)));
                    ghost.Aggro(null);
                    ghost.state = ghost.WANDERING;
                    ghost.defendingPos = cell;
                    ghost.movingToDefendPos = true;
                    return;
                }

                if (ghost.fieldOfView == null || ghost.fieldOfView.Length != Dungeon.level.Length())
                {
                    ghost.fieldOfView = new bool[Dungeon.level.Length()];
                }
                Dungeon.level.UpdateFieldOfView(ghost, ghost.fieldOfView);

                if (Actor.FindChar(cell) == Dungeon.hero)
                {
                    ghost.Yell(Messages.Get(ghost, "directed_follow_" + Rnd.IntRange(1, 5)));
                    ghost.Aggro(null);
                    ghost.state = ghost.WANDERING;
                    ghost.defendingPos = -1;
                    ghost.movingToDefendPos = false;
                }
                else if (Actor.FindChar(cell).alignment == Character.Alignment.ENEMY)
                {
                    ghost.Yell(Messages.Get(ghost, "directed_attack_" + Rnd.IntRange(1, 5)));
                    ghost.Aggro(Actor.FindChar(cell));
                    ghost.SetTarget(cell);
                    ghost.movingToDefendPos = false;
                }
            }

            public string Prompt()
            {
                return "\"" + Messages.Get(typeof(GhostHero), "direct_prompt") + "\"";
            }
        }

        [SPDStatic]
        public class Petal : Item
        {
            public Petal()
            {
                stackable = true;
                dropsDownHeap = true;

                image = ItemSpriteSheet.PETAL;
            }

            public override bool DoPickUp(Hero hero)
            {
                DriedRose rose = hero.belongings.GetItem<DriedRose>();

                if (rose == null)
                {
                    GLog.Warning(Messages.Get(this, "no_rose"));
                    return false;
                }

                if (rose.GetLevel() >= rose.levelCap)
                {
                    GLog.Information(Messages.Get(this, "no_room"));
                    hero.SpendAndNext(TIME_TO_PICK_UP);
                    return true;
                }
                else
                {
                    rose.Upgrade();
                    if (rose.GetLevel() == rose.levelCap)
                        GLog.Positive(Messages.Get(this, "maxlevel"));
                    else
                        GLog.Information(Messages.Get(this, "levelup"));

                    Sample.Instance.Play(Assets.Sounds.DEWDROP);
                    hero.SpendAndNext(TIME_TO_PICK_UP);
                    return true;
                }
            }
        }

        [SPDStatic]
        public class GhostHero : NPC
        {
            private void InitInstance1()
            {
                spriteClass = typeof(GhostSprite);

                flying = true;

                alignment = Alignment.ALLY;
                intelligentAlly = true;
                WANDERING = new GhostWandering(this);

                state = HUNTING;

                //before other mobs
                actPriority = MOB_PRIO + 1;

                properties.Add(Property.UNDEAD);
            }

            private DriedRose rose;

            public GhostHero()
            {
                InitInstance1();
                InitInstance2();
            }

            public GhostHero(DriedRose rose)
            {
                this.rose = rose;
                UpdateRose();
                HP = HT;

                InitInstance1();
                InitInstance2();
            }

            public void UpdateRose()
            {
                if (rose == null)
                    rose = Dungeon.hero.belongings.GetItem<DriedRose>();

                //same dodge as the hero
                defenseSkill = (Dungeon.hero.lvl + 4);
                if (rose == null)
                    return;
                HT = 20 + 8 * rose.GetLevel();
            }

            public int defendingPos = -1;
            public bool movingToDefendPos;

            public void ClearDefensingPos()
            {
                defendingPos = -1;
                movingToDefendPos = false;
            }

            public override bool Act()
            {
                UpdateRose();
                if (rose == null || !rose.IsEquipped(Dungeon.hero))
                    Damage(1, this);

                if (!IsAlive())
                    return true;
                if (!Dungeon.hero.IsAlive())
                {
                    SayHeroKilled();
                    sprite.Die();
                    Destroy();
                    return true;
                }
                return base.Act();
            }

            protected override Character ChooseEnemy()
            {
                var enemy = base.ChooseEnemy();

                int targetPos = defendingPos != -1 ? defendingPos : Dungeon.hero.pos;

                //will never attack something far from their target
                if (enemy != null &&
                    Dungeon.level.mobs.Contains((Mob)enemy) &&
                    (Dungeon.level.Distance(enemy.pos, targetPos) <= 8))
                {
                    return enemy;
                }

                return null;
            }

            public override int AttackSkill(Character target)
            {
                //same accuracy as the hero.
                int acc = Dungeon.hero.lvl + 9;

                if (rose != null && rose.weapon != null)
                    acc = (int)(acc * rose.weapon.AccuracyFactor(this));

                return acc;
            }

            protected override float AttackDelay()
            {
                float delay = base.AttackDelay();
                if (rose != null && rose.weapon != null)
                    delay *= rose.weapon.SpeedFactor(this);

                return delay;
            }

            protected override bool CanAttack(Character enemy)
            {
                return base.CanAttack(enemy) ||
                    (rose != null &&
                    rose.weapon != null &&
                    rose.weapon.CanReach(this, enemy.pos));
            }

            public override int DamageRoll()
            {
                int dmg = 0;
                if (rose != null && rose.weapon != null)
                    dmg += rose.weapon.DamageRoll(this);
                else
                    dmg += Rnd.NormalIntRange(0, 5);

                return dmg;
            }

            public override int AttackProc(Character enemy, int damage)
            {
                damage = base.AttackProc(enemy, damage);
                if (rose != null && rose.weapon != null)
                {
                    damage = rose.weapon.Proc(this, enemy, damage);
                    if (!enemy.IsAlive() && enemy == Dungeon.hero)
                    {
                        Dungeon.Fail(GetType());
                        GLog.Negative(Messages.Capitalize(Messages.Get(typeof(Character), "kill", Name())));
                    }
                }
                return damage;
            }

            public override int DefenseProc(Character enemy, int damage)
            {
                if (rose != null && rose.armor != null)
                    return rose.armor.Proc(enemy, this, damage);
                else
                    return base.DefenseProc(enemy, damage);
            }

            public override void Damage(int dmg, object src)
            {
                //TODO improve this when I have proper damage source logic
                if (rose != null &&
                    rose.armor != null &&
                    rose.armor.HasGlyph(typeof(AntiMagic), this) &&
                    AntiMagic.RESISTS.Contains(src.GetType()))
                {
                    dmg -= AntiMagic.DrRoll(rose.armor.GetLevel());
                }

                base.Damage(dmg, src);

                //for the rose status indicator
                Item.UpdateQuickslot();
            }

            public override float Speed()
            {
                float speed = base.Speed();

                if (rose != null && rose.armor != null)
                    speed = rose.armor.SpeedFactor(this, speed);

                return speed;
            }

            public override int DefenseSkill(Character enemy)
            {
                int defense = base.DefenseSkill(enemy);

                if (defense != 0 && rose != null && rose.armor != null)
                {
                    defense = (int)Math.Round(rose.armor.EvasionFactor(this, defense), MidpointRounding.AwayFromZero);
                }

                return defense;
            }

            public override float Stealth()
            {
                float stealth = base.Stealth();

                if (rose != null && rose.armor != null)
                    stealth = rose.armor.StealthFactor(this, stealth);

                return stealth;
            }

            public override int DrRoll()
            {
                int block = 0;
                if (rose != null && rose.armor != null)
                    block += Rnd.NormalIntRange(rose.armor.DRMin(), rose.armor.DRMax());

                if (rose != null && rose.weapon != null)
                    block += Rnd.NormalIntRange(0, rose.weapon.DefenseFactor(this));

                return block;
            }

            public void SetTarget(int cell)
            {
                target = cell;
            }

            public override bool IsImmune(Type effect)
            {
                if (effect.Equals(typeof(Burning)) &&
                    rose != null &&
                    rose.armor != null &&
                    rose.armor.HasGlyph(typeof(Brimstone), this))
                {
                    return true;
                }

                return base.IsImmune(effect);
            }

            public override bool Interact(Character c)
            {
                UpdateRose();
                if (c == Dungeon.hero && rose != null && !rose.talkedTo)
                {
                    rose.talkedTo = true;

                    GameScene.Show(new WndQuest(this, Messages.Get(this, "introduce")));

                    return true;
                }
                else
                {
                    return base.Interact(c);
                }
            }

            public override void Die(object cause)
            {
                SayDefeated();
                base.Die(cause);
            }

            public override void Destroy()
            {
                UpdateRose();
                if (rose != null)
                {
                    rose.ghost = null;
                    rose.charge = 0;
                    rose.partialCharge = 0;
                    rose.ghostID = -1;
                    rose.defaultAction = AC_SUMMON;
                }
                base.Destroy();
            }

            public void SayAppeared()
            {
                int depth = (Dungeon.depth - 1) / 5;

                //only some lines are said on the first floor of a depth
                int variant = Dungeon.depth % 5 == 1 ? Rnd.IntRange(1, 3) : Rnd.IntRange(1, 6);

                switch (depth)
                {
                    case 0:
                        Yell(Messages.Get(this, "dialogue_sewers_" + variant));
                        break;
                    case 1:
                        Yell(Messages.Get(this, "dialogue_prison_" + variant));
                        break;
                    case 2:
                        Yell(Messages.Get(this, "dialogue_caves_" + variant));
                        break;
                    case 3:
                        Yell(Messages.Get(this, "dialogue_city_" + variant));
                        break;
                    case 4:
                    default:
                        Yell(Messages.Get(this, "dialogue_halls_" + variant));
                        break;
                }

                if (ShatteredPixelDungeonDash.Scene() is GameScene)
                    Sample.Instance.Play(Assets.Sounds.GHOST);
            }

            public void SayBoss()
            {
                int depth = (Dungeon.depth - 1) / 5;

                switch (depth)
                {
                    case 0:
                        Yell(Messages.Get(this, "seen_goo_" + Rnd.IntRange(1, 3)));
                        break;
                    case 1:
                        Yell(Messages.Get(this, "seen_tengu_" + Rnd.IntRange(1, 3)));
                        break;
                    case 2:
                        Yell(Messages.Get(this, "seen_dm300_" + Rnd.IntRange(1, 3)));
                        break;
                    case 3:
                        Yell(Messages.Get(this, "seen_king_" + Rnd.IntRange(1, 3)));
                        break;
                    case 4:
                    default:
                        Yell(Messages.Get(this, "seen_yog_" + Rnd.IntRange(1, 3)));
                        break;
                }
                Sample.Instance.Play(Assets.Sounds.GHOST);
            }

            public void SayDefeated()
            {
                if (BossHealthBar.IsAssigned())
                {
                    Yell(Messages.Get(this, "defeated_by_boss_" + Rnd.IntRange(1, 3)));
                }
                else
                {
                    Yell(Messages.Get(this, "defeated_by_enemy_" + Rnd.IntRange(1, 3)));
                }
                Sample.Instance.Play(Assets.Sounds.GHOST);
            }

            public void SayHeroKilled()
            {
                if (Dungeon.BossLevel())
                {
                    Yell(Messages.Get(this, "hero_killed_boss_" + Rnd.IntRange(1, 3)));
                }
                else
                {
                    Yell(Messages.Get(this, "hero_killed_" + Rnd.IntRange(1, 3)));
                }
                Sample.Instance.Play(Assets.Sounds.GHOST);
            }

            public void SayAnhk()
            {
                Yell(Messages.Get(this, "blessed_ankh_" + Rnd.IntRange(1, 3)));
                Sample.Instance.Play(Assets.Sounds.GHOST);
            }

            private const string DEFEND_POS = "defend_pos";
            private const string MOVING_TO_DEFEND = "moving_to_defend";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(DEFEND_POS, defendingPos);
                bundle.Put(MOVING_TO_DEFEND, movingToDefendPos);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                if (bundle.Contains(DEFEND_POS))
                    defendingPos = bundle.GetInt(DEFEND_POS);
                movingToDefendPos = bundle.GetBoolean(MOVING_TO_DEFEND);
            }

            private void InitInstance2()
            {
                immunities.Add(typeof(ToxicGas));
                immunities.Add(typeof(CorrosiveGas));
                immunities.Add(typeof(Burning));
                immunities.Add(typeof(ScrollOfRetribution));
                immunities.Add(typeof(ScrollOfPsionicBlast));
                immunities.Add(typeof(Corruption));
            }

            private class GhostWandering : Mob.Wandering
            {
                public GhostWandering(Mob mob)
                    : base(mob)
                { }

                public override bool Act(bool enemyInFOV, bool justAlerted)
                {
                    var ghostHero = (GhostHero)mob;

                    if (enemyInFOV && !ghostHero.movingToDefendPos)
                    {
                        ghostHero.enemySeen = true;

                        ghostHero.Notice();
                        ghostHero.alerted = true;
                        ghostHero.state = ghostHero.HUNTING;
                        ghostHero.target = ghostHero.enemy.pos;
                    }
                    else
                    {
                        ghostHero.enemySeen = false;

                        int oldPos = ghostHero.pos;
                        ghostHero.target = ghostHero.defendingPos != -1 ? ghostHero.defendingPos : Dungeon.hero.pos;
                        //always move towards the hero when wandering
                        if (ghostHero.GetCloser(ghostHero.target))
                        {
                            //moves 2 tiles at a time when returning to the hero
                            if (ghostHero.defendingPos == -1 && !Dungeon.level.Adjacent(ghostHero.target, ghostHero.pos))
                                ghostHero.GetCloser(ghostHero.target);

                            ghostHero.Spend(1 / ghostHero.Speed());
                            if (ghostHero.pos == ghostHero.defendingPos)
                                ghostHero.movingToDefendPos = false;
                            return ghostHero.MoveSprite(oldPos, ghostHero.pos);
                        }
                        else
                        {
                            ghostHero.Spend(Actor.TICK);
                        }
                    }
                    return true;
                }
            }
        }

        private class WndGhostHero : Window
        {
            private const int BTN_SIZE = 32;
            private const float GAP = 2;
            private const float BTN_GAP = 12;
            private const int WIDTH = 116;

            private WndBlacksmith.ItemButton btnWeapon;
            private WndBlacksmith.ItemButton btnArmor;

            public WndGhostHero(DriedRose rose)
            {
                IconTitle titlebar = new IconTitle();
                titlebar.Icon(new ItemSprite(rose));
                titlebar.Label(Messages.Get(this, "title"));
                titlebar.SetRect(0, 0, WIDTH, 0);
                Add(titlebar);

                var message = PixelScene.RenderTextBlock(Messages.Get(this, "desc", rose.GhostStrength()), 6);
                message.MaxWidth(WIDTH);
                message.SetPos(0, titlebar.Bottom() + GAP);
                Add(message);

                btnWeapon = new WndBlacksmith.ItemButton();
                btnWeapon.action = () =>
                {
                    if (rose.weapon != null)
                    {
                        btnWeapon.Item(new WndBag.Placeholder(ItemSpriteSheet.WEAPON_HOLDER));
                        if (!rose.weapon.DoPickUp(Dungeon.hero))
                            Dungeon.level.Drop(rose.weapon, Dungeon.hero.pos);

                        rose.weapon = null;
                    }
                    else
                    {
                        GameScene.SelectItem(new WndGhostWeaponListener(this, rose, btnWeapon),
                            WndBag.Mode.WEAPON,
                            Messages.Get(typeof(WndGhostHero), "weapon_prompt"));
                    }
                };

                btnWeapon.SetRect((WIDTH - BTN_GAP) / 2 - BTN_SIZE, message.Top() + message.Height() + GAP, BTN_SIZE, BTN_SIZE);
                if (rose.weapon != null)
                {
                    btnWeapon.Item(rose.weapon);
                }
                else
                {
                    btnWeapon.Item(new WndBag.Placeholder(ItemSpriteSheet.WEAPON_HOLDER));
                }
                Add(btnWeapon);

                btnArmor = new WndBlacksmith.ItemButton();
                btnArmor.action = () =>
                {
                    if (rose.armor != null)
                    {
                        btnArmor.Item(new WndBag.Placeholder(ItemSpriteSheet.ARMOR_HOLDER));
                        if (!rose.armor.DoPickUp(Dungeon.hero))
                        {
                            Dungeon.level.Drop(rose.armor, Dungeon.hero.pos);
                        }
                        rose.armor = null;
                    }
                    else
                    {
                        GameScene.SelectItem(new WndGhostArmorListener(this, rose, btnArmor),
                            WndBag.Mode.ARMOR,
                            Messages.Get(typeof(WndGhostHero), "armor_prompt"));
                    }
                };

                btnArmor.SetRect(btnWeapon.Right() + BTN_GAP, btnWeapon.Top(), BTN_SIZE, BTN_SIZE);
                if (rose.armor != null)
                {
                    btnArmor.Item(rose.armor);
                }
                else
                {
                    btnArmor.Item(new WndBag.Placeholder(ItemSpriteSheet.ARMOR_HOLDER));
                }
                Add(btnArmor);

                Resize(WIDTH, (int)(btnArmor.Bottom() + GAP));
            }

            class WndGhostWeaponListener : WndBag.IListener
            {
                WndGhostHero wndGhost;
                DriedRose rose;
                WndBlacksmith.ItemButton button;

                public WndGhostWeaponListener(WndGhostHero wndGhost, DriedRose rose, WndBlacksmith.ItemButton button)
                {
                    this.wndGhost = wndGhost;
                    this.rose = rose;
                    this.button = button;
                }

                public void OnSelect(Item item)
                {
                    if (!(item is MeleeWeapon))
                    {
                        //do nothing, should only happen when window is cancelled
                    }
                    else if (item.unique)
                    {
                        GLog.Warning(Messages.Get(typeof(WndGhostHero), "cant_unique"));
                        wndGhost.Hide();
                    }
                    else if (!item.IsIdentified())
                    {
                        GLog.Warning(Messages.Get(typeof(WndGhostHero), "cant_unidentified"));
                        wndGhost.Hide();
                    }
                    else if (item.cursed)
                    {
                        GLog.Warning(Messages.Get(typeof(WndGhostHero), "cant_cursed"));
                        wndGhost.Hide();
                    }
                    else if (((MeleeWeapon)item).STRReq() > rose.GhostStrength())
                    {
                        GLog.Warning(Messages.Get(typeof(WndGhostHero), "cant_strength"));
                        wndGhost.Hide();
                    }
                    else
                    {
                        if (item.IsEquipped(Dungeon.hero))
                        {
                            ((MeleeWeapon)item).DoUnequip(Dungeon.hero, false, false);
                        }
                        else
                        {
                            item.Detach(Dungeon.hero.belongings.backpack);
                        }
                        rose.weapon = (MeleeWeapon)item;
                        button.Item(rose.weapon);
                    }
                }
            }

            class WndGhostArmorListener : WndBag.IListener
            {
                WndGhostHero wndGhost;
                DriedRose rose;
                WndBlacksmith.ItemButton button;

                public WndGhostArmorListener(WndGhostHero wndGhost, DriedRose rose, WndBlacksmith.ItemButton button)
                {
                    this.wndGhost = wndGhost;
                    this.rose = rose;
                    this.button = button;
                }

                public void OnSelect(Item item)
                {
                    if (!(item is Armor))
                    {
                        //do nothing, should only happen when window is cancelled
                    }
                    else if (item.unique || ((Armor)item).CheckSeal() != null)
                    {
                        GLog.Warning(Messages.Get(typeof(WndGhostHero), "cant_unique"));
                        wndGhost.Hide();
                    }
                    else if (!item.IsIdentified())
                    {
                        GLog.Warning(Messages.Get(typeof(WndGhostHero), "cant_unidentified"));
                        wndGhost.Hide();
                    }
                    else if (item.cursed)
                    {
                        GLog.Warning(Messages.Get(typeof(WndGhostHero), "cant_cursed"));
                        wndGhost.Hide();
                    }
                    else if (((Armor)item).STRReq() > rose.GhostStrength())
                    {
                        GLog.Warning(Messages.Get(typeof(WndGhostHero), "cant_strength"));
                        wndGhost.Hide();
                    }
                    else
                    {
                        if (item.IsEquipped(Dungeon.hero))
                        {
                            ((Armor)item).DoUnequip(Dungeon.hero, false, false);
                        }
                        else
                        {
                            item.Detach(Dungeon.hero.belongings.backpack);
                        }
                        rose.armor = (Armor)item;
                        button.Item(rose.armor);
                    }
                }
            }
        }
    }
}