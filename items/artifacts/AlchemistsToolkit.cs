using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.utils;
using spdd.items.rings;
using spdd.actors;
using spdd.actors.hero;
using spdd.sprites;
using spdd.windows;
using spdd.utils;
using spdd.scenes;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class AlchemistsToolkit : Artifact
    {
        public AlchemistsToolkit()
        {
            image = ItemSpriteSheet.ARTIFACT_TOOLKIT;
            defaultAction = AC_BREW;

            levelCap = 10;

            charge = 0;
            partialCharge = 0;
            chargeCap = 100;
        }

        public const string AC_BREW = "BREW";

        protected WndBag.Mode mode = WndBag.Mode.POTION;

        private bool alchemyReady;

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            if (IsEquipped(hero) && !cursed)
                actions.Add(AC_BREW);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_BREW))
            {
                if (!IsEquipped(hero))
                    GLog.Information(Messages.Get(this, "need_to_equip"));
                else if (cursed)
                    GLog.Warning(Messages.Get(this, "cursed"));
                else if (!alchemyReady)
                    GLog.Information(Messages.Get(this, "not_ready"));
                else if (hero.VisibleEnemies() > hero.mindVisionEnemies.Count)
                    GLog.Information(Messages.Get(this, "enemy_near"));
                else
                {
                    AlchemyScene.SetProvider(hero.FindBuff<KitEnergy>());
                    Game.SwitchScene(typeof(AlchemyScene));
                }
            }
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new KitEnergy(this);
        }

        public override void Charge(Hero target)
        {
            if (charge < chargeCap)
            {
                partialCharge += 0.5f;
                if (partialCharge >= 1)
                {
                    --partialCharge;
                    ++charge;
                    UpdateQuickslot();
                }
            }
        }

        public void AbsorbEnergy(int energy)
        {
            exp += energy;
            while (exp >= 10 && GetLevel() < levelCap)
            {
                Upgrade();
                exp -= 10;
            }
            if (GetLevel() == levelCap)
            {
                partialCharge += exp;
                energy -= exp;
                exp = 0;
            }

            partialCharge += energy / 3f;
            while (partialCharge >= 1)
            {
                partialCharge -= 1;
                ++charge;

                if (charge >= chargeCap)
                {
                    charge = chargeCap;
                    partialCharge = 0;
                    break;
                }
            }
            UpdateQuickslot();
        }

        public override string Desc()
        {
            var result = Messages.Get(this, "desc");

            if (IsEquipped(Dungeon.hero))
            {
                if (cursed)
                    result += "\n\n" + Messages.Get(this, "desc_cursed");
                else if (!alchemyReady)
                    result += "\n\n" + Messages.Get(this, "desc_warming");
                else
                    result += "\n\n" + Messages.Get(this, "desc_hint");
            }

            return result;
        }

        public override bool DoEquip(Hero hero)
        {
            if (base.DoEquip(hero))
            {
                alchemyReady = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        private const string READY = "ready";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(READY, alchemyReady);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            alchemyReady = bundle.GetBoolean(READY);
        }

        public void GainCharge(float levelPortion, Character target)
        {
            alchemyReady = true;

            if (cursed)
                return;

            if (charge < chargeCap)
            {
                //generates 2 energy every hero level, +0.1 energy per toolkit level
                //to a max of 12 energy per hero level
                //This means that energy absorbed into the kit is recovered in 6.67 hero levels (as 33% of input energy is kept)
                //exp towards toolkit levels is included here
                float effectiveLevel = GameMath.Gate(0, GetLevel() + exp / 10f, 10);
                float chargeGain = (2 + (1f * effectiveLevel)) * levelPortion;
                chargeGain *= RingOfEnergy.ArtifactChargeMultiplier(target);
                partialCharge += chargeGain;

                //charge is in increments of 1/10 max hunger value.
                while (partialCharge >= 1)
                {
                    ++charge;
                    partialCharge -= 1;

                    if (charge == chargeCap)
                    {
                        GLog.Positive(Messages.Get(typeof(AlchemistsToolkit), "full"));
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

        public class KitEnergy : ArtifactBuff, AlchemyScene.IAlchemyProvider
        {
            public KitEnergy(Artifact artifact)
                : base(artifact)
            { }

            public void GainCharge(float levelPortion)
            {
                var kit = (AlchemistsToolkit)artifact;
                kit.GainCharge(levelPortion, target);
            }

            public int GetEnergy()
            {
                return artifact.charge;
            }

            public void SpendEnergy(int reduction)
            {
                artifact.charge = Math.Max(0, artifact.charge - reduction);
            }
        }

        public class UpgradeKit : Recipe
        {
            public override bool TestIngredients(List<Item> ingredients)
            {
                return ingredients[0] is AlchemistsToolkit &&
                    !AlchemyScene.ProviderIsToolkit();
            }

            private static int lastCost;

            public override int Cost(List<Item> ingredients)
            {
                return lastCost = Math.Max(1, AlchemyScene.AvailableEnergy());
            }

            public override Item Brew(List<Item> ingredients)
            {
                AlchemistsToolkit existing = (AlchemistsToolkit)ingredients[0];

                existing.AbsorbEnergy(lastCost);

                return existing;
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                AlchemistsToolkit sample = new AlchemistsToolkit();
                sample.Identify();

                AlchemistsToolkit existing = (AlchemistsToolkit)ingredients[0];

                sample.charge = existing.charge;
                sample.partialCharge = existing.partialCharge;
                sample.exp = existing.exp;
                sample.SetLevel(existing.GetLevel());
                sample.AbsorbEnergy(AlchemyScene.AvailableEnergy());
                return sample;
            }
        }
    }
}