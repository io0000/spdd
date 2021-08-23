using watabou.noosa.audio;
using spdd.sprites;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;

namespace spdd.items.potions.exotic
{
    public class PotionOfCleansing : ExoticPotion
    {
        public PotionOfCleansing()
        {
            icon = ItemSpriteSheet.Icons.POTION_CLEANSE;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();

            Cleanse(hero);
        }

        public override void Shatter(int cell)
        {
            if (Actor.FindChar(cell) == null)
            {
                base.Shatter(cell);
            }
            else
            {
                if (Dungeon.level.heroFOV[cell])
                {
                    Sample.Instance.Play(Assets.Sounds.SHATTER);
                    Splash(cell);
                    SetKnown();
                }

                if (Actor.FindChar(cell) != null)
                {
                    Cleanse(Actor.FindChar(cell));
                }
            }
        }

        public static void Cleanse(Character ch)
        {
            foreach (Buff b in ch.Buffs())
            {
                if (b.type == Buff.BuffType.NEGATIVE &&
                    !(b is Corruption))
                {
                    b.Detach();
                }

                if (b is Hunger)
                {
                    ((Hunger)b).Satisfy(Hunger.STARVING);
                }
            }
        }
    }
}
