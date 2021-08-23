using System.Collections.Generic;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects.particles;
using spdd.sprites;

namespace spdd.items
{
    public class Torch : Item
    {
        public const string AC_LIGHT = "LIGHT";

        public const float TIME_TO_LIGHT = 1;

        public Torch()
        {
            image = ItemSpriteSheet.TORCH;

            stackable = true;

            defaultAction = AC_LIGHT;
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            actions.Add(AC_LIGHT);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_LIGHT))
            {
                hero.Spend(TIME_TO_LIGHT);
                hero.Busy();

                hero.sprite.Operate(hero.pos);

                Detach(hero.belongings.backpack);

                Buff.Affect<Light>(hero, Light.DURATION);
                Sample.Instance.Play(Assets.Sounds.BURNING);

                var emitter = hero.sprite.CenterEmitter();
                emitter.Start(FlameParticle.Factory, 0.2f, 3);
            }
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override int Value()
        {
            return 8 * quantity;
        }
    }
}