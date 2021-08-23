using System;
using System.Linq;
using System.Collections.Generic;
using watabou.noosa;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.actors.mobs;
using spdd.effects;
using spdd.items;
using spdd.items.artifacts;
using spdd.items.armor.glyphs;
using spdd.items.keys;
using spdd.items.potions;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.items.potions.elixirs;
using spdd.items.wands;
using spdd.items.weapon;
using spdd.items.weapon.melee;
using spdd.items.weapon.missiles;
using spdd.items.weapon.enchantments;
using spdd.levels;
using spdd.levels.features;
using spdd.plants;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;
using spdd.utils;
using spdd.windows;
using spdd.journal;
using spdd.messages;

namespace spdd.actors.hero
{
    public class Hero : Character
    {
        private void InitInstance()
        {
            actPriority = HERO_PRIO;

            alignment = Alignment.ALLY;
        }

        public const int MAX_LEVEL = 30;
        public const int STARTING_STR = 10;

        private const float TIME_TO_REST = 1f;
        private const float TIME_TO_SEARCH = 2f;
        private const float HUNGER_FOR_SEARCH = 6f;

        public HeroClass heroClass = HeroClass.ROGUE;
        public HeroSubClass subClass = HeroSubClass.NONE;

        private int attackSkill = 10;
        private int defenseSkill = 5;

        public bool ready;
        private bool damageInterrupt = true;
        public HeroAction curAction;
        public HeroAction lastAction;

        public Character enemy;

        public bool resting;

        public Belongings belongings;

        public int STR;

        //public float awareness;

        public int lvl = 1;
        public int exp;

        public int HTBoost;

        private List<Mob> visibleEnemies;

        //This list is maintained so that some logic checks can be skipped
        // for enemies we know we aren't seeing normally, resultign in better performance
        public List<Mob> mindVisionEnemies = new List<Mob>();

        public Hero()
        {
            InitInstance();

            HP = HT = 20;
            STR = STARTING_STR;

            belongings = new Belongings(this);

            visibleEnemies = new List<Mob>();
        }

        public void UpdateHT(bool boostHP)
        {
            int curHT = HT;

            HT = 20 + 5 * (lvl - 1) + HTBoost;
            float multiplier = RingOfMight.HTMultiplier(this);
            HT = (int)Math.Round(multiplier * HT, MidpointRounding.AwayFromZero);

            if (FindBuff<ElixirOfMight.HTBoost>() != null)
            {
                HT += FindBuff<ElixirOfMight.HTBoost>().Boost();
            }

            if (boostHP)
                HP += Math.Max(HT - curHT, 0);

            HP = Math.Min(HP, HT);
        }

        //public int STR()
        public int GetSTR()
        {
            int STR = this.STR;

            STR += RingOfMight.StrengthBonus(this);

            var buff = FindBuff<AdrenalineSurge>();
            if (buff != null)
                STR += buff.Boost();

            return STR;
        }

        private const string ATTACK = "attackSkill";
        private const string DEFENSE = "defenseSkill";
        private const string STRENGTH = "STR";
        private const string LEVEL = "lvl";
        private const string EXPERIENCE = "exp";
        private const string HTBOOST = "htboost";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);

            heroClass.StoreInBundle(bundle);
            subClass.StoreInBundle(bundle);

            bundle.Put(ATTACK, attackSkill);
            bundle.Put(DEFENSE, defenseSkill);

            bundle.Put(STRENGTH, STR);

            bundle.Put(LEVEL, lvl);
            bundle.Put(EXPERIENCE, exp);
            bundle.Put(HTBOOST, HTBoost);

            belongings.StoreInBundle(bundle);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            heroClass = HeroClassExtensions.RestoreInBundle(bundle);
            subClass = HeroSubClassExtensions.RestoreInBundle(bundle);

            attackSkill = bundle.GetInt(ATTACK);
            defenseSkill = bundle.GetInt(DEFENSE);

            STR = bundle.GetInt(STRENGTH);

            lvl = bundle.GetInt(LEVEL);
            exp = bundle.GetInt(EXPERIENCE);
            HTBoost = bundle.GetInt(HTBOOST);

            belongings.RestoreFromBundle(bundle);
        }

        public static void Preview(GamesInProgress.Info info, Bundle bundle)
        {
            info.level = bundle.GetInt(LEVEL);
            info.str = bundle.GetInt(STRENGTH);
            info.exp = bundle.GetInt(EXPERIENCE);
            info.hp = bundle.GetInt(Character.TAG_HP);
            info.ht = bundle.GetInt(Character.TAG_HT);
            info.shld = bundle.GetInt(Character.TAG_SHLD);
            info.heroClass = HeroClassExtensions.RestoreInBundle(bundle);
            info.subClass = HeroSubClassExtensions.RestoreInBundle(bundle);
            Belongings.Preview(info, bundle);
        }

        public string ClassName()
        {
            if (subClass == HeroSubClass.NONE)
            {
                return heroClass.Title();
            }
            else
            {
                return subClass.Title();
            }
        }

        public override string Name()
        {
            return ClassName();
        }

        public override void HitSound(float pitch)
        {
            if (belongings.weapon != null)
            {
                belongings.weapon.HitSound(pitch);
            }
            else if (RingOfForce.GetBuffedBonus<RingOfForce.Force>(this) > 0)
            {
                //pitch deepens by 2.5% (additive) per point of strength, down to 75%
                base.HitSound(pitch * GameMath.Gate(0.75f, 1.25f - 0.025f * GetSTR(), 1f));
            }
            else
            {
                base.HitSound(pitch * 1.1f);
            }
        }

        public override bool BlockSound(float pitch)
        {
            if (belongings.weapon != null && belongings.weapon.DefenseFactor(this) >= 4)
            {
                Sample.Instance.Play(Assets.Sounds.HIT_PARRY, 1, pitch);
                return true;
            }
            return base.BlockSound(pitch);
        }

        public void Live()
        {
            Buff.Affect<Regeneration>(this);
            Buff.Affect<Hunger>(this);
        }

        public int Tier()
        {
            return belongings.armor == null ? 0 : belongings.armor.tier;
        }

        public bool Shoot(Character enemy, MissileWeapon wep)
        {
            //temporarily set the hero's weapon to the missile weapon being used
            belongings.stashedWeapon = belongings.weapon;
            belongings.weapon = wep;
            bool hit = Attack(enemy);
            Invisibility.Dispel();
            belongings.weapon = belongings.stashedWeapon;
            belongings.stashedWeapon = null;

            if (subClass == HeroSubClass.GLADIATOR)
            {
                if (hit)
                {
                    Buff.Affect<Combo>(this).Hit(enemy);
                }
                else
                {
                    Combo combo = FindBuff<Combo>();
                    if (combo != null)
                        combo.Miss(enemy);
                }
            }

            return hit;
        }

        public override int AttackSkill(Character target)
        {
            var wep = belongings.weapon;

            float accuracy = 1.0f;
            accuracy *= RingOfAccuracy.AccuracyMultiplier(this);

            if (wep is MissileWeapon)
            {
                if (Dungeon.level.Adjacent(pos, target.pos))
                    accuracy *= 0.5f;
                else
                    accuracy *= 1.5f;
            }

            if (wep != null)
            {
                float result = attackSkill * accuracy * wep.AccuracyFactor(this);
                if (result == float.PositiveInfinity)
                    return int.MaxValue;       // otherwise c# result: -2147483648, java result: 2147483647

                return (int)result;
            }
            else
            {
                return (int)(attackSkill * accuracy);
            }
        }

        public override int DefenseSkill(Character enemy)
        {
            float evasion = defenseSkill;
            evasion *= RingOfEvasion.EvasionMultiplier(this);

            if (paralysed > 0)
                evasion /= 2;

            if (belongings.armor != null)
                evasion = belongings.armor.EvasionFactor(this, evasion);

            return (int)Math.Round(evasion, MidpointRounding.AwayFromZero);
        }

        public override int DrRoll()
        {
            int dr = 0;
            int str = GetSTR();

            if (belongings.armor != null)
            {
                int armDr = Rnd.NormalIntRange(belongings.armor.DRMin(), belongings.armor.DRMax());
                int strReq = belongings.armor.STRReq();
                if (str < strReq)
                    armDr -= 2 * (strReq - str);

                if (armDr > 0)
                    dr += armDr;
            }

            if (belongings.weapon != null)
            {
                int wepDr = Rnd.NormalIntRange(0, belongings.weapon.DefenseFactor(this));
                var strReq = ((Weapon)belongings.weapon).STRReq();
                if (str < strReq)
                    wepDr -= 2 * (strReq - str);

                if (wepDr > 0)
                    dr += wepDr;
            }

            var bark = FindBuff<Barkskin>();
            if (bark != null)
                dr += Rnd.NormalIntRange(0, bark.Level());

            var block = FindBuff<Blocking.BlockBuff>();
            if (block != null)
                dr += block.BlockingRoll();

            return dr;
        }

        public override int DamageRoll()
        {
            var wep = belongings.weapon;
            int dmg;

            if (wep != null)
            {
                dmg = wep.DamageRoll(this);
                if (!(wep is MissileWeapon))
                    dmg += RingOfForce.ArmedDamageBonus(this);
            }
            else
            {
                dmg = RingOfForce.DamageRoll(this);
            }

            if (dmg < 0)
                dmg = 0;

            var berserk = FindBuff<Berserk>();
            if (berserk != null)
                dmg = berserk.DamageFactor(dmg);

            return FindBuff<Fury>() != null ? (int)(dmg * 1.5f) : dmg;
        }

        public override float Speed()
        {
            float speed = base.Speed();

            speed *= RingOfHaste.SpeedMultiplier(this);

            if (belongings.armor != null)
                speed = belongings.armor.SpeedFactor(this, speed);

            var momentum = FindBuff<Momentum>();
            if (momentum != null)
            {
                ((HeroSprite)sprite).Sprint(1f + 0.05f * momentum.Stacks());
                speed *= momentum.SpeedMultiplier();
            }

            return speed;
        }

        public bool CanSurpriseAttack()
        {
            if (belongings.weapon == null || !(belongings.weapon is Weapon))
                return true;
            if (GetSTR() < ((Weapon)belongings.weapon).STRReq())
                return false;
            if (belongings.weapon is Flail)
                return false;

            return true;
        }

        public bool CanAttack(Character enemy)
        {
            if (enemy == null || pos == enemy.pos)
                return false;
            
            //can always attack adjacent enemies
            if (Dungeon.level.Adjacent(pos, enemy.pos))
                return true;
            
            KindOfWeapon wep = Dungeon.hero.belongings.weapon;
            
            if (wep != null)
            {
                return wep.CanReach(this, enemy.pos);
            }
            else
            {
                return false;
            }
        }

        public float AttackDelay()
        {
            if (belongings.weapon != null)
            {
                return belongings.weapon.SpeedFactor(this);
            }
            else
            {
                //Normally putting furor speed on unarmed attacks would be unnecessary
                //But there's going to be that one guy who gets a furor+force ring combo
                //This is for that one guy, you shall get your fists of fury!
                return RingOfFuror.AttackDelayMultiplier(this);
            }
        }

        public override void Spend(float time)
        {
            justMoved = false;

            var freeze = FindBuff<TimekeepersHourglass.TimeFreeze>();
            if (freeze != null)
            {
                freeze.ProcessTime(time);
                return;
            }

            var bubble = FindBuff<Swiftthistle.TimeBubble>();
            if (bubble != null)
            {
                bubble.ProcessTime(time);
                return;
            }

            base.Spend(time);
        }

        public void SpendAndNext(float time)
        {
            Busy();
            Spend(time);
            Next();
        }

        public override bool Act()
        {
            //calls to dungeon.observe will also update hero's local FOV.
            fieldOfView = Dungeon.level.heroFOV;

            if (!ready)
            {
                //do a full observe (including fog update) if not resting.
                if (!resting ||
                    FindBuff<MindVision>() != null ||
                    FindBuff<Awareness>() != null)
                {
                    Dungeon.Observe();
                }
                else
                {
                    //otherwise just directly re-calculate FOV
                    Dungeon.level.UpdateFieldOfView(this, fieldOfView);
                }
            }

            CheckVisibleMobs();
            BuffIndicator.RefreshHero();

            if (paralysed > 0)
            {
                curAction = null;

                SpendAndNext(TICK);
                return false;
            }

            bool actResult;

            if (curAction == null)
            {
                if (resting)
                {
                    Spend(TIME_TO_REST);
                    Next();
                }
                else
                {
                    Ready();
                }

                actResult = false;
            }
            else
            {
                resting = false;

                ready = false;
                Console.WriteLine("Act() ready = false");

                if (curAction is HeroAction.Move)
                    actResult = ActMove((HeroAction.Move)curAction);
                else if (curAction is HeroAction.Interact)
                    actResult = ActInteract((HeroAction.Interact)curAction);
                else if (curAction is HeroAction.Buy)
                    actResult = ActBuy((HeroAction.Buy)curAction);
                else if (curAction is HeroAction.PickUp)
                    actResult = ActPickUp((HeroAction.PickUp)curAction);
                else if (curAction is HeroAction.OpenChest)
                    actResult = ActOpenChest((HeroAction.OpenChest)curAction);
                else if (curAction is HeroAction.Unlock)
                    actResult = ActUnlock((HeroAction.Unlock)curAction);
                else if (curAction is HeroAction.Descend)
                    actResult = ActDescend((HeroAction.Descend)curAction);
                else if (curAction is HeroAction.Ascend)
                    actResult = ActAscend((HeroAction.Ascend)curAction);
                else if (curAction is HeroAction.Attack)
                    actResult = ActAttack((HeroAction.Attack)curAction);
                else if (curAction is HeroAction.Alchemy)
                    actResult = ActAlchemy((HeroAction.Alchemy)curAction);
                else
                    actResult = false;
            }

            if (subClass == HeroSubClass.WARDEN && Dungeon.level.map[pos] == Terrain.FURROWED_GRASS)
            {
                Buff.Affect<Barkskin>(this).Set(lvl + 5, 1);
            }

            return actResult;
        }

        public void Busy()
        {
            ready = false;
            Console.WriteLine("Busy() ready = false");
        }

        private void Ready()
        {
            if (sprite.Looping())
                sprite.Idle();

            curAction = null;
            damageInterrupt = true;
            ready = true;
            Console.WriteLine("Ready() ready = true");

            AttackIndicator.UpdateState();

            GameScene.Ready();
        }

        public void Interrupt()
        {
            if (IsAlive() &&
                curAction != null &&
                ((curAction is HeroAction.Move && curAction.dst != pos) ||
                curAction is HeroAction.Ascend || 
                curAction is HeroAction.Descend))
            {
                lastAction = curAction;
            }

            curAction = null;
            GameScene.ResetKeyHold();
        }

        public void Resume()
        {
            curAction = lastAction;
            lastAction = null;
            damageInterrupt = false;
            Next();
        }

        private bool ActMove(HeroAction.Move action)
        {
            if (GetCloser(action.dst))
            {
                return true;
            }
            else
            {
                Ready();
                return false;
            }
        }

        private bool ActInteract(HeroAction.Interact action)
        {
            var ch = action.ch;

            if (ch.CanInteract(this))
            {
                Ready();
                sprite.TurnTo(pos, ch.pos);
                return ch.Interact(this);
            }
            else
            {
                if (fieldOfView[ch.pos] && GetCloser(ch.pos))
                {
                    return true;
                }
                else
                {
                    Ready();
                    return false;
                }
            }
        }

        private bool ActBuy(HeroAction.Buy action)
        {
            var dst = action.dst;
            if (pos == dst)
            {
                Ready();

                var heap = Dungeon.level.heaps[dst];
                if (heap != null &&
                    heap.type == Heap.Type.FOR_SALE &&
                    heap.Size() == 1)
                {
                    GameScene.Show(new WndTradeItem(heap));
                }

                return false;
            }

            if (GetCloser(dst))
                return true;

            Ready();
            return false;
        }

        private bool ActAlchemy(HeroAction.Alchemy action)
        {
            var dst = action.dst;
            if (Dungeon.level.Distance(dst, pos) <= 1)
            {
                Ready();

                var kit = FindBuff<AlchemistsToolkit.KitEnergy>();
                if (kit != null && kit.IsCursed())
                {
                    GLog.Warning(Messages.Get(typeof(AlchemistsToolkit), "cursed"));
                    return false;
                }

                var alch = (Alchemy)Dungeon.level.GetBlob(typeof(Alchemy));
                //TODO logic for a well having dried up?
                if (alch != null)
                {
                    Alchemy.alchPos = dst;
                    AlchemyScene.SetProvider(alch);
                }

                ShatteredPixelDungeonDash.SwitchScene(typeof(AlchemyScene));
                return false;
            }

            if (GetCloser(dst))
                return true;

            Ready();
            return false;
        }

        private bool ActPickUp(HeroAction.PickUp action)
        {
            var dst = action.dst;
            if (pos == dst)
            {
                var heap = Dungeon.level.heaps[pos];
                if (heap != null)
                {
                    var item = heap.Peek();
                    if (item.DoPickUp(this))
                    {
                        heap.PickUp();

                        if (item is Dewdrop ||
                            item is TimekeepersHourglass.SandBag ||
                            item is DriedRose.Petal ||
                            item is Key)
                        {
                            //Do Nothing
                        }
                        else
                        {
                            bool important = item.unique && item.IsIdentified() && (item is Scroll || item is Potion);

                            if (important)
                            {
                                GLog.Positive(Messages.Get(this, "you_now_have", item.Name()));
                            }
                            else
                            {
                                GLog.Information(Messages.Get(this, "you_now_have", item.Name()));
                            }
                        }

                        curAction = null;
                    }
                    else
                    {
                        if (item is Dewdrop ||
                            item is TimekeepersHourglass.SandBag ||
                            item is DriedRose.Petal ||
                            item is Key)
                        {
                            //Do Nothing
                        }
                        else
                        {
                            //TODO temporary until 0.8.0a, when all languages will get this phrase
                            if (Messages.Lang() == Languages.ENGLISH)
                            {
                                GLog.NewLine();
                                GLog.Negative(Messages.Get(this, "you_cant_have", item.Name()));
                            }
                        }

                        heap.sprite.Drop();
                        Ready();
                    }
                }
                else
                {
                    Ready();
                }

                return false;
            }

            if (GetCloser(dst))
                return true;

            Ready();
            return false;
        }

        private bool ActOpenChest(HeroAction.OpenChest action)
        {
            var dst = action.dst;
            if (Dungeon.level.Adjacent(pos, dst) || pos == dst)
            {
                var heap = Dungeon.level.heaps[dst];
                if (heap != null && (heap.type != Heap.Type.HEAP && heap.type != Heap.Type.FOR_SALE))
                {
                    if ((heap.type == Heap.Type.LOCKED_CHEST && Notes.KeyCount(new GoldenKey(Dungeon.depth)) < 1) ||
                        (heap.type == Heap.Type.CRYSTAL_CHEST && Notes.KeyCount(new CrystalKey(Dungeon.depth)) < 1))
                    {
                        GLog.Warning(Messages.Get(this, "locked_chest"));
                        Ready();
                        return false;
                    }

                    switch (heap.type)
                    {
                        case Heap.Type.TOMB:
                            Sample.Instance.Play(Assets.Sounds.TOMB);
                            Camera.main.Shake(1, 0.5f);
                            break;
                        case Heap.Type.SKELETON:
                        case Heap.Type.REMAINS:
                            break;
                        default:
                            Sample.Instance.Play(Assets.Sounds.UNLOCK);
                            break;
                    }

                    sprite.Operate(dst);
                }
                else
                {
                    Ready();
                }

                return false;
            }

            if (GetCloser(dst))
                return true;

            Ready();
            return false;
        }

        private bool ActUnlock(HeroAction.Unlock action)
        {
            var doorCell = action.dst;
            if (Dungeon.level.Adjacent(pos, doorCell))
            {
                bool hasKey = false;
                int door = Dungeon.level.map[doorCell];

                if (door == Terrain.LOCKED_DOOR &&
                    Notes.KeyCount(new IronKey(Dungeon.depth)) > 0)
                {
                    hasKey = true;
                }
                else if (door == Terrain.LOCKED_EXIT &&
                    Notes.KeyCount(new SkeletonKey(Dungeon.depth)) > 0)
                {
                    hasKey = true;
                }

                if (hasKey)
                {
                    sprite.Operate(doorCell);
                    Sample.Instance.Play(Assets.Sounds.UNLOCK);
                }
                else
                {
                    GLog.Warning(Messages.Get(this, "locked_door"));
                    Ready();
                }

                return false;
            }

            if (GetCloser(doorCell))
                return true;

            Ready();
            return false;
        }

        private bool ActDescend(HeroAction.Descend action)
        {
            int stairs = action.dst;

            if (rooted)
            {
                Camera.main.Shake(1, 1f);
                Ready();
                return false;
            }
            else if (Dungeon.level.map[pos] == Terrain.EXIT || Dungeon.level.map[pos] == Terrain.UNLOCKED_EXIT)
            {
                //there can be multiple exit tiles, so descend on any of them
                //TODO this is slightly brittle, it assumes there are no disjointed sets of exit tiles
                curAction = null;

                var buff1 = FindBuff<TimekeepersHourglass.TimeFreeze>();
                if (buff1 != null)
                    buff1.Detach();

                var buff2 = FindBuff<Swiftthistle.TimeBubble>();
                if (buff2 != null)
                    buff2.Detach();

                InterlevelScene.mode = InterlevelScene.Mode.DESCEND;
                Game.SwitchScene(typeof(InterlevelScene));

                return false;
            }
            else if (GetCloser(stairs))
            {
                return true;
            }
            else
            {
                Ready();
                return false;
            }
        }

        private bool ActAscend(HeroAction.Ascend action)
        {
            var stairs = action.dst;

            if (rooted)
            {
                Camera.main.Shake(1, 1f);
                Ready();
                return false;
            }
            else if (Dungeon.level.map[pos] == Terrain.ENTRANCE)
            {
                //there can be multiple entrance tiles, so descend on any of them
                //TODO this is slightly brittle, it assumes there are no disjointed sets of entrance tiles

                if (Dungeon.depth == 1)
                {
                    if (belongings.GetItem<Amulet>() == null)
                    {
                        GameScene.Show(new WndMessage(Messages.Get(this, "leave")));
                        Ready();
                    }
                    else
                    {
                        BadgesExtensions.SilentValidateHappyEnd();
                        Dungeon.Win(typeof(Amulet));
                        Dungeon.DeleteGame(GamesInProgress.curSlot, true);
                        Game.SwitchScene(typeof(SurfaceScene));
                    }
                }
                else
                {
                    curAction = null;

                    var buff1 = FindBuff<TimekeepersHourglass.TimeFreeze>();
                    if (buff1 != null)
                        buff1.Detach();

                    var buff2 = FindBuff<Swiftthistle.TimeBubble>();
                    if (buff2 != null)
                        buff2.Detach();

                    InterlevelScene.mode = InterlevelScene.Mode.ASCEND;
                    Game.SwitchScene(typeof(InterlevelScene));
                }

                return false;
            }
            else if (GetCloser(stairs))
            {
                return true;
            }
            else
            {
                Ready();
                return false;
            }
        }

        private bool ActAttack(HeroAction.Attack action)
        {
            enemy = action.target;

            if (enemy.IsAlive() && CanAttack(enemy) && !IsCharmedBy(enemy))
            {
                sprite.Attack(enemy.pos);
                return false;
            }

            if (fieldOfView[enemy.pos] && GetCloser(enemy.pos))
                return true;

            Ready();
            return false;
        }

        public Character Enemy()
        {
            return enemy;
        }

        public void Rest(bool fullRest)
        {
            SpendAndNext(TIME_TO_REST);

            if (!fullRest)
                sprite.ShowStatus(CharSprite.DEFAULT, Messages.Get(this, "wait"));

            resting = fullRest;
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);

            var wep = belongings.weapon;

            if (wep != null)
                damage = wep.Proc(this, enemy, damage);

            switch (subClass)
            {
                case HeroSubClass.SNIPER:
                    if (wep is MissileWeapon && !(wep is SpiritBow.SpiritArrow))
                    {
                        var actor = new ActionActor();
                        actor.actPriority = VFX_PRIO;
                        actor.action = () =>
                        {
                            if (enemy.IsAlive())
                            {
                                Buff.Prolong<SnipersMark>(this, SnipersMark.DURATION).obj = enemy.Id();
                            }
                            Actor.Remove(actor);
                            return true;
                        };
                        Actor.Add(actor);
                    }
                    break;
            }
            return damage;
        }

        public override int DefenseProc(Character enemy, int damage)
        {
            if (damage > 0 && subClass == HeroSubClass.BERSERKER)
            {
                Berserk berserk = Buff.Affect<Berserk>(this);
                berserk.Damage(damage);
            }

            if (belongings.armor != null)
                damage = belongings.armor.Proc(enemy, this, damage);

            var armor = FindBuff<Earthroot.Armor>();
            if (armor != null)
                damage = armor.Absorb(damage);

            var rockArmor = FindBuff<WandOfLivingEarth.RockArmor>();
            if (rockArmor != null)
                damage = rockArmor.Absorb(damage);

            return damage;
        }

        public override void Damage(int dmg, object src)
        {
            if (FindBuff<TimekeepersHourglass.TimeStasis>() != null)
                return;

            if (!(src is Hunger || src is Viscosity.DeferedDamage) && damageInterrupt)
            {
                Interrupt();
                resting = false;
            }

            if (FindBuff<Drowsy>() != null)
            {
                Buff.Detach<Drowsy>(this);
                GLog.Warning(Messages.Get(this, "pain_resist"));
            }

            var thorns = FindBuff<CapeOfThorns.Thorns>();
            if (thorns != null)
            {
                dmg = thorns.Proc(dmg, (src is Character ? (Character)src : null), this);
            }

            dmg = (int)Math.Ceiling(dmg * RingOfTenacity.DamageMultiplier(this));

            //TODO improve this when I have proper damage source logic
            if (belongings.armor != null &&
                belongings.armor.HasGlyph(typeof(AntiMagic), this) &&
                AntiMagic.RESISTS.Contains(src.GetType()))
            {
                dmg -= AntiMagic.DrRoll(belongings.armor.BuffedLvl());
            }

            int preHP = HP + Shielding();
            base.Damage(dmg, src);
            int postHP = HP + Shielding();
            int effectiveDamage = preHP - postHP;

            //flash red when hit for serious damage.
            float percentDMG = effectiveDamage / (float)preHP; //percent of current HP that was taken
            float percentHP = 1 - ((HT - postHP) / (float)HT); //percent health after damage was taken

            // The flash intensity increases primarily based on damage taken and secondarily on missing HP.

            float flashIntensity = 0.25f * (percentDMG * percentDMG) / percentHP;
            //if the intensity is very low don't flash at all
            if (flashIntensity >= 0.05f)
            {
                flashIntensity = Math.Min(1 / 3f, flashIntensity); //cap intensity at 1/3

                byte red = (byte)(0xFF * flashIntensity);
                var flashColor = new Color(red, 0x00, 0x00, 0xFF);
                GameScene.Flash(flashColor);

                if (IsAlive())
                {
                    if (flashIntensity >= 1 / 6f)
                    {
                        Sample.Instance.Play(Assets.Sounds.HEALTH_CRITICAL, 1 / 3f + flashIntensity * 2f);
                    }
                    else
                    {
                        Sample.Instance.Play(Assets.Sounds.HEALTH_WARN, 1 / 3f + flashIntensity * 4f);
                    }
                }
            }
        }

        public void CheckVisibleMobs()
        {
            var visible = new List<Mob>();

            var newMob = false;

            Mob target = null;
            foreach (var m in Dungeon.level.mobs)
            {
                if (fieldOfView[m.pos] && m.alignment == Alignment.ENEMY)
                {
                    visible.Add(m);
                    if (!visibleEnemies.Contains(m))
                        newMob = true;

                    if (!mindVisionEnemies.Contains(m) && QuickSlotButton.AutoAim(m) != -1)
                    {
                        if (target == null)
                            target = m;
                        else if (Distance(target) > Distance(m))
                            target = m;
                    }
                }
            }

            var lastTarget = QuickSlotButton.lastTarget;
            if (target != null && (lastTarget == null ||
                                !lastTarget.IsAlive() ||
                                !fieldOfView[lastTarget.pos]) ||
                                (lastTarget is WandOfWarding.Ward && mindVisionEnemies.Contains(lastTarget)))
            {
                QuickSlotButton.Target(target);
            }

            if (newMob)
            {
                Interrupt();
                if (resting)
                {
                    Dungeon.Observe();
                    resting = false;
                }
            }

            visibleEnemies = visible;
        }

        public int VisibleEnemies()
        {
            return visibleEnemies.Count;
        }

        public Mob VisibleEnemy(int index)
        {
            return visibleEnemies[index % visibleEnemies.Count];
        }

        private bool walkingToVisibleTrapInFog;

        //FIXME this is a fairly crude way to track this, really it would be nice to have a short
        //history of hero actions
        public bool justMoved;

        private bool GetCloser(int target)
        {
            if (target == pos)
                return false;

            if (rooted)
            {
                Camera.main.Shake(1, 1f);
                return false;
            }

            var step = -1;

            if (Dungeon.level.Adjacent(pos, target))
            {
                path = null;

                if (Actor.FindChar(target) == null)
                {
                    if (Dungeon.level.pit[target] && !flying && !Dungeon.level.solid[target])
                    {
                        if (!Chasm.jumpConfirmed)
                        {
                            Chasm.HeroJump(this);
                            Interrupt();
                        }
                        else
                        {
                            Chasm.HeroFall(target);
                        }
                        return false;
                    }

                    if (Dungeon.level.passable[target] || Dungeon.level.avoid[target])
                        step = target;

                    if (walkingToVisibleTrapInFog &&
                        Dungeon.level.traps[target] != null &&
                        Dungeon.level.traps[target].visible)
                    {
                        return false;
                    }
                }
            }
            else
            {
                bool newPath = false;
                if (path == null || path.Count == 0 || !Dungeon.level.Adjacent(pos, path[0]))
                    newPath = true;
                else if (path[path.Count - 1] != target)
                    newPath = true;
                else
                {
                    if (!Dungeon.level.passable[path[0]] || Actor.FindChar(path[0]) != null)
                    {
                        newPath = true;
                    }
                }

                if (newPath)
                {
                    int len = Dungeon.level.Length();
                    var p = Dungeon.level.passable;
                    var v = Dungeon.level.visited;
                    var m = Dungeon.level.mapped;
                    var passable = new bool[len];
                    for (var i = 0; i < len; ++i)
                        passable[i] = p[i] && (v[i] || m[i]);

                    var newpath = Dungeon.FindPath(this, target, passable, fieldOfView, true);
                    if (newpath != null && path != null && newpath.Count > 2 * path.Count)
                    {
                        path = null;
                    }
                    else
                    {
                        path = newpath;
                    }
                }

                if (path == null)
                    return false;

                step = path[0];
                path.RemoveAt(0);
            }

            if (step == -1)
                return false;

            float speed = Speed();

            sprite.Move(pos, step);
            Move(step);

            Spend(1 / speed);
            justMoved = true;

            Search(false);

            if (subClass == HeroSubClass.FREERUNNER)
            {
                Buff.Affect<Momentum>(this).GainStack();
            }

            //FIXME this is a fairly sloppy fix for a crash involving pitfall traps.
            //really there should be a way for traps to specify whether action should continue or
            //not when they are pressed.
            return InterlevelScene.mode != InterlevelScene.Mode.FALL;
        }

        public bool Handle(int? c)
        {
            if (c == null)
                return false;
            
            int cell = c.Value;
            if (cell == -1)
                return false;

            Character ch;
            Heap heap;

            int terrain = Dungeon.level.map[cell];

            if (terrain == Terrain.ALCHEMY && cell != pos)
            {
                curAction = new HeroAction.Alchemy(cell);
            }
            else if (fieldOfView[cell] && (ch = FindChar(cell)) is Mob)
            {
                if (ch.alignment != Alignment.ENEMY && ch.FindBuff<Amok>() == null)
                {
                    curAction = new HeroAction.Interact(ch);
                }
                else
                {
                    curAction = new HeroAction.Attack(ch);
                }
            }
            else if ((heap = Dungeon.level.heaps[cell]) != null &&
                (visibleEnemies.Count == 0 || 
                cell == pos ||
                //...but only for standard heaps, chests and similar open as normal.
                (heap.type != Heap.Type.HEAP && heap.type != Heap.Type.FOR_SALE)))
            {
                switch (heap.type)
                {
                    case Heap.Type.HEAP:
                        curAction = new HeroAction.PickUp(cell);
                        break;
                    case Heap.Type.FOR_SALE:
                        if (heap.Size() == 1 && heap.Peek().Value() > 0)
                            curAction = new HeroAction.Buy(cell);
                        else
                            curAction = new HeroAction.PickUp(cell);
                        break;
                    default:
                        curAction = new HeroAction.OpenChest(cell);
                        break;
                }
            }
            else if (terrain == Terrain.LOCKED_DOOR || terrain == Terrain.LOCKED_EXIT)
            {
                curAction = new HeroAction.Unlock(cell);
            }
            else if ((cell == Dungeon.level.exit || terrain == Terrain.EXIT || terrain == Terrain.UNLOCKED_EXIT) &&
                Dungeon.depth < 26)
            {
                curAction = new HeroAction.Descend(cell);
            }
            else if (cell == Dungeon.level.entrance || terrain == Terrain.ENTRANCE)
            {
                curAction = new HeroAction.Ascend(cell);
            }
            else
            {
                if (!Dungeon.level.visited[cell] &&
                    !Dungeon.level.mapped[cell] &&
                    Dungeon.level.traps[cell] != null &&
                    Dungeon.level.traps[cell].visible)
                {
                    walkingToVisibleTrapInFog = true;
                }
                else
                {
                    walkingToVisibleTrapInFog = false;
                }

                curAction = new HeroAction.Move(cell);
                lastAction = null;
            }

            return true;
        }

        public void EarnExp(int exp, Type source)
        {
            this.exp += exp;
            float percent = exp / (float)MaxExp();

            var chains = this.FindBuff<EtherealChains.ChainsRecharge>();
            if (chains != null)
                chains.GainExp(percent);

            var horn = this.FindBuff<HornOfPlenty.HornRecharge>();
            if (horn != null)
                horn.GainCharge(percent);

            var kit = this.FindBuff<AlchemistsToolkit.KitEnergy>();
            if (kit != null)
                kit.GainCharge(percent);

            var berserk = this.FindBuff<Berserk>();
            if (berserk != null)
                berserk.Recover(percent);

            if (source != typeof(PotionOfExperience))
            {
                foreach (Item i in belongings)
                {
                    i.OnHeroGainExp(percent, this);
                }
            }

            var levelUp = false;
            while (this.exp >= MaxExp())
            {
                this.exp -= MaxExp();
                if (lvl < MAX_LEVEL)
                {
                    ++lvl;
                    levelUp = true;

                    if (this.FindBuff<ElixirOfMight.HTBoost>() != null)
                        this.FindBuff<ElixirOfMight.HTBoost>().OnLevelUp();

                    UpdateHT(true);

                    ++attackSkill;
                    ++defenseSkill;
                }
                else
                {
                    Buff.Prolong<Bless>(this, Bless.DURATION);
                    this.exp = 0;

                    GLog.NewLine();
                    GLog.Positive(Messages.Get(this, "level_cap"));
                    Sample.Instance.Play(Assets.Sounds.LEVELUP);
                }
            }

            if (levelUp)
            {
                if (sprite != null)
                {
                    GLog.NewLine();
                    GLog.Positive(Messages.Get(this, "new_level"), lvl);
                    sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(typeof(Hero), "level_up"));
                    Sample.Instance.Play(Assets.Sounds.LEVELUP);
                }

                Item.UpdateQuickslot();

                BadgesExtensions.ValidateLevelReached();
            }
        }

        public int MaxExp()
        {
            return MaxExp(lvl);
        }

        public static int MaxExp(int lvl)
        {
            return 5 + lvl * 5;
        }

        public bool IsStarving()
        {
            return Buff.Affect<Hunger>(this).IsStarving();
        }

        public override void Add(Buff buff)
        {
            if (FindBuff<TimekeepersHourglass.TimeFreeze>() != null)
                return;

            base.Add(buff);

            if (sprite != null)
            {
                string msg = buff.HeroMessage();
                if (msg != null)
                {
                    GLog.Warning(msg);
                }

                if (buff is buffs.Paralysis || buff is buffs.Vertigo)
                {
                    Interrupt();
                }
            }

            BuffIndicator.RefreshHero();
        }

        public override void Remove(Buff buff)
        {
            base.Remove(buff);

            BuffIndicator.RefreshHero();
        }

        public override float Stealth()
        {
            float stealth = base.Stealth();
            if (belongings.armor != null)
                stealth = belongings.armor.StealthFactor(this, stealth);

            return stealth;
        }

        public override void Die(object cause)
        {
            curAction = null;

            Ankh ankh = null;

            //look for ankhs in player inventory, prioritize ones which are blessed.
            foreach (Item item in belongings)
            {
                if (item is Ankh)
                {
                    if (ankh == null || ((Ankh)item).IsBlessed())
                    {
                        ankh = (Ankh)item;
                    }
                }
            }

            if (ankh != null && ankh.IsBlessed())
            {
                this.HP = HT / 4;

                //ensures that you'll get to act first in almost any case, to prevent reviving and then instantly dieing again.
                PotionOfHealing.Cure(this);
                Buff.Detach<buffs.Paralysis>(this);
                Spend(-Cooldown());

                new Flare(8, 32).Color(new Color(0xFF, 0xFF, 0x66, 0xFF), true).Show(sprite, 2f);
                CellEmitter.Get(this.pos).Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);

                ankh.Detach(belongings.backpack);

                Sample.Instance.Play(Assets.Sounds.TELEPORT);
                GLog.Warning(Messages.Get(this, "revive"));
                ++Statistics.ankhsUsed;

                foreach (var ch in Actor.Chars())
                {
                    if (ch is DriedRose.GhostHero)
                    {
                        ((DriedRose.GhostHero)ch).SayAnhk();
                        return;
                    }
                }

                return;
            }

            FixTime();
            base.Die(cause);

            if (ankh == null)
            {
                ReallyDie(cause);
            }
            else
            {
                Dungeon.DeleteGame(GamesInProgress.curSlot, false);
                Ankh finalAnkh = ankh;
                GameScene.Show(new WndResurrect(finalAnkh, cause));
            }
        }

        public static void ReallyDie(object cause)
        {
            int length = Dungeon.level.Length();
            var map = Dungeon.level.map;
            var visited = Dungeon.level.visited;
            var discoverable = Dungeon.level.discoverable;

            for (var i = 0; i < length; ++i)
            {
                var terr = map[i];

                if (discoverable[i])
                {
                    visited[i] = true;

                    if ((Terrain.flags[terr] & Terrain.SECRET) != 0)
                        Dungeon.level.Discover(i);
                }
            }

            Bones.Leave();

            Dungeon.Observe();
            GameScene.UpdateFog();

            Dungeon.hero.belongings.Identify();

            int pos = Dungeon.hero.pos;

            List<int> passable = new List<int>();
            foreach (int ofs in PathFinder.NEIGHBORS8)
            {
                int cell = pos + ofs;
                if ((Dungeon.level.passable[cell] || Dungeon.level.avoid[cell]) &&
                    Dungeon.level.heaps[cell] == null)
                {
                    passable.Add(cell);
                }
            }

            Rnd.Shuffle(passable);

            List<Item> items = new List<Item>(Dungeon.hero.belongings.backpack.items);
            foreach (int cell in passable)
            {
                if (items.Count == 0)
                    break;

                Item item = Rnd.Element(items);
                Dungeon.level.Drop(item, cell).sprite.Drop(pos);
                items.Remove(item);
            }

            GameScene.GameOver();

            var doom = cause as IDoom;
            if (doom != null)
                doom.OnDeath();

            Dungeon.DeleteGame(GamesInProgress.curSlot, true);
        }

        //effectively cache this buff to prevent having to call buff(Berserk.class) a bunch.
        //This is relevant because we call isAlive during drawing, which has both performance
        //and concurrent modification implications if that method calls buff(Berserk.class)
        private Berserk berserk;

        public override bool IsAlive()
        {
            if (HP <= 0)
            {
                if (berserk == null)
                    berserk = FindBuff<Berserk>();
                return berserk != null && berserk.Berserking();
            }
            else
            {
                berserk = null;
                return base.IsAlive();
            }
        }

        public override void Move(int step)
        {
            bool wasHighGrass = Dungeon.level.map[step] == Terrain.HIGH_GRASS;

            base.Move(step);

            if (flying)
                return;

            var terrain = Dungeon.level.map[pos];

            if (Dungeon.level.water[pos])
            {
                Sample.Instance.Play(Assets.Sounds.WATER, 1, 1, Rnd.Float(0.8f, 1.25f));
            }
            else if (terrain == Terrain.EMPTY_SP)
            {
                Sample.Instance.Play(Assets.Sounds.STURDY, 1, Rnd.Float(0.96f, 1.05f));
            }
            else if (terrain == Terrain.GRASS ||
                terrain == Terrain.EMBERS ||
                terrain == Terrain.FURROWED_GRASS)
            {
                if (step == pos && wasHighGrass)
                {
                    Sample.Instance.Play(Assets.Sounds.TRAMPLE, 1, Rnd.Float(0.96f, 1.05f));
                }
                else
                {
                    Sample.Instance.Play(Assets.Sounds.GRASS, 1, Rnd.Float(0.96f, 1.05f));
                }
            }
            else
            {
                Sample.Instance.Play(Assets.Sounds.STEP, 1, Rnd.Float(0.96f, 1.05f));
            }
        }

        public override void OnAttackComplete()
        {
            AttackIndicator.Target(enemy);

            bool hit = Attack(enemy);

            if (subClass == HeroSubClass.GLADIATOR)
            {
                if (hit)
                {
                    Buff.Affect<Combo>(this).Hit(enemy);
                }
                else
                {
                    Combo combo = FindBuff<Combo>();
                    if (combo != null)
                        combo.Miss(enemy);
                }
            }

            Invisibility.Dispel();
            Spend(AttackDelay());

            curAction = null;

            base.OnAttackComplete();
        }

        public override void OnMotionComplete()
        {
            GameScene.CheckKeyHold();
        }

        public override void OnOperateComplete()
        {
            if (curAction is HeroAction.Unlock)
            {
                var unlock = (HeroAction.Unlock)curAction;

                int doorCell = unlock.dst;
                int door = Dungeon.level.map[doorCell];

                if (Dungeon.level.Distance(pos, doorCell) <= 1)
                {
                    bool hasKey = true;
                    if (door == Terrain.LOCKED_DOOR)
                    {
                        hasKey = Notes.Remove(new IronKey(Dungeon.depth));
                        if (hasKey)
                            Level.Set(doorCell, Terrain.DOOR);
                    }
                    else
                    {
                        hasKey = Notes.Remove(new SkeletonKey(Dungeon.depth));
                        if (hasKey)
                            Level.Set(doorCell, Terrain.UNLOCKED_EXIT);
                    }

                    if (hasKey)
                    {
                        GameScene.UpdateKeyDisplay();
                        Level.Set(doorCell, door == Terrain.LOCKED_DOOR ? Terrain.DOOR : Terrain.UNLOCKED_EXIT);
                        GameScene.UpdateMap(doorCell);
                        Spend(Key.TIME_TO_UNLOCK);
                    }
                }
            }
            else if (curAction is HeroAction.OpenChest)
            {
                var chest = (HeroAction.OpenChest)curAction;

                Heap heap = Dungeon.level.heaps[chest.dst];

                if (Dungeon.level.Distance(pos, heap.pos) <= 1)
                {
                    bool hasKey = true;
                    if (heap.type == Heap.Type.SKELETON || heap.type == Heap.Type.REMAINS)
                    {
                        Sample.Instance.Play(Assets.Sounds.BONES);
                    }
                    else if (heap.type == Heap.Type.LOCKED_CHEST)
                    {
                        hasKey = Notes.Remove(new GoldenKey(Dungeon.depth));
                    }
                    else if (heap.type == Heap.Type.CRYSTAL_CHEST)
                    {
                        hasKey = Notes.Remove(new CrystalKey(Dungeon.depth));
                    }

                    if (hasKey)
                    {
                        GameScene.UpdateKeyDisplay();
                        heap.Open(this);
                        Spend(Key.TIME_TO_UNLOCK);
                    }
                }
            }

            curAction = null;

            base.OnOperateComplete();
        }

        public override bool IsImmune(Type effect)
        {
            if (effect == typeof(Burning) &&
                belongings.armor != null &&
                belongings.armor.HasGlyph(typeof(Brimstone), this))
            {
                return true;
            }

            return base.IsImmune(effect);
        }

        public bool Search(bool intentional)
        {
            if (!IsAlive())
                return false;

            var smthFound = false;
            int distance = heroClass == HeroClass.ROGUE ? 2 : 1;

            bool foresight = FindBuff<Foresight>() != null;

            if (foresight)
                ++distance;

            int cx = pos % Dungeon.level.Width();
            int cy = pos / Dungeon.level.Width();
            int ax = cx - distance;
            if (ax < 0)
                ax = 0;

            int bx = cx + distance;
            if (bx >= Dungeon.level.Width())
                bx = Dungeon.level.Width() - 1;
            int ay = cy - distance;
            if (ay < 0)
                ay = 0;

            int by = cy + distance;
            if (by >= Dungeon.level.Height())
                by = Dungeon.level.Height() - 1;

            var talisman = FindBuff<TalismanOfForesight.Foresight>();
            bool cursed = talisman != null && talisman.IsCursed();

            for (int y = ay; y <= by; ++y)
            {
                for (int x = ax, p = ax + y * Dungeon.level.Width(); x <= bx; ++x, ++p)
                {
                    if (fieldOfView[p] || p != pos)
                    {
                        if (intentional)
                            GameScene.EffectOverFog(new CheckedCell(p, pos));

                        if (!Dungeon.level.secret[p])
                            continue;

                        var trap = Dungeon.level.traps[p];
                        float chance;

                        if (foresight)
                        {
                            //searches aided by foresight always succeed, even if trap isn't searchable
                            chance = 1f;
                        }
                        else if (trap != null && !trap.canBeSearched)
                        {
                            //otherwise if the trap isn't searchable, searching always fails
                            chance = 0f;
                        }
                        else if (intentional)
                        {
                            //intentional searches always succeed against regular traps and doors
                            chance = 1f;
                        }
                        else if (cursed)
                        {
                            //unintentional searches always fail with a cursed talisman
                            chance = 0f;
                        }
                        else if (Dungeon.level.map[p] == Terrain.SECRET_TRAP)
                        {
                            //unintentional trap detection scales from 40% at floor 0 to 30% at floor 25
                            chance = 0.4f - (Dungeon.depth / 250f);
                        }
                        else
                        {
                            //unintentional door detection scales from 20% at floor 0 to 0% at floor 20
                            chance = 0.2f - (Dungeon.depth / 100f);
                        }

                        if (Rnd.Float() < chance)
                        {
                            int oldValue = Dungeon.level.map[p];

                            GameScene.DiscoverTile(p, oldValue);

                            Dungeon.level.Discover(p);

                            ScrollOfMagicMapping.Discover(p);

                            smthFound = true;

                            if (talisman != null)
                            {
                                if (oldValue == Terrain.SECRET_TRAP)
                                {
                                    talisman.Charge(2);
                                }
                                else if (oldValue == Terrain.SECRET_DOOR)
                                {
                                    talisman.Charge(10);
                                }
                            }
                        }
                    }
                }
            }

            if (intentional)
            {
                sprite.ShowStatus(CharSprite.DEFAULT, Messages.Get(this, "search"));
                sprite.Operate(pos);

                if (!Dungeon.level.locked)
                {
                    if (cursed)
                    {
                        GLog.Negative(Messages.Get(this, "search_distracted"));
                        Buff.Affect<Hunger>(this).ReduceHunger(TIME_TO_SEARCH - (2 * HUNGER_FOR_SEARCH));
                    }
                    else
                    {
                        Buff.Affect<Hunger>(this).ReduceHunger(TIME_TO_SEARCH - HUNGER_FOR_SEARCH);
                    }
                }

                SpendAndNext(TIME_TO_SEARCH);
            }

            if (smthFound)
            {
                GLog.Warning(Messages.Get(this, "noticed_smth"));
                Sample.Instance.Play(Assets.Sounds.SECRET);
                Interrupt();
            }

            return smthFound;
        }

        public void Resurrect(int resetLevel)
        {
            HP = HT;
            Dungeon.gold = 0;
            exp = 0;

            belongings.Resurrect(resetLevel);

            Live();
        }

        public override void Next()
        {
            if (IsAlive())
                base.Next();
        }

        public interface IDoom
        {
            void OnDeath();
        }
    }
}