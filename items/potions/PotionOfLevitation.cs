using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.actors.hero;
using spdd.utils;
using spdd.sprites;
using spdd.scenes;
using spdd.messages;

namespace spdd.items.potions
{
    public class PotionOfLevitation : Potion
    {
        public PotionOfLevitation()
        {
            icon = ItemSpriteSheet.Icons.POTION_LEVITATE;
        }

        public override void Shatter(int cell)
        {
            if (Dungeon.level.heroFOV[cell])
            {
                SetKnown();

                Splash(cell);
                Sample.Instance.Play(Assets.Sounds.SHATTER);
                Sample.Instance.Play(Assets.Sounds.GAS);
            }

            GameScene.Add(Blob.Seed(cell, 1000, typeof(ConfusionGas)));
        }

        public override void Apply(Hero hero)
        {
            SetKnown();
            Buff.Affect<Levitation>(hero, Levitation.DURATION);
            GLog.Information(Messages.Get(this, "float"));
        }

        public override int Value()
        {
            return IsKnown() ? 40 * quantity : base.Value();
        }
    }
}