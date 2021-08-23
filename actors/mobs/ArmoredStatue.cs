using System;
using watabou.utils;
using spdd.sprites;
using spdd.items.armor;
using spdd.items;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class ArmoredStatue : Statue
    {
        protected Armor armor;

        public ArmoredStatue()
        {
            spriteClass = typeof(StatueSprite);

            do
            {
                armor = Generator.RandomArmor();
            } while (armor.cursed);

            armor.Inscribe(Armor.Glyph.Random());

            //double HP
            HP = HT = 30 + Dungeon.depth * 10;
        }

        private const string ARMOR = "armor";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(ARMOR, armor);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            armor = (Armor)bundle.Get(ARMOR);
        }

        public override int DrRoll()
        {
            return base.DrRoll() + Rnd.NormalIntRange(armor.DRMin(), armor.DRMax());
        }

        public override int DefenseProc(Character enemy, int damage)
        {
            return armor.Proc(enemy, this, damage);
        }

        public override CharSprite GetSprite()
        {
            CharSprite sprite = base.GetSprite();
            ((StatueSprite)sprite).SetArmor(armor.tier);
            return sprite;
        }

        public override float Speed()
        {
            return armor.SpeedFactor(this, base.Speed());
        }

        public override float Stealth()
        {
            return armor.StealthFactor(this, base.Stealth());
        }

        public override int DefenseSkill(Character enemy)
        {
            return (int)Math.Round(armor.EvasionFactor(this, base.DefenseSkill(enemy)), MidpointRounding.AwayFromZero);
        }

        public override void Die(object cause)
        {
            armor.Identify();
            Dungeon.level.Drop(armor, pos).sprite.Drop();
            base.Die(cause);
        }

        public override string Description()
        {
            return Messages.Get(this, "desc", weapon.Name(), armor.Name());
        }
    }
}