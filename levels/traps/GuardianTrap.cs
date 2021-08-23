using watabou.utils;
using watabou.noosa.audio;
using spdd.scenes;
using spdd.effects;
using spdd.utils;
using spdd.sprites;
using spdd.actors.mobs;
using spdd.messages;

namespace spdd.levels.traps
{
    public class GuardianTrap : Trap
    {
        public GuardianTrap()
        {
            color = RED;
            shape = STARS;
        }

        public override void Activate()
        {
            foreach (var mob in Dungeon.level.mobs)
                mob.Beckon(pos);

            if (Dungeon.level.heroFOV[pos])
            {
                GLog.Warning(Messages.Get(this, "alarm"));
                CellEmitter.Center(pos).Start(Speck.Factory(Speck.SCREAM), 0.3f, 3);
            }

            Sample.Instance.Play(Assets.Sounds.ALERT);

            for (int i = 0; i < (Dungeon.depth - 5) / 5; ++i)
            {
                Guardian guardian = new Guardian();
                guardian.state = guardian.WANDERING;
                guardian.pos = Dungeon.level.RandomRespawnCell(guardian);
                GameScene.Add(guardian);
                guardian.Beckon(Dungeon.hero.pos);
            }
        }

        [SPDStatic]
        public class Guardian : Statue
        {
            public Guardian()
            {
                spriteClass = typeof(GuardianSprite);

                EXP = 0;
                state = WANDERING;

                weapon.Enchant(null);
                weapon.Degrade(weapon.GetLevel());
            }

            public override void Beckon(int cell)
            {
                //Beckon works on these ones, unlike their superclass.
                Notice();

                if (state != HUNTING)
                    state = WANDERING;

                target = cell;
            }
        }

        public class GuardianSprite : StatueSprite
        {
            public GuardianSprite()
            {
                Tint(0, 0, 1, 0.2f);
            }

            public override void ResetColor()
            {
                base.ResetColor();
                Tint(0, 0, 1, 0.2f);
            }
        }
    }
}