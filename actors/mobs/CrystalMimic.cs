using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.sprites;
using spdd.items;
using spdd.utils;
using spdd.effects;
using spdd.actors.buffs;
using spdd.items.artifacts;
using spdd.items.rings;
using spdd.items.wands;
using spdd.items.scrolls;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class CrystalMimic : Mimic
    {
        public CrystalMimic()
        {
            spriteClass = typeof(MimicSprite.Crystal);

            FLEEING = new CrystalFleeing(this);
        }

        public override string Name()
        {
            if (alignment == Alignment.NEUTRAL)
            {
                return Messages.Get(typeof(Heap), "crystal_chest");
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
                string desc = null;
                foreach (Item i in items)
                {
                    if (i is Artifact)
                    {
                        desc = Messages.Get(typeof(Heap), "crystal_chest_desc", Messages.Get(typeof(Heap), "artifact"));
                        break;
                    }
                    else if (i is Ring)
                    {
                        desc = Messages.Get(typeof(Heap), "crystal_chest_desc", Messages.Get(typeof(Heap), "ring"));
                        break;
                    }
                    else if (i is Wand)
                    {
                        desc = Messages.Get(typeof(Heap), "crystal_chest_desc", Messages.Get(typeof(Heap), "wand"));
                        break;
                    }
                }

                if (desc == null)
                    desc = Messages.Get(typeof(Heap), "locked_chest_desc");

                return desc + "\n\n" + Messages.Get(this, "hidden_hint");
            }
            else
            {
                return base.Description();
            }
        }

        public override int DamageRoll()
        {
            if (alignment == Alignment.NEUTRAL)
            {
                alignment = Alignment.ENEMY;
                int dmg = base.DamageRoll();
                alignment = Alignment.NEUTRAL;
                return dmg;
            }
            else
            {
                return base.DamageRoll();
            }
        }

        public override void StopHiding()
        {
            state = FLEEING;
            //haste for 2 turns if attacking
            if (alignment == Alignment.NEUTRAL)
            {
                Buff.Affect<Haste>(this, 2f);
            }
            else
            {
                Buff.Affect<Haste>(this, 1f);
            }

            if (Actor.Chars().Contains(this) && Dungeon.level.heroFOV[pos])
            {
                enemy = Dungeon.hero;
                target = Dungeon.hero.pos;
                enemySeen = true;
                GLog.Warning(Messages.Get(this, "reveal"));
                CellEmitter.Get(pos).Burst(Speck.Factory(Speck.STAR), 10);
                Sample.Instance.Play(Assets.Sounds.MIMIC, 1, 1.25f);
            }
        }

        public override int AttackProc(Character enemy, int damage)
        {
            if (alignment == Alignment.NEUTRAL && enemy == Dungeon.hero)
            {
                Steal(Dungeon.hero);
            }
            else
            {
                List<int> candidates = new List<int>();
                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    if (Dungeon.level.passable[pos + i] && Actor.FindChar(pos + i) == null)
                        candidates.Add(pos + i);
                }

                if (candidates.Count > 0)
                    ScrollOfTeleportation.Appear(enemy, Rnd.Element(candidates));

                if (alignment == Alignment.ENEMY)
                    state = FLEEING;
            }

            return base.AttackProc(enemy, damage);
        }

        protected void Steal(Hero hero)
        {
            int tries = 10;
            Item item;

            do
            {
                item = hero.belongings.RandomUnequipped();
            } while (tries-- > 0 && (item == null || item.unique || item.GetLevel() > 0));

            if (item != null && !item.unique && item.GetLevel() < 1)
            {
                GLog.Warning(Messages.Get(this, "ate", item.Name()));
                if (!item.stackable)
                    Dungeon.quickslot.ConvertToPlaceholder(item);

                Item.UpdateQuickslot();

                if (item is Honeypot)
                {
                    items.Add(((Honeypot)item).Shatter(this, this.pos));
                    item.Detach(hero.belongings.backpack);
                }
                else
                {
                    items.Add(item.Detach(hero.belongings.backpack));
                    if (item is Honeypot.ShatteredPot)
                        ((Honeypot.ShatteredPot)item).PickupPot(this);
                }
            }
        }

        protected override void GeneratePrize()
        {
            //Crystal mimic already contains a prize item. Just guarantee it isn't cursed.
            foreach (Item i in items)
            {
                i.cursed = false;
                i.cursedKnown = true;
            }
        }

        private class CrystalFleeing : Mob.Fleeing
        {
            public CrystalFleeing(Mob mob)
                : base(mob)
            {
            }

            public override void NowhereToRun()
            {
                var mimic = (CrystalMimic)mob;
                if (mimic.NowhereToRun() == false)
                    base.NowhereToRun();
            }
        }

        public bool NowhereToRun()
        {
            if (FindBuff<Terror>() == null && FindBuff<Corruption>() == null)
            {
                if (enemySeen)
                {
                    sprite.ShowStatus(CharSprite.NEGATIVE, Messages.Get(typeof(Mob), "rage"));
                    state = HUNTING;
                }
                else
                {
                    GLog.Negative(Messages.Get(typeof(CrystalMimic), "escaped"));
                    if (Dungeon.level.heroFOV[pos])
                        CellEmitter.Get(pos).Burst(Speck.Factory(Speck.WOOL), 6);
                    Destroy();
                    sprite.KillAndErase();
                }

                return true;
            }
            else
            {
                //super.nowhereToRun();
                return false;
            }
        }
    }
}