//using System.IO;
using spdd.sprites;
using spdd.actors.hero;

namespace spdd.items.keys
{
    public class SkeletonKey : Key
    {
        public SkeletonKey()
           : this(0)
        { }

        public SkeletonKey(int depth)
        {
            image = ItemSpriteSheet.SKELETON_KEY;
            this.depth = depth;
        }

        public override bool DoPickUp(Hero hero)
        {
            //if (!SPDSettings.SupportNagged())
            //{
            //    try
            //    {
            //        Dungeon.SaveAll();
            //        ShatteredPixelDungeon.Scene().Add(new WndSupportPrompt());
            //    }
            //    catch (IOException e)
            //    {
            //        ShatteredPixelDungeon.ReportException(e);
            //    }
            //}

            return base.DoPickUp(hero);
        }
    }
}
