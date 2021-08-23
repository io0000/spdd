using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items;
using spdd.items.scrolls;
using spdd.mechanics;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Succubus : Mob
    {
        private int blinkCooldown = 0;

        public Succubus()
        {
            InitInstance();

            spriteClass = typeof(SuccubusSprite);

            HP = HT = 80;
            defenseSkill = 25;
            viewDistance = Light.DISTANCE;

            EXP = 12;
            maxLvl = 25;

            loot = Generator.Category.SCROLL;
            lootChance = 0.33f;

            properties.Add(Property.DEMONIC);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(22, 30);
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);

            if (enemy.FindBuff<Charm>() != null)
            {
                int shield = (HP - HT) + (5 + damage);
                if (shield > 0)
                {
                    HP = HT;
                    Buff.Affect<Barrier>(this).SetShield(shield);
                }
                else
                {
                    HP += 5 + damage;
                }
                sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 2);
                Sample.Instance.Play(Assets.Sounds.CHARMS);
            }
            else if (Rnd.Int(3) == 0)
            {
                Charm c = Buff.Affect<Charm>(enemy, Charm.DURATION / 2f);
                c.obj = Id();
                c.ignoreNextHit = true; //so that the -5 duration from succubus hit is ignored
                enemy.sprite.CenterEmitter().Start(Speck.Factory(Speck.HEART), 0.2f, 5);
                Sample.Instance.Play(Assets.Sounds.CHARMS);
            }

            return damage;
        }

        public override bool GetCloser(int target)
        {
            if (fieldOfView[target] && Dungeon.level.Distance(pos, target) > 2 && blinkCooldown <= 0)
            {
                Blink(target);
                Spend(-1 / Speed());
                return true;
            }
            else
            {
                --blinkCooldown;
                return base.GetCloser(target);
            }
        }

        private void Blink(int target)
        {
            Ballistic route = new Ballistic(pos, target, Ballistic.PROJECTILE);
            int cell = route.collisionPos;

            //can't occupy the same cell as another char, so move back one.
            if (Actor.FindChar(cell) != null && cell != this.pos)
                cell = route.path[route.dist - 1];

            if (Dungeon.level.avoid[cell])
            {
                List<int> candidates = new List<int>();
                foreach (int n in PathFinder.NEIGHBORS8)
                {
                    cell = route.collisionPos + n;
                    if (Dungeon.level.passable[cell] && Actor.FindChar(cell) == null)
                    {
                        candidates.Add(cell);
                    }
                }
                if (candidates.Count > 0)
                {
                    cell = Rnd.Element(candidates);
                }
                else
                {
                    blinkCooldown = Rnd.IntRange(4, 6);
                    return;
                }
            }

            ScrollOfTeleportation.Appear(this, cell);

            blinkCooldown = Rnd.IntRange(4, 6);
        }

        public override int AttackSkill(Character target)
        {
            return 40;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 10);
        }

        public override Item CreateLoot()
        {
            Type loot;
            do
            {
                loot = (Type)Rnd.OneOf(Generator.Category.SCROLL.GetClasses());
            } 
            while (loot.Equals(typeof(ScrollOfIdentify)) || loot.Equals(typeof(ScrollOfUpgrade)));

            return (Item)Reflection.NewInstance(loot);
        }

        private void InitInstance()
        {
            immunities.Add(typeof(Charm));
        }

        private const string BLINK_CD = "blink_cd";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(BLINK_CD, blinkCooldown);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            blinkCooldown = bundle.GetInt(BLINK_CD);
        }
    }
}