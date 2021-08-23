using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.actors.buffs;
using spdd.items.weapon.missiles;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.items.armor
{
    public class HuntressArmor : ClassArmor
    {
        public HuntressArmor()
        {
            image = ItemSpriteSheet.ARMOR_HUNTRESS;
        }

        public Dictionary<ICallback, Mob> targets = new Dictionary<ICallback, Mob>();

        public override void DoSpecial()
        {
            charge -= 35;
            UpdateQuickslot();

            var proto = new Shuriken();

            foreach (var mob in Dungeon.level.mobs)
            {
                if (Dungeon.level.Distance(curUser.pos, mob.pos) <= 12 &&
                    Dungeon.level.heroFOV[mob.pos] &&
                    mob.alignment != Character.Alignment.ALLY)
                {
                    var callback = new ActionCallback();
                    callback.action = () =>
                    {
                        var user = Item.curUser;

                        user.Attack(targets[callback]);
                        targets.Remove(callback);
                        if (targets.Count == 0)
                        {
                            Invisibility.Dispel();
                            user.SpendAndNext(user.AttackDelay());
                        }
                    };
                    curUser.sprite.parent.Recycle<MissileSprite>().
                        Reset(curUser.sprite, mob.pos, proto, callback);

                    targets.Add(callback, mob);
                }
            }

            if (targets.Count == 0)
            {
                GLog.Warning(Messages.Get(this, "no_enemies"));
                return;
            }

            curUser.sprite.Zap(curUser.pos);
            curUser.Busy();
        }
    }
}