using System;
using System.Collections.Generic;
using spdd.actors;
using spdd.levels;

namespace spdd.mechanics
{
    public class Ballistic
    {
        //note that the path is the FULL path of the projectile, including tiles after collision.
        //make sure to generate a subPath for the common case of going source to collision.
        public List<int> path = new List<int>();
        public int sourcePos;
        public int collisionPos;
        public int dist;           // path array에서 collisionPos의 index

        //parameters to specify the colliding cell
        public const int STOP_TARGET = 1;    //ballistica will stop at the target cell
        public const int STOP_CHARS = 2;     //ballistica will stop on first char hit
        public const int STOP_TERRAIN = 4;   //ballistica will stop on solid terrain
        public const int IGNORE_DOORS = 8;   //ballistica will ignore doors instead of colliding

        public const int PROJECTILE = STOP_TARGET | STOP_CHARS | STOP_TERRAIN;

        public const int MAGIC_BOLT = STOP_CHARS | STOP_TERRAIN;

        public const int WONT_STOP = 0;

        public Ballistic(int from, int to, int param)
        {
            sourcePos = from;
            
            collisionPos = -1;      // 충돌위치 미설정, Build함수 내부에서 설정됨

            Build(from, to,
                (param & STOP_TARGET) > 0,
                (param & STOP_CHARS) > 0,
                (param & STOP_TERRAIN) > 0,
                (param & IGNORE_DOORS) > 0);

            if (collisionPos != -1)
            {
                dist = path.IndexOf(collisionPos);
            }
            else if (path.Count > 0)
            {
                dist = path.Count - 1;
                collisionPos = path[dist];
            }
            else
            {
                path.Add(from);
                collisionPos = from;
                dist = 0;
            }
        }

        private void Build(int from, int to, bool stopTarget, bool stopChars, bool stopTerrain, bool ignoreDoors)
        {
            int w = Dungeon.level.Width();

            int x0 = from % w;
            int x1 = to % w;
            int y0 = from / w;
            int y1 = to / w;

            int dx = x1 - x0;
            int dy = y1 - y0;

            int stepX = dx > 0 ? +1 : -1;
            int stepY = dy > 0 ? +1 : -1;

            dx = Math.Abs(dx);
            dy = Math.Abs(dy);

            int stepA;
            int stepB;
            int dA;
            int dB;

            if (dx > dy)
            {
                stepA = stepX;
                stepB = stepY * w;
                dA = dx;
                dB = dy;
            }
            else
            {
                stepA = stepY * w;
                stepB = stepX;
                dA = dy;
                dB = dx;
            }

            int cell = from;

            int err = dA / 2;
            while (Dungeon.level.InsideMap(cell))
            {
                //if we're in a wall, collide with the previous cell along the path.
                //we don't use solid here because we don't want to stop short of closed doors
                if (stopTerrain && cell != sourcePos && !Dungeon.level.passable[cell] && !Dungeon.level.avoid[cell])
                {
                    Collide(path[path.Count - 1]);
                }

                path.Add(cell);

                if ((stopTerrain && cell != sourcePos && Dungeon.level.solid[cell]) ||
                    (cell != sourcePos && stopChars && Actor.FindChar(cell) != null) ||
                    (cell == to && stopTarget))
                {
                    if (!ignoreDoors || Dungeon.level.map[cell] != Terrain.DOOR)
                    {
                        Collide(cell); //only collide if this isn't a door, or we aren't ignoring doors
                    }
                }

                cell += stepA;

                err += dB;
                if (err >= dA)
                {
                    err = err - dA;
                    cell = cell + stepB;
                }
            }
        }

        //we only want to record the first position collision occurs at.
        private void Collide(int cell)
        {
            if (collisionPos == -1)
                collisionPos = cell;
        }

        //returns a segment of the path from start to end, inclusive.
        //if there is an error, returns an empty arraylist instead.
        public List<int> SubPath(int start, int end)
        {
            try
            {
                end = Math.Min(end, path.Count - 1);
                //java - subList(fromIndex, toIndex);
                //    fromIndex: low endpoint(inclusive) of the subList
                //    toIndex: high endpoint(exclusive) of the subList
                //c# - GetRange(int index, int count);

                //return path.subList(start, end + 1);
                int count = end - start + 1;
                return path.GetRange(start, count);
            }
            catch (Exception e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
                return new List<int>();
            }
        }
    }
}