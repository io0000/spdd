using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.noosa.tweeners;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.effects;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items
{
    /*
    벌
    꿀단지(Honeypot)를 던지거나 깨뜨리면 나옴
    깨진 꿀단지로부터 5×5 범위 내에 들어오는 개체들을 피아 구분없이 공격
    깨진 꿀단지 주변을 지키려 하므로, 깨진 꿀단지의 위치가 이동되면 벌도 깨진 꿀단지 쪽으로 이동
    */
    public class Honeypot : Item
    {
        public const string AC_SHATTER = "SHATTER";

        public Honeypot()
        {
            image = ItemSpriteSheet.HONEYPOT;

            defaultAction = AC_THROW;
            usesTargeting = true;

            stackable = true;
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(AC_SHATTER);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_SHATTER))
            {
                hero.sprite.Zap(hero.pos);

                Detach(hero.belongings.backpack);

                Item item = Shatter(hero, hero.pos);
                if (!item.Collect())
                {
                    Dungeon.level.Drop(item, hero.pos);
                    if (item is ShatteredPot)
                    {
                        ((ShatteredPot)item).DropPot(hero, hero.pos);
                    }
                }

                hero.Next();
            }
        }

        public override void OnThrow(int cell)
        {
            if (Dungeon.level.pit[cell])
            {
                base.OnThrow(cell);
            }
            else
            {
                Dungeon.level.Drop(Shatter(null, cell), cell);
            }
        }

        public Item Shatter(Character owner, int pos)
        {
            if (Dungeon.level.heroFOV[pos])
            {
                Sample.Instance.Play(Assets.Sounds.SHATTER);
                Splash.At(pos, new Color(0xff, 0xd5, 0x00, 0xff), 5);
            }

            int newPos = pos;
            if (Actor.FindChar(pos) != null)
            {
                List<int> candidates = new List<int>();
                var passable = Dungeon.level.passable;

                foreach (int n in PathFinder.NEIGHBORS4)
                {
                    int c = pos + n;
                    if (passable[c] && Actor.FindChar(c) == null)
                    {
                        candidates.Add(c);
                    }
                }

                newPos = candidates.Count > 0 ? Rnd.Element(candidates) : -1;
            }

            if (newPos != -1)
            {
                Bee bee = new Bee();
                bee.Spawn(Dungeon.depth);
                bee.SetPotInfo(pos, owner);
                bee.HP = bee.HT;
                bee.pos = newPos;

                GameScene.Add(bee);
                Actor.AddDelayed(new Pushing(bee, pos, newPos), -1f);

                bee.sprite.Alpha(0);
                bee.sprite.parent.Add(new AlphaTweener(bee.sprite, 1, 0.15f));

                Sample.Instance.Play(Assets.Sounds.BEE);
                return new ShatteredPot();
            }
            else
            {
                return this;
            }
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override int Value()
        {
            return 30 * quantity;
        }

        [SPDStatic]
        public class ShatteredPot : Item
        {
            public ShatteredPot()
            {
                image = ItemSpriteSheet.SHATTPOT;
                stackable = true;
            }

            public override bool DoPickUp(Hero hero)
            {
                if (base.DoPickUp(hero))
                {
                    PickupPot(hero);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override void DoDrop(Hero hero)
            {
                base.DoDrop(hero);
                DropPot(hero, hero.pos);
            }

            public override void OnThrow(int cell)
            {
                base.OnThrow(cell);
                DropPot(curUser, cell);
            }

            public void PickupPot(Character holder)
            {
                foreach (Bee bee in FindBees(holder.pos))
                {
                    UpdateBee(bee, -1, holder);
                }
            }

            public void DropPot(Character holder, int dropPos)
            {
                foreach (Bee bee in FindBees(holder))
                {
                    UpdateBee(bee, dropPos, null);
                }
            }

            public void DestroyPot(int potPos)
            {
                foreach (Bee bee in FindBees(potPos))
                {
                    UpdateBee(bee, -1, null);
                }
            }

            private void UpdateBee(Bee bee, int cell, Character holder)
            {
                if (bee != null && bee.alignment == Character.Alignment.ENEMY)
                    bee.SetPotInfo(cell, holder);
            }

            //returns up to quantity bees which match the current pot Pos
            private List<Bee> FindBees(int potPos)
            {
                var bees = new List<Bee>();
                foreach (var c in Actor.Chars())
                {
                    if (c is Bee && ((Bee)c).PotPos() == potPos)
                    {
                        bees.Add((Bee)c);
                        if (bees.Count >= quantity)
                            break;
                    }
                }

                return bees;
            }

            //returns up to quantity bees which match the current pot holder
            private List<Bee> FindBees(Character potHolder)
            {
                var bees = new List<Bee>();
                foreach (var c in Actor.Chars())
                {
                    if (c is Bee && ((Bee)c).PotHolderID() == potHolder.Id())
                    {
                        bees.Add((Bee)c);
                        if (bees.Count >= quantity)
                            break;
                    }
                }

                return bees;
            }

            public override bool IsUpgradable()
            {
                return false;
            }

            public override bool IsIdentified()
            {
                return true;
            }

            public override int Value()
            {
                return 5 * quantity;
            }
        }
    }
}