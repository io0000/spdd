using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.items.wands;
using spdd.levels.traps;
using spdd.messages;
using spdd.sprites;
using spdd.utils;

namespace spdd.items.weapon.missiles
{
    public class ForceCube : MissileWeapon
    {
        public ForceCube()
        {
            image = ItemSpriteSheet.FORCE_CUBE;

            tier = 5;
            baseUses = 5;

            sticky = false;
        }

        public override void HitSound(float pitch)
        {
            //no hitsound as it never hits enemies directly
        }

        public override void OnThrow(int cell)
        {
            if (Dungeon.level.pit[cell])
            {
                base.OnThrow(cell);
                return;
            }

            Dungeon.level.PressCell(cell);

            List<Character> targets = new List<Character>();
            var ch = Actor.FindChar(cell);
            if (ch != null)
                targets.Add(ch);

            foreach (int i in PathFinder.NEIGHBORS8)
            {
                if (!(Dungeon.level.traps[cell + i] is TenguDartTrap))
                    Dungeon.level.PressCell(cell + i);

                ch = Actor.FindChar(cell + i);
                if (ch != null)
                    targets.Add(ch);
            }

            foreach (Character target in targets)
            {
                curUser.Shoot(target, this);
                if (target == Dungeon.hero && !target.IsAlive())
                {
                    Dungeon.Fail(this.GetType());
                    GLog.Negative(Messages.Get(this, "ondeath"));
                }
            }

            RangedHit(null, cell);

            WandOfBlastWave.BlastWave.Blast(cell);
            Sample.Instance.Play(Assets.Sounds.BLAST);
        }
    }
}