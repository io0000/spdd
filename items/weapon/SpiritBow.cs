using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.effects;
using spdd.items.rings;
using spdd.items.weapon.missiles;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.items.weapon
{
    public class SpiritBow : Weapon
    {
        public const string AC_SHOOT = "SHOOT";

        public SpiritBow()
        {
            image = ItemSpriteSheet.SPIRIT_BOW;

            defaultAction = AC_SHOOT;
            usesTargeting = true;

            unique = true;
            bones = false;

            shooter = new Shooter(this);
        }

        public bool sniperSpecial;

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Remove(AC_EQUIP);
            actions.Add(AC_SHOOT);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_SHOOT))
            {
                curUser = hero;
                curItem = this;
                GameScene.SelectCell(shooter);
            }
        }

        public override string Info()
        {
            string info = Desc();

            int str = Dungeon.hero.GetSTR();
            int strReq = STRReq();

            info += "\n\n" + Messages.Get(typeof(SpiritBow), "stats",
                    augment.DamageFactor(Min()),
                    augment.DamageFactor(Max()),
                    strReq);

            if (strReq > str)
            {
                info += " " + Messages.Get(typeof(Weapon), "too_heavy");
            }
            else if (str > strReq)
            {
                info += " " + Messages.Get(typeof(Weapon), "excess_str", str - strReq);
            }

            switch (augment)
            {
                case Augment.SPEED:
                    info += "\n\n" + Messages.Get(typeof(Weapon), "faster");
                    break;
                case Augment.DAMAGE:
                    info += "\n\n" + Messages.Get(typeof(Weapon), "stronger");
                    break;
            }

            if (enchantment != null && (cursedKnown || !enchantment.Curse()))
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "enchanted", enchantment.Name());
                info += " " + Messages.Get(enchantment, "desc");
            }

            if (cursed && IsEquipped(Dungeon.hero))
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "cursed_worn");
            }
            else if (cursedKnown && cursed)
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "cursed");
            }
            else if (!IsIdentified() && cursedKnown)
            {
                info += "\n\n" + Messages.Get(typeof(Weapon), "not_cursed");
            }

            info += "\n\n" + Messages.Get(typeof(MissileWeapon), "distance");

            return info;
        }

        public override int STRReq(int lvl)
        {
            lvl = Math.Max(0, lvl);
            //strength req decreases at +1,+3,+6,+10,etc.
            return 10 - (int)(Math.Sqrt(8 * lvl + 1) - 1) / 2;
        }

        public override int Min(int lvl)
        {
            return 1 + Dungeon.hero.lvl / 5
                    + RingOfSharpshooting.LevelDamageBonus(Dungeon.hero)
                    + (curseInfusionBonus ? 1 : 0);
        }

        public override int Max(int lvl)
        {
            return 6 + (int)(Dungeon.hero.lvl / 2.5f)
                    + 2 * RingOfSharpshooting.LevelDamageBonus(Dungeon.hero)
                    + (curseInfusionBonus ? 2 : 0);
        }

        private int targetPos;

        public override int DamageRoll(Character owner)
        {
            int damage = augment.DamageFactor(base.DamageRoll(owner));

            if (owner is Hero)
            {
                int exStr = ((Hero)owner).GetSTR() - STRReq();
                if (exStr > 0)
                {
                    damage += Rnd.IntRange(0, exStr);
                }
            }

            if (sniperSpecial)
            {
                switch (augment)
                {
                    case Augment.NONE:
                        damage = (int)Math.Round(damage * 0.667f, MidpointRounding.AwayFromZero);
                        break;
                    case Augment.SPEED:
                        damage = (int)Math.Round(damage * 0.5f, MidpointRounding.AwayFromZero);
                        break;
                    case Augment.DAMAGE:
                        //as distance increases so does damage, capping at 3x:
                        //1.20x|1.35x|1.52x|1.71x|1.92x|2.16x|2.43x|2.74x|3.00x
                        int distance = Dungeon.level.Distance(owner.pos, targetPos) - 1;
                        float multiplier = Math.Min(3f, 1.2f * (float)Math.Pow(1.125f, distance));
                        damage = (int)Math.Round(damage * multiplier, MidpointRounding.AwayFromZero);
                        break;
                }
            }

            return damage;
        }

        public override float SpeedFactor(Character owner)
        {
            if (sniperSpecial)
            {
                switch (augment)
                {
                    case Augment.NONE:
                    default:
                        return 0f;
                    case Augment.SPEED:
                        return 1f * RingOfFuror.AttackDelayMultiplier(owner);
                    case Augment.DAMAGE:
                        return 2f * RingOfFuror.AttackDelayMultiplier(owner);
                }
            }
            else
            {
                return base.SpeedFactor(owner);
            }
        }

        public override int GetLevel()
        {
            return (Dungeon.hero == null ? 0 : Dungeon.hero.lvl / 5) + (curseInfusionBonus ? 1 : 0);
        }

        public override int BuffedLvl()
        {
            //level isn't affected by buffs/debuffs
            return GetLevel();
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public SpiritArrow KnockArrow()
        {
            return new SpiritArrow(this);
        }

        public class SpiritArrow : MissileWeapon
        {
            SpiritBow bow;

            public SpiritArrow(SpiritBow bow)
            {
                this.bow = bow;

                image = ItemSpriteSheet.SPIRIT_ARROW;

                hitSound = Assets.Sounds.HIT_ARROW;
            }

            public override int DamageRoll(Character owner)
            {
                return bow.DamageRoll(owner);
            }

            public override bool HasEnchant(Type type, Character owner)
            {
                return bow.HasEnchant(type, owner);
            }

            public override int Proc(Character attacker, Character defender, int damage)
            {
                return bow.Proc(attacker, defender, damage);
            }

            public override float SpeedFactor(Character user)
            {
                return bow.SpeedFactor(user);
            }

            public override float AccuracyFactor(Character owner)
            {
                if (bow.sniperSpecial && bow.augment == Augment.DAMAGE)
                {
                    return float.PositiveInfinity;
                }
                else
                {
                    return base.AccuracyFactor(owner);
                }
            }

            public override int STRReq(int lvl)
            {
                return bow.STRReq(lvl);
            }

            public override void OnThrow(int cell)
            {
                var enemy = Actor.FindChar(cell);
                if (enemy == null || enemy == curUser)
                {
                    parent = null;
                    Splash.At(cell, new Color(0x99, 0xFF, 0xFF, 0xCC), 1);
                }
                else
                {
                    if (!curUser.Shoot(enemy, this))
                    {
                        Splash.At(cell, new Color(0x99, 0xFF, 0xFF, 0xCC), 1);
                    }

                    if (bow.sniperSpecial && bow.augment != Augment.SPEED)
                        bow.sniperSpecial = false;
                }
            }

            public override void ThrowSound()
            {
                Sample.Instance.Play(Assets.Sounds.ATK_SPIRITBOW, 1, Rnd.Float(0.87f, 1.15f));
            }

            int flurryCount = -1;

            public override void Cast(Hero user, int dst)
            {
                int cell = ThrowPos(user, dst);
                bow.targetPos = cell;
                if (bow.sniperSpecial && bow.augment == Augment.SPEED)
                {
                    if (flurryCount == -1)
                        flurryCount = 3;

                    var enemy = Actor.FindChar(cell);

                    if (enemy == null)
                    {
                        user.SpendAndNext(CastDelay(user, dst));
                        bow.sniperSpecial = false;
                        flurryCount = -1;
                        return;
                    }
                    QuickSlotButton.Target(enemy);

                    bool last = flurryCount == 1;

                    user.Busy();

                    ThrowSound();

                    var callback1 = new ActionCallback();
                    callback1.action = () =>
                    {
                        if (enemy.IsAlive())
                        {
                            curUser = user;
                            OnThrow(cell);
                        }

                        if (last)
                        {
                            user.SpendAndNext(CastDelay(user, dst));
                            bow.sniperSpecial = false;
                            flurryCount = -1;
                        }
                    };

                    var obj = user.sprite.parent.Recycle<MissileSprite>();
                    obj.Reset(user.sprite,
                        cell,
                        this,
                        callback1);

                    var callback2 = new ActionCallback();
                    callback2.action = () =>
                    {
                        --flurryCount;
                        if (flurryCount > 0)
                        {
                            Cast(user, dst);
                        }
                    };

                    user.sprite.Zap(cell, callback2);
                }
                else
                {
                    base.Cast(user, dst);
                }
            }
        }

        private CellSelector.IListener shooter;

        class Shooter : CellSelector.IListener
        {
            SpiritBow bow;
            public Shooter(SpiritBow bow)
            {
                this.bow = bow;
            }

            public void OnSelect(int? target)
            {
                if (target != null)
                {
                    bow.KnockArrow().Cast(curUser, target.Value);
                }
            }

            public string Prompt()
            {
                return Messages.Get(typeof(SpiritBow), "prompt");
            }
        }
    }
}