using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.levels.painters;
using spdd.items.potions;
using spdd.actors.blobs;

namespace spdd.levels.rooms.secret
{
    public class SecretLaboratoryRoom : SecretRoom
    {
        private static Dictionary<Type, float> potionChances = new Dictionary<Type, float>();

        static SecretLaboratoryRoom()
        {
            potionChances[typeof(PotionOfHealing)] = 1f;
            potionChances[typeof(PotionOfMindVision)] = 2f;
            potionChances[typeof(PotionOfFrost)] = 3f;
            potionChances[typeof(PotionOfLiquidFlame)] = 3f;
            potionChances[typeof(PotionOfToxicGas)] = 3f;
            potionChances[typeof(PotionOfHaste)] = 4f;
            potionChances[typeof(PotionOfInvisibility)] = 4f;
            potionChances[typeof(PotionOfLevitation)] = 4f;
            potionChances[typeof(PotionOfParalyticGas)] = 4f;
            potionChances[typeof(PotionOfPurity)] = 4f;
            potionChances[typeof(PotionOfExperience)] = 6f;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);

            Entrance().Set(Door.Type.HIDDEN);

            Point pot = Center();
            Painter.Set(level, pot, Terrain.ALCHEMY);

            Blob.Seed(pot.x + level.Width() * pot.y, 1 + Rnd.NormalIntRange(20, 30), typeof(Alchemy), level);

            int n = Rnd.IntRange(2, 3);
            Dictionary<Type, float> chances = new Dictionary<Type, float>(potionChances);
            for (int i = 0; i < n; ++i)
            {
                int pos;
                do
                {
                    pos = level.PointToCell(Random());
                }
                while (level.map[pos] != Terrain.EMPTY_SP || level.heaps[pos] != null);

                Type potionCls = Rnd.Chances(chances);
                chances[potionCls] = 0f;
                level.Drop((Potion)Reflection.NewInstance(potionCls), pos);
            }
        }
    }
}