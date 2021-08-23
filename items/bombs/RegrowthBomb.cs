using System.Collections.Generic;
using watabou.utils;
using spdd.levels;
using spdd.plants;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.items.potions;
using spdd.items.wands;
using spdd.sprites;
using spdd.scenes;
using spdd.effects;
using spdd.utils;

namespace spdd.items.bombs
{
    public class RegrowthBomb : Bomb
    {
        public RegrowthBomb()
        {
            //TODO visuals
            image = ItemSpriteSheet.REGROWTH_BOMB;
        }

        public override bool ExplodesDestructively()
        {
            return false;
        }

        public override void Explode(int cell)
        {
            base.Explode(cell);

            if (Dungeon.level.heroFOV[cell])
                Splash.At(cell, new Color(0x00, 0xFF, 0x00, 0xFF), 30);

            List<int> plantCandidates = new List<int>();

            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    var ch = Actor.FindChar(i);
                    if (ch != null)
                    {
                        if (ch.alignment == Dungeon.hero.alignment)
                        {
                            //same as a healing potion
                            Buff.Affect<Healing>(ch).SetHeal((int)(0.8f * ch.HT + 14), 0.25f, 0);
                            PotionOfHealing.Cure(ch);
                        }
                    }
                    else if (Dungeon.level.map[i] == Terrain.EMPTY ||
                              Dungeon.level.map[i] == Terrain.EMBERS ||
                              Dungeon.level.map[i] == Terrain.EMPTY_DECO ||
                              Dungeon.level.map[i] == Terrain.GRASS ||
                              Dungeon.level.map[i] == Terrain.HIGH_GRASS ||
                              Dungeon.level.map[i] == Terrain.FURROWED_GRASS)
                    {
                        plantCandidates.Add(i);
                    }
                    GameScene.Add(Blob.Seed(i, 10, typeof(Regrowth)));
                }
            }

            int plants = Rnd.Chances(new float[] { 0, 6, 3, 1 });

            int? plantPos;
            for (int i = 0; i < plants; ++i)
            {
                plantPos = Rnd.Element(plantCandidates);
                if (plantPos != null)
                {
                    Dungeon.level.Plant((Plant.Seed)Generator.RandomUsingDefaults(Generator.Category.SEED), plantPos.Value);
                    plantCandidates.Remove(plantPos.Value);
                }
            }

            plantPos = Rnd.Element(plantCandidates);
            if (plantPos != null)
            {
                Plant.Seed plant;
                switch (Rnd.Chances(new float[] { 0, 6, 3, 1 }))
                {
                    case 1:
                    default:
                        plant = new WandOfRegrowth.Dewcatcher.Seed();
                        break;
                    case 2:
                        plant = new WandOfRegrowth.Seedpod.Seed();
                        break;
                    case 3:
                        plant = new Starflower.Seed();
                        break;
                }
                Dungeon.level.Plant(plant, plantPos.Value);
            }
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 30);
        }
    }
}