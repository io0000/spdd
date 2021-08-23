using System;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.sprites;
using spdd.utils;
using spdd.mechanics;
using spdd.items.scrolls;
using spdd.messages;

namespace spdd.items.spells
{
    public class PhaseShift : TargetedSpell
    {
        public PhaseShift()
        {
            image = ItemSpriteSheet.PHASE_SHIFT;
        }

        protected override void AffectTarget(Ballistic bolt, Hero hero)
        {
            var ch = Actor.FindChar(bolt.collisionPos);

            if (ch == hero)
            {
                ScrollOfTeleportation.TeleportHero(curUser);
            }
            else if (ch != null)
            {
                int count = 10;
                int pos;
                do
                {
                    pos = Dungeon.level.RandomRespawnCell(hero);
                    if (count-- <= 0)
                        break;

                }
                while (pos == -1);

                if (pos == -1 || Dungeon.BossLevel())
                {
                    GLog.Warning(Messages.Get(typeof(ScrollOfTeleportation), "no_tele"));
                }
                else if (ch.Properties().Contains(Character.Property.IMMOVABLE))
                {
                    GLog.Warning(Messages.Get(this, "tele_fail"));
                }
                else
                {
                    ch.pos = pos;
                    if (ch is Mob && ((Mob)ch).state == ((Mob)ch).HUNTING)
                    {
                        ((Mob)ch).state = ((Mob)ch).WANDERING;
                    }
                    ch.sprite.Place(ch.pos);
                    ch.sprite.visible = Dungeon.level.heroFOV[pos];
                }
            }
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((30 + 40) / 8f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(ScrollOfTeleportation), typeof(ArcaneCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(PhaseShift);
                outQuantity = 8;
            }
        }
    }
}