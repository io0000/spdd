using watabou.utils;
using spdd.effects;
using spdd.actors;
using spdd.actors.buffs;

namespace spdd.levels.traps
{
    public class OozeTrap : Trap
    {
        public OozeTrap()
        {
            color = GREEN;
            shape = DOTS;
        }

        public override void Activate()
        {
            foreach (int i in PathFinder.NEIGHBORS9)
            {
                if (!Dungeon.level.solid[pos + i])
                {
                    Splash.At(pos + i, new Color(0x00, 0x00, 0x00, 0xFF), 5);
                    var ch = Actor.FindChar(pos + i);
                    if (ch != null && !ch.flying)
                    {
                        Buff.Affect<Ooze>(ch).Set(Ooze.DURATION);
                    }
                }
            }
        }
    }
}