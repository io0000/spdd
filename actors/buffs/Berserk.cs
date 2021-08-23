using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.effects;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.ui;


namespace spdd.actors.buffs
{
    public class Berserk : Buff
    {
        private enum State
        {
            NORMAL, BERSERK, RECOVERING
        }
        private State state = State.NORMAL;

        private const float LEVEL_RECOVER_START = 2f;
        private float levelRecovery;

        private float power;

        private const string STATE = "state";
        private const string LEVEL_RECOVERY = "levelrecovery";
        private const string POWER = "power";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);

            bundle.Put(STATE, state.ToString());    // enum
            bundle.Put(POWER, power);
            if (state == State.RECOVERING)
                bundle.Put(LEVEL_RECOVERY, levelRecovery);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            state = bundle.GetEnum<State>(STATE);
            power = bundle.GetFloat(POWER);
            if (state == State.RECOVERING)
                levelRecovery = bundle.GetFloat(LEVEL_RECOVERY);
        }

        public override bool Act()
        {
            if (Berserking())
            {
                var buff = target.FindBuff<BrokenSeal.WarriorShield>();
                if (target.HP <= 0)
                {
                    int dmg = 1 + (int)Math.Ceiling(target.Shielding() * 0.1f);
                    if (buff != null && buff.Shielding() > 0)
                    {
                        buff.AbsorbDamage(dmg);
                    }
                    else
                    {
                        //if there is no shield buff, or it is empty, then try to remove from other shielding buffs
                        foreach (ShieldBuff s in target.Buffs<ShieldBuff>())
                        {
                            dmg = s.AbsorbDamage(dmg);
                            if (dmg == 0)
                                break;
                        }
                    }

                    if (target.Shielding() <= 0)
                    {
                        target.Die(this);
                        if (!target.IsAlive())
                            Dungeon.Fail(GetType());
                    }
                }
                else
                {
                    state = State.RECOVERING;
                    levelRecovery = LEVEL_RECOVER_START;
                    if (buff != null)
                        buff.AbsorbDamage(buff.Shielding());
                    power = 0f;
                }
            }
            else if (state == State.NORMAL)
            {
                float value = (float)(GameMath.Gate(0.1f, power, 1f) * 0.067f * Math.Pow((target.HP / (float)target.HT), 2));
                power -= value;

                if (power <= 0)
                    Detach();
            }

            Spend(TICK);
            return true;
        }

        public int DamageFactor(int dmg)
        {
            float bonus = Math.Min(1.5f, 1f + (power / 2f));
            return (int)Math.Round(dmg * bonus, MidpointRounding.AwayFromZero);
        }

        public bool Berserking()
        {
            if (target.HP == 0 && state == State.NORMAL && power >= 1f)
            {
                var shield = target.FindBuff<BrokenSeal.WarriorShield>();
                if (shield != null)
                {
                    state = State.BERSERK;
                    shield.Supercharge(shield.MaxShield() * 10);

                    SpellSprite.Show(target, SpellSprite.BERSERK);
                    Sample.Instance.Play(Assets.Sounds.CHALLENGE);
                    GameScene.Flash(new Color(0xFF, 0x00, 0x00, 0xFF));
                }
            }

            return state == State.BERSERK && target.Shielding() > 0;
        }

        public void Damage(int damage)
        {
            if (state == State.RECOVERING)
                return;
            power = Math.Min(1.1f, power + (damage / (float)target.HT) / 3f);
            BuffIndicator.RefreshHero(); //show new power immediately
        }

        public void Recover(float percent)
        {
            if (levelRecovery > 0)
            {
                levelRecovery -= percent;
                if (levelRecovery <= 0)
                {
                    state = State.NORMAL;
                    levelRecovery = 0;
                }
            }
        }

        public override int Icon()
        {
            return BuffIndicator.BERSERK;
        }

        public override void TintIcon(Image icon)
        {
            switch (state)
            {
                case State.NORMAL:
                default:
                    if (power < 1.0f)
                        icon.Hardlight(1.0f, 0.5f, 0.0f);
                    else
                        icon.Hardlight(1.0f, 0.0f, 0.0f);
                    break;
                case State.BERSERK:
                    icon.Hardlight(1.0f, 0.0f, 0.0f);
                    break;
                case State.RECOVERING:
                    icon.Hardlight(0.0f, 0.0f, 1.0f);
                    break;
            }
        }

        public override float IconFadePercent()
        {
            switch (state)
            {
                case State.NORMAL:
                default:
                    return Math.Max(0.0f, 1.0f - power);
                case State.BERSERK:
                    return 0.0f;
                case State.RECOVERING:
                    return 1.0f - levelRecovery / 2.0f;
            }
        }

        public override string ToString()
        {
            switch (state)
            {
                case State.NORMAL:
                default:
                    return Messages.Get(this, "angered");
                case State.BERSERK:
                    return Messages.Get(this, "berserk");
                case State.RECOVERING:
                    return Messages.Get(this, "recovering");
            }
        }

        public override string Desc()
        {
            float dispDamage = (DamageFactor(10000) / 100f) - 100f;
            switch (state)
            {
                case State.NORMAL:
                default:
                    return Messages.Get(this, "angered_desc", Math.Floor(power * 100f), dispDamage);    // %f %f 
                case State.BERSERK:
                    return Messages.Get(this, "berserk_desc");
                case State.RECOVERING:
                    return Messages.Get(this, "recovering_desc", levelRecovery);
            }
        }
    }
}