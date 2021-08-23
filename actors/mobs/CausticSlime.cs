using watabou.utils;
using spdd.sprites;
using spdd.actors.buffs;
using spdd.items.quest;

namespace spdd.actors.mobs
{
    public class CausticSlime : Slime
    {
        public CausticSlime()
        {
            spriteClass = typeof(CausticSlimeSprite);

            properties.Add(Property.ACIDIC);
        }

        public override int AttackProc(Character enemy, int damage)
        {
            if (Rnd.Int(2) == 0)
            {
                Buff.Affect<Ooze>(enemy).Set(Ooze.DURATION);
                enemy.sprite.Burst(new Color(0x00, 0x00, 0x00, 0xFF), 5);
            }

            return base.AttackProc(enemy, damage);
        }

        public override void RollToDropLoot()
        {
            if (Dungeon.hero.lvl > maxLvl + 2)
                return;

            base.RollToDropLoot();

            int ofs;
            do
            {
                ofs = PathFinder.NEIGHBORS8[Rnd.Int(8)];
            } 
            while (Dungeon.level.solid[pos + ofs]);

            Dungeon.level.Drop(new GooBlob(), pos + ofs).sprite.Drop(pos);
        }
    }
}