using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.hero;
using spdd.effects.particles;
using spdd.items.wands;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.windows;

namespace spdd.items.artifacts
{
    public class ChaliceOfBlood : Artifact
    {
        public ChaliceOfBlood()
        {
            image = ItemSpriteSheet.ARTIFACT_CHALICE1;

            levelCap = 10;
        }

        public const string AC_PRICK = "PRICK";

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            if (IsEquipped(hero) && GetLevel() < levelCap && !cursed)
                actions.Add(AC_PRICK);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_PRICK))
            {
                int damage = 3 * (GetLevel() * GetLevel());

                if (damage > hero.HP * 0.75)
                {
                    var wnd = new WndOptions(Messages.TitleCase(Messages.Get(this, "name")),
                                Messages.Get(this, "prick_warn"),
                                Messages.Get(this, "yes"),
                                Messages.Get(this, "no"));
                    wnd.selectAction = (index) =>
                    {
                        if (index == 0)
                            Prick(Dungeon.hero);
                    };

                    GameScene.Show(wnd);
                }
                else
                {
                    Prick(hero);
                }
            }
        }

        private void Prick(Hero hero)
        {
            int damage = 3 * (GetLevel() * GetLevel());

            var armor = hero.FindBuff<Earthroot.Armor>();
            if (armor != null)
                damage = armor.Absorb(damage);

            var rockArmor = hero.FindBuff<WandOfLivingEarth.RockArmor>();
            if (rockArmor != null)
                damage = rockArmor.Absorb(damage);

            damage -= hero.DrRoll();

            hero.sprite.Operate(hero.pos);
            hero.Busy();
            hero.Spend(3f);
            GLog.Warning(Messages.Get(this, "onprick"));
            if (damage <= 0)
            {
                damage = 1;
            }
            else
            {
                Sample.Instance.Play(Assets.Sounds.CURSED);
                hero.sprite.Emitter().Burst(ShadowParticle.Curse, 4 + (damage / 10));
            }

            hero.Damage(damage, this);

            if (!hero.IsAlive())
            {
                Dungeon.Fail(GetType());
                GLog.Negative(Messages.Get(this, "ondeath"));
            }
            else
            {
                Upgrade();
            }
        }

        public override Item Upgrade()
        {
            if (GetLevel() >= 6)
                image = ItemSpriteSheet.ARTIFACT_CHALICE3;
            else if (GetLevel() >= 2)
                image = ItemSpriteSheet.ARTIFACT_CHALICE2;
            return base.Upgrade();
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            if (GetLevel() >= 7)
                image = ItemSpriteSheet.ARTIFACT_CHALICE3;
            else if (GetLevel() >= 3)
                image = ItemSpriteSheet.ARTIFACT_CHALICE2;
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new ChaliceRegen(this);
        }

        public override void Charge(Hero target)
        {
            target.HP = Math.Min(target.HT, target.HP + 1 + Dungeon.depth / 5);
        }

        public override string Desc()
        {
            string desc = base.Desc();

            if (IsEquipped(Dungeon.hero))
            {
                desc += "\n\n";
                if (cursed)
                    desc += Messages.Get(this, "desc_cursed");
                else if (GetLevel() == 0)
                    desc += Messages.Get(this, "desc_1");
                else if (GetLevel() < levelCap)
                    desc += Messages.Get(this, "desc_2");
                else
                    desc += Messages.Get(this, "desc_3");
            }

            return desc;
        }

        public class ChaliceRegen : ArtifactBuff
        {
            //see Regeneration.class for effect
            public ChaliceRegen(Artifact artifact)
                : base(artifact)
            { }
        }
    }
}