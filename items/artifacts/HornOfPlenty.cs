using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.items.food;
using spdd.items.rings;
using spdd.sprites;
using spdd.scenes;
using spdd.windows;
using spdd.effects;
using spdd.utils;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class HornOfPlenty : Artifact
    {
        public HornOfPlenty()
        {
            image = ItemSpriteSheet.ARTIFACT_HORN1;

            levelCap = 10;

            charge = 0;
            partialCharge = 0;
            chargeCap = 10 + GetLevel();

            defaultAction = AC_EAT;
        }

        private int storedFoodEnergy;

        public const string AC_EAT = "EAT";
        public const string AC_STORE = "STORE";

        protected WndBag.Mode mode = WndBag.Mode.FOOD;

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);

            if (IsEquipped(hero) && charge > 0)
                actions.Add(AC_EAT);
            if (IsEquipped(hero) && GetLevel() < levelCap && !cursed)
                actions.Add(AC_STORE);

            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_EAT))
            {
                if (!IsEquipped(hero))
                {
                    GLog.Information(Messages.Get(typeof(Artifact), "need_to_equip"));
                }
                else if (charge == 0)
                {
                    GLog.Information(Messages.Get(this, "no_food"));
                }
                else
                {
                    //consume as much food as it takes to be full, to a minimum of 1
                    var hunger = Buff.Affect<Hunger>(Dungeon.hero);
                    int chargesToUse = Math.Max(1, hunger.HungerValue() / (int)(Hunger.STARVING / 10));
                    if (chargesToUse > charge)
                        chargesToUse = charge;
                    hunger.Satisfy((Hunger.STARVING / 10) * chargesToUse);

                    Food.FoodProc(hero);

                    ++Statistics.foodEaten;

                    charge -= chargesToUse;

                    hero.sprite.Operate(hero.pos);
                    hero.Busy();
                    SpellSprite.Show(hero, SpellSprite.FOOD);
                    Sample.Instance.Play(Assets.Sounds.EAT);
                    GLog.Information(Messages.Get(this, "eat"));

                    hero.Spend(Food.TIME_TO_EAT);

                    BadgesExtensions.ValidateFoodEaten();

                    if (charge >= 15)
                        image = ItemSpriteSheet.ARTIFACT_HORN4;
                    else if (charge >= 10)
                        image = ItemSpriteSheet.ARTIFACT_HORN3;
                    else if (charge >= 5)
                        image = ItemSpriteSheet.ARTIFACT_HORN2;
                    else
                        image = ItemSpriteSheet.ARTIFACT_HORN1;

                    UpdateQuickslot();
                }
            }
            else if (action.Equals(AC_STORE))
            {
                GameScene.SelectItem(itemSelector, mode, Messages.Get(this, "prompt"));
            }
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new HornRecharge(this);
        }

        public override void Charge(Hero target)
        {
            if (charge < chargeCap)
            {
                partialCharge += 0.25f;
                if (partialCharge >= 1)
                {
                    --partialCharge;
                    ++charge;

                    if (charge == chargeCap)
                    {
                        GLog.Positive(Messages.Get(typeof(HornOfPlenty), "full"));
                        partialCharge = 0;
                    }

                    if (charge >= 15)
                        image = ItemSpriteSheet.ARTIFACT_HORN4;
                    else if (charge >= 10)
                        image = ItemSpriteSheet.ARTIFACT_HORN3;
                    else if (charge >= 5)
                        image = ItemSpriteSheet.ARTIFACT_HORN2;

                    UpdateQuickslot();
                }
            }
        }

        public override string Desc()
        {
            string desc = base.Desc();

            if (IsEquipped(Dungeon.hero))
            {
                if (!cursed)
                {
                    if (GetLevel() < levelCap)
                        desc += "\n\n" + Messages.Get(this, "desc_hint");
                }
                else
                {
                    desc += "\n\n" + Messages.Get(this, "desc_cursed");
                }
            }

            return desc;
        }

        public override void SetLevel(int value)
        {
            base.SetLevel(value);
            chargeCap = 10 + GetLevel();
        }

        public override Item Upgrade()
        {
            base.Upgrade();
            chargeCap = 10 + GetLevel();
            return this;
        }

        public void GainFoodValue(Food food)
        {
            if (GetLevel() >= 10)
                return;

            storedFoodEnergy += (int)food.energy;
            if (storedFoodEnergy >= Hunger.HUNGRY)
            {
                int upgrades = storedFoodEnergy / (int)Hunger.HUNGRY;
                upgrades = Math.Min(upgrades, 10 - GetLevel());
                Upgrade(upgrades);
                storedFoodEnergy -= (int)(upgrades * Hunger.HUNGRY);
                if (GetLevel() == 10)
                {
                    storedFoodEnergy = 0;
                    GLog.Positive(Messages.Get(this, "maxlevel"));
                }
                else
                {
                    GLog.Positive(Messages.Get(this, "levelup"));
                }
            }
            else
            {
                GLog.Information(Messages.Get(this, "feed"));
            }
        }

        private const string STORED = "stored";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(STORED, storedFoodEnergy);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            storedFoodEnergy = bundle.GetInt(STORED);

            if (charge >= 15)
                image = ItemSpriteSheet.ARTIFACT_HORN4;
            else if (charge >= 10)
                image = ItemSpriteSheet.ARTIFACT_HORN3;
            else if (charge >= 5)
                image = ItemSpriteSheet.ARTIFACT_HORN2;
        }

        public void HornRechargeGainCharge(float levelPortion, HornRecharge buff)
        {
            if (cursed)
                return;

            var target = buff.target;

            if (charge < chargeCap)
            {
                //generates 0.25x max hunger value every hero level, +0.125x max value per horn level
                //to a max of 1.5x max hunger value per hero level
                //This means that a standard ration will be recovered in ~5.333 hero levels
                float chargeGain = Hunger.STARVING * levelPortion * (0.25f + (0.125f * GetLevel()));
                chargeGain *= RingOfEnergy.ArtifactChargeMultiplier(target);
                partialCharge += chargeGain;

                //charge is in increments of 1/10 max hunger value.
                while (partialCharge >= Hunger.STARVING / 10)
                {
                    ++charge;
                    partialCharge -= Hunger.STARVING / 10;

                    if (charge >= 15) image = ItemSpriteSheet.ARTIFACT_HORN4;
                    else if (charge >= 10) image = ItemSpriteSheet.ARTIFACT_HORN3;
                    else if (charge >= 5) image = ItemSpriteSheet.ARTIFACT_HORN2;
                    else image = ItemSpriteSheet.ARTIFACT_HORN1;

                    if (charge == chargeCap)
                    {
                        GLog.Positive(Messages.Get(typeof(HornOfPlenty), "full"));
                        partialCharge = 0;
                    }

                    UpdateQuickslot();
                }
            }
            else
            {
                partialCharge = 0;
            }
        }

        public class HornRecharge : ArtifactBuff
        {
            public HornRecharge(Artifact artifact)
                : base(artifact)
            { }

            public void GainCharge(float levelPortion)
            {
                var hp = (HornOfPlenty)artifact;
                hp.HornRechargeGainCharge(levelPortion, this);
            }
        }

        protected static ItemSelector itemSelector = new ItemSelector();

        public class ItemSelector : WndBag.IListener
        {
            public void OnSelect(Item item)
            {
                if (item != null && item is Food)
                {
                    if (item is Blandfruit && ((Blandfruit)item).potionAttrib == null)
                    {
                        GLog.Warning(Messages.Get(typeof(HornOfPlenty), "reject"));
                    }
                    else
                    {
                        Hero hero = Dungeon.hero;
                        hero.sprite.Operate(hero.pos);
                        hero.Busy();
                        hero.Spend(Food.TIME_TO_EAT);

                        ((HornOfPlenty)curItem).GainFoodValue(((Food)item));
                        item.Detach(hero.belongings.backpack);
                    }
                }
            }
        }
    }
}