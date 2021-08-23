using System;
using System.Collections.Generic;
using System.Linq;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.items.bags;
using spdd.messages;
using spdd.sprites;
using spdd.utils;

namespace spdd.items.quest
{
    public class CorpseDust : Item
    {
        public CorpseDust()
        {
            image = ItemSpriteSheet.DUST;

            cursed = true;
            cursedKnown = true;

            unique = true;
        }

        public override List<string> Actions(Hero hero)
        {
            return new List<string>(); //yup, no dropping this one
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        // [����]��ũ��Ȱ ���� ��ü������ ���������� �۵����� ����
        //public override bool DoPickUp(Hero hero)
        //{
        //    if (base.DoPickUp(hero))
        //    {
        //        GLog.Negative(Messages.Get(this, "chill"));
        //        Buff.Affect<DustGhostSpawner>(hero);
        //        return true;
        //    }
        //    return false;
        //}

        public override bool Collect(Bag container)
        {
            if (base.Collect(container))
            {
                GLog.Negative(Messages.Get(this, "chill"));
                Buff.Affect<DustGhostSpawner>(Dungeon.hero);
                return true;
            }

            return false;
        }

        protected override void OnDetach()
        {
            var spawner = Dungeon.hero.FindBuff<DustGhostSpawner>();
            if (spawner != null)
            {
                spawner.Dispel();
            }
        }

        [SPDStatic]
        public class DustGhostSpawner : Buff
        {
            int spawnPower;

            public override bool Act()
            {
                ++spawnPower;

                int wraiths = 1; //we include the wraith we're trying to spawn
                foreach (Mob mob in Dungeon.level.mobs)
                {
                    if (mob is Wraith)
                        ++wraiths;
                }

                int powerNeeded = Math.Min(25, wraiths * wraiths);

                if (powerNeeded <= spawnPower)
                {
                    spawnPower -= powerNeeded;
                    int pos;
                    int tries = 20;

                    do
                    {
                        pos = Rnd.Int(Dungeon.level.Length());
                        --tries;
                    }
                    while (tries > 0 &&
                        (!Dungeon.level.heroFOV[pos] ||
                        Dungeon.level.solid[pos] ||
                        Actor.FindChar(pos) != null));

                    if (tries > 0)
                    {
                        Wraith.SpawnAt(pos);
                        Sample.Instance.Play(Assets.Sounds.CURSED);
                    }
                }

                Spend(TICK);
                return true;
            }

            public void Dispel()
            {
                Detach();
                foreach (Mob mob in Dungeon.level.mobs.ToArray())
                {
                    if (mob is Wraith)
                        mob.Die(null);
                }
            }

            private const string SPAWNPOWER = "spawnpower";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(SPAWNPOWER, spawnPower);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                spawnPower = bundle.GetInt(SPAWNPOWER);
            }
        }
    }
}