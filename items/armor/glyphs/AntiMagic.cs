using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.sprites;
using spdd.levels.traps;
using spdd.items.wands;

namespace spdd.items.armor.glyphs
{
    public class AntiMagic : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing TEAL = new ItemSprite.Glowing(new Color(0x88, 0xEE, 0xFF, 0xFF));

        public static HashSet<Type> RESISTS = new HashSet<Type>();

        static AntiMagic()
        {
            RESISTS.Add(typeof(Rat));
            RESISTS.Add(typeof(Charm));
            RESISTS.Add(typeof(Weakness));
            RESISTS.Add(typeof(Vulnerable));
            RESISTS.Add(typeof(Hex));
            RESISTS.Add(typeof(Degrade));

            RESISTS.Add(typeof(DisintegrationTrap));
            RESISTS.Add(typeof(GrimTrap));

            RESISTS.Add(typeof(WandOfBlastWave));
            RESISTS.Add(typeof(WandOfDisintegration));
            RESISTS.Add(typeof(WandOfFireblast));
            RESISTS.Add(typeof(WandOfFrost));
            RESISTS.Add(typeof(WandOfLightning));
            RESISTS.Add(typeof(WandOfLivingEarth));
            RESISTS.Add(typeof(WandOfMagicMissile));
            RESISTS.Add(typeof(WandOfPrismaticLight));
            RESISTS.Add(typeof(WandOfTransfusion));
            RESISTS.Add(typeof(WandOfWarding.Ward));

            RESISTS.Add(typeof(DM100.LightningBolt));
            RESISTS.Add(typeof(Shaman.EarthenBolt));
            RESISTS.Add(typeof(Warlock.DarkBolt));
            RESISTS.Add(typeof(Eye.DeathGaze));
            RESISTS.Add(typeof(Yog.BurningFist.DarkBolt));
            RESISTS.Add(typeof(YogFist.BrightFist.LightBeam));
            RESISTS.Add(typeof(YogFist.DarkFist.DarkBolt));
        }

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            //no proc effect, see Hero.damage
            return damage;
        }

        public static int DrRoll(int level)
        {
            return Rnd.NormalIntRange(level, 3 + (int)Math.Round(level * 1.5f, MidpointRounding.AwayFromZero));
        }

        public override ItemSprite.Glowing Glowing()
        {
            return TEAL;
        }
    }
}