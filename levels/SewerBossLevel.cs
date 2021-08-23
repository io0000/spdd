using System.Collections.Generic;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.items;
using spdd.levels.builders;
using spdd.levels.painters;
using spdd.levels.rooms;
using spdd.levels.rooms.secret;
using spdd.levels.rooms.sewerboss;
using spdd.levels.rooms.standard;
using spdd.scenes;

namespace spdd.levels
{
    public class SewerBossLevel : SewerLevel
    {
        public SewerBossLevel()
        {
            color1 = new Color(0x48, 0x76, 0x3c, 0xFF);
            color2 = new Color(0x59, 0x99, 0x4a, 0xFF);
        }

        private int stairs;

        public override List<Room> InitRooms()
        {
            List<Room> initRooms = new List<Room>();

            initRooms.Add(roomEntrance = new SewerBossEntranceRoom());
            initRooms.Add(roomExit = new SewerBossExitRoom());

            int standards = StandardRooms();
            for (int i = 0; i < standards; ++i)
            {
                StandardRoom s = StandardRoom.CreateRoom();
                //force to normal size
                s.SetSizeCat(0, 0);
                initRooms.Add(s);
            }

            GooBossRoom gooRoom = GooBossRoom.RandomGooRoom();
            initRooms.Add(gooRoom);
            ((FigureEightBuilder)builder).SetLandmarkRoom(gooRoom);
            initRooms.Add(new RatKingRoom());
            return initRooms;
        }

        protected override int StandardRooms()
        {
            //2 to 3, average 2.5
            return 2 + Rnd.Chances(new float[] { 1, 1 });
        }

        protected override Builder Builder()
        {
            return new FigureEightBuilder()
                    .SetLoopShape(2, Rnd.Float(0.4f, 0.7f), Rnd.Float(0f, 0.5f))
                    .SetPathLength(1f, new float[] { 1 })
                    .SetTunnelLength(new float[] { 1, 2 }, new float[] { 1 });
        }

        protected override Painter Painter()
        {
            return new SewerPainter()
                    .SetWater(0.50f, 5)
                    .SetGrass(0.20f, 4)
                    .SetTraps(NTraps(), TrapClasses(), TrapChances());
        }

        protected override int NTraps()
        {
            return 0;
        }

        public override void CreateMobs()
        { }

        public override Actor AddRespawner()
        {
            return null;
        }

        public override void CreateItems()
        {
            Item item = Bones.Get();
            if (item != null)
            {
                int pos;
                do
                {
                    pos = PointToCell(roomEntrance.Random());
                }
                while (pos == entrance || solid[pos]);

                Drop(item, pos).SetHauntedIfCursed().type = Heap.Type.REMAINS;
            }
        }

        public override int RandomRespawnCell(Character ch)
        {
            int pos;
            do
            {
                pos = PointToCell(roomEntrance.Random());
            }
            while (pos == entrance ||
                   !passable[pos] ||
                   (Character.HasProp(ch, Character.Property.LARGE) && !openSpace[pos]) ||
                   Actor.FindChar(pos) != null);
            return pos;
        }

        public override void Seal()
        {
            if (entrance != 0)
            {
                base.Seal();

                Set(entrance, Terrain.WATER);
                GameScene.UpdateMap(entrance);
                GameScene.Ripple(entrance);

                stairs = entrance;
                entrance = 0;
            }
        }

        public override void Unseal()
        {
            if (stairs != 0)
            {
                base.Unseal();

                entrance = stairs;
                stairs = 0;

                Set(entrance, Terrain.ENTRANCE);
                GameScene.UpdateMap(entrance);
            }
        }

        public override Group AddVisuals()
        {
            base.AddVisuals();
            if (map[exit - 1] != Terrain.WALL_DECO)
                visuals.Add(new PrisonLevel.Torch(exit - 1));
            if (map[exit + 1] != Terrain.WALL_DECO)
                visuals.Add(new PrisonLevel.Torch(exit + 1));
            return visuals;
        }

        private const string STAIRS = "stairs";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(STAIRS, stairs);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            stairs = bundle.GetInt(STAIRS);
            roomExit = roomEntrance;
        }
    }
}