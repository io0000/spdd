using watabou.noosa;

namespace spdd.ui
{
    public class BusyIndicator : Image
    {
        public BusyIndicator()
        {
            Copy(Icons.BUSY.Get());

            origin.Set(width / 2, height / 2);
            angularSpeed = 720;
        }

        public override void Update()
        {
            base.Update();
            visible = Dungeon.hero.IsAlive() && !Dungeon.hero.ready;
        }
    }
}