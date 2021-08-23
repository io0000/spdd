using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.effects;
using spdd.items;
using spdd.mechanics;
using spdd.messages;
using spdd.tiles;
using spdd.utils;

namespace spdd.levels.traps
{
    public class DisintegrationTrap : Trap
    {
        public DisintegrationTrap()
        {
            color = VIOLET;
            shape = CROSSHAIR;

            canBeHidden = false;
        }

        public override void Activate()
        {
            var target = Actor.FindChar(pos);

            //find the closest char that can be aimed at
            if (target == null)
            {
                float closestDist = float.MaxValue;
                foreach (var ch in Actor.Chars())
                {
                    float curDist = Dungeon.level.TrueDistance(pos, ch.pos);
                    if (ch.invisible > 0)
                        curDist += 1000;

                    Ballistic bolt = new Ballistic(pos, ch.pos, Ballistic.PROJECTILE);
                    if (bolt.collisionPos == ch.pos && curDist < closestDist)
                    {
                        target = ch;
                        closestDist = curDist;
                    }
                }
            }

            Heap heap = Dungeon.level.heaps[pos];
            if (heap != null)
                heap.Explode();

            if (target != null)
            {
                if (Dungeon.level.heroFOV[pos] || Dungeon.level.heroFOV[target.pos])
                {
                    Sample.Instance.Play(Assets.Sounds.RAY);
                    ShatteredPixelDungeonDash.Scene().Add(new Beam.DeathRay(DungeonTilemap.TileCenterToWorld(pos), target.sprite.Center()));
                }
                target.Damage(Rnd.NormalIntRange(30, 50) + Dungeon.depth, this);
                if (target == Dungeon.hero)
                {
                    Hero hero = (Hero)target;
                    if (!hero.IsAlive())
                    {
                        Dungeon.Fail(GetType());
                        GLog.Negative(Messages.Get(this, "ondeath"));
                    }
                }
            }
        }
    }
}