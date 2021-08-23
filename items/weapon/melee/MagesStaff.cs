using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.effects.particles;
using spdd.items.bags;
using spdd.items.scrolls;
using spdd.items.wands;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.windows;

namespace spdd.items.weapon.melee
{
    public class MagesStaff : MeleeWeapon
    {
        private Wand wand;

        public const string AC_IMBUE = "IMBUE";
        public const string AC_ZAP = "ZAP";

        private const float STAFF_SCALE_FACTOR = 0.75f;

        private void InitInstance()
        {
            staffParticleFactory = new StaffParticleFactory(this);

            image = ItemSpriteSheet.MAGES_STAFF;
            hitSound = Assets.Sounds.HIT;
            hitSoundPitch = 1.1f;

            tier = 1;

            defaultAction = AC_ZAP;
            usesTargeting = true;

            unique = true;
            bones = false;
        }

        public MagesStaff()
        {
            InitInstance();
            wand = null;

            itemSelector = new ItemSelector(this);
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +    //8 base damage, down from 10
                   lvl * (tier + 1);   //scaling unaffected
        }

        public MagesStaff(Wand wand)
            : this()
        {
            wand.Identify();
            wand.cursed = false;
            this.wand = wand;
            UpdateWand(false);
            wand.curCharges = wand.maxCharges;
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(AC_IMBUE);
            if (wand != null && wand.curCharges > 0)
            {
                actions.Add(AC_ZAP);
            }
            return actions;
        }

        public override void Activate(Character ch)
        {
            if (wand != null)
                wand.Charge(ch, STAFF_SCALE_FACTOR);
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_IMBUE))
            {
                curUser = hero;
                GameScene.SelectItem(itemSelector, WndBag.Mode.WAND, Messages.Get(this, "prompt"));
            }
            else if (action.Equals(AC_ZAP))
            {
                if (wand == null)
                {
                    GameScene.Show(new WndUseItem(null, this));
                    return;
                }

                if (cursed || HasCurseEnchant())
                    wand.cursed = true;
                else
                    wand.cursed = false;

                wand.Execute(hero, AC_ZAP);
            }
        }

        public override int BuffedLvl()
        {
            int lvl = base.BuffedLvl();
            if (curUser != null && wand != null)
            {
                var buff = curUser.FindBuff<WandOfMagicMissile.MagicCharge>();
                if (buff != null && buff.Level() > lvl)
                {
                    return buff.Level();
                }
            }
            return lvl;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            if (wand != null &&
                attacker is Hero &&
                ((Hero)attacker).subClass == HeroSubClass.BATTLEMAGE)
            {
                if (wand.curCharges < wand.maxCharges)
                    wand.partialCharge += 0.33f;
                ScrollOfRecharging.Charge((Hero)attacker);
                wand.OnHit(this, attacker, defender, damage);
            }
            return base.Proc(attacker, defender, damage);
        }

        public override int ReachFactor(Character owner)
        {
            int reach = base.ReachFactor(owner);
            if (owner is Hero &&
                wand is WandOfDisintegration &&
                ((Hero)owner).subClass == HeroSubClass.BATTLEMAGE)
            {
                ++reach;
            }
            return reach;
        }

        public override bool Collect(Bag container)
        {
            if (base.Collect(container))
            {
                if (container.owner != null && wand != null)
                {
                    wand.Charge(container.owner, STAFF_SCALE_FACTOR);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnDetach()
        {
            if (wand != null)
                wand.StopCharging();
        }

        public Item ImbueWand(Wand wand, Character owner)
        {
            this.wand = null;

            //syncs the level of the two items.
            int targetLevel = Math.Max(this.GetLevel() - (curseInfusionBonus ? 1 : 0), wand.GetLevel());

            //if the staff's level is being overridden by the wand, preserve 1 upgrade
            if (wand.GetLevel() >= this.GetLevel() && this.GetLevel() > (curseInfusionBonus ? 1 : 0))
                ++targetLevel;

            SetLevel(targetLevel);
            this.wand = wand;
            UpdateWand(false);
            wand.curCharges = wand.maxCharges;
            if (owner != null)
                wand.Charge(owner);

            //This is necessary to reset any particles.
            //FIXME this is gross, should implement a better way to fully reset quickslot visuals
            int slot = Dungeon.quickslot.GetSlot(this);
            if (slot != -1)
            {
                Dungeon.quickslot.ClearSlot(slot);
                UpdateQuickslot();
                Dungeon.quickslot.SetSlot(slot, this);
                UpdateQuickslot();
            }

            BadgesExtensions.ValidateItemLevelAquired(this);

            return this;
        }

        public void GainCharge(float amt)
        {
            if (wand != null)
            {
                wand.GainCharge(amt);
            }
        }

        public Type WandClass()
        {
            return wand != null ? wand.GetType() : null;
        }

        public override Item Upgrade(bool enchant)
        {
            base.Upgrade(enchant);

            UpdateWand(true);

            return this;
        }

        public override Item Degrade()
        {
            base.Degrade();

            UpdateWand(false);

            return this;
        }

        public void UpdateWand(bool levelled)
        {
            if (wand != null)
            {
                int curCharges = wand.curCharges;
                wand.SetLevel(GetLevel());
                //gives the wand one additional max charge
                wand.maxCharges = Math.Min(wand.maxCharges + 1, 10);
                wand.curCharges = Math.Min(curCharges + (levelled ? 1 : 0), wand.maxCharges);
                UpdateQuickslot();
            }
        }

        public override string Status()
        {
            if (wand == null)
                return base.Status();
            else
                return wand.Status();
        }

        public override string Name()
        {
            if (wand == null)
            {
                return base.Name();
            }
            else
            {
                string name = Messages.Get(wand, "staff_name");
                return enchantment != null && (cursedKnown || !enchantment.Curse()) ? enchantment.Name(name) : name;
            }
        }

        public override string Info()
        {
            var info = base.Info();

            if (wand == null)
            {
                //FIXME this is removed because of journal stuff, and is generally unused.
                //perhaps reword to fit in journal better
                //info += "\n\n" + Messages.get(this, "no_wand");
            }
            else
            {
                info += "\n\n" + Messages.Get(this, "has_wand", Messages.Get(wand, "name")) + " " + wand.StatsDesc();
            }

            return info;
        }

        public override Emitter Emitter()
        {
            if (wand == null)
                return null;

            Emitter emitter = new Emitter();
            emitter.Pos(12.5f, 3);
            emitter.fillTarget = false;
            emitter.Pour(staffParticleFactory, 0.1f);

            return emitter;
        }

        private const string WAND = "wand";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(WAND, wand);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            wand = (Wand)bundle.Get(WAND);
            if (wand != null)
            {
                wand.maxCharges = Math.Min(wand.maxCharges + 1, 10);
            }
        }

        public override int Value()
        {
            return 0;
        }

        public override Weapon Enchant(Enchantment ench)
        {
            if (curseInfusionBonus && (ench == null || !ench.Curse()))
            {
                curseInfusionBonus = false;
                UpdateWand(false);
            }
            return base.Enchant(ench);
        }

        private ItemSelector itemSelector;

        class ItemSelector : WndBag.IListener
        {
            MagesStaff staff;

            public ItemSelector(MagesStaff staff)
            {
                this.staff = staff;
            }

            public void OnSelect(Item item)
            {
                if (item != null)
                {
                    if (!item.IsIdentified())
                    {
                        GLog.Warning(Messages.Get(typeof(MagesStaff), "id_first"));
                        return;
                    }
                    else if (item.cursed)
                    {
                        GLog.Warning(Messages.Get(typeof(MagesStaff), "cursed"));
                        return;
                    }

                    if (staff.wand == null)
                    {
                        ApplyWand((Wand)item);
                    }
                    else
                    {
                        int itemLevel = item.GetLevel();
                        int staffLevel = staff.GetLevel();

                        int newLevel;
                        if (itemLevel >= staffLevel)
                        {
                            newLevel = staffLevel > 0 ? itemLevel + 1 : itemLevel;
                        }
                        else
                        {
                            newLevel = staffLevel;
                        }

                        var wnd = new WndOptions("",
                                        Messages.Get(typeof(MagesStaff), "warning", newLevel),
                                        Messages.Get(typeof(MagesStaff), "yes"),
                                        Messages.Get(typeof(MagesStaff), "no"));

                        wnd.selectAction = (index) =>
                        {
                            if (index == 0)
                            {
                                ApplyWand((Wand)item);
                            }
                        };

                        GameScene.Show(wnd);
                    }
                }
            }

            public void ApplyWand(Wand wand)
            {
                Sample.Instance.Play(Assets.Sounds.BURNING);
                curUser.sprite.Emitter().Burst(ElmoParticle.Factory, 12);
                Evoke(curUser);

                Dungeon.quickslot.ClearItem(wand);

                wand.Detach(curUser.belongings.backpack);

                GLog.Positive(Messages.Get(typeof(MagesStaff), "imbue", wand.Name()));
                staff.ImbueWand(wand, curUser);

                UpdateQuickslot();
            }
        }

        private StaffParticleFactory staffParticleFactory;

        public class StaffParticleFactory : Emitter.Factory
        {
            MagesStaff staff;

            public StaffParticleFactory(MagesStaff staff)
            {
                this.staff = staff;
            }

            public override void Emit(Emitter emitter, int index, float x, float y)
            {
                StaffParticle c = (StaffParticle)emitter.GetFirstAvailable(typeof(StaffParticle));
                if (c == null)
                {
                    c = new StaffParticle(staff);
                    emitter.Add(c);
                }
                c.Reset(x, y);
            }

            public override bool LightMode()
            {
                var wand = staff.wand;

                return !((wand is WandOfDisintegration) ||
                    (wand is WandOfCorruption) ||
                    (wand is WandOfCorrosion) ||
                    (wand is WandOfRegrowth) ||
                    (wand is WandOfLivingEarth));
            }
        }

        //determines particle effects to use based on wand the staff owns.
        public class StaffParticle : PixelParticle
        {
            MagesStaff staff;
            private float minSize;
            private float maxSize;
            public float sizeJitter;

            public StaffParticle(MagesStaff staff)
            {
                this.staff = staff;
            }

            public void Reset(float x, float y)
            {
                Revive();

                speed.Set(0);

                this.x = x;
                this.y = y;

                if (staff.wand != null)
                    staff.wand.StaffFx(this);
            }

            public void SetSize(float minSize, float maxSize)
            {
                this.minSize = minSize;
                this.maxSize = maxSize;
            }

            public void SetLifespan(float life)
            {
                lifespan = left = life;
            }

            public void ShuffleXY(float amt)
            {
                x += Rnd.Float(-amt, amt);
                y += Rnd.Float(-amt, amt);
            }

            public void RadiateXY(float amt)
            {
                float hypot = (float)Math.Sqrt(speed.x * speed.x + speed.y * speed.y);
                this.x += speed.x / hypot * amt;
                this.y += speed.y / hypot * amt;
            }

            public override void Update()
            {
                base.Update();
                Size(minSize + (left / lifespan) * (maxSize - minSize) + Rnd.Float(sizeJitter));
            }
        }
    }
}