using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.effects;
using spdd.effects.particles;
using spdd.mechanics;
using spdd.messages;
using spdd.tiles;
using spdd.utils;

namespace spdd.levels.traps
{
    public class GrimTrap : Trap
    {
        public GrimTrap()
        {
            color = GREY;
            shape = LARGE_DOT;

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

            if (target != null)
            {
                var finalTarget = target;
                GrimTrap trap = this;
                int damage;

                if (finalTarget == Dungeon.hero && ((float)finalTarget.HP / finalTarget.HT) >= 0.9f)
                {
                    //almost kill the player
                    damage = finalTarget.HP - 1;
                }
                else
                {
                    //kill 'em
                    damage = finalTarget.HP;
                }

                int finalDmg = damage;

                Actor.Add(new GrimTrapActor(this, finalTarget, finalDmg));
            }
            else
            {
                CellEmitter.Get(pos).Burst(ShadowParticle.Up, 10);
                Sample.Instance.Play(Assets.Sounds.BURNING);
            }
        }

        private class GrimTrapActor : Actor
        {
            GrimTrap trap;
            Character finalTarget;
            int finalDmg;

            public GrimTrapActor(GrimTrap trap, Character finalTarget, int finalDmg)
            {
                //it's a visual effect, gets priority no matter what
                actPriority = VFX_PRIO;

                this.trap = trap;
                this.finalTarget = finalTarget;
                this.finalDmg = finalDmg;
            }

            public override bool Act()
            {
                Actor toRemove = this;

                var missile = finalTarget.sprite.parent.Recycle<MagicMissile>();

                var callback = new ActionCallback();
                callback.action = () =>
                {
                    finalTarget.Damage(finalDmg, trap);
                    if (finalTarget == Dungeon.hero)
                    {
                        Sample.Instance.Play(Assets.Sounds.CURSED);
                        if (!finalTarget.IsAlive())
                        {
                            Dungeon.Fail(typeof(GrimTrap));
                            GLog.Negative(Messages.Get(typeof(GrimTrap), "ondeath"));
                        }
                    }
                    else
                    {
                        Sample.Instance.Play(Assets.Sounds.BURNING);
                    }
                    finalTarget.sprite.Emitter().Burst(ShadowParticle.Up, 10);
                    Actor.Remove(toRemove);
                    Next();
                };

                missile.Reset(MagicMissile.SHADOW,
                    DungeonTilemap.TileCenterToWorld(trap.pos),
                    finalTarget.sprite.Center(),
                    callback);
                return false;
            }
        }
    }
}