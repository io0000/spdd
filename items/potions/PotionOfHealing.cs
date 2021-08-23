using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.potions
{
    public class PotionOfHealing : Potion
    {
        public PotionOfHealing()
        {
            icon = ItemSpriteSheet.Icons.POTION_HEALING;

            bones = true;
        }

        public override void Apply(Hero hero)
        {
            SetKnown();
            //starts out healing 30 hp, equalizes with hero health total at level 11
            Buff.Affect<Healing>(hero).SetHeal((int)(0.8f * hero.HT + 14), 0.25f, 0);
            Cure(Dungeon.hero);
            GLog.Positive(Messages.Get(this, "heal"));
        }

        public static void Cure(Character hero)
        {
            Buff.Detach<Poison>(hero);
            Buff.Detach<Cripple>(hero);
            Buff.Detach<Weakness>(hero);
            Buff.Detach<Vulnerable>(hero);
            Buff.Detach<Bleeding>(hero);
            Buff.Detach<Blindness>(hero);
            Buff.Detach<Drowsy>(hero);
            Buff.Detach<Slow>(hero);
            Buff.Detach<Vertigo>(hero);
        }

        public override int Value()
        {
            return IsKnown() ? 30 * quantity : base.Value();
        }
    }
}