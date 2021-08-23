using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.items.weapon.missiles.darts;
using spdd.mechanics;
using spdd.sprites;

namespace spdd.levels.traps
{
    public class WornDartTrap : Trap
    {
        public WornDartTrap()
        {
            color = GREY;
            shape = CROSSHAIR;

            canBeHidden = false;
        }

        public override void Activate()
        {
            Character target = Actor.FindChar(pos);

            if (target == null)
            {
                float closestDist = float.MaxValue;
                foreach (Character ch in Actor.Chars())
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

            if (target != null)
            {
                Character finalTarget = target;
                WornDartTrap trap = this;
                if (Dungeon.level.heroFOV[pos] || Dungeon.level.heroFOV[target.pos])
                {
                    var actor = new ActionActor();
                    //it's a visual effect, gets priority no matter what
                    actor.actPriority = Actor.VFX_PRIO;
                    actor.action = () =>
                    {
                        Actor toRemove = actor;

                        var callback = new ActionCallback();
                        callback.action = () =>
                        {
                            int dmg = Rnd.NormalIntRange(4, 8) - finalTarget.DrRoll();
                            finalTarget.Damage(dmg, trap);
                            if (finalTarget == Dungeon.hero && !finalTarget.IsAlive())
                                Dungeon.Fail(trap.GetType());

                            Sample.Instance.Play(Assets.Sounds.HIT, 1, 1, Rnd.Float(0.8f, 1.25f));
                            finalTarget.sprite.BloodBurstA(finalTarget.sprite.Center(), dmg);
                            finalTarget.sprite.Flash();

                            Actor.Remove(toRemove);
                            actor.Next();
                        };

                        var missile = ShatteredPixelDungeonDash.Scene().Recycle<MissileSprite>();
                        missile.Reset(trap.pos, finalTarget.sprite, new Dart(), callback);
                        return false;
                    };

                    Actor.Add(actor);
                }
                else
                {
                    finalTarget.Damage(Rnd.NormalIntRange(4, 8) - finalTarget.DrRoll(), trap);
                }
            }
        }
    }
}