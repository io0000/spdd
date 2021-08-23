using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.mechanics;
using spdd.scenes;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.armor
{
    public class WarriorArmor : ClassArmor
    {
        public const int LEAP_TIME = 1;
        public const int SHOCK_TIME = 5;

        public WarriorArmor()
        {
            image = ItemSpriteSheet.ARMOR_WARRIOR;
            leaper = new WarriorArmorLeaper(this);
        }

        public override void DoSpecial()
        {
            GameScene.SelectCell(leaper);
        }

        public void OnSelect(int? t)
        {
            if (t == null)
                return;
            int target = t.Value;

            if (target == curUser.pos)
                return;

            var route = new Ballistic(curUser.pos, target, Ballistic.PROJECTILE);
            int cell = route.collisionPos;

            //can't occupy the same cell as another char, so move back one.
            if (Actor.FindChar(cell) != null && cell != curUser.pos)
                cell = route.path[route.dist - 1];

            charge -= 35;
            UpdateQuickslot();

            int dest = cell;
            curUser.Busy();

            var callback = new ActionCallback();
            callback.action = () =>
            {
                curUser.Move(dest);
                Dungeon.level.OccupyCell(curUser);
                Dungeon.Observe();
                GameScene.UpdateFog();

                for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
                {
                    var mob = Actor.FindChar(curUser.pos + PathFinder.NEIGHBORS8[i]);

                    if (mob != null &&
                        mob != curUser &&
                        mob.alignment != Character.Alignment.ALLY)
                    {
                        Buff.Prolong<Paralysis>(mob, SHOCK_TIME);
                    }
                }

                CellEmitter.Center(dest).Burst(Speck.Factory(Speck.DUST), 10);
                Camera.main.Shake(2, 0.5f);

                Invisibility.Dispel();
                curUser.SpendAndNext(LEAP_TIME);
            };

            ((HeroSprite)curUser.sprite).Jump(curUser.pos,
                cell,
                callback);
        }


        protected static CellSelector.IListener leaper;

        public class WarriorArmorLeaper : CellSelector.IListener
        {
            private WarriorArmor armor;

            public WarriorArmorLeaper(WarriorArmor armor)
            {
                this.armor = armor;
            }

            public void OnSelect(int? target)
            {
                armor.OnSelect(target);
            }

            public string Prompt()
            {
                return Messages.Get(typeof(WarriorArmor), "prompt");
            }
        }
    }
}