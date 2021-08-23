using watabou.utils;
using spdd.actors.buffs;
using spdd.items;
using spdd.items.quest;
using spdd.items.rings;
using spdd.levels;
using spdd.scenes;
using spdd.sprites;
using spdd.windows;
using spdd.journal;
using spdd.messages;

namespace spdd.actors.mobs.npcs
{
    public class Imp : NPC
    {
        public Imp()
        {
            spriteClass = typeof(ImpSprite);
            properties.Add(Property.IMMOVABLE);
        }

        private bool seenBefore;

        public override bool Act()
        {
            if (!Quest.given && Dungeon.level.heroFOV[pos])
            {
                if (!seenBefore)
                    Yell(Messages.Get(this, "hey", Dungeon.hero.Name()));
                Notes.Add(Notes.Landmark.IMP);
                seenBefore = true;
            }
            else
            {
                seenBefore = false;
            }

            return base.Act();
        }

        public override int DefenseSkill(Character enemy)
        {
            return INFINITE_EVASION;
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
            sprite.TurnTo(pos, Dungeon.hero.pos);

            if (c != Dungeon.hero)
            {
                return true;
            }

            if (Quest.given)
            {
                var tokens = Dungeon.hero.belongings.GetItem<DwarfToken>();
                if (tokens != null && (tokens.Quantity() >= 5 || (!Quest.alternative && tokens.Quantity() >= 4)))
                {
                    GameScene.Show(new WndImp(this, tokens));
                }
                else
                {
                    Tell(Quest.alternative ?
                        Messages.Get(this, "monks_2", Dungeon.hero.Name())
                        : Messages.Get(this, "golems_2", Dungeon.hero.Name()));
                }
            }
            else
            {
                Tell(Quest.alternative ? Messages.Get(this, "monks_1") : Messages.Get(this, "golems_1"));
                Quest.given = true;
                Quest.completed = false;
            }

            return true;
        }

        private void Tell(string text)
        {
            GameScene.Show(new WndQuest(this, text));
        }

        public void Flee()
        {
            Yell(Messages.Get(this, "cya", Dungeon.hero.Name()));

            Destroy();
            sprite.Die();
        }

        public class Quest
        {
            public static bool alternative;

            public static bool spawned;
            public static bool given;
            public static bool completed;

            public static Ring reward;

            public static void Reset()
            {
                spawned = false;

                reward = null;
            }

            private const string NODE = "demon";

            private const string ALTERNATIVE = "alternative";
            private const string SPAWNED = "spawned";
            private const string GIVEN = "given";
            private const string COMPLETED = "completed";
            private const string REWARD = "reward";

            public static void StoreInBundle(Bundle bundle)
            {
                var node = new Bundle();

                node.Put(SPAWNED, spawned);

                if (spawned)
                {
                    node.Put(ALTERNATIVE, alternative);

                    node.Put(GIVEN, given);
                    node.Put(COMPLETED, completed);
                    node.Put(REWARD, reward);
                }

                bundle.Put(NODE, node);
            }

            public static void RestoreFromBundle(Bundle bundle)
            {
                var node = bundle.GetBundle(NODE);

                if (!node.IsNull() && (spawned = node.GetBoolean(SPAWNED)))
                {
                    alternative = node.GetBoolean(ALTERNATIVE);

                    given = node.GetBoolean(GIVEN);
                    completed = node.GetBoolean(COMPLETED);
                    reward = (Ring)node.Get(REWARD);
                }
            }

            public static void Spawn(CityLevel level)
            {
                if (!spawned && Dungeon.depth > 16 && Rnd.Int(20 - Dungeon.depth) == 0)
                {
                    var npc = new Imp();
                    do
                    {
                        npc.pos = level.RandomRespawnCell(npc);
                    }
                    while (npc.pos == -1 ||
                        level.heaps[npc.pos] != null ||
                        level.traps[npc.pos] != null ||
                        level.FindMob(npc.pos) != null ||
                        //The imp doesn't move, so he cannot obstruct a passageway
                        !(level.passable[npc.pos + PathFinder.CIRCLE4[0]] && level.passable[npc.pos + PathFinder.CIRCLE4[2]]) ||
                        !(level.passable[npc.pos + PathFinder.CIRCLE4[1]] && level.passable[npc.pos + PathFinder.CIRCLE4[3]]));

                    level.mobs.Add(npc);

                    spawned = true;

                    //always assigns monks on floor 17, golems on floor 19, and 50/50 between either on 18
                    switch (Dungeon.depth)
                    {
                        case 17:
                        default:
                            alternative = true;
                            break;
                        case 18:
                            alternative = Rnd.Int(2) == 0;
                            break;
                        case 19:
                            alternative = false;
                            break;
                    }

                    given = false;

                    do
                    {
                        reward = (Ring)Generator.Random(Generator.Category.RING);
                    }
                    while (reward.cursed);

                    reward.Upgrade(2);
                    reward.cursed = true;
                }
            }

            public static void Process(Mob mob)
            {
                if (spawned && given && !completed)
                {
                    if ((alternative && mob is Monk) ||
                        (!alternative && mob is Golem))
                    {
                        Dungeon.level.Drop(new DwarfToken(), mob.pos).sprite.Drop();
                    }
                }
            }

            public static void Complete()
            {
                reward = null;
                completed = true;

                Notes.Remove(Notes.Landmark.IMP);
            }

            public static bool IsCompleted()
            {
                return completed;
            }
        }
    }
}