using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.painters;

namespace spdd.levels.rooms.standard
{
    public class PlatformRoom : StandardRoom
    {
        public override int MinWidth()
        {
            return Math.Max(base.MinWidth(), 6);
        }

        public override int MinHeight()
        {
            return Math.Max(base.MinHeight(), 6);
        }

        public override float[] SizeCatProbs()
        {
            return new float[] { 6, 3, 1 };
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);

            Painter.Fill(level, this, 1, Terrain.CHASM);

            List<Rect> platforms = new List<Rect>();
            SplitPlatforms(new Rect(left + 2, top + 2, right - 2, bottom - 2), platforms);

            foreach (Rect platform in platforms)
            {
                Painter.Fill(level, platform.left, platform.top, platform.Width() + 1, platform.Height() + 1, Terrain.EMPTY_SP);
            }

            foreach (Door door in connected.Values)
            {
                door.Set(Door.Type.REGULAR);
                Painter.DrawInside(level, this, door, 2, Terrain.EMPTY_SP);
            }
        }

        private void SplitPlatforms(Rect curPlatform, List<Rect> allPlatforms)
        {
            int curArea = (curPlatform.Width() + 1) * (curPlatform.Height() + 1);

            //chance to split scales between 0% and 100% between areas of 25 and 36
            if (Rnd.Float() < (curArea - 25) / 11f)
            {
                if (curPlatform.Width() > curPlatform.Height() || (curPlatform.Width() == curPlatform.Height() && Rnd.Int(2) == 0))
                {
                    //split the platform
                    int splitX = Rnd.IntRange(curPlatform.left + 2, curPlatform.right - 2);
                    SplitPlatforms(new Rect(curPlatform.left, curPlatform.top, splitX - 1, curPlatform.bottom), allPlatforms);
                    SplitPlatforms(new Rect(splitX + 1, curPlatform.top, curPlatform.right, curPlatform.bottom), allPlatforms);

                    //add a bridge between
                    int bridgeY = Rnd.NormalIntRange(curPlatform.top, curPlatform.bottom);
                    allPlatforms.Add(new Rect(splitX - 1, bridgeY, splitX + 1, bridgeY));
                }
                else
                {
                    //split the platform
                    int splitY = Rnd.IntRange(curPlatform.top + 2, curPlatform.bottom - 2);
                    SplitPlatforms(new Rect(curPlatform.left, curPlatform.top, curPlatform.right, splitY - 1), allPlatforms);
                    SplitPlatforms(new Rect(curPlatform.left, splitY + 1, curPlatform.right, curPlatform.bottom), allPlatforms);

                    //add a bridge between
                    int bridgeX = Rnd.NormalIntRange(curPlatform.left, curPlatform.right);
                    allPlatforms.Add(new Rect(bridgeX, splitY - 1, bridgeX, splitY + 1));
                }
            }
            else
            {
                allPlatforms.Add(curPlatform);
            }
        }
    }
}