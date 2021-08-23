using watabou.noosa.audio;
using spdd.actors.mobs;
using spdd.utils;
using spdd.effects;
using spdd.messages;

namespace spdd.levels.traps
{
    public class AlarmTrap : Trap
    {
        public AlarmTrap()
        {
            color = RED;
            shape = DOTS;
        }

        public override void Activate()
        {
            foreach (Mob mob in Dungeon.level.mobs)
            {
                mob.Beckon(pos);
            }

            if (Dungeon.level.heroFOV[pos])
            {
                GLog.Warning(Messages.Get(this, "alarm"));
                CellEmitter.Center(pos).Start(Speck.Factory(Speck.SCREAM), 0.3f, 3);
            }

            Sample.Instance.Play(Assets.Sounds.ALERT);
        }
    }
}