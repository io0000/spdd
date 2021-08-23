using watabou.utils;
using watabou.noosa;
using spdd.sprites;
using spdd.effects.particles;
using spdd.items;
using spdd.utils;
using spdd.mechanics;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class DM100 : Mob, ICallback
    {
        private const float TIME_TO_ZAP = 1f;

        public DM100()
        {
            spriteClass = typeof(DM100Sprite);

            HP = HT = 20;
            defenseSkill = 8;

            EXP = 6;
            maxLvl = 13;

            loot = Generator.Category.SCROLL;
            lootChance = 0.25f;

            properties.Add(Property.ELECTRIC);
            properties.Add(Property.INORGANIC);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(2, 8);
        }

        public override int AttackSkill(Character target)
        {
            return 11;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 4);
        }

        protected override bool CanAttack(Character enemy)
        {
            return new Ballistic(pos, enemy.pos, Ballistic.MAGIC_BOLT).collisionPos == enemy.pos;
        }

        //used so resistances can differentiate between melee and magical attacks
        public class LightningBolt
        { }

        protected override bool DoAttack(Character enemy)
        {
            if (Dungeon.level.Distance(pos, enemy.pos) <= 1)
            {
                return base.DoAttack(enemy);
            }
            else
            {
                if (sprite != null && (sprite.visible || enemy.sprite.visible))
                    sprite.Zap(enemy.pos);


                Spend(TIME_TO_ZAP);

                if (Hit(this, enemy, true))
                {
                    int dmg = Rnd.NormalIntRange(3, 10);
                    enemy.Damage(dmg, new LightningBolt());

                    enemy.sprite.CenterEmitter().Burst(SparkParticle.Factory, 3);
                    enemy.sprite.Flash();

                    if (enemy == Dungeon.hero)
                    {
                        Camera.main.Shake(2, 0.3f);

                        if (!enemy.IsAlive())
                        {
                            Dungeon.Fail(GetType());
                            GLog.Negative(Messages.Get(this, "zap_kill"));
                        }
                    }
                }
                else
                {
                    enemy.sprite.ShowStatus(CharSprite.NEUTRAL, enemy.DefenseVerb());
                }

                if (sprite != null && sprite.visible)
                {
                    sprite.Zap(enemy.pos);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        // pdsharp.utils.ICallback
        public void Call()
        {
            Next();
        }
    }
}