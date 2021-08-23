using watabou.utils;
using spdd.items;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Snake : Mob
    {
        public Snake()
        {
            spriteClass = typeof(SnakeSprite);

            HP = HT = 4;
            defenseSkill = 25;

            EXP = 2;
            maxLvl = 7;

            loot = Generator.Category.SEED;
            lootChance = 0.25f;
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(1, 4);
        }

        public override int AttackSkill(Character target)
        {
            return 10;
        }

        private static int dodges;

        public override string DefenseVerb()
        {
            ++dodges;

            if (dodges >= 3 && !BadgesExtensions.IsUnlocked(Badges.Badge.BOSS_SLAIN_1))
            {
                GLog.Highlight(Messages.Get(this, "hint"));
                dodges = 0;
            }
            return base.DefenseVerb();
        }
    }
}