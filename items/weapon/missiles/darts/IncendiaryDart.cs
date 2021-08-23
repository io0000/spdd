using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class IncendiaryDart : TippedDart
    {
        public IncendiaryDart()
        {
            image = ItemSpriteSheet.INCENDIARY_DART;
        }

        public override void OnThrow(int cell)
        {
            Character enemy = Actor.FindChar(cell);
            if ((enemy == null || enemy == curUser) && Dungeon.level.flamable[cell])
            {
                GameScene.Add(Blob.Seed(cell, 4, typeof(Fire)));
                Dungeon.level.Drop(new Dart(), cell).sprite.Drop();
            }
            else
            {
                base.OnThrow(cell);
            }
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            Buff.Affect<Burning>(defender).Reignite(defender);
            return base.Proc(attacker, defender, damage);
        }
    }
}