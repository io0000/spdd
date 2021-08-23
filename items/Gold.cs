using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.hero;
using spdd.items.artifacts;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items
{
    public class Gold : Item
    {
        private const string TXT_VALUE = "%+d";

        private void InitInstance()
        {
            image = ItemSpriteSheet.GOLD;
            stackable = true;
        }

        public Gold()
            : this(1)
        { }

        public Gold(int value)
        {
            InitInstance();

            this.quantity = value;
        }

        public override List<string> Actions(Hero hero)
        {
            return new List<string>();
        }

        public override bool DoPickUp(Hero hero)
        {
            Dungeon.gold += quantity;
            Statistics.goldCollected += quantity;
            BadgesExtensions.ValidateGoldCollected();

            var thievery = hero.FindBuff<MasterThievesArmband.Thievery>();
            if (thievery != null)
                thievery.Collect(quantity);

            GameScene.PickUp(this, hero.pos);
            hero.sprite.ShowStatus(CharSprite.NEUTRAL, TXT_VALUE, quantity);
            hero.SpendAndNext(TIME_TO_PICK_UP);

            Sample.Instance.Play(Assets.Sounds.GOLD, 1, 1, Rnd.Float(0.9f, 1.1f));

            return true;
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override Item Random()
        {
            quantity = Rnd.Int(30 + Dungeon.depth * 10, 60 + Dungeon.depth * 20);
            return this;
        }

        private const string VALUE = "value";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(VALUE, quantity);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            quantity = bundle.GetInt(VALUE);
        }
    }
}