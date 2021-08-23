using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.effects;
using spdd.effects.particles;
using spdd.levels;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items.weapon.enchantments
{
    public class Blooming : Weapon.Enchantment
    {
        private static ItemSprite.Glowing DARK_GREEN = new ItemSprite.Glowing(new Color(0x00, 0x88, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            // lvl 0 - 33%
            // lvl 1 - 50%
            // lvl 2 - 60%
            int level = Math.Max(0, weapon.BuffedLvl());

            if (Rnd.Int(level + 3) >= 2)
            {
                bool secondPlant = level > Rnd.Int(10);
                if (PlantGrass(defender.pos))
                {
                    if (secondPlant)
                        secondPlant = false;
                    else
                        return damage;
                }

                List<int> positions = new List<int>();
                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    positions.Add(i);
                }

                Rnd.Shuffle(positions);
                foreach (int i in positions)
                {
                    if (PlantGrass(defender.pos + i))
                    {
                        if (secondPlant)
                            secondPlant = false;
                        else
                            return damage;
                    }
                }
            }

            return damage;
        }

        private bool PlantGrass(int cell)
        {
            int c = Dungeon.level.map[cell];
            if (c == Terrain.EMPTY ||
                c == Terrain.EMPTY_DECO ||
                c == Terrain.EMBERS ||
                c == Terrain.GRASS)
            {
                Level.Set(cell, Terrain.HIGH_GRASS);
                GameScene.UpdateMap(cell);
                CellEmitter.Get(cell).Burst(LeafParticle.LevelSpecific, 4);
                return true;
            }
            return false;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return DARK_GREEN;
        }
    }
}