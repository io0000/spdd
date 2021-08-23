using System;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.items.quest;
using spdd.sprites;
using spdd.utils;

namespace spdd.items.potions.brews
{
    public class CausticBrew : Brew
    {
        public CausticBrew()
        {
            //TODO finish visuals
            image = ItemSpriteSheet.BREW_CAUSTIC;
        }

        public override void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                Splash(cell);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
            }

            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 3);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    effects.Splash.At(i, new Color(0x00, 0x00, 0x00, 0xFF), 5);
                    var ch = Actor.FindChar(i);

                    if (ch != null)
                    {
                        Buff.Affect<Ooze>(ch).Set(Ooze.DURATION);
                    }
                }
            }
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (30 + 50);
        }

        public class Recipe : spdd.items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfToxicGas), typeof(GooBlob) };
                inQuantity = new int[] { 1, 1 };

                cost = 4;

                output = typeof(CausticBrew);
                outQuantity = 1;
            }
        }
    }
}

