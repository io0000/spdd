using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using watabou.noosa.tweeners;
using spdd.actors;
using spdd.actors.hero;
using spdd.utils;
using spdd.sprites;
using spdd.scenes;
using spdd.effects;
using spdd.levels;
using spdd.levels.rooms;
using spdd.levels.rooms.special;
using spdd.levels.rooms.secret;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfTeleportation : Scroll
    {
        public ScrollOfTeleportation()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_TELEPORT;
        }

        public override void DoRead()
        {
            Sample.Instance.Play(Assets.Sounds.READ);

            TeleportPreferringUnseen(curUser);
            SetKnown();

            ReadAnimation();
        }

        public static void TeleportToLocation(Hero hero, int pos)
        {
            PathFinder.BuildDistanceMap(pos, BArray.Or(Dungeon.level.passable, Dungeon.level.avoid, null));
            if (PathFinder.distance[hero.pos] == int.MaxValue ||
                (!Dungeon.level.passable[pos] && !Dungeon.level.avoid[pos]) ||
                Actor.FindChar(pos) != null)
            {
                GLog.Warning(Messages.Get(typeof(ScrollOfTeleportation), "cant_reach"));
                return;
            }

            Appear(hero, pos);
            Dungeon.level.OccupyCell(hero);
            Dungeon.Observe();
            GameScene.UpdateFog();
        }

        public static void TeleportHero(Hero hero)
        {
            TeleportChar(hero);
        }

        public static void TeleportChar(Character ch)
        {
            if (Dungeon.BossLevel())
            {
                GLog.Warning(Messages.Get(typeof(ScrollOfTeleportation), "no_tele"));
                return;
            }

            int count = 10;
            int pos;
            do
            {
                pos = Dungeon.level.RandomRespawnCell(ch);
                if (count-- <= 0)
                {
                    break;
                }
            }
            while (pos == -1 || Dungeon.level.secret[pos]);

            if (pos == -1)
            {
                GLog.Warning(Messages.Get(typeof(ScrollOfTeleportation), "no_tele"));
            }
            else
            {
                Appear(ch, pos);
                Dungeon.level.OccupyCell(ch);

                if (ch == Dungeon.hero)
                {
                    GLog.Information(Messages.Get(typeof(ScrollOfTeleportation), "tele"));

                    Dungeon.Observe();
                    GameScene.UpdateFog();
                }
            }
        }

        public static void TeleportPreferringUnseen(Hero hero)
        {
            if (Dungeon.BossLevel() || !(Dungeon.level is RegularLevel))
            {
                TeleportHero(hero);
                return;
            }

            RegularLevel level = (RegularLevel)Dungeon.level;
            List<int> candidates = new List<int>();

            foreach (Room r in level.Rooms())
            {
                if (r is SpecialRoom)
                {
                    int terr;
                    bool locked = false;
                    foreach (Point p in r.GetPoints())
                    {
                        terr = level.map[level.PointToCell(p)];
                        if (terr == Terrain.LOCKED_DOOR || terr == Terrain.BARRICADE)
                        {
                            locked = true;
                            break;
                        }
                    }
                    if (locked)
                    {
                        continue;
                    }
                }

                int cell;
                foreach (Point p in r.CharPlaceablePoints(level))
                {
                    cell = level.PointToCell(p);
                    if (Dungeon.level.passable[cell] &&
                        !level.visited[cell] &&
                        !Dungeon.level.secret[cell] &&
                        Actor.FindChar(cell) == null)
                    {
                        candidates.Add(cell);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                TeleportHero(hero);
            }
            else
            {
                int pos = Rnd.Element(candidates);
                bool secretDoor = false;
                int doorPos = -1;
                if (level.Room(pos) is SpecialRoom)
                {
                    SpecialRoom room = (SpecialRoom)level.Room(pos);
                    if (room.Entrance() != null)
                    {
                        doorPos = level.PointToCell(room.Entrance());
                        foreach (int i in PathFinder.NEIGHBORS8)
                        {
                            if (!room.Inside(level.CellToPoint(doorPos + i)) &&
                                Dungeon.level.passable[doorPos + i] &&
                                Actor.FindChar(doorPos + i) == null)
                            {
                                secretDoor = room is SecretRoom;
                                pos = doorPos + i;
                                break;
                            }
                        }
                    }
                }
                GLog.Information(Messages.Get(typeof(ScrollOfTeleportation), "tele"));
                Appear(hero, pos);
                Dungeon.level.OccupyCell(hero);
                if (secretDoor && level.map[doorPos] == Terrain.SECRET_DOOR)
                {
                    Sample.Instance.Play(Assets.Sounds.SECRET);
                    int oldValue = Dungeon.level.map[doorPos];
                    GameScene.DiscoverTile(doorPos, oldValue);
                    Dungeon.level.Discover(doorPos);
                    ScrollOfMagicMapping.Discover(doorPos);
                }
                Dungeon.Observe();
                GameScene.UpdateFog();
            }
        }

        public static void Appear(Character ch, int pos)
        {
            ch.sprite.InterruptMotion();

            if (Dungeon.level.heroFOV[pos] || Dungeon.level.heroFOV[ch.pos])
            {
                Sample.Instance.Play(Assets.Sounds.TELEPORT);
            }

            ch.Move(pos);
            if (ch.pos == pos)
                ch.sprite.Place(pos);

            if (ch.invisible == 0)
            {
                ch.sprite.Alpha(0);
                ch.sprite.parent.Add(new AlphaTweener(ch.sprite, 1, 0.4f));
            }

            if (Dungeon.level.heroFOV[pos] || ch == Dungeon.hero)
            {
                ch.sprite.Emitter().Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);
            }
        }

        public override int Value()
        {
            return IsKnown() ? 30 * quantity : base.Value();
        }
    }
}