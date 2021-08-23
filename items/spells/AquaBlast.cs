using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.mechanics;
using spdd.effects;
using spdd.items.potions.exotic;

namespace spdd.items.spells
{
    public class AquaBlast : TargetedSpell
    {
        public AquaBlast()
        {
            image = ItemSpriteSheet.AQUA_BLAST;
        }

        protected override void AffectTarget(Ballistic bolt, Hero hero)
        {
            int cell = bolt.collisionPos;

            Splash.At(cell, new Color(0x00, 0xAA, 0xFF, 0xFF), 10);

            foreach (int i in PathFinder.NEIGHBORS9)
            {
                if (i == 0 || Rnd.Int(5) != 0)
                {
                    Dungeon.level.SetCellToWater(false, cell + i);
                }
            }

            var target = Actor.FindChar(cell);

            if (target != null && target != hero)
            {
                //just enough to skip their current turn
                Buff.Affect<Paralysis>(target, 0f);
            }
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((40 + 40) / 4f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(PotionOfStormClouds), typeof(ArcaneCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 4;

                output = typeof(AquaBlast);
                outQuantity = 12;
            }
        }
    }
}