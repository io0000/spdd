using System.Linq;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects.particles;
using spdd.sprites;

namespace spdd.items.armor
{
    public class MageArmor : ClassArmor
    {
        public MageArmor()
        {
            image = ItemSpriteSheet.ARMOR_MAGE;
        }

        public override void DoSpecial()
        {
            charge -= 35;
            UpdateQuickslot();

            foreach (var mob in Dungeon.level.mobs.ToArray())
            {
                if (Dungeon.level.heroFOV[mob.pos] &&
                    mob.alignment != Character.Alignment.ALLY)
                {
                    Buff.Affect<Burning>(mob).Reignite(mob);
                    Buff.Prolong<Roots>(mob, Roots.DURATION);
                    mob.Damage(Rnd.NormalIntRange(4, 16 + Dungeon.depth), new Burning());
                }
            }

            curUser.Spend(Actor.TICK);
            curUser.sprite.Operate(curUser.pos);
            Invisibility.Dispel();
            curUser.Busy();

            curUser.sprite.Emitter().Start(ElmoParticle.Factory, 0.025f, 20);
            Sample.Instance.Play(Assets.Sounds.BURNING);
            Sample.Instance.Play(Assets.Sounds.BURNING);
            Sample.Instance.Play(Assets.Sounds.BURNING);
        }
    }
}