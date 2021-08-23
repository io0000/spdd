using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.painters;
using spdd.items.scrolls;

namespace spdd.levels.rooms.secret
{
    public class SecretLibraryRoom : SecretRoom
    {
        public override int MinWidth()
        {
            return Math.Max(7, base.MinWidth());
        }

        public override int MinHeight()
        {
            return Math.Max(7, base.MinHeight());
        }

        private static Dictionary<Type, float> scrollChances = new Dictionary<Type, float>();

        static SecretLibraryRoom()
        {
            scrollChances[typeof(ScrollOfIdentify)] = 1f;
            scrollChances[typeof(ScrollOfRemoveCurse)] = 2f;
            scrollChances[typeof(ScrollOfMirrorImage)] = 3f;
            scrollChances[typeof(ScrollOfRecharging)] = 3f;
            scrollChances[typeof(ScrollOfTeleportation)] = 3f;
            scrollChances[typeof(ScrollOfLullaby)] = 4f;
            scrollChances[typeof(ScrollOfMagicMapping)] = 4f;
            scrollChances[typeof(ScrollOfRage)] = 4f;
            scrollChances[typeof(ScrollOfRetribution)] = 4f;
            scrollChances[typeof(ScrollOfTerror)] = 4f;
            scrollChances[typeof(ScrollOfTransmutation)] = 6f;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.BOOKSHELF);

            Painter.FillEllipse(level, this, 2, Terrain.EMPTY_SP);

            Door entrance = Entrance();
            if (entrance.x == left || entrance.x == right)
            {
                Painter.DrawInside(level, this, entrance, (Width() - 3) / 2, Terrain.EMPTY_SP);
            }
            else
            {
                Painter.DrawInside(level, this, entrance, (Height() - 3) / 2, Terrain.EMPTY_SP);
            }
            entrance.Set(Door.Type.HIDDEN);

            int n = Rnd.IntRange(2, 3);
            Dictionary<Type, float> chances = new Dictionary<Type, float>(scrollChances);
            for (int i = 0; i < n; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != Terrain.EMPTY_SP || level.heaps[pos] != null);

                Type scrollCls = Rnd.Chances(chances);
                chances[scrollCls] = 0f;
                level.Drop((Scroll)Reflection.NewInstance(scrollCls), pos);
            }
        }
    }
}