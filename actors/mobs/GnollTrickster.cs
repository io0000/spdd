using watabou.utils;
using spdd.actors.blobs;
using spdd.actors.mobs.npcs;
using spdd.actors.buffs;
using spdd.items;
using spdd.items.weapon.missiles;
using spdd.mechanics;
using spdd.sprites;
using spdd.scenes;

namespace spdd.actors.mobs
{
    public class GnollTrickster : Gnoll
    {
        public GnollTrickster()
        {
            spriteClass = typeof(GnollTricksterSprite);

            HP = HT = 20;
            defenseSkill = 5;

            EXP = 5;

            state = WANDERING;

            //at half quantity, see createLoot()
            loot = Generator.Category.MISSILE;
            lootChance = 1f;

            properties.Add(Property.MINIBOSS);
        }

        private int combo;

        public override int AttackSkill(Character target)
        {
            return 16;
        }

        protected override bool CanAttack(Character enemy)
        {
            Ballistic attack = new Ballistic(pos, enemy.pos, Ballistic.PROJECTILE);
            return !Dungeon.level.Adjacent(pos, enemy.pos) && attack.collisionPos == enemy.pos;
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);
            //The gnoll's attacks get more severe the more the player lets it hit them
            ++combo;
            int effect = Rnd.Int(4) + combo;

            if (effect > 2)
            {
                if (effect >= 6 && enemy.FindBuff<Burning>() == null)
                {
                    if (Dungeon.level.flamable[enemy.pos])
                        GameScene.Add(Blob.Seed(enemy.pos, 4, typeof(Fire)));

                    Buff.Affect<Burning>(enemy).Reignite(enemy);
                }
                else
                {
                    Buff.Affect<Poison>(enemy).Set((effect - 2));
                }
            }

            return damage;
        }

        public override bool GetCloser(int target)
        {
            combo = 0; //if he's moving, he isn't attacking, reset combo.
            if (state == HUNTING)
                return enemySeen && GetFurther(target);
            else
                return base.GetCloser(target);
        }

        public override Item CreateLoot()
        {
            MissileWeapon drop = (MissileWeapon)base.CreateLoot();
            //half quantity, rounded up
            drop.Quantity((drop.Quantity() + 1) / 2);
            return drop;
        }

        public override void Die(object cause)
        {
            base.Die(cause);

            Ghost.Quest.Process();
        }

        private const string COMBO = "combo";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(COMBO, combo);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            combo = bundle.GetInt(COMBO);
        }
    }
}