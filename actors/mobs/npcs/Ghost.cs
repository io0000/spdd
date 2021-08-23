using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items;
using spdd.items.armor;
using spdd.items.weapon;
using spdd.items.weapon.melee;
using spdd.levels;
using spdd.scenes;
using spdd.sprites;
using spdd.windows;
using spdd.journal;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.mobs.npcs
{
    public class Ghost : NPC
    {
        public Ghost()
        {
            spriteClass = typeof(GhostSprite);

            flying = true;

            state = WANDERING;
        }

        public override bool Act()
        {
            if (Quest.Processed())
            {
                target = Dungeon.hero.pos;
            }
            if (Dungeon.level.heroFOV[pos] && !Quest.Completed())
            {
                Notes.Add(Notes.Landmark.GHOST);
            }
            return base.Act();
        }

        public override int DefenseSkill(Character enemy)
        {
            return INFINITE_EVASION;
        }

        public override float Speed()
        {
            return Quest.Processed() ? 2f : 0.5f;
        }

        protected override Character ChooseEnemy()
        {
            return null;
        }

        public override void Damage(int dmg, object src)
        { }

        public override void Add(Buff buff)
        { }

        public override bool Reset()
        {
            return true;
        }

        public override bool Interact(Character c)
        {
            sprite.TurnTo(pos, c.pos);

            Sample.Instance.Play(Assets.Sounds.GHOST);

            if (c != Dungeon.hero)
            {
                return base.Interact(c);
            }

            if (Quest.given)
            {
                if (Quest.weapon != null)
                {
                    if (Quest.processed)
                    {
                        GameScene.Show(new WndSadGhost(this, Quest.type));
                    }
                    else
                    {
                        switch (Quest.type)
                        {
                            case 1:
                            default:
                                GameScene.Show(new WndQuest(this, Messages.Get(this, "rat_2")));
                                break;
                            case 2:
                                GameScene.Show(new WndQuest(this, Messages.Get(this, "gnoll_2")));
                                break;
                            case 3:
                                GameScene.Show(new WndQuest(this, Messages.Get(this, "crab_2")));
                                break;
                        }

                        int newPos = -1;
                        for (int i = 0; i < 10; ++i)
                        {
                            newPos = Dungeon.level.RandomRespawnCell(this);
                            if (newPos != -1)
                            {
                                break;
                            }
                        }
                        if (newPos != -1)
                        {
                            CellEmitter.Get(pos).Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);
                            pos = newPos;
                            sprite.Place(pos);
                            sprite.visible = Dungeon.level.heroFOV[pos];
                        }
                    }
                }
            }
            else
            {
                Mob questBoss;
                string txt_quest;

                switch (Quest.type)
                {
                    case 1:
                    default:
                        questBoss = new FetidRat();
                        txt_quest = Messages.Get(this, "rat_1", Dungeon.hero.Name());
                        break;
                    case 2:
                        questBoss = new GnollTrickster();
                        txt_quest = Messages.Get(this, "gnoll_1", Dungeon.hero.Name());
                        break;
                    case 3:
                        questBoss = new GreatCrab();
                        txt_quest = Messages.Get(this, "crab_1", Dungeon.hero.Name());
                        break;
                }

                questBoss.pos = Dungeon.level.RandomRespawnCell(this);

                if (questBoss.pos != -1)
                {
                    GameScene.Add(questBoss);
                    Quest.given = true;
                    GameScene.Show(new WndQuest(this, txt_quest));
                }
            }

            return true;
        }

        public class Quest
        {
            public static bool spawned;

            public static int type;

            public static bool given;
            public static bool processed;

            public static int depth;

            public static Weapon weapon;
            public static Armor armor;

            public static void Reset()
            {
                spawned = false;

                weapon = null;
                armor = null;
            }

            private const string NODE = "sadGhost";

            private const string SPAWNED = "spawned";
            private const string TYPE = "type";
            private const string GIVEN = "given";
            private const string PROCESSED = "processed";
            private const string DEPTH = "depth";
            private const string WEAPON = "weapon";
            private const string ARMOR = "armor";

            public static void StoreInBundle(Bundle bundle)
            {
                var node = new Bundle();

                node.Put(SPAWNED, spawned);

                if (spawned)
                {
                    node.Put(TYPE, type);

                    node.Put(GIVEN, given);
                    node.Put(DEPTH, depth);
                    node.Put(PROCESSED, processed);

                    node.Put(WEAPON, weapon);
                    node.Put(ARMOR, armor);
                }

                bundle.Put(NODE, node);
            }

            public static void RestoreFromBundle(Bundle bundle)
            {
                var node = bundle.GetBundle(NODE);

                if (!node.IsNull() && (spawned = node.GetBoolean(SPAWNED)))
                {
                    type = node.GetInt(TYPE);
                    given = node.GetBoolean(GIVEN);
                    processed = node.GetBoolean(PROCESSED);

                    depth = node.GetInt(DEPTH);

                    weapon = (Weapon)node.Get(WEAPON);
                    armor = (Armor)node.Get(ARMOR);
                }
                else
                {
                    Reset();
                }
            }

            public static void Spawn(SewerLevel level)
            {
                if (!spawned && Dungeon.depth > 1 && Rnd.Int(5 - Dungeon.depth) == 0)
                {
                    var ghost = new Ghost();
                    do
                    {
                        ghost.pos = level.RandomRespawnCell(ghost);
                    } 
                    while (ghost.pos == -1);

                    level.mobs.Add(ghost);

                    spawned = true;

                    //dungeon depth determines type of quest.
                    //depth2=fetid rat, 3=gnoll trickster, 4=great crab
                    type = Dungeon.depth - 1;

                    given = false;
                    processed = false;
                    depth = Dungeon.depth;

                    //50%:tier2, 30%:tier3, 15%:tier4, 5%:tier5
                    float itemTierRoll = Rnd.Float();
                    int wepTier;

                    if (itemTierRoll < 0.5f)
                    {
                        wepTier = 2;
                        armor = new LeatherArmor();
                    }
                    else if (itemTierRoll < 0.8f)
                    {
                        wepTier = 3;
                        armor = new MailArmor();
                    }
                    else if (itemTierRoll < 0.95f)
                    {
                        wepTier = 4;
                        armor = new ScaleArmor();
                    }
                    else
                    {
                        wepTier = 5;
                        armor = new PlateArmor();
                    }

                    Generator.Category c = Generator.wepTiers[wepTier - 1];
                    var classes = c.GetClasses();
                    var probs = c.GetProbs();
                    weapon = (MeleeWeapon)Reflection.NewInstance(classes[Rnd.Chances(probs)]);


                    //50%:+0, 30%:+1, 15%:+2, 5%:+3
                    float itemLevelRoll = Rnd.Float();
                    int itemLevel;
                    if (itemLevelRoll < 0.5f)
                        itemLevel = 0;
                    else if (itemLevelRoll < 0.8f)
                        itemLevel = 1;
                    else if (itemLevelRoll < 0.95f)
                        itemLevel = 2;
                    else
                        itemLevel = 3;

                    weapon.Upgrade(itemLevel);
                    armor.Upgrade(itemLevel);

                    //10% to be enchanted
                    if (Rnd.Int(10) == 0)
                    {
                        weapon.Enchant();
                        armor.Inscribe();
                    }
                }
            }

            public static void Process()
            {
                if (spawned && given && !processed && (depth == Dungeon.depth))
                {
                    GLog.Negative(Messages.Get(typeof(Ghost), "find_me"));
                    Sample.Instance.Play(Assets.Sounds.GHOST);
                    processed = true;
                }
            }

            public static void Complete()
            {
                weapon = null;
                armor = null;

                Notes.Remove(Notes.Landmark.GHOST);
            }

            public static bool Processed()
            {
                return spawned && processed;
            }

            public static bool Completed()
            {
                return Processed() && weapon == null && armor == null;
            }
        }
    }
}