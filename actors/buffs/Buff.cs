using System;
using System.Collections.Generic;
using System.Globalization;
using watabou.noosa;
using watabou.utils;
using spdd.ui;

namespace spdd.actors.buffs
{
    public class Buff : Actor
    {
        public Character target;

        public Buff()
        {
            actPriority = BUFF_PRIO; //low priority, towards the end of a turn
        }

        //determines how the buff is announced when it is shown.
        public enum BuffType { POSITIVE, NEGATIVE, NEUTRAL }
        public BuffType type = BuffType.NEUTRAL;

        //whether or not the buff announces its name
        public bool announced;

        protected HashSet<Type> resistances = new HashSet<Type>();

        public HashSet<Type> Resistances()
        {
            return new HashSet<Type>(resistances);
        }

        protected HashSet<Type> immunities = new HashSet<Type>();

        public HashSet<Type> Immunities()
        {
            return new HashSet<Type>(immunities);
        }

        public virtual bool AttachTo(Character target)
        {
            if (target.IsImmune(GetType()))
                return false;

            this.target = target;
            this.target.Add(this);

            if (target.Buffs().Contains(this))
            {
                if (target.sprite != null)
                    Fx(true);
                return true;
            }
            else
            {
                this.target = null;
                return false;
            }
        }

        public virtual void Detach()
        {
            if (target.sprite != null)
                Fx(false);
            target.Remove(this);
        }

        public override bool Act()
        {
            Deactivate();
            return true;
        }

        public virtual int Icon()
        {
            return BuffIndicator.NONE;
        }

        //some buffs may want to tint the base texture color of their icon
        public virtual void TintIcon(Image icon)
        {
            //do nothing by default
        }

        //percent (0-1) to fade out out the buff icon, usually if buff is expiring
        public virtual float IconFadePercent()
        {
            return 0;
        }

        //visual effect usually attached to the sprite of the character the buff is attacked to
        public virtual void Fx(bool on)
        {
            //do nothing by default
        }

        public virtual string HeroMessage()
        {
            return null;
        }

        public virtual string Desc()
        {
            return "";
        }

        //to handle the common case of showing how many turns are remaining in a buff description.
        protected string DispTurns(float input)
        {
            return input.ToString("0.00", CultureInfo.InvariantCulture);
        }

        //buffs act after the hero, so it is often useful to use cooldown+1 when display buff time remaining
        public float Visualcooldown()
        {
            return Cooldown() + 1f;
        }

        public static T Append<T>(Character target) where T : Buff
        {
            T buff = (T)Reflection.NewInstance(typeof(T));
            buff.AttachTo(target);
            return buff;
        }

        public static Buff Append(Character target, Type type)
        {
            Buff buff = (Buff)Reflection.NewInstance(type);
            buff.AttachTo(target);
            return buff;
        }

        public static T Append<T>(Character target, float duration) where T : FlavourBuff
        {
            T buff = Append<T>(target);
            buff.Spend(duration * target.Resist(typeof(T)));
            return buff;
        }

        public static FlavourBuff Append(Character target, Type type, float duration)
        {
            FlavourBuff buff = (FlavourBuff)Append(target, type);
            buff.Spend(duration * target.Resist(type));
            return buff;
        }

        //same as append, but prevents duplication.
        public static T Affect<T>(Character target) where T : Buff
        {
            var buff = target.FindBuff<T>();
            if (buff != null)
                return buff;
            else
                return Append<T>(target);
        }

        // 기존에 버프가 걸려 있었으면? duration 리셋
        public static T Affect<T>(Character target, float duration) where T : FlavourBuff
        {
            T buff = Affect<T>(target);
            buff.Spend(duration * target.Resist(typeof(T)));

            return buff;
        }

        // 버프가 있으면 연장, 없으면 추가 
        //postpones an already active buff, or creates & attaches a new buff and delays that.
        public static T Prolong<T>(Character target, float duration) where T : FlavourBuff
        {
            var buff = Affect<T>(target);
            buff.Postpone(duration * target.Resist(typeof(T)));

            return buff;
        }

        public static void Detach<T>(Character target) where T : Buff
        {
            foreach (Buff b in target.Buffs<T>())
            {
                b.Detach();
            }
        }
    }
}