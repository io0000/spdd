using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa;
using watabou.noosa.audio;
using spdd.items.rings;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.plants;
using spdd.utils;
using spdd.effects;
using spdd.effects.particles;
using spdd.scenes;
using spdd.windows;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class SandalsOfNature : Artifact
    {
        public SandalsOfNature()
        {
            image = ItemSpriteSheet.ARTIFACT_SANDALS;

            levelCap = 3;

            charge = 0;

            defaultAction = AC_ROOT;

            itemSelector = new ItemSelector(this);
        }

        public const string AC_FEED = "FEED";
        public const string AC_ROOT = "ROOT";

        protected WndBag.Mode mode = WndBag.Mode.SEED;

        public List<Type> seeds = new List<Type>();

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);

            if (IsEquipped(hero) && GetLevel() < 3 && !cursed)
                actions.Add(AC_FEED);
            if (IsEquipped(hero) && charge > 0)
                actions.Add(AC_ROOT);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_FEED))
            {
                GameScene.SelectItem(itemSelector, mode, Messages.Get(this, "prompt"));
            }
            else if (action.Equals(AC_ROOT) && GetLevel() > 0)
            {
                if (!IsEquipped(hero))
                    GLog.Information(Messages.Get(typeof(Artifact), "need_to_equip"));
                else if (charge == 0)
                    GLog.Information(Messages.Get(this, "no_charge"));
                else
                {
                    Buff.Prolong<Roots>(hero, Roots.DURATION);
                    Buff.Affect<Earthroot.Armor>(hero).level = charge;
                    CellEmitter.Bottom(hero.pos).Start(EarthParticle.Factory, 0.05f, 8);
                    Camera.main.Shake(1, 0.4f);
                    charge = 0;
                    UpdateQuickslot();
                }
            }
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new Naturalism(this);
        }

        public override void Charge(Hero target)
        {
            target.FindBuff<Naturalism>().Charge();
        }

        public override string Name()
        {
            if (GetLevel() == 0)
                return base.Name();
            else
                return Messages.Get(this, "name_" + GetLevel());
        }

        public override string Desc()
        {
            string desc = Messages.Get(this, "desc_" + (GetLevel() + 1));

            if (IsEquipped(Dungeon.hero))
            {
                desc += "\n\n";

                if (!cursed)
                    desc += Messages.Get(this, "desc_hint");
                else
                    desc += Messages.Get(this, "desc_cursed");

                if (GetLevel() > 0)
                    desc += "\n\n" + Messages.Get(this, "desc_ability");
            }

            if (seeds.Count > 0)
            {
                desc += "\n\n" + Messages.Get(this, "desc_seeds", seeds.Count);
            }

            return desc;
        }

        public override Item Upgrade()
        {
            if (GetLevel() < 0) image = ItemSpriteSheet.ARTIFACT_SANDALS;
            else if (GetLevel() == 0) image = ItemSpriteSheet.ARTIFACT_SHOES;
            else if (GetLevel() == 1) image = ItemSpriteSheet.ARTIFACT_BOOTS;
            else if (GetLevel() >= 2) image = ItemSpriteSheet.ARTIFACT_GREAVES;
            return base.Upgrade();
        }

        public static bool CanUseSeed(Item item)
        {
            if (item is Plant.Seed)
            {
                return !(curItem is SandalsOfNature) ||
                        !((SandalsOfNature)curItem).seeds.Contains(item.GetType());
            }
            return false;
        }

        private const string SEEDS = "seeds";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(SEEDS, seeds.ToArray());
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            if (bundle.Contains(SEEDS))
            {
                foreach (var type in bundle.GetClassArray(SEEDS))
                {
                    seeds.Add(type);
                }
            }
            if (GetLevel() == 1)
                image = ItemSpriteSheet.ARTIFACT_SHOES;
            else if (GetLevel() == 2)
                image = ItemSpriteSheet.ARTIFACT_BOOTS;
            else if (GetLevel() >= 3)
                image = ItemSpriteSheet.ARTIFACT_GREAVES;
        }

        public class Naturalism : ArtifactBuff
        {
            public Naturalism(Artifact artifact)
                : base(artifact)
            { }

            public void Charge()
            {
                if (artifact.GetLevel() > 0 && artifact.charge < target.HT)
                {
                    //gain 1+(1*level)% of the difference between current charge and max HP.
                    float chargeGain = (target.HT - artifact.charge) * (.01f + artifact.GetLevel() * 0.01f);
                    chargeGain *= RingOfEnergy.ArtifactChargeMultiplier(target);
                    artifact.partialCharge += Math.Max(0, chargeGain);
                    while (artifact.partialCharge > 1)
                    {
                        ++artifact.charge;
                        --artifact.partialCharge;
                    }
                    Item.UpdateQuickslot();
                }
            }
        }

        protected ItemSelector itemSelector;

        public class ItemSelector : WndBag.IListener
        {
            SandalsOfNature son;
            public ItemSelector(SandalsOfNature son)
            {
                this.son = son;
            }
            public void OnSelect(Item item)
            {
                son.ItemSelectorOnSelect(item);
            }
        }

        public void ItemSelectorOnSelect(Item item)
        {
            if (item != null && item is Plant.Seed)
            {
                if (seeds.Contains(item.GetType()))
                {
                    GLog.Warning(Messages.Get(typeof(SandalsOfNature), "already_fed"));
                }
                else
                {
                    seeds.Add(item.GetType());

                    Hero hero = Dungeon.hero;
                    hero.sprite.Operate(hero.pos);
                    Sample.Instance.Play(Assets.Sounds.PLANT);
                    hero.Busy();
                    hero.Spend(2f);
                    if (seeds.Count >= 3 + (GetLevel() * 3))
                    {
                        seeds.Clear();
                        Upgrade();
                        if (GetLevel() >= 1 && GetLevel() <= 3)
                        {
                            GLog.Positive(Messages.Get(typeof(SandalsOfNature), "levelup"));
                        }
                    }
                    else
                    {
                        GLog.Information(Messages.Get(typeof(SandalsOfNature), "absorb_seed"));
                    }

                    item.Detach(hero.belongings.backpack);
                }
            }
        }
    }
}