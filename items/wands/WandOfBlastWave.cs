using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.mechanics;
using spdd.sprites;
using spdd.levels;
using spdd.utils;
using spdd.levels.traps;
using spdd.levels.features;
using spdd.items.weapon.melee;
using spdd.items.weapon.enchantments;
using spdd.messages;
using spdd.tiles;

using Paralysis = spdd.actors.buffs.Paralysis;

namespace spdd.items.wands
{
    public class WandOfBlastWave : DamageWand
    {
        public WandOfBlastWave()
        {
            image = ItemSpriteSheet.WAND_BLAST_WAVE;

            collisionProperties = Ballistic.PROJECTILE;
        }

        public override int Min(int lvl)
        {
            return 1 + lvl;
        }

        public override int Max(int lvl)
        {
            return 3 + 3 * lvl;
        }

        protected override void OnZap(Ballistic bolt)
        {
            int collisionPos = bolt.collisionPos;

            Sample.Instance.Play(Assets.Sounds.BLAST);

            BlastWave.Blast(bolt.collisionPos);

            //presses all tiles in the AOE first, with the exception of tengu dart traps
            foreach (int i in PathFinder.NEIGHBORS9)
            {
                if (!(Dungeon.level.traps[collisionPos + i] is TenguDartTrap))
                {
                    Dungeon.level.PressCell(collisionPos + i);
                }
            }

            Character ch;
            //throws other chars around the center.
            foreach (int i in PathFinder.NEIGHBORS8)
            {
                ch = Actor.FindChar(collisionPos + i);

                if (ch != null)
                {
                    ProcessSoulMark(ch, ChargesPerCast());
                    if (ch.alignment != Character.Alignment.ALLY)
                        ch.Damage(DamageRoll(), this);

                    if (ch.IsAlive() && ch.pos == collisionPos + i)
                    {
                        Ballistic trajectory = new Ballistic(ch.pos, ch.pos + i, Ballistic.MAGIC_BOLT);
                        int strength = 1 + (int)Math.Round(BuffedLvl() / 2f, MidpointRounding.AwayFromZero);
                        ThrowChar(ch, trajectory, strength, false);
                    }
                    else if (ch == Dungeon.hero)
                    {
                        Dungeon.Fail(GetType());
                        GLog.Negative(Messages.Get(this, "ondeath"));
                    }
                }
            }

            //throws the char at the center of the blast
            ch = Actor.FindChar(collisionPos);
            if (ch != null)
            {
                ProcessSoulMark(ch, ChargesPerCast());
                ch.Damage(DamageRoll(), this);

                if (ch.IsAlive() && bolt.path.Count > bolt.dist + 1 && ch.pos == collisionPos)
                {
                    Ballistic trajectory = new Ballistic(ch.pos, bolt.path[bolt.dist + 1], Ballistic.MAGIC_BOLT);
                    int strength = BuffedLvl() + 3;
                    ThrowChar(ch, trajectory, strength, false);
                }
            }
        }

        //public static void ThrowChar(Character ch, Ballistic trajectory, int power)
        //{
        //    ThrowChar(ch, trajectory, power, true);
        //}

        public static void ThrowChar(Character ch, Ballistic trajectory, int power, bool closeDoors)
        {
            ThrowChar(ch, trajectory, power, closeDoors, true);
        }

        public static void ThrowChar(Character ch, Ballistic trajectory, int power, bool closeDoors, bool collideDmg)
        {
            if (ch.Properties().Contains(Character.Property.BOSS))
            {
                power /= 2;
            }

            int dist = Math.Min(trajectory.dist, power);

            bool collided = dist == trajectory.dist;

            if (dist == 0 || ch.Properties().Contains(Character.Property.IMMOVABLE))
                return;

            //large characters cannot be moved into non-open space
            if (Character.HasProp(ch, Character.Property.LARGE))
            {
                for (int i = 1; i <= dist; ++i)
                {
                    if (!Dungeon.level.openSpace[trajectory.path[i]])
                    {
                        dist = i - 1;
                        collided = true;
                        break;
                    }
                }
            }

            if (Actor.FindChar(trajectory.path[dist]) != null)
            {
                --dist;
                collided = true;
            }

            if (dist < 0)
                return;

            int newPos = trajectory.path[dist];

            if (newPos == ch.pos)
                return;

            int finalDist = dist;
            bool finalCollided = collided && collideDmg;
            int initialpos = ch.pos;

            var callback = new ActionCallback();
            callback.action = () =>
            {
                if (initialpos != ch.pos)
                {
                    //something caused movement before pushing resolved, cancel to be safe.
                    ch.sprite.Place(ch.pos);
                    return;
                }
                int oldPos = ch.pos;
                ch.pos = newPos;
                if (finalCollided && ch.IsAlive())
                {
                    ch.Damage(Rnd.NormalIntRange(finalDist, 2 * finalDist), callback);
                    Buff.Prolong<Paralysis>(ch, 1 + finalDist / 2f);
                }
                if (closeDoors && Dungeon.level.map[oldPos] == Terrain.OPEN_DOOR)
                {
                    Door.Leave(oldPos);
                }
                Dungeon.level.OccupyCell(ch);
                if (ch == Dungeon.hero)
                {
                    //FIXME currently no logic here if the throw effect kills the hero
                    Dungeon.Observe();
                }
            };

            Actor.AddDelayed(new Pushing(ch, ch.pos, newPos, callback), -1);
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            new Elastic().Proc(staff, attacker, defender, damage);
        }

        public override void Fx(Ballistic bolt, ICallback callback)
        {
            MagicMissile.BoltFromChar(curUser.sprite.parent,
                MagicMissile.FORCE,
                curUser.sprite,
                bolt.collisionPos,
                callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            particle.SetColor(new Color(0x66, 0x44, 0x22, 0xFF));
            particle.am = 0.6f;
            particle.SetLifespan(3f);
            particle.speed.Polar(Rnd.Float(PointF.PI2), 0.3f);
            particle.SetSize(1f, 2f);
            particle.RadiateXY(2.5f);
        }

        public class BlastWave : Image
        {
            private const float TIME_TO_FADE = 0.2f;

            private float time;

            public BlastWave()
                : base(Effects.Get(Effects.Type.RIPPLE))
            {
                origin.Set(width / 2, height / 2);
            }

            public void Reset(int pos)
            {
                Revive();

                x = (pos % Dungeon.level.Width()) * DungeonTilemap.SIZE + (DungeonTilemap.SIZE - width) / 2;
                y = (pos / Dungeon.level.Width()) * DungeonTilemap.SIZE + (DungeonTilemap.SIZE - height) / 2;

                time = TIME_TO_FADE;
            }

            public override void Update()
            {
                base.Update();

                if ((time -= Game.elapsed) <= 0)
                {
                    Kill();
                }
                else
                {
                    float p = time / TIME_TO_FADE;
                    Alpha(p);
                    scale.y = scale.x = (1 - p) * 3;
                }
            }

            public static void Blast(int pos)
            {
                Group parent = Dungeon.hero.sprite.parent;
                var b = parent.Recycle<BlastWave>();
                parent.BringToFront(b);
                b.Reset(pos);
            }
        }
    }
}