using watabou.utils;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.scenes;
using spdd.utils;
using spdd.items.quest;
using spdd.actors.blobs;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class DM201 : DM200
    {
        public DM201()
        {
            spriteClass = typeof(DM201Sprite);

            HP = HT = 120;

            properties.Add(Property.IMMOVABLE);

            HUNTING = new Mob.Hunting(this);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(15, 25);
        }

        private bool threatened;

        public override bool Act()
        {
            //in case DM-201 hasn't been able to act yet
            if (fieldOfView == null || fieldOfView.Length != Dungeon.level.Length())
            {
                fieldOfView = new bool[Dungeon.level.Length()];
                Dungeon.level.UpdateFieldOfView(this, fieldOfView);
            }

            GameScene.Add(Blob.Seed(pos, 0, typeof(CorrosiveGas)));

            if (state == HUNTING &&
                enemy != null &&
                enemySeen &&
                threatened &&
                !Dungeon.level.Adjacent(pos, enemy.pos) &&
                fieldOfView[enemy.pos])
            {
                enemySeen = enemy.IsAlive() &&
                    fieldOfView[enemy.pos] &&
                    enemy.invisible <= 0;

                if (sprite != null &&
                    (sprite.visible || enemy.sprite.visible))
                {
                    sprite.Zap(enemy.pos);
                    return false;
                }
                else
                {
                    Zap();
                    return true;
                }
            }
            return base.Act();
        }

        public override void Damage(int dmg, object src)
        {
            if ((src is Character && !Dungeon.level.Adjacent(pos, ((Character)src).pos)) || 
                enemy == null || 
                !Dungeon.level.Adjacent(pos, enemy.pos))
            {
                threatened = true;
            }
            base.Damage(dmg, src);
        }

        public override void OnZapComplete()
        {
            Zap();
            Next();
        }

        private void Zap()
        {
            threatened = false;
            Spend(TICK);

            GLog.Warning(Messages.Get(this, "vent"));
            var gas = (CorrosiveGas)Blob.Seed(enemy.pos, 15, typeof(CorrosiveGas));
            GameScene.Add(gas.SetStrength(8));

            foreach (int i in PathFinder.NEIGHBORS8)
            {
                if (!Dungeon.level.solid[enemy.pos + i])
                {
                    gas = (CorrosiveGas)Blob.Seed(enemy.pos + i, 5, typeof(CorrosiveGas));
                    gas.SetStrength(8);
                    GameScene.Add(gas);
                }
            }

            Sample.Instance.Play(Assets.Sounds.GAS);
        }

        public override bool GetCloser(int target)
        {
            return true;
        }

        public override bool GetFurther(int target)
        {
            return true;
        }

        public override void RollToDropLoot()
        {
            if (Dungeon.hero.lvl > maxLvl + 2)
                return;

            base.RollToDropLoot();

            int ofs;
            do
            {
                ofs = PathFinder.NEIGHBORS8[Rnd.Int(8)];
            }
            while (Dungeon.level.solid[pos + ofs]);

            Dungeon.level.Drop(new MetalShard(), pos + ofs).sprite.Drop(pos);
        }
    }
}