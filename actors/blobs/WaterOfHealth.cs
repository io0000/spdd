using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.items.potions;
using spdd.items.scrolls;
using spdd.utils;
using spdd.journal;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class WaterOfHealth : WellWater
    {
        protected override bool AffectHero(Hero hero)
        {
            if (!hero.IsAlive())
                return false;

            Sample.Instance.Play(Assets.Sounds.DRINK);

            hero.HP = hero.HT;
            hero.sprite.Emitter().Start(Speck.Factory(Speck.HEALING), 0.4f, 4);

            PotionOfHealing.Cure(hero);
            hero.belongings.UncurseEquipped();
            hero.FindBuff<Hunger>().Satisfy(Hunger.STARVING);

            CellEmitter.Get(hero.pos).Start(ShaftParticle.Factory, 0.2f, 3);

            Dungeon.hero.Interrupt();

            GLog.Positive(Messages.Get(this, "procced"));

            return true;
        }

        protected override Item AffectItem(Item item, int pos)
        {
            if ((item is DewVial) && !((DewVial)item).IsFull())
            {
                ((DewVial)item).Fill();
                CellEmitter.Get(pos).Start(Speck.Factory(Speck.HEALING), 0.4f, 4);
                Sample.Instance.Play(Assets.Sounds.DRINK);

                return item;
            }
            else if ((item is Ankh) && !((Ankh)item).IsBlessed())
            {
                ((Ankh)item).Bless();
                CellEmitter.Get(pos).Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);
                Sample.Instance.Play(Assets.Sounds.DRINK);

                return item;
            }
            else if (ScrollOfRemoveCurse.Uncursable(item))
            {
                if (ScrollOfRemoveCurse.Uncurse(null, item))
                {
                    CellEmitter.Get(pos).Start(ShadowParticle.Up, 0.05f, 10);
                }
                Sample.Instance.Play(Assets.Sounds.DRINK);
                return item;
            }

            return null;
        }

        protected override Notes.Landmark Record()
        {
            return Notes.Landmark.WELL_OF_HEALTH;
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Start(Speck.Factory(Speck.HEALING), 0.5f, 0);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}