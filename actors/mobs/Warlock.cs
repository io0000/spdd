using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.items;
using spdd.items.potions;
using spdd.mechanics;
using spdd.sprites;
using spdd.utils;
using spdd.levels;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Warlock : Mob, ICallback
    {
        private const float TIME_TO_ZAP = 1f;

        public Warlock()
        {
            spriteClass = typeof(WarlockSprite);

            HP = HT = 70;
            defenseSkill = 18;

            EXP = 11;
            maxLvl = 21;

            loot = Generator.Category.POTION;
            lootChance = 0.5f;

            properties.Add(Property.UNDEAD);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(12, 18);
        }

        public override int AttackSkill(Character target)
        {
            return 25;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 8);
        }

        protected override bool CanAttack(Character enemy)
        {
            return new Ballistic(pos, enemy.pos, Ballistic.MAGIC_BOLT).collisionPos == enemy.pos;
        }

        protected override bool DoAttack(Character enemy)
        {
            if (Dungeon.level.Adjacent(pos, enemy.pos))
                return base.DoAttack(enemy);

            if (sprite != null && (sprite.visible || enemy.sprite.visible))
            {
                sprite.Zap(enemy.pos);
                return false;
            }
            else
            {
                Zap();
                return true;
            }
        }

        //used so resistances can differentiate between melee and magical attacks
        public class DarkBolt
        { }

        private void Zap()
        {
            Spend(TIME_TO_ZAP);

            if (Hit(this, enemy, true))
            {
                //TODO would be nice for this to work on ghost/statues too
                if (enemy == Dungeon.hero && Rnd.Int(2) == 0)
                {
                    Buff.Prolong<Weakness>(enemy, 1.0f);
                    Sample.Instance.Play(Assets.Sounds.DEBUFF);
                }

                var dmg = Rnd.Int(12, 18);
                enemy.Damage(dmg, new DarkBolt());

                if (enemy == Dungeon.hero && !enemy.IsAlive())
                {
                    Dungeon.Fail(GetType());
                    GLog.Negative(Messages.Get(this, "bolt_kill"));
                }
            }
            else
            {
                enemy.sprite.ShowStatus(CharSprite.NEUTRAL, enemy.DefenseVerb());
            }
        }

        public virtual void OnZapComplete()
        {
            Zap();
            Next();
        }

        // ICallBack
        public void Call()
        {
            Next();
        }

        public override Item CreateLoot()
        {
            // 1/6 chance for healing, scaling to 0 over 8 drops
            if (Rnd.Int(2) == 0 && Rnd.Int(8) > Dungeon.LimitedDrops.WARLOCK_HP.count)
            {
                Dungeon.LimitedDrops.WARLOCK_HP.Drop();
                return new PotionOfHealing();
            }
            else
            {
                Item i = Generator.Random(Generator.Category.POTION);
                int healingTried = 0;
                while (i is PotionOfHealing)
                {
                    ++healingTried;
                    i = Generator.Random(Generator.Category.POTION);
                }

                //return the attempted healing potion drops to the pool
                if (healingTried > 0)
                {
                    var classes = Generator.Category.POTION.GetClasses();
                    var probs = Generator.Category.POTION.GetProbs();

                    for (int j = 0; j < classes.Length; ++j)
                    {
                        if (classes[j].Equals(typeof(PotionOfHealing)))
                        {
                            probs[j] += healingTried;
                        }
                    }
                }

                return i;
            }
        }
    }
}