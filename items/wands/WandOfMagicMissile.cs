using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items.weapon.melee;
using spdd.mechanics;
using spdd.messages;
using spdd.sprites;
using spdd.ui;

namespace spdd.items.wands
{
    public class WandOfMagicMissile : DamageWand
    {
        public WandOfMagicMissile()
        {
            image = ItemSpriteSheet.WAND_MAGIC_MISSILE;
        }

        public override int Min(int lvl)
        {
            return 2 + lvl;
        }

        public override int Max(int lvl)
        {
            return 8 + 2 * lvl;
        }

        protected override void OnZap(Ballistic bolt)
        {
            var ch = Actor.FindChar(bolt.collisionPos);
            if (ch != null)
            {
                ProcessSoulMark(ch, ChargesPerCast());
                ch.Damage(DamageRoll(), this);
                Sample.Instance.Play(Assets.Sounds.HIT_MAGIC, 1, Rnd.Float(0.87f, 1.15f));

                ch.sprite.Burst(new Color(0xFF, 0xFF, 0xFF, 0xFF), BuffedLvl() / 2 + 2);

                //apply the magic charge buff if we have another wand in inventory of a lower level, or already have the buff
                foreach (Wand.Charger wandCharger in curUser.Buffs<Wand.Charger>())
                {
                    if (wandCharger.Wand().BuffedLvl() < BuffedLvl() || curUser.FindBuff<MagicCharge>() != null)
                    {
                        Buff.Prolong<MagicCharge>(curUser, MagicCharge.DURATION).SetLevel(BuffedLvl());
                        break;
                    }
                }
            }
            else
            {
                Dungeon.level.PressCell(bolt.collisionPos);
            }
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            SpellSprite.Show(attacker, SpellSprite.CHARGE);
            foreach (Wand.Charger c in attacker.Buffs<Wand.Charger>())
            {
                if (c.Wand() != this)
                {
                    c.GainCharge(0.33f);
                }
            }
        }

        protected override int InitialCharges()
        {
            return 3;
        }

        [SPDStatic]
        public class MagicCharge : FlavourBuff
        {
            public MagicCharge()
            {
                type = BuffType.POSITIVE;
                announced = true;
            }

            public const float DURATION = 4f;

            private int level;

            public void SetLevel(int level)
            {
                this.level = Math.Max(level, this.level);
            }

            public override void Detach()
            {
                base.Detach();
                QuickSlotButton.Refresh();
            }

            public int Level()
            {
                return level;
            }

            public override int Icon()
            {
                return BuffIndicator.RECHARGING;
            }

            public override void TintIcon(Image icon)
            {
                icon.Hardlight(0.2f, 0.6f, 1f);
            }

            public override float IconFadePercent()
            {
                return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", Level(), DispTurns());
            }
        }
    }
}