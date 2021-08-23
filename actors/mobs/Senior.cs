using watabou.utils;
using spdd.sprites;
using spdd.items.food;

namespace spdd.actors.mobs
{
    public class Senior : Monk
    {
        public Senior()
        {
            spriteClass = typeof(SeniorSprite);

            loot = new Pasty();
            lootChance = 1f;
        }

        public override void Move(int step)
        {
            // on top of the existing move bonus, senior monks get a further 1.66 cooldown reduction
            // for a total of 3.33, double the normal 1.67 for regular monks
            focusCooldown -= 1.66f;
            base.Move(step);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(16, 25);
        }
    }
}