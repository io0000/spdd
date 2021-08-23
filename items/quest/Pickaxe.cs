using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.effects;
using spdd.items.weapon;
using spdd.levels;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.items.quest
{
    public class Pickaxe : Weapon
    {
        public const string AC_MINE = "MINE";

        public const float TIME_TO_MINE = 2;

        private static ItemSprite.Glowing BLOODY = new ItemSprite.Glowing(new Color(0x55, 0x00, 0x00, 0xFF));

        public Pickaxe()
        {
            image = ItemSpriteSheet.PICKAXE;

            levelKnown = true;

            unique = true;
            bones = false;

            defaultAction = AC_MINE;
        }

        public bool bloodStained;

        public override int Min(int lvl)
        {
            return 2;   //tier 2
        }

        public override int Max(int lvl)
        {
            return 15;  //tier 2
        }

        public override int STRReq(int lvl)
        {
            return 14;  //tier 3
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            actions.Add(AC_MINE);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action == AC_MINE)
            {
                if (Dungeon.depth < 11 || Dungeon.depth > 15)
                {
                    GLog.Warning(Messages.Get(this, "no_vein"));
                    return;
                }

                for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
                {
                    int pos = hero.pos + PathFinder.NEIGHBORS8[i];

                    if (Dungeon.level.map[pos] == Terrain.WALL_DECO)
                    {
                        hero.Spend(TIME_TO_MINE);
                        hero.Busy();

                        var callback = new ActionCallback();
                        callback.action = () =>
                        {
                            CellEmitter.Center(pos).Burst(Speck.Factory(Speck.STAR), 7);
                            Sample.Instance.Play(Assets.Sounds.EVOKE);

                            Level.Set(pos, Terrain.WALL);
                            GameScene.UpdateMap(pos);

                            DarkGold gold = new DarkGold();
                            if (gold.DoPickUp(Dungeon.hero))
                                GLog.Information(Messages.Get(Dungeon.hero, "you_now_have", gold.Name()));
                            else
                                Dungeon.level.Drop(gold, hero.pos).sprite.Drop();

                            hero.OnOperateComplete();
                        };

                        hero.sprite.Attack(pos, callback);
                        return;
                    }
                }

                GLog.Warning(Messages.Get(this, "no_vein"));
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

        public override int Proc(Character c, Character defender, int damage)
        {
            if (!bloodStained && defender is Bat)
            {
                Actor.Add(new PickaxeActor(this, defender));
            }

            return damage;
        }

        public class PickaxeActor : Actor
        {
            Pickaxe axe;
            Character defender;

            public PickaxeActor(Pickaxe axe, Character defender)
            {
                actPriority = VFX_PRIO;

                this.axe = axe;
                this.defender = defender;
            }

            public override bool Act()
            {
                if (!defender.IsAlive())
                {
                    axe.bloodStained = true;
                    UpdateQuickslot();
                }

                Actor.Remove(this);
                return true;
            }
        }

        private const string BLOODSTAINED = "bloodStained";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);

            bundle.Put(BLOODSTAINED, bloodStained);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            bloodStained = bundle.GetBoolean(BLOODSTAINED);
        }

        public override ItemSprite.Glowing Glowing()
        {
            return bloodStained ? BLOODY : null;
        }
    }
}