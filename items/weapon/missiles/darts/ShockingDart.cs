using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.effects;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class ShockingDart : TippedDart
    {
        public ShockingDart()
        {
            image = ItemSpriteSheet.SHOCKING_DART;
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            defender.Damage(Rnd.NormalIntRange(8, 12), this);

            CharSprite s = defender.sprite;
            if (s != null && s.parent != null)
            {
                List<Lightning.Arc> arcs = new List<Lightning.Arc>();
                arcs.Add(new Lightning.Arc(new PointF(s.x, s.y + s.height / 2), new PointF(s.x + s.width, s.y + s.height / 2)));
                arcs.Add(new Lightning.Arc(new PointF(s.x + s.width / 2, s.y), new PointF(s.x + s.width / 2, s.y + s.height)));
                s.parent.Add(new Lightning(arcs, null));
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);
            }

            return base.Proc(attacker, defender, damage);
        }
    }
}