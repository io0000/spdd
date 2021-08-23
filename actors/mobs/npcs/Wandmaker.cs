using System.Collections.Generic;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items;
using spdd.items.quest;
using spdd.items.wands;
using spdd.levels;
using spdd.levels.rooms;
using spdd.levels.rooms.special;
using spdd.levels.rooms.standard;
using spdd.plants;
using spdd.scenes;
using spdd.sprites;
using spdd.windows;
using spdd.journal;
using spdd.messages;

namespace spdd.actors.mobs.npcs
{
    public class Wandmaker : NPC
    {
        public Wandmaker()
        {
            spriteClass = typeof(WandmakerSprite);

            properties.Add(Property.IMMOVABLE);
        }

        public override bool Act()
        {
            if (Dungeon.level.heroFOV[pos] && Quest.wand1 != null)
            {
                Notes.Add(Notes.Landmark.WANDMAKER);
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
                return true;

            if (Quest.given)
            {
                Item item;
                switch (Quest.type)
                {
                    default:
                    case 1:
                        item = Dungeon.hero.belongings.GetItem<CorpseDust>();
                        break;
                    case 2:
                        item = Dungeon.hero.belongings.GetItem<Embers>();
                        break;
                    case 3:
                        item = Dungeon.hero.belongings.GetItem<Rotberry.Seed>();
                        break;
                }

                if (item != null)
                {
                    GameScene.Show(new WndWandmaker(this, item));
                }
                else
                {
                    string msg;
                    switch (Quest.type)
                    {
                        case 1:
                        default:
                            msg = Messages.Get(this, "reminder_dust", Dungeon.hero.Name());
                            break;
                        case 2:
                            msg = Messages.Get(this, "reminder_ember", Dungeon.hero.Name());
                            break;
                        case 3:
                            msg = Messages.Get(this, "reminder_berry", Dungeon.hero.Name());
                            break;
                    }

                    GameScene.Show(new WndQuest(this, msg));
                }
            }
            else
            {
                string msg1 = "";
                string msg2 = "";

                switch (Dungeon.hero.heroClass)
                {
                    case HeroClass.WARRIOR:
                        msg1 += Messages.Get(this, "intro_warrior");
                        break;
                    case HeroClass.ROGUE:
                        msg1 += Messages.Get(this, "intro_rogue");
                        break;
                    case HeroClass.MAGE:
                        msg1 += Messages.Get(this, "intro_mage", Dungeon.hero.Name());
                        break;
                    case HeroClass.HUNTRESS:
                        msg1 += Messages.Get(this, "intro_huntress");
                        break;
                }

                msg1 += Messages.Get(this, "intro_1");

                switch (Quest.type)
                {
                    case 1:
                        msg2 += Messages.Get(this, "intro_dust");
                        break;
                    case 2:
                        msg2 += Messages.Get(this, "intro_ember");
                        break;
                    case 3:
                        msg2 += Messages.Get(this, "intro_berry");
                        break;
                }

                msg2 += Messages.Get(this, "intro_2");

                GameScene.Show(new WndQuestWandMaker(this, msg1, msg2));

                Quest.given = true;
            }

            return true;
        }

        private class WndQuestWandMaker : WndQuest
        {
            private string msg2;
            private Wandmaker wandMaker;

            public WndQuestWandMaker(NPC questgiver, string msg1, string msg2)
                : base(questgiver, msg1)
            {
                wandMaker = (Wandmaker)questgiver;
                this.msg2 = msg2;
            }

            public override void Hide()
            {
                base.Hide();
                GameScene.Show(new WndQuest(wandMaker, msg2));
            }
        }

        public class Quest
        {
            public static int type;
            // 1 = corpse dust quest
            // 2 = elemental embers quest
            // 3 = rotberry quest

            public static bool spawned;

            public static bool given;

            public static Wand wand1;
            public static Wand wand2;

            public static void Reset()
            {
                spawned = false;
                type = 0;

                wand1 = null;
                wand2 = null;
            }

            private const string NODE = "wandmaker";

            private const string SPAWNED = "spawned";
            private const string TYPE = "type";
            private const string GIVEN = "given";
            private const string WAND1 = "wand1";
            private const string WAND2 = "wand2";

            private const string RITUALPOS = "ritualpos";

            public static void StoreInBundle(Bundle bundle)
            {
                var node = new Bundle();

                node.Put(SPAWNED, spawned);

                if (spawned)
                {
                    node.Put(TYPE, type);

                    node.Put(GIVEN, given);

                    node.Put(WAND1, wand1);
                    node.Put(WAND2, wand2);

                    if (type == 2)
                        node.Put(RITUALPOS, CeremonialCandle.ritualPos);
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

                    wand1 = (Wand)node.Get(WAND1);
                    wand2 = (Wand)node.Get(WAND2);

                    if (type == 2)
                        CeremonialCandle.ritualPos = node.GetInt(RITUALPOS);
                }
                else
                {
                    Reset();
                }
            }

            private static bool questRoomSpawned;

            public static void SpawnWandmaker(Level level, Room room)
            {
                if (questRoomSpawned)
                {
                    questRoomSpawned = false;

                    Wandmaker npc = new Wandmaker();
                    bool validPos;
                    //Do not spawn wandmaker on the entrance, a trap, or in front of a door.
                    do
                    {
                        validPos = true;
                        npc.pos = level.PointToCell(room.Random());
                        if (npc.pos == level.entrance)
                            validPos = false;

                        foreach (var pair in room.connected)
                        {
                            Point door = pair.Value;
                            if (level.TrueDistance(npc.pos, level.PointToCell(door)) <= 1)
                            {
                                validPos = false;
                            }
                        }

                        if (level.traps[npc.pos] != null)
                            validPos = false;

                    } 
                    while (!validPos);
                    level.mobs.Add(npc);

                    spawned = true;

                    given = false;
                    wand1 = (Wand)Generator.Random(Generator.Category.WAND);
                    wand1.cursed = false;
                    wand1.Upgrade();

                    do
                    {
                        wand2 = (Wand)Generator.Random(Generator.Category.WAND);
                    } 
                    while (wand2.GetType().Equals(wand1.GetType()));
                    wand2.cursed = false;
                    wand2.Upgrade();
                }
            }

            public static List<Room> SpawnRoom(List<Room> rooms)
            {
                questRoomSpawned = false;
                if (!spawned && (type != 0 || (Dungeon.depth > 6 && Rnd.Int(10 - Dungeon.depth) == 0)))
                {
                    // decide between 1,2, or 3 for quest type.
                    if (type == 0)
                        type = Rnd.Int(3) + 1;

                    switch (type)
                    {
                        case 1:
                        default:
                            rooms.Add(new MassGraveRoom());
                            break;
                        case 2:
                            rooms.Add(new RitualSiteRoom());
                            break;
                        case 3:
                            rooms.Add(new RotGardenRoom());
                            break;
                    }

                    questRoomSpawned = true;
                }
                return rooms;
            }

            public static void Complete()
            {
                wand1 = null;
                wand2 = null;

                Notes.Remove(Notes.Landmark.WANDMAKER);
            }
        }
    }
}