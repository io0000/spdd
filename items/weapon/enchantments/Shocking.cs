using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.effects;
using spdd.effects.particles;
using spdd.sprites;
using spdd.utils;

namespace spdd.items.weapon.enchantments
{
    public class Shocking : Weapon.Enchantment
    {
        private static ItemSprite.Glowing WHITE = new ItemSprite.Glowing(new Color(0xFF, 0xFF, 0xFF, 0xFF), 0.5f);

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            // lvl 0 - 25%
            // lvl 1 - 40%
            // lvl 2 - 50%
            int level = Math.Max(0, weapon.BuffedLvl());

            //if (pdsharp.utils.Random.Int(level + 4) >= 3)
            {
                affected.Clear();
                arcs.Clear();

                Arc(attacker, defender, 2, affected, arcs);

                affected.Remove(defender); //defender isn't hurt by lightning
                foreach (Character ch in affected)
                {
                    if (ch.alignment != attacker.alignment)
                    {
                        ch.Damage((int)Math.Round(damage * 0.4f, MidpointRounding.AwayFromZero), this);
                    }
                }

                attacker.sprite.parent.AddToFront(new Lightning(arcs, null));
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return WHITE;
        }

        private List<Character> affected = new List<Character>();

        private List<Lightning.Arc> arcs = new List<Lightning.Arc>();

        public static void Arc(Character attacker, Character defender, int dist, List<Character> affected, List<Lightning.Arc> arcs)
        {
            affected.Add(defender);

            defender.sprite.CenterEmitter().Burst(SparkParticle.Factory, 3);
            defender.sprite.Flash();

            PathFinder.BuildDistanceMap(defender.pos, BArray.Not(Dungeon.level.solid, null), dist);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    Character n = Actor.FindChar(i);
                    if (n != null && n != attacker && !affected.Contains(n))
                    {
                        arcs.Add(new Lightning.Arc(defender.sprite.Center(), n.sprite.Center()));
                        Arc(attacker, n, (Dungeon.level.water[n.pos] && !n.flying) ? 2 : 1, affected, arcs);
                    }
                }
            }
        }
    }
}