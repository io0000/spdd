using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.sprites;
using spdd.utils;
using spdd.messages;

namespace spdd.items.potions
{
    public class PotionOfPurity : Potion
    {
        private const int DISTANCE = 3;
        private static List<Type> affectedBlobs;

        public PotionOfPurity()
        {
            icon = ItemSpriteSheet.Icons.POTION_PURITY;

            affectedBlobs = new List<Type>(new BlobImmunity().Immunities());
        }

        public override void Shatter(int cell)
        {
            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.losBlocking, null), DISTANCE);

            List<Blob> blobs = new List<Blob>();
            foreach (var c in affectedBlobs)
            {
                Blob b = Dungeon.level.GetBlob(c);
                if (b != null && b.volume > 0)
                    blobs.Add(b);
            }

            for (int i = 0; i < Dungeon.level.Length(); ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    foreach (var blob in blobs)
                    {
                        int value = blob.cur[i];
                        if (value > 0)
                        {
                            blob.Clear(i);
                            blob.cur[i] = 0;
                            blob.volume -= value;
                        }
                    }

                    if (Dungeon.level.heroFOV[i])
                        CellEmitter.Get(i).Burst(Speck.Factory(Speck.DISCOVER), 2);
                }
            }

            if (Dungeon.level.heroFOV[cell])
            {
                Splash(cell);
                Sample.Instance.Play(Assets.Sounds.SHATTER);

                SetKnown();
                GLog.Information(Messages.Get(this, "freshness"));
            }
        }

        public override void Apply(Hero hero)
        {
            GLog.Warning(Messages.Get(this, "protected"));
            Buff.Prolong<BlobImmunity>(hero, BlobImmunity.DURATION);
            SetKnown();
        }

        public override int Value()
        {
            return IsKnown() ? 40 * quantity : base.Value();
        }
    }
}