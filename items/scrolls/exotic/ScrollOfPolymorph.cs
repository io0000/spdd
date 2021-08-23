using System.Linq;
using watabou.utils;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.scenes;
using spdd.effects;
using spdd.actors;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.ui;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfPolymorph : ExoticScroll
    {
        public ScrollOfPolymorph()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_POLYMORPH;
        }

        public override void DoRead()
        {
            new Flare(5, 32).Color(new Color(0xFF, 0xFF, 0xFF, 0xFF), true).Show(curUser.sprite, 2f);
            Sample.Instance.Play(Assets.Sounds.READ);

            foreach (Mob mob in Dungeon.level.mobs.ToArray())
            {
                if (mob.alignment != Character.Alignment.ALLY && Dungeon.level.heroFOV[mob.pos])
                {
                    if (!mob.Properties().Contains(Character.Property.BOSS) && 
                        !mob.Properties().Contains(Character.Property.MINIBOSS))
                    {
                        Sheep sheep = new Sheep();
                        sheep.lifespan = 10;
                        sheep.pos = mob.pos;

                        //awards half exp for each sheep-ified mob
                        //50% chance to round up, 50% to round down
                        if (mob.EXP % 2 == 1) 
                            mob.EXP += Rnd.Int(2);
                        mob.EXP /= 2;

                        mob.Destroy();
                        mob.sprite.KillAndErase();
                        Dungeon.level.mobs.Remove(mob);
                        TargetHealthIndicator.instance.Target(null);
                        GameScene.Add(sheep);
                        CellEmitter.Get(sheep.pos).Burst(Speck.Factory(Speck.WOOL), 4);
                        Sample.Instance.Play(Assets.Sounds.PUFF);
                        Sample.Instance.Play(Assets.Sounds.SHEEP);
                    }
                }
            }
            SetKnown();

            ReadAnimation();
        }
    }
}