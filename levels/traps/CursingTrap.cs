using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.hero;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.items.armor;
using spdd.items.weapon;
using spdd.items.weapon.melee;
using spdd.items.weapon.missiles;
using spdd.messages;
using spdd.utils;

namespace spdd.levels.traps
{
    public class CursingTrap : Trap
    {
        public CursingTrap()
        {
            color = VIOLET;
            shape = WAVES;
        }

        public override void Activate()
        {
            if (Dungeon.level.heroFOV[pos])
            {
                CellEmitter.Get(pos).Burst(ShadowParticle.Up, 5);
                Sample.Instance.Play(Assets.Sounds.CURSED);
            }

            Heap heap = Dungeon.level.heaps[pos];
            if (heap != null)
            {
                foreach (Item item in heap.items)
                {
                    if (item.IsUpgradable() && !(item is MissileWeapon))
                        Curse(item);
                }
            }

            if (Dungeon.hero.pos == pos && !Dungeon.hero.flying)
                Curse(Dungeon.hero);
        }

        public static void Curse(Hero hero)
        {
            //items the trap wants to curse because it will create a more negative effect
            List<Item> priorityCurse = new List<Item>();
            //items the trap can curse if nothing else is available.
            List<Item> canCurse = new List<Item>();

            KindOfWeapon weapon = hero.belongings.weapon;
            if (weapon is Weapon && !(weapon is MagesStaff))
            {
                if (((Weapon)weapon).enchantment == null)
                    priorityCurse.Add(weapon);
                else
                    canCurse.Add(weapon);
            }

            Armor armor = hero.belongings.armor;
            if (armor != null)
            {
                if (armor.glyph == null)
                    priorityCurse.Add(armor);
                else
                    canCurse.Add(armor);
            }

            Rnd.Shuffle(priorityCurse);
            Rnd.Shuffle(canCurse);

            if (priorityCurse.Count > 0)
            {
                var toRemove = priorityCurse[0];
                priorityCurse.RemoveAt(0);
                Curse(toRemove);
            }
            else if (canCurse.Count > 0)
            {
                var toRemove = canCurse[0];
                canCurse.RemoveAt(0);
                Curse(toRemove);
            }

            EquipableItem.EquipCursed(hero);
            GLog.Negative(Messages.Get(typeof(CursingTrap), "curse"));
        }

        private static void Curse(Item item)
        {
            item.cursed = item.cursedKnown = true;

            if (item is Weapon)
            {
                var w = (Weapon)item;
                if (w.enchantment == null)
                    w.Enchant(Weapon.Enchantment.RandomCurse());
            }
            else if (item is Armor)
            {
                var a = (Armor)item;
                if (a.glyph == null)
                    a.Inscribe(Armor.Glyph.RandomCurse());
            }
        }
    }
}