using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.mobs.npcs
{
    public class Sheep : Shopkeeper
    {
        private static readonly string[] LINE_KEYS = { "Baa!", "Baa?", "Baa.", "Baa..." };

        public Sheep()
        {
            spriteClass = typeof(SheepSprite);
        }

        public float lifespan;

        private bool initialized;

        public override bool Act()
        {
            if (initialized)
            {
                HP = 0;

                Destroy();
                sprite.Die();
            }
            else
            {
                initialized = true;
                Spend(lifespan + Rnd.Float(2));
            }
            return true;
        }

        public override int DefenseSkill(Character enemy)
        {
            return INFINITE_EVASION;
        }

        public override void Damage(int dmg, object src)
        { }

        public override void Add(Buff buff)
        { }

        public override bool Interact(Character c)
        {
            sprite.ShowStatus(CharSprite.NEUTRAL, Messages.Get(this, Rnd.Element(LINE_KEYS)));
            if (c == Dungeon.hero)
            {
                Dungeon.hero.SpendAndNext(1f);
                Sample.Instance.Play(Assets.Sounds.SHEEP, 1, Rnd.Float(0.91f, 1.1f));
            }

            return true;
        }
    }
}