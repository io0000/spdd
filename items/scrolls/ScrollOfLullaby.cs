using System.Linq;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.effects;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfLullaby : Scroll
    {
        public ScrollOfLullaby()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_LULLABY;
        }

        public override void DoRead()
        {
            curUser.sprite.CenterEmitter().Start(Speck.Factory(Speck.NOTE), 0.3f, 5);
            Sample.Instance.Play(Assets.Sounds.LULLABY);

            foreach (var mob in Dungeon.level.mobs.ToArray())
            {
                if (Dungeon.level.heroFOV[mob.pos])
                {
                    Buff.Affect<Drowsy>(mob);
                    mob.sprite.CenterEmitter().Start(Speck.Factory(Speck.NOTE), 0.3f, 5);
                }
            }

            Buff.Affect<Drowsy>(curUser);

            GLog.Information(Messages.Get(this, "sooth"));

            SetKnown();

            ReadAnimation();
        }

        public override int Value()
        {
            return IsKnown() ? 40 * quantity : base.Value();
        }
    }
}