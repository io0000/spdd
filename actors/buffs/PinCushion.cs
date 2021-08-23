using System.Collections.Generic;
using watabou.utils;
using spdd.items.weapon.missiles;

namespace spdd.actors.buffs
{
    public class PinCushion : Buff
    {
        private List<MissileWeapon> items = new List<MissileWeapon>();

        public void Stick(MissileWeapon projectile)
        {
            foreach (var item in items)
            {
                if (item.IsSimilar(projectile))
                {
                    item.Merge(projectile);
                    return;
                }
            }
            items.Add(projectile);
        }

        public override void Detach()
        {
            foreach (var item in items)
                Dungeon.level.Drop(item, target.pos).sprite.Drop();
            base.Detach();
        }

        private const string ITEMS = "items";

        public override void StoreInBundle(Bundle bundle)
        {
            bundle.Put(ITEMS, items);
            base.StoreInBundle(bundle);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            //items = new ArrayList<>((Collection<MissileWeapon>)((Collection <?>) ));
            items = new List<MissileWeapon>();
            foreach (var item in bundle.GetCollection(ITEMS))
            {
                items.Add((MissileWeapon)item);
            }

            base.RestoreFromBundle(bundle);
        }
    }
}