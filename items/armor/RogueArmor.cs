using System.Linq;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items.scrolls;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.items.armor
{
    public class RogueArmor : ClassArmor
    {
        public RogueArmor()
        {
            image = ItemSpriteSheet.ARMOR_ROGUE;

            teleporter = new RogueArmorListener(this);
        }

        public override void DoSpecial()
        {
            GameScene.SelectCell(teleporter);
        }

        class RogueArmorListener : CellSelector.IListener
        {
            RogueArmor armor;

            public RogueArmorListener(RogueArmor armor)
            {
                this.armor = armor;
            }

            public void OnSelect(int? target)
            {
                armor.OnSelect(target);
            }

            public string Prompt()
            {
                return Messages.Get(typeof(RogueArmor), "prompt");
            }
        }

        protected CellSelector.IListener teleporter;

        public void OnSelect(int? t)
        {
            if (t == null)
                return;
            int target = t.Value;

            PathFinder.BuildDistanceMap(curUser.pos, BArray.Not(Dungeon.level.solid, null), 8);

            if (PathFinder.distance[target] == int.MaxValue ||
                !Dungeon.level.heroFOV[target] ||
                Actor.FindChar(target) != null)
            {
                GLog.Warning(Messages.Get(typeof(RogueArmor), "fov"));
                return;
            }

            charge -= 35;
            UpdateQuickslot();

            foreach (var mob in Dungeon.level.mobs.ToArray())
            {
                if (Dungeon.level.Adjacent(mob.pos, curUser.pos) &&
                    mob.alignment != Character.Alignment.ALLY)
                {
                    Buff.Prolong<Blindness>(mob, Blindness.DURATION / 2f);
                    if (mob.state == mob.HUNTING)
                        mob.state = mob.WANDERING;
                    mob.sprite.Emitter().Burst(Speck.Factory(Speck.LIGHT), 4);
                }
            }

            Buff.Affect<Invisibility>(curUser, Invisibility.DURATION / 2f);

            CellEmitter.Get(curUser.pos).Burst(Speck.Factory(Speck.WOOL), 10);
            ScrollOfTeleportation.Appear(curUser, target);
            Sample.Instance.Play(Assets.Sounds.PUFF);
            Dungeon.level.OccupyCell(curUser);
            Dungeon.Observe();
            GameScene.UpdateFog();

            curUser.SpendAndNext(Actor.TICK);
        }
    }
}