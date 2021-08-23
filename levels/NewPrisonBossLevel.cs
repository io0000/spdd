using System;
using System.Collections.Generic;
using System.Linq;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.tweeners;
using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.effects;
using spdd.items;
using spdd.items.keys;
using spdd.items.weapon.missiles;
using spdd.levels.painters;
using spdd.levels.traps;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.tiles;
using spdd.ui;
using spdd.utils;

namespace spdd.levels
{
    public class NewPrisonBossLevel : Level
    {
        public NewPrisonBossLevel()
        {
            color1 = new Color(0x6a, 0x72, 0x3d, 0xff);
            color2 = new Color(0x88, 0x92, 0x4c, 0xff);

            //the player should be able to see all of Tengu's arena
            viewDistance = 12;
        }

        public enum State
        {
            START,
            FIGHT_START,
            TRAP_MAZES, //pre-0.8.1 saves
            FIGHT_PAUSE,
            FIGHT_ARENA,
            WON
        }

        private State state;
        private NewTengu tengu;

        public State GetState()
        {
            return state;
        }

        public override string TilesTex()
        {
            return Assets.Environment.TILES_PRISON;
        }

        public override string WaterTex()
        {
            return Assets.Environment.WATER_PRISON;
        }

        private const string STATE = "state";
        private const string TENGU = "tengu";
        private const string STORED_ITEMS = "storeditems";
        private const string TRIGGERED = "triggered";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(STATE, state.ToString());    // enum
            bundle.Put(TENGU, tengu);
            bundle.Put(STORED_ITEMS, storedItems);
            bundle.Put(TRIGGERED, triggered);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            state = bundle.GetEnum<State>(STATE);

            //in some states tengu won't be in the world, in others he will be.
            if (state == State.START ||
                state == State.TRAP_MAZES ||
                state == State.FIGHT_PAUSE)
            {
                tengu = (NewTengu)bundle.Get(TENGU);
            }
            else
            {
                foreach (Mob mob in mobs)
                {
                    if (mob is NewTengu)
                    {
                        tengu = (NewTengu)mob;
                        break;
                    }
                }
            }

            foreach (var item in bundle.GetCollection(STORED_ITEMS))
            {
                storedItems.Add((Item)item);
            }

            triggered = bundle.GetBooleanArray(TRIGGERED);
        }

        public override bool Build()
        {
            SetSize(32, 32);

            state = State.START;
            SetMapStart();

            return true;
        }

        private const int ENTRANCE_POS = 10 + 4 * 32;
        private static Rect entranceRoom = new Rect(8, 2, 13, 8);
        private static Rect startHallway = new Rect(9, 7, 12, 24);
        private static Rect[] startCells = new Rect[]
        {
            new Rect(5, 9, 10, 16),
            new Rect(11, 9, 16, 16),
            new Rect(5, 15, 10, 22),
            new Rect(11, 15, 16, 22)
        };

        private static Rect tenguCell = new Rect(6, 23, 15, 32);
        private static Point tenguCellCenter = new Point(10, 27);
        private static Point tenguCellDoor = new Point(10, 23);
        private static Point[] startTorches = new Point[]
        {
            new Point(10, 2),
            new Point(7, 9),
            new Point(13, 9),
            new Point(7, 15),
            new Point(13, 15),
            new Point(8, 23),
            new Point(12, 23)
        };

        private void SetMapStart()
        {
            entrance = ENTRANCE_POS;
            exit = 0;

            Painter.Fill(this, 0, 0, 32, 32, Terrain.WALL);

            //Start
            Painter.Fill(this, entranceRoom, Terrain.WALL);
            Painter.Fill(this, entranceRoom, 1, Terrain.EMPTY);
            Painter.Set(this, entrance, Terrain.ENTRANCE);

            Painter.Fill(this, startHallway, Terrain.WALL);
            Painter.Fill(this, startHallway, 1, Terrain.EMPTY);

            Painter.Set(this, startHallway.left + 1, startHallway.top, Terrain.DOOR);

            foreach (Rect r in startCells)
            {
                Painter.Fill(this, r, Terrain.WALL);
                Painter.Fill(this, r, 1, Terrain.EMPTY);
            }

            Painter.Set(this, startHallway.left, startHallway.top + 5, Terrain.DOOR);
            Painter.Set(this, startHallway.right - 1, startHallway.top + 5, Terrain.DOOR);
            Painter.Set(this, startHallway.left, startHallway.top + 11, Terrain.DOOR);
            Painter.Set(this, startHallway.right - 1, startHallway.top + 11, Terrain.DOOR);

            Painter.Fill(this, tenguCell, Terrain.WALL);
            Painter.Fill(this, tenguCell, 1, Terrain.EMPTY);

            Painter.Set(this, tenguCell.left + 4, tenguCell.top, Terrain.LOCKED_DOOR);

            foreach (Point p in startTorches)
            {
                Painter.Set(this, p, Terrain.WALL_DECO);
            }
        }

        //area where items/chars are preserved when moving to the arena
        private static Rect pauseSafeArea = new Rect(9, 2, 12, 12);

        private void SetMapPause()
        {
            SetMapStart();

            exit = entrance = 0;

            Painter.Set(this, tenguCell.left + 4, tenguCell.top, Terrain.DOOR);

            Painter.Fill(this, startCells[1].left, startCells[1].top + 3, 1, 7, Terrain.EMPTY);
            Painter.Fill(this, startCells[1].left + 2, startCells[1].top + 2, 3, 10, Terrain.EMPTY);

            Painter.Fill(this, entranceRoom, Terrain.WALL);
            Painter.Set(this, startHallway.left + 1, startHallway.top, Terrain.EMPTY);
            Painter.Set(this, startHallway.left + 1, startHallway.top + 1, Terrain.DOOR);
        }

        private static Rect arena = new Rect(3, 1, 18, 16);

        private void SetMapArena()
        {
            exit = entrance = 0;

            Painter.Fill(this, 0, 0, 32, 32, Terrain.WALL);

            Painter.Fill(this, arena, Terrain.WALL);
            Painter.FillEllipse(this, arena, 1, Terrain.EMPTY);
        }

        private const int W = Terrain.WALL;
        private const int D = Terrain.WALL_DECO;
        private const int e = Terrain.EMPTY;
        private const int E = Terrain.EXIT;
        private const int C = Terrain.CHASM;

        private static Point endStart = new Point(startHallway.left + 2, startHallway.top + 2);
        private static Point levelExit = new Point(endStart.x + 12, endStart.y + 6);
        private static int[] endMap = new int[]{
            W, W, D, W, W, W, W, W, W, W, W, W, W, W,
            W, e, e, e, W, W, W, W, W, W, W, W, W, W,
            W, e, e, e, e, e, e, e, e, W, W, W, W, W,
            e, e, e, e, e, e, e, e, e, e, e, e, W, W,
            e, e, e, e, e, e, e, e, e, e, e, e, e, W,
            e, e, e, C, C, C, C, C, C, C, C, e, e, W,
            e, W, C, C, C, C, C, C, C, C, C, E, E, W,
            e, e, e, C, C, C, C, C, C, C, C, E, E, W,
            e, e, e, e, e, C, C, C, C, C, C, E, E, W,
            e, e, e, e, e, e, e, W, W, W, C, C, C, W,
            W, e, e, e, e, e, W, W, W, W, C, C, C, W,
            W, e, e, e, e, W, W, W, W, W, W, C, C, W,
            W, W, W, W, W, W, W, W, W, W, W, C, C, W,
            W, W, W, W, W, W, W, W, W, W, W, C, C, W,
            W, D, W, W, W, W, W, W, W, W, W, C, C, W,
            e, e, e, W, W, W, W, W, W, W, W, C, C, W,
            e, e, e, W, W, W, W, W, W, W, W, C, C, W,
            e, e, e, W, W, W, W, W, W, W, W, C, C, W,
            e, e, e, W, W, W, W, W, W, W, W, C, C, W,
            e, e, e, W, W, W, W, W, W, W, W, C, C, W,
            e, e, e, W, W, W, W, W, W, W, W, C, C, W,
            e, e, e, W, W, W, W, W, W, W, W, C, C, W,
            W, W, W, W, W, W, W, W, W, W, W, C, C, W
        };

        private void SetMapEnd()
        {
            Painter.Fill(this, 0, 0, 32, 32, Terrain.WALL);

            SetMapStart();

            foreach (Heap h in heaps.Values)
            {
                if (h.Peek() is IronKey)
                {
                    h.Destroy();
                }
            }

            CustomTilemap vis = new ExitVisual();
            vis.Pos(11, 10);
            customTiles.Add(vis);
            GameScene.Add(vis, false);

            vis = new ExitVisualWalls();
            vis.Pos(11, 10);
            customWalls.Add(vis);
            GameScene.Add(vis, true);

            Painter.Set(this, tenguCell.left + 4, tenguCell.top, Terrain.DOOR);

            int cell = PointToCell(endStart);
            int i = 0;
            while (cell < Length())
            {
                Array.Copy(endMap, i, map, cell, 14);
                i += 14;
                cell += Width();
            }

            exit = PointToCell(levelExit);
        }

        //keep track of removed items as the level is changed. Dump them back into the level at the end.
        private List<Item> storedItems = new List<Item>();

        private void ClearEntities(Rect safeArea)
        {
            foreach (Heap heap in heaps.Values.ToList())
            {
                if (safeArea == null || !safeArea.Inside(CellToPoint(heap.pos)))
                {
                    storedItems.AddRange(heap.items);
                    heap.Destroy();
                }
            }

            foreach (var b in Dungeon.hero.Buffs<HeavyBoomerang.CircleBack>())
            {
                if (safeArea == null || !safeArea.Inside(CellToPoint(b.ReturnPos())))
                {
                    storedItems.Add(b.Cancel());
                }
            }

            foreach (Mob mob in Dungeon.level.mobs.ToList())
            {
                if (mob != tengu && (safeArea == null || !safeArea.Inside(CellToPoint(mob.pos))))
                {
                    mob.Destroy();
                    if (mob.sprite != null)
                        mob.sprite.KillAndErase();
                }
            }

            foreach (Plant plant in plants.Values.ToList())
            {
                if (safeArea == null || !safeArea.Inside(CellToPoint(plant.pos)))
                {
                    plants.Remove(plant.pos);
                }
            }
        }

        private void CleanMapState()
        {
            BuildFlagMaps();
            CleanWalls();

            BArray.SetFalse(visited);
            BArray.SetFalse(mapped);

            foreach (Blob blob in blobs.Values)
            {
                blob.FullyClear();
            }
            AddVisuals(); //this also resets existing visuals
            traps.Clear();

            GameScene.ResetMap();
            Dungeon.Observe();
        }

        public override Group AddVisuals()
        {
            base.AddVisuals();
            PrisonLevel.AddPrisonVisuals(this, visuals);
            return visuals;
        }

        public void Progress()
        {
            switch (state)
            {
                case State.START:

                    int tenguPos = PointToCell(tenguCellCenter);

                    //if something is occupying Tengu's space, try to put him in an adjacent cell
                    if (Actor.FindChar(tenguPos) != null)
                    {
                        List<int> candidates = new List<int>();
                        foreach (int i in PathFinder.NEIGHBORS8)
                        {
                            if (Actor.FindChar(tenguPos + i) == null)
                            {
                                candidates.Add(tenguPos + i);
                            }
                        }

                        if (candidates.Count > 0)
                        {
                            tenguPos = Rnd.Element(candidates);
                            //if there are no adjacent cells, wait and do nothing
                        }
                        else
                        {
                            return;
                        }
                    }

                    Seal();
                    Set(PointToCell(tenguCellDoor), Terrain.LOCKED_DOOR);
                    GameScene.UpdateMap(PointToCell(tenguCellDoor));

                    foreach (Mob m in mobs)
                    {
                        //bring the first ally with you
                        if (m.alignment == Character.Alignment.ALLY && !m.Properties().Contains(Character.Property.IMMOVABLE))
                        {
                            m.pos = PointToCell(tenguCellDoor); //they should immediately walk out of the door
                            m.sprite.Place(m.pos);
                            break;
                        }
                    }

                    tengu.state = tengu.HUNTING;
                    tengu.pos = tenguPos;
                    GameScene.Add(tengu);
                    tengu.Notice();

                    state = State.FIGHT_START;
                    break;

                case State.FIGHT_START:

                    ClearEntities(tenguCell); //clear anything not in tengu's cell

                    SetMapPause();
                    CleanMapState();

                    Doom d = tengu.FindBuff<Doom>();
                    Actor.Remove(tengu);
                    mobs.Remove(tengu);
                    TargetHealthIndicator.instance.Target(null);
                    tengu.sprite.Kill();
                    if (d != null)
                        tengu.Add(d);

                    GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                    Sample.Instance.Play(Assets.Sounds.BLAST);

                    state = State.FIGHT_PAUSE;
                    break;

                case State.TRAP_MAZES: //for pre-0.8.1 saves
                case State.FIGHT_PAUSE:

                    Dungeon.hero.Interrupt();

                    ClearEntities(pauseSafeArea);

                    SetMapArena();
                    CleanMapState();

                    tengu.state = tengu.HUNTING;
                    tengu.pos = (arena.left + arena.Width() / 2) + Width() * (arena.top + 2);
                    GameScene.Add(tengu);
                    tengu.Notice();

                    GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                    Sample.Instance.Play(Assets.Sounds.BLAST);

                    state = State.FIGHT_ARENA;
                    break;

                case State.FIGHT_ARENA:

                    Unseal();

                    Dungeon.hero.Interrupt();
                    Dungeon.hero.pos = tenguCell.left + 4 + (tenguCell.top + 2) * Width();
                    Dungeon.hero.sprite.InterruptMotion();
                    Dungeon.hero.sprite.Place(Dungeon.hero.pos);
                    Camera.main.SnapTo(Dungeon.hero.sprite.Center());

                    tengu.pos = PointToCell(tenguCellCenter);
                    tengu.sprite.Place(tengu.pos);

                    //remove all mobs, but preserve allies
                    List<Mob> allies = new List<Mob>();
                    foreach (Mob m in mobs.ToList())
                    {
                        if (m.alignment == Character.Alignment.ALLY && !m.Properties().Contains(Character.Property.IMMOVABLE))
                        {
                            allies.Add(m);
                            mobs.Remove(m);
                        }
                    }

                    SetMapEnd();

                    foreach (Mob m in allies)
                    {
                        do
                        {
                            m.pos = RandomTenguCellPos();
                        }
                        while (FindMob(m.pos) != null);

                        if (m.sprite != null)
                            m.sprite.Place(m.pos);

                        mobs.Add(m);
                    }

                    tengu.Die(Dungeon.hero);

                    ClearEntities(tenguCell);
                    CleanMapState();

                    foreach (Item item in storedItems)
                    {
                        if (!(item is NewTengu.BombAbility.BombItem) &&
                            !(item is NewTengu.ShockerAbility.ShockerItem))
                        {
                            Drop(item, RandomTenguCellPos());
                        }
                    }

                    GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                    Sample.Instance.Play(Assets.Sounds.BLAST);

                    state = State.WON;
                    break;
            }
        }

        private bool[] triggered = new bool[] { false, false, false, false };

        public override void OccupyCell(Character ch)
        {
            base.OccupyCell(ch);

            if (ch == Dungeon.hero)
            {
                switch (state)
                {
                    case State.START:
                        if (CellToPoint(ch.pos).y > tenguCell.top)
                        {
                            Progress();
                        }
                        break;
                    case State.TRAP_MAZES: //pre-0.8.1
                    case State.FIGHT_PAUSE:
                        if (CellToPoint(ch.pos).y <= startHallway.top + 1)
                        {
                            Progress();
                        }
                        break;
                }
            }
        }

        public override void CreateMobs()
        {
            tengu = new NewTengu(); //We want to keep track of tengu independently of other mobs, he's not always in the level.
        }

        public override Actor AddRespawner()
        {
            return null;
        }

        public override void CreateItems()
        {
            Item item = Bones.Get();
            if (item != null)
            {
                Drop(item, RandomRespawnCell(null)).SetHauntedIfCursed().type = Heap.Type.REMAINS;
            }
            Drop(new IronKey(10), RandomPrisonCellPos());
        }

        private int RandomPrisonCellPos()
        {
            Rect room = startCells[Rnd.Int(startCells.Length)];

            return Rnd.IntRange(room.left + 1, room.right - 2)
                    + Width() * Rnd.IntRange(room.top + 1, room.bottom - 2);
        }

        public int RandomTenguCellPos()
        {
            return Rnd.IntRange(tenguCell.left + 1, tenguCell.right - 2)
                    + Width() * Rnd.IntRange(tenguCell.top + 1, tenguCell.bottom - 2);
        }

        public void CleanTenguCell()
        {
            traps.Clear();
            Painter.Fill(this, tenguCell, 1, Terrain.EMPTY);
            BuildFlagMaps();
        }

        public void PlaceTrapsInTenguCell(float fill)
        {
            foreach (CustomTilemap vis in customTiles.ToList())
            {
                if (vis is FadingTraps)
                {
                    ((FadingTraps)vis).Remove();
                }
            }

            Point tenguPoint = CellToPoint(tengu.pos);
            Point heroPoint = CellToPoint(Dungeon.hero.pos);

            PathFinder.SetMapSize(7, 7);

            int tenguPos = tenguPoint.x - (tenguCell.left + 1) + (tenguPoint.y - (tenguCell.top + 1)) * 7;
            int heroPos = heroPoint.x - (tenguCell.left + 1) + (heroPoint.y - (tenguCell.top + 1)) * 7;

            bool[] trapsPatch;

            do
            {
                trapsPatch = Patch.Generate(7, 7, fill, 0, false);

                PathFinder.BuildDistanceMap(tenguPos, BArray.Not(trapsPatch, null));
                //note that the effective range of fill is 40%-90%
                //so distance to tengu starts at 3-6 tiles and scales up to 7-8 as fill increases
            }
            while ((PathFinder.distance[heroPos] < Math.Ceiling(7 * fill)) ||
                    (PathFinder.distance[heroPos] > Math.Ceiling(4 + 4 * fill)));

            PathFinder.SetMapSize(Width(), Height());

            for (int i = 0; i < trapsPatch.Length; ++i)
            {
                if (trapsPatch[i])
                {
                    int x = i % 7;
                    int y = i / 7;
                    int cell = x + tenguCell.left + 1 + (y + tenguCell.top + 1) * Width();
                    if (Blob.VolumeAt(cell, typeof(StormCloud)) == 0 &&
                        Blob.VolumeAt(cell, typeof(Regrowth)) <= 9 &&
                        Dungeon.level.plants[cell] == null &&
                        Actor.FindChar(cell) == null)
                    {
                        Level.Set(cell, Terrain.SECRET_TRAP);
                        SetTrap(new TenguDartTrap().Hide(), cell);
                        CellEmitter.Get(cell).Burst(Speck.Factory(Speck.LIGHT), 2);
                    }
                }
            }

            GameScene.UpdateMap();

            FadingTraps t = new FadingTraps();
            t.fadeDelay = 2f;
            t.SetCoveringArea(tenguCell);
            GameScene.Add(t, false);
            customTiles.Add(t);
        }

        public override int RandomRespawnCell(Character ch)
        {
            int pos = ENTRANCE_POS; //random cell adjacent to the entrance.
            int cell;
            do
            {
                cell = pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
            }
            while (!passable[cell] ||
                    (Character.HasProp(ch, Character.Property.LARGE) && !openSpace[cell]) ||
                    Actor.FindChar(cell) != null);

            return cell;
        }

        public override string TileName(int tile)
        {
            switch (tile)
            {
                case Terrain.WATER:
                    return Messages.Get(typeof(PrisonLevel), "water_name");
                default:
                    return base.TileName(tile);
            }
        }

        public override string TileDesc(int tile)
        {
            switch (tile)
            {
                case Terrain.EMPTY_DECO:
                    return Messages.Get(typeof(PrisonLevel), "empty_deco_desc");
                case Terrain.BOOKSHELF:
                    return Messages.Get(typeof(PrisonLevel), "bookshelf_desc");
                default:
                    return base.TileDesc(tile);
            }
        }

        //TODO consider making this external to the prison boss level
        [SPDStatic]
        public class FadingTraps : CustomTilemap
        {
            public FadingTraps()
            {
                texture = Assets.Environment.TERRAIN_FEATURES;
            }

            Rect area;

            internal float fadeDuration = 1f;
            internal float initialAlpha = .4f;
            internal float fadeDelay = 1f;

            public void SetCoveringArea(Rect area)
            {
                tileX = area.left;
                tileY = area.top;
                tileH = area.bottom - area.top;
                tileW = area.right - area.left;

                this.area = area;
            }

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int[] data = new int[tileW * tileH];
                int cell;
                Trap t;
                int i = 0;
                for (int y = tileY; y < tileY + tileH; ++y)
                {
                    cell = tileX + y * Dungeon.level.Width();
                    for (int x = tileX; x < tileX + tileW; ++x)
                    {
                        t = Dungeon.level.traps[cell];
                        if (t != null)
                        {
                            data[i] = t.color + t.shape * 16;
                        }
                        else
                        {
                            data[i] = -1;
                        }
                        ++cell;
                        ++i;
                    }
                }

                v.Map(data, tileW);
                SetFade();
                return v;
            }

            public override string Name(int tileX, int tileY)
            {
                int cell = (this.tileX + tileX) + Dungeon.level.Width() * (this.tileY + tileY);
                if (Dungeon.level.traps[cell] != null)
                {
                    return Messages.TitleCase(Dungeon.level.traps[cell].name);
                }
                return base.Name(tileX, tileY);
            }

            public override string Desc(int tileX, int tileY)
            {
                int cell = (this.tileX + tileX) + Dungeon.level.Width() * (this.tileY + tileY);
                if (Dungeon.level.traps[cell] != null)
                {
                    return Dungeon.level.traps[cell].Desc();
                }
                return base.Desc(tileX, tileY);
            }

            private void SetFade()
            {
                if (vis == null)
                    return;

                vis.Alpha(initialAlpha);

                var actor = new ActionActor();
                actor.actPriority = Actor.HERO_PRIO + 1;
                actor.action = () =>
                {
                    Actor.Remove(actor);

                    if (vis != null && vis.parent != null)
                    {
                        Dungeon.level.customTiles.Remove(this);
                        var tweener = new FadingTrapsActorAlphaTweener(vis, 0f, fadeDuration);
                        vis.parent.Add(tweener);
                    }

                    return true;
                };
                Actor.AddDelayed(actor, fadeDelay);
            }

            public class FadingTrapsActorAlphaTweener : AlphaTweener
            {
                public FadingTrapsActorAlphaTweener(Visual image, float alpha, float time)
                    : base(image, alpha, time)
                { }

                public override void OnComplete()
                {
                    Tilemap vis = (Tilemap)target;

                    base.OnComplete();
                    vis.KillAndErase();
                    KillAndErase();
                }
            }

            public void Remove()
            {
                if (vis != null)
                {
                    vis.KillAndErase();
                }
                Dungeon.level.customTiles.Remove(this);
            }
        }

        [SPDStatic]
        public class ExitVisual : CustomTilemap
        {
            public ExitVisual()
            {
                texture = Assets.Environment.PRISON_EXIT_NEW;

                tileW = 14;
                tileH = 11;
            }

            const int TEX_WIDTH = 256;

            private static byte[] render = new byte[]{
                0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0,
                1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
                1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
                1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
                1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0,
                1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0,
                1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0,
                0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0,
                0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int[] data = MapSimpleImage(0, 0, TEX_WIDTH);
                for (int i = 0; i < data.Length; ++i)
                {
                    if (render[i] == 0)
                        data[i] = -1;
                }
                v.Map(data, tileW);
                return v;
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                tileX = 11;
                tileY = 10;
                tileW = 14;
                tileH = 11;
            }
        }

        [SPDStatic]
        public class ExitVisualWalls : CustomTilemap
        {
            public ExitVisualWalls()
            {
                texture = Assets.Environment.PRISON_EXIT_NEW;

                tileW = 14;
                tileH = 22;
            }

            const int TEX_WIDTH = 256;

            private static byte[] render = new byte[]{
                0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1,
                1, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1,
                0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1
            };

            public override Tilemap Create()
            {
                Tilemap v = base.Create();
                int[] data = MapSimpleImage(0, 10, TEX_WIDTH);
                for (int i = 0; i < data.Length; ++i)
                {
                    if (render[i] == 0)
                        data[i] = -1;
                }
                v.Map(data, tileW);
                return v;
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                tileX = 11;
                tileY = 10;
                tileW = 14;
                tileH = 22;
            }
        }
    }
}