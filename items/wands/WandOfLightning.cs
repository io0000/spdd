using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.effects;
using spdd.effects.particles;
using spdd.items.weapon.enchantments;
using spdd.items.weapon.melee;
using spdd.mechanics;
using spdd.messages;
using spdd.sprites;
using spdd.tiles;
using spdd.utils;

namespace spdd.items.wands
{
    public class WandOfLightning : DamageWand
    {
        public WandOfLightning()
        {
            image = ItemSpriteSheet.WAND_LIGHTNING;
        }

        private List<Character> affected = new List<Character>();

        private List<Lightning.Arc> arcs = new List<Lightning.Arc>();

        public override int Min(int lvl)
        {
            return 5 + lvl;
        }

        public override int Max(int lvl)
        {
            return 10 + 5 * lvl;
        }

        protected override void OnZap(Ballistic bolt)
        {
            int collisionPos = bolt.collisionPos;

            //lightning deals less damage per-target, the more targets that are hit.
            float multipler = 0.4f + (0.6f / affected.Count);
            //if the main target is in water, all affected take full damage
            if (Dungeon.level.water[collisionPos])
                multipler = 1f;

            foreach (var ch in affected)
            {
                if (ch == Dungeon.hero)
                    Camera.main.Shake(2, 0.3f);

                ch.sprite.CenterEmitter().Burst(SparkParticle.Factory, 3);
                ch.sprite.Flash();

                if (ch != curUser && ch.alignment == curUser.alignment && ch.pos != collisionPos)
                    continue;

                ProcessSoulMark(ch, ChargesPerCast());
                if (ch == curUser)
                {
                    ch.Damage((int)Math.Round(DamageRoll() * multipler * 0.5f, MidpointRounding.AwayFromZero), this);
                }
                else
                {
                    ch.Damage((int)Math.Round(DamageRoll() * multipler, MidpointRounding.AwayFromZero), this);
                }
            }

            if (!curUser.IsAlive())
            {
                Dungeon.Fail(GetType());
                GLog.Negative(Messages.Get(this, "ondeath"));
            }
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            //acts like shocking enchantment
            (new Shocking()).Proc(staff, attacker, defender, damage);
        }

        private void Arc(Character ch)
        {
            int dist = (Dungeon.level.water[ch.pos] && !ch.flying) ? 2 : 1;

            var hitThisArc = new List<Character>();
            PathFinder.BuildDistanceMap(ch.pos, BArray.Not(Dungeon.level.solid, null), dist);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    var n = Actor.FindChar(i);
                    if (n == Dungeon.hero && PathFinder.distance[i] > 1)
                    {
                        //the hero is only zapped if they are adjacent
                        continue;
                    }
                    else if (n != null && !affected.Contains(n))
                    {
                        hitThisArc.Add(n);
                    }
                }
            }

            affected.AddRange(hitThisArc);
            foreach (Character hit in hitThisArc)
            {
                arcs.Add(new Lightning.Arc(ch.sprite.Center(), hit.sprite.Center()));
                Arc(hit);
            }
        }

        public override void Fx(Ballistic bolt, ICallback callback)
        {
            affected.Clear();
            arcs.Clear();

            int cell = bolt.collisionPos;

            var ch = Actor.FindChar(cell);
            if (ch != null)
            {
                affected.Add(ch);
                arcs.Add(new Lightning.Arc(curUser.sprite.Center(), ch.sprite.Center()));
                Arc(ch);
            }
            else
            {
                arcs.Add(new Lightning.Arc(curUser.sprite.Center(), DungeonTilemap.RaisedTileCenterToWorld(bolt.collisionPos)));
                CellEmitter.Center(cell).Burst(SparkParticle.Factory, 3);
            }

            //don't want to wait for the effect before processing damage.
            curUser.sprite.parent.AddToFront(new Lightning(arcs, null));
            Sample.Instance.Play(Assets.Sounds.LIGHTNING);
            callback.Call();
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            particle.SetColor(new Color(0xFF, 0xFF, 0xFF, 0xFF));
            particle.am = 0.6f;
            particle.SetLifespan(0.6f);
            particle.acc.Set(0, +10);
            particle.speed.Polar(-Rnd.Float(3.1415926f), 6f);
            particle.SetSize(0f, 1.5f);
            particle.sizeJitter = 1f;
            particle.ShuffleXY(1f);
            float dst = Rnd.Float(1f);
            particle.x -= dst;
            particle.y += dst;
        }
    }
}