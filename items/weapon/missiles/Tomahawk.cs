using System;
using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class Tomahawk : MissileWeapon
    {
        public Tomahawk()
        {
            image = ItemSpriteSheet.TOMAHAWK;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 0.9f;

            tier = 4;
            baseUses = 5;
        }

        public override int Min(int lvl)
        {
            return (int)Math.Round(1.5f * tier, MidpointRounding.AwayFromZero) +   //6 base, down from 8
                    2 * lvl;                        //scaling unchanged		
        }

        public override int Max(int lvl)
        {
            return (int)Math.Round(3.75f * tier, MidpointRounding.AwayFromZero) +  //15 base, down from 20
                   tier * lvl;                    //scaling unchanged        
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            var value = (int)Math.Round(damage * 0.6f, MidpointRounding.AwayFromZero);
            Buff.Affect<Bleeding>(defender).Set(value);
            return base.Proc(attacker, defender, damage);
        }
    }
}