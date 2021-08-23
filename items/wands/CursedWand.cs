using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items.artifacts;
using spdd.items.bombs;
using spdd.items.scrolls;
using spdd.levels;
using spdd.levels.traps;
using spdd.mechanics;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.ui;
using spdd.utils;
using spdd.windows;

namespace spdd.items.wands
{
    //helper class to contain all the cursed wand zapping logic, so the main wand class doesn't get huge.
    public class CursedWand
    {
        private const float COMMON_CHANCE = 0.6f;
        private const float UNCOMMON_CHANCE = 10.3f;
        private const float RARE_CHANCE = 0.09f;
        private const float VERY_RARE_CHANCE = 0.01f;

        public static void CursedZap(Item origin, Character user, Ballistic bolt, ICallback afterZap)
        {
            var callback = new ActionCallback();
            callback.action = () =>
            {
                if (CursedEffect(origin, user, bolt.collisionPos))
                {
                    if (afterZap != null)
                        afterZap.Call();
                }
            };

            CursedFX(user, bolt, callback);
        }

        public static bool CursedEffect(Item origin, Character user, Character target)
        {
            return CursedEffect(origin, user, target.pos);
        }

        public static bool CursedEffect(Item origin, Character user, int targetPos)
        {
            int n = Rnd.Chances(new float[] { COMMON_CHANCE, UNCOMMON_CHANCE, RARE_CHANCE, VERY_RARE_CHANCE });

            switch (n)
            {
                case 0:
                default:
                    return CommonEffect(origin, user, targetPos);
                case 1:
                    return UncommonEffect(origin, user, targetPos);
                case 2:
                    return RareEffect(origin, user, targetPos);
                case 3:
                    return VeryRareEffect(origin, user, targetPos);
            }
        }

        private static bool CommonEffect(Item origin, Character user, int targetPos)
        {
            switch (Rnd.Int(4))
            {
                //anti-entropy
                case 0:
                default:
                    Character target = Actor.FindChar(targetPos);
                    if (Rnd.Int(2) == 0)
                    {
                        if (target != null)
                            Buff.Affect<Burning>(target).Reignite(target);
                        Buff.Affect<Frost>(user, Frost.DURATION);
                    }
                    else
                    {
                        Buff.Affect<Burning>(user).Reignite(user);
                        if (target != null)
                            Buff.Affect<Frost>(target, Frost.DURATION);
                    }
                    return true;

                //spawns some regrowth
                case 1:
                    GameScene.Add(Blob.Seed(targetPos, 30, typeof(Regrowth)));
                    return true;

                //random teleportation
                case 2:
                    if (Rnd.Int(2) == 0)
                    {
                        if (user != null && !user.Properties().Contains(Character.Property.IMMOVABLE))
                        {
                            ScrollOfTeleportation.TeleportChar(user);
                        }
                        else
                        {
                            return CursedEffect(origin, user, targetPos);
                        }
                    }
                    else
                    {
                        Character ch = Actor.FindChar(targetPos);
                        if (ch != null && !ch.Properties().Contains(Character.Property.IMMOVABLE))
                        {
                            ScrollOfTeleportation.TeleportChar(ch);
                        }
                        else
                        {
                            return CursedEffect(origin, user, targetPos);
                        }
                    }
                    return true;

                //random gas at location
                case 3:
                    Sample.Instance.Play(Assets.Sounds.GAS);
                    switch (Rnd.Int(3))
                    {
                        case 0:
                        default:
                            GameScene.Add(Blob.Seed(targetPos, 800, typeof(ConfusionGas)));
                            return true;
                        case 1:
                            GameScene.Add(Blob.Seed(targetPos, 500, typeof(ToxicGas)));
                            return true;
                        case 2:
                            GameScene.Add(Blob.Seed(targetPos, 200, typeof(ParalyticGas)));
                            return true;
                    }
            }
        }

        private static bool UncommonEffect(Item origin, Character user, int targetPos)
        {
            switch (Rnd.Int(4))
            {
                //Random plant
                case 0:
                default:
                    int pos = targetPos;

                    if (Dungeon.level.map[pos] != Terrain.ALCHEMY &&
                        !Dungeon.level.pit[pos] &&
                        Dungeon.level.traps[pos] == null &&
                        !Dungeon.IsChallenged(Challenges.NO_HERBALISM))
                    {
                        Dungeon.level.Plant((Plant.Seed)Generator.RandomUsingDefaults(Generator.Category.SEED), pos);
                    }
                    else
                    {
                        return CursedEffect(origin, user, targetPos);
                    }

                    return true;

                //Health transfer
                case 1:
                    Character target = Actor.FindChar(targetPos);
                    if (target != null)
                    {
                        int damage = Dungeon.depth * 2;
                        Character toHeal, toDamage;

                        if (Rnd.Int(2) == 0)
                        {
                            toHeal = user;
                            toDamage = target;
                        }
                        else
                        {
                            toHeal = target;
                            toDamage = user;
                        }
                        toHeal.HP = Math.Min(toHeal.HT, toHeal.HP + damage);
                        toHeal.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 3);
                        if (origin == null)
                            toDamage.Damage(damage, toHeal);
                        else
                            toDamage.Damage(damage, origin);
                        toDamage.sprite.Emitter().Start(ShadowParticle.Up, 0.05f, 10);

                        if (toDamage == Dungeon.hero)
                        {
                            Sample.Instance.Play(Assets.Sounds.CURSED);
                            if (!toDamage.IsAlive())
                            {
                                if (origin != null)
                                {
                                    Dungeon.Fail(origin.GetType());
                                    GLog.Negative(Messages.Get(typeof(CursedWand), "ondeath", origin.Name()));
                                }
                                else
                                {
                                    Dungeon.Fail(toHeal.GetType());
                                }
                            }
                        }
                        else
                        {
                            Sample.Instance.Play(Assets.Sounds.BURNING);
                        }
                    }
                    else
                    {
                        return CursedEffect(origin, user, targetPos);
                    }
                    return true;

                //Bomb explosion
                case 2:
                    new Bomb().Explode(targetPos);
                    return true;

                //shock and recharge
                case 3:
                    new ShockingTrap().Set(user.pos).Activate();
                    Buff.Prolong<Recharging>(user, Recharging.DURATION);
                    ScrollOfRecharging.Charge(user);
                    SpellSprite.Show(user, SpellSprite.CHARGE);
                    return true;
            }
        }

        private static bool RareEffect(Item origin, Character user, int targetPos)
        {
            switch (Rnd.Int(4))
            {
                //sheep transformation
                case 0:
                default:
                    Character ch = Actor.FindChar(targetPos);
                    if (ch != null &&
                        !(ch is Hero) &&
                        !ch.Properties().Contains(Character.Property.BOSS) &&
                        !ch.Properties().Contains(Character.Property.MINIBOSS))
                    {
                        Sheep sheep = new Sheep();
                        sheep.lifespan = 10;
                        sheep.pos = ch.pos;
                        ch.Destroy();
                        ch.sprite.KillAndErase();
                        Dungeon.level.mobs.Remove((Mob)ch);
                        TargetHealthIndicator.instance.Target(null);
                        GameScene.Add(sheep);
                        CellEmitter.Get(sheep.pos).Burst(Speck.Factory(Speck.WOOL), 4);
                        Sample.Instance.Play(Assets.Sounds.PUFF);
                        Sample.Instance.Play(Assets.Sounds.SHEEP);
                    }
                    else
                    {
                        return CursedEffect(origin, user, targetPos);
                    }
                    return true;

                //curses!
                case 1:
                    if (user is Hero)
                    {
                        CursingTrap.Curse((Hero)user);
                    }
                    else
                    {
                        return CursedEffect(origin, user, targetPos);
                    }
                    return true;

                //inter-level teleportation
                case 2:
                    if (Dungeon.depth > 1 && !Dungeon.BossLevel() && user == Dungeon.hero)
                    {
                        //each depth has 1 more weight than the previous depth.
                        float[] depths = new float[Dungeon.depth - 1];
                        for (int i = 1; i < Dungeon.depth; ++i)
                            depths[i - 1] = i;

                        int depth = 1 + Rnd.Chances(depths);

                        var buff1 = Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>();
                        if (buff1 != null) buff1.Detach();

                        var buff2 = Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>();
                        if (buff2 != null) buff2.Detach();

                        InterlevelScene.mode = InterlevelScene.Mode.RETURN;
                        InterlevelScene.returnDepth = depth;
                        InterlevelScene.returnPos = -1;
                        Game.SwitchScene(typeof(InterlevelScene));
                    }
                    else
                    {
                        ScrollOfTeleportation.TeleportChar(user);
                    }
                    return true;

                //summon monsters
                case 3:
                    new SummoningTrap().Set(targetPos).Activate();
                    return true;
            }
        }

        private static bool VeryRareEffect(Item origin, Character user, int targetPos)
        {
            //switch(Random.Int(4))
            switch (Rnd.Int(3)) // mod (3번 case가 문제가 있어서 막아둠)
            {
                //great forest fire!
                case 0:
                default:
                    for (int i = 0; i < Dungeon.level.Length(); ++i)
                    {
                        GameScene.Add(Blob.Seed(i, 15, typeof(Regrowth)));
                    }

                    do
                    {
                        GameScene.Add(Blob.Seed(Dungeon.level.RandomDestination(null), 10, typeof(Fire)));
                    }
                    while (Rnd.Int(5) != 0);

                    new Flare(8, 32).Color(new Color(0xFF, 0xFF, 0x66, 0xFF), true).Show(user.sprite, 2f);
                    Sample.Instance.Play(Assets.Sounds.TELEPORT);
                    GLog.Positive(Messages.Get(typeof(CursedWand), "grass"));
                    GLog.Warning(Messages.Get(typeof(CursedWand), "fire"));
                    return true;

                //golden mimic
                case 1:
                    Character ch = Actor.FindChar(targetPos);
                    int spawnCell = targetPos;
                    if (ch != null)
                    {
                        var candidates = new List<int>();
                        foreach (int n in PathFinder.NEIGHBORS8)
                        {
                            int cell = targetPos + n;
                            if (Dungeon.level.passable[cell] && Actor.FindChar(cell) == null)
                                candidates.Add(cell);
                        }
                        if (candidates.Count != 0)
                        {
                            spawnCell = Rnd.Element(candidates);
                        }
                        else
                        {
                            return CursedEffect(origin, user, targetPos);
                        }
                    }

                    Mimic mimic = Mimic.SpawnAt(spawnCell, new List<Item>(), typeof(GoldenMimic));
                    mimic.StopHiding();
                    mimic.alignment = Character.Alignment.ENEMY;
                    Item reward;
                    do
                    {
                        reward = Generator.Random(Rnd.OneOf(Generator.Category.WEAPON, Generator.Category.ARMOR,
                                Generator.Category.RING, Generator.Category.WAND));
                    }
                    while (reward.GetLevel() < 1);

                    //play vfx/sfx manually as mimic isn't in the scene yet
                    Sample.Instance.Play(Assets.Sounds.MIMIC, 1, 0.85f);
                    CellEmitter.Get(mimic.pos).Burst(Speck.Factory(Speck.STAR), 10);
                    mimic.items.Clear();
                    mimic.items.Add(reward);
                    GameScene.Add(mimic);
                    return true;

                //crashes the game, yes, really.
                case 2:
                    try
                    {
                        Dungeon.SaveAll();
                        if (Messages.Lang() != Languages.ENGLISH)
                        {
                            //Don't bother doing this joke to none-english speakers, I doubt it would translate.
                            return CursedEffect(origin, user, targetPos);
                        }
                        else
                        {
                            var wnd = new WndOptions(
                                "CURSED WAND ERROR",
                                "this application will now self-destruct",
                                "abort",
                                "retry",
                                "fail");

                            wnd.selectAction = (index) =>
                            {
                                Game.instance.Finish();
                            };
                            wnd.skipBackPressed = true;

                            GameScene.Show(wnd);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        ShatteredPixelDungeonDash.ReportException(e);
                        //maybe don't kill the game if the save failed.
                        return CursedEffect(origin, user, targetPos);
                    }

                    ////random transmogrification
                    //case 3:
                    //    //skips this effect if there is no item to transmogrify
                    //    // 아래 코드의 문제점
                    //    // 1. origin은 Wand
                    //    // 2. belongings.Contains에 넘겨야 하는 것은 MagesStaff
                    //    // 3. 1과2의 이유로 Contains의 리턴값은 항상 false
                    //    //if (origin == null || user != Dungeon.hero || !Dungeon.hero.belongings.Contains(origin))   
                    //    {
                    //        return CursedEffect(origin, user, targetPos);
                    //    }
                    //    origin.Detach(Dungeon.hero.belongings.backpack);
                    //    Item result;
                    //    do
                    //    {
                    //        result = Generator.Random(Random.OneOf(Generator.Category.WEAPON, Generator.Category.ARMOR,
                    //                Generator.Category.RING, Generator.Category.ARTIFACT));
                    //    } while (result.cursed);
                    //
                    //    if (result.IsUpgradable()) 
                    //        result.Upgrade();
                    //
                    //    result.cursed = result.cursedKnown = true;
                    //    if (origin is Wand)
                    //    {
                    //        GLog.Warning(Messages.Get(typeof(CursedWand), "transmogrify_wand"));
                    //    }
                    //    else
                    //    {
                    //        GLog.Warning(Messages.Get(typeof(CursedWand), "transmogrify_other"));
                    //    }
                    //    Dungeon.level.Drop(result, user.pos).sprite.Drop();
                    //    return true;
            }
        }

        private static void CursedFX(Character user, Ballistic bolt, ICallback callback)
        {
            MagicMissile.BoltFromChar(user.sprite.parent,
                    MagicMissile.RAINBOW,
                    user.sprite,
                    bolt.collisionPos,
                    callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }
    }
}