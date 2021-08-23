using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;
using spdd.mechanics;
using spdd.levels.traps;
using spdd.items.scrolls;
using spdd.items.quest;
using spdd.messages;

namespace spdd.items.spells
{
    public class ReclaimTrap : TargetedSpell
    {
        public ReclaimTrap()
        {
            image = ItemSpriteSheet.RECLAIM_TRAP;
        }

        private Type storedTrap;

        protected override void AffectTarget(Ballistic bolt, Hero hero)
        {
            if (storedTrap == null)
            {
                ++quantity; //storing a trap doesn't consume the spell
                Trap t = Dungeon.level.traps[bolt.collisionPos];
                if (t != null && t.active && t.visible)
                {
                    t.Disarm();

                    Sample.Instance.Play(Assets.Sounds.LIGHTNING);
                    ScrollOfRecharging.Charge(hero);
                    storedTrap = t.GetType();
                }
                else
                {
                    GLog.Warning(Messages.Get(this, "no_trap"));
                }
            }
            else
            {
                Trap t = (Trap)Reflection.NewInstance(storedTrap);
                storedTrap = null;

                t.pos = bolt.collisionPos;
                t.Activate();
            }
        }

        public override string Desc()
        {
            var desc = base.Desc();
            if (storedTrap != null)
            {
                desc += "\n\n" + Messages.Get(this, "desc_trap", Messages.Get(storedTrap, "name"));
            }
            return desc;
        }

        public override void OnThrow(int cell)
        {
            storedTrap = null;
            base.OnThrow(cell);
        }

        public override void DoDrop(Hero hero)
        {
            storedTrap = null;
            base.DoDrop(hero);
        }

        private static ItemSprite.Glowing[] COLORS = new ItemSprite.Glowing[]
        {
            new ItemSprite.Glowing( new Color(0xFF, 0x00, 0x00, 0xFF) ),
            new ItemSprite.Glowing( new Color(0xFF, 0x80, 0x00, 0xFF) ),
            new ItemSprite.Glowing( new Color(0xFF, 0xFF, 0x00, 0xFF) ),
            new ItemSprite.Glowing( new Color(0x00, 0xFF, 0x00, 0xFF) ),
            new ItemSprite.Glowing( new Color(0x00, 0xFF, 0xFF, 0xFF) ),
            new ItemSprite.Glowing( new Color(0x80, 0x00, 0xFF, 0xFF) ),
            new ItemSprite.Glowing( new Color(0xFF, 0xFF, 0xFF, 0xFF) ),
            new ItemSprite.Glowing( new Color(0x80, 0x80, 0x80, 0xFF) ),
            new ItemSprite.Glowing( new Color(0x00, 0x00, 0x00, 0xFF) )
        };

        public override ItemSprite.Glowing Glowing()
        {
            if (storedTrap != null)
            {
                Trap trap = (Trap)Reflection.NewInstance(storedTrap);
                return COLORS[trap.color];
            }
            return null;
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((40 + 100) / 3f), MidpointRounding.AwayFromZero);
        }

        private const string STORED_TRAP = "stored_trap";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(STORED_TRAP, storedTrap);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            storedTrap = bundle.GetClass(STORED_TRAP);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(ScrollOfMagicMapping), typeof(MetalShard) };
                inQuantity = new int[] { 1, 1 };

                cost = 6;

                output = typeof(ReclaimTrap);
                outQuantity = 3;
            }
        }
    }
}