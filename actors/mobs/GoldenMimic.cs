using System;
using watabou.utils;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.items;
using spdd.items.wands;
using spdd.items.weapon;
using spdd.items.weapon.missiles;
using spdd.items.armor;
using spdd.utils;
using spdd.effects;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class GoldenMimic : Mimic
    {
        public GoldenMimic()
        {
            spriteClass = typeof(MimicSprite.Golden);
        }

        public override string Name()
        {
            if (alignment == Alignment.NEUTRAL)
            {
                return Messages.Get(typeof(Heap), "locked_chest");
            } 
            else 
            {
                return base.Name();
            }
        }

        public override string Description()
        {
            if (alignment == Alignment.NEUTRAL)
            {
                return Messages.Get(typeof(Heap), "locked_chest_desc") + "\n\n" + Messages.Get(this, "hidden_hint");
            } 
            else 
            {
                return base.Description();
            }
        }

        public override void StopHiding()
        {
            state = HUNTING;
            if (Actor.Chars().Contains(this) && Dungeon.level.heroFOV[pos])
            {
                enemy = Dungeon.hero;
                target = Dungeon.hero.pos;
                enemySeen = true;
                GLog.Warning(Messages.Get(this, "reveal"));
                CellEmitter.Get(pos).Burst(Speck.Factory(Speck.STAR), 10);
                Sample.Instance.Play(Assets.Sounds.MIMIC, 1, 0.85f);
            }
        }

        public override void SetLevel(int level)
        {
            base.SetLevel((int)Math.Round(level * 1.33f, MidpointRounding.AwayFromZero));
        }

        protected override void GeneratePrize()
        {
            base.GeneratePrize();
            //all existing prize items are guaranteed uncursed, and have a 50% chance to be +1 if they were +0
            foreach (Item i in items)
            {
                if (i is EquipableItem || i is Wand)
                {
                    i.cursed = false;
                    i.cursedKnown = true;
                    
                    if (i is Weapon && ((Weapon)i).HasCurseEnchant())
                        ((Weapon)i).Enchant(null);
                    
                    if (i is Armor && ((Armor)i).HasCurseGlyph())
                        ((Armor)i).Inscribe(null);
                
                    if (!(i is MissileWeapon) && i.GetLevel() == 0 && Rnd.Int(2) == 0)
                    {
                        i.Upgrade();
                    }
                }
            }
        }
    }
}