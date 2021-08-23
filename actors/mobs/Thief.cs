using System;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items;
using spdd.sprites;
using spdd.utils;
using spdd.effects;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Thief : Mob
    {
        public Item item;

        public Thief()
        {
            spriteClass = typeof(ThiefSprite);

            HP = HT = 20;
            defenseSkill = 12;

            EXP = 5;
            maxLvl = 11;

            loot = Rnd.OneOf(Generator.Category.RING, Generator.Category.ARTIFACT);
            lootChance = 0.03f; //initially, see rollToDropLoot

            WANDERING = new ThiefWandering(this);
            FLEEING = new ThiefFleeing(this);

            properties.Add(Property.UNDEAD);
        }

        private const string ITEM = "item";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(ITEM, item);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            item = (Item)bundle.Get(ITEM);
        }

        public override float Speed()
        {
            if (item != null)
                return (5 * base.Speed()) / 6;
            else
                return base.Speed();
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(1, 10);
        }

        protected override float AttackDelay()
        {
            return base.AttackDelay() * 0.5f;
        }

        public override void RollToDropLoot()
        {
            if (item != null)
            {
                Dungeon.level.Drop(item, pos).sprite.Drop();
                //updates position
                if (item is Honeypot.ShatteredPot)
                    ((Honeypot.ShatteredPot)item).DropPot(this, pos);
                item = null;
            }

            //each drop makes future drops 1/3 as likely
            // so loot chance looks like: 1/33, 1/100, 1/300, 1/900, etc.
            lootChance *= (float)Math.Pow(1 / 3f, Dungeon.LimitedDrops.THEIF_MISC.count);
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.THEIF_MISC.count;
            return base.CreateLoot();
        }

        public override int AttackSkill(Character target)
        {
            return 12;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 3);
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);

            if (alignment == Alignment.ENEMY &&
                item == null &&
                enemy is Hero &&
                Steal((Hero)enemy))
            {
                state = FLEEING;
            }

            return damage;
        }

        public override int DefenseProc(Character enemy, int damage)
        {
            if (state == FLEEING)
                Dungeon.level.Drop(new Gold(), pos).sprite.Drop();

            return base.DefenseProc(enemy, damage);
        }

        public virtual bool Steal(Hero hero)
        {
            var item = hero.belongings.RandomUnequipped();
            if (item != null && !item.unique && item.GetLevel() < 1)
            {
                GLog.Warning(Messages.Get(typeof(Thief), "stole", item.Name()));
                if (!item.stackable)
                {
                    Dungeon.quickslot.ConvertToPlaceholder(item);
                }
                Item.UpdateQuickslot();

                if (item is Honeypot)
                {
                    this.item = ((Honeypot)item).Shatter(this, this.pos);
                    item.Detach(hero.belongings.backpack);
                }
                else
                {
                    this.item = item.Detach(hero.belongings.backpack);
                    if (item is Honeypot.ShatteredPot)
                        ((Honeypot.ShatteredPot)item).PickupPot(this);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override string Description()
        {
            var desc = base.Description();

            if (item != null)
                desc += Messages.Get(this, "carries", item.Name());

            return desc;
        }

        private class ThiefWandering : Wandering
        {
            public ThiefWandering(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                Thief thief = (Thief)mob;
                base.Act(enemyInFOV, justAlerted);

                //if an enemy is just noticed and the thief posses an item, run, don't fight.
                if (thief.state == thief.HUNTING && thief.item != null)
                {
                    thief.state = thief.FLEEING;
                }

                return true;
            }
        }

        private class ThiefFleeing : Fleeing
        {
            public ThiefFleeing(Mob mob)
                : base(mob)
            { }

            public override void NowhereToRun()
            {
                Thief thief = (Thief)mob;
                
                bool callBase = false;
                thief.ThieftNowhereToRun(ref callBase);

                if (callBase)
                    base.NowhereToRun();
            }
        }

        public void ThieftNowhereToRun(ref bool callBase)
        {
            if (FindBuff<Terror>() == null && FindBuff<Corruption>() == null)
            {
                if (enemySeen)
                {
                    sprite.ShowStatus(CharSprite.NEGATIVE, Messages.Get(this, "rage"));
                    state = HUNTING;
                }
                else if (item != null
                        && !Dungeon.level.heroFOV[pos]
                        && Dungeon.level.Distance(Dungeon.hero.pos, pos) >= 6)
                {
                    int count = 32;
                    int newPos;
                    do
                    {
                        newPos = Dungeon.level.RandomRespawnCell(this);
                        if (count-- <= 0)
                        {
                            break;
                        }
                    } 
                    while (newPos == -1 || Dungeon.level.heroFOV[newPos] || Dungeon.level.Distance(newPos, pos) < (count / 3));

                    if (newPos != -1)
                    {
                        if (Dungeon.level.heroFOV[pos])
                            CellEmitter.Get(pos).Burst(Speck.Factory(Speck.WOOL), 6);

                        pos = newPos;
                        sprite.Place(pos);
                        sprite.visible = Dungeon.level.heroFOV[pos];

                        if (Dungeon.level.heroFOV[pos])
                            CellEmitter.Get(pos).Burst(Speck.Factory(Speck.WOOL), 6);
                    }

                    if (item != null)
                        GLog.Negative(Messages.Get(typeof(Thief), "escapes", item.Name()));

                    item = null;
                    state = WANDERING;
                }
                else
                {
                    callBase = true;   // super.nowhereToRun();
                }
            }
        }
    }
}