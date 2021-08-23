using watabou.utils;
using watabou.noosa;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items.keys;
using spdd.items.quest;
using spdd.items.artifacts;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Goo : Mob
    {
        public Goo()
        {
            HP = HT = 100;
            EXP = 10;
            defenseSkill = 8;
            spriteClass = typeof(GooSprite);

            properties.Add(Property.BOSS);
            properties.Add(Property.DEMONIC);
            properties.Add(Property.ACIDIC);
        }

        private int pumpedUp;

        public override int DamageRoll()
        {
            int min = 1;
            int max = (HP * 2 <= HT) ? 12 : 8;
            if (pumpedUp > 0)
            {
                pumpedUp = 0;
                Sample.Instance.Play(Assets.Sounds.BURNING);
                return Rnd.NormalIntRange(min * 3, max * 3);
            }
            else
            {
                return Rnd.NormalIntRange(min, max);
            }
        }

        public override int AttackSkill(Character target)
        {
            int attack = 10;
            if (HP * 2 <= HT)
                attack = 15;
            if (pumpedUp > 0)
                attack *= 2;
            return attack;
        }

        public override int DefenseSkill(Character enemy)
        {
            return (int)(base.DefenseSkill(enemy) * ((HP * 2 <= HT) ? 1.5 : 1));
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 2);
        }

        public override bool Act()
        {
            if (Dungeon.level.water[pos] && HP < HT)
            {
                sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);
                if (HP * 2 == HT)
                {
                    BossHealthBar.Bleed(false);
                    ((GooSprite)sprite).Spray(false);
                }
                ++HP;
            }

            if (state != SLEEPING)
                Dungeon.level.Seal();

            return base.Act();
        }

        protected override bool CanAttack(Character enemy)
        {
            return (pumpedUp > 0) ? Distance(enemy) <= 2 : base.CanAttack(enemy);
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);

            if (Rnd.Int(3) == 0)
            {
                Buff.Affect<Ooze>(enemy).Set(Ooze.DURATION);
                enemy.sprite.Burst(new Color(0x00, 0x00, 0x00, 0xFF), 5);
            }

            if (pumpedUp > 0)
                Camera.main.Shake(3, 0.2f);

            return damage;
        }

        public override void UpdateSpriteState()
        {
            base.UpdateSpriteState();

            if (pumpedUp > 0)
            {
                ((GooSprite)sprite).PumpUp(pumpedUp);
            }
        }

        protected override bool DoAttack(Character enemy)
        {
            if (pumpedUp == 1)
            {
                ((GooSprite)sprite).PumpUp(2);
                ++pumpedUp;
                Sample.Instance.Play(Assets.Sounds.CHARGEUP);

                Spend(AttackDelay());

                return true;
            }
            else if (pumpedUp >= 2 || Rnd.Int((HP * 2 <= HT) ? 2 : 5) > 0)
            {
                bool visible = Dungeon.level.heroFOV[pos];

                if (visible)
                {
                    if (pumpedUp >= 2)
                    {
                        ((GooSprite)sprite).PumpAttack();
                    }
                    else
                    {
                        sprite.Attack(enemy.pos);
                    }
                }
                else
                {
                    Attack(enemy);
                }

                Spend(AttackDelay());

                return !visible;

            }
            else
            {
                ++pumpedUp;

                ((GooSprite)sprite).PumpUp(1);

                if (Dungeon.level.heroFOV[pos])
                {
                    sprite.ShowStatus(CharSprite.NEGATIVE, Messages.Get(this, "!!!"));
                    GLog.Negative(Messages.Get(this, "pumpup"));
                    Sample.Instance.Play(Assets.Sounds.CHARGEUP, 1f, 0.8f);
                }

                Spend(AttackDelay());

                return true;
            }
        }

        public override bool Attack(Character enemy)
        {
            bool result = base.Attack(enemy);
            pumpedUp = 0;
            return result;
        }

        public override bool GetCloser(int target)
        {
            pumpedUp = 0;
            sprite.Idle();
            return base.GetCloser(target);
        }

        public override void Damage(int dmg, object src)
        {
            if (!BossHealthBar.IsAssigned())
                BossHealthBar.AssignBoss(this);

            bool bleeding = (HP * 2 <= HT);
            base.Damage(dmg, src);
            if ((HP * 2 <= HT) && !bleeding)
            {
                BossHealthBar.Bleed(true);
                sprite.ShowStatus(CharSprite.NEGATIVE, Messages.Get(this, "enraged"));
                ((GooSprite)sprite).Spray(true);
                Yell(Messages.Get(this, "gluuurp"));
            }
            var lockedFloor = Dungeon.hero.FindBuff<LockedFloor>();
            if (lockedFloor != null)
                lockedFloor.AddTime(dmg * 2);
        }

        public override void Die(object cause)
        {
            base.Die(cause);

            Dungeon.level.Unseal();

            GameScene.BossSlain();
            Dungeon.level.Drop(new SkeletonKey(Dungeon.depth), pos).sprite.Drop();

            //60% chance of 2 blobs, 30% chance of 3, 10% chance for 4. Average of 2.5
            int blobs = Rnd.Chances(new float[] { 0, 0, 6, 3, 1 });
            for (int i = 0; i < blobs; ++i)
            {
                int ofs;
                do
                {
                    ofs = PathFinder.NEIGHBORS8[Rnd.Int(8)];
                }
                while (!Dungeon.level.passable[pos + ofs]);
                Dungeon.level.Drop(new GooBlob(), pos + ofs).sprite.Drop(pos);
            }

            BadgesExtensions.ValidateBossSlain();

            Yell(Messages.Get(this, "defeated"));
        }

        public override void Notice()
        {
            base.Notice();
            if (!BossHealthBar.IsAssigned())
            {
                BossHealthBar.AssignBoss(this);
                Yell(Messages.Get(this, "notice"));
                foreach (var ch in Actor.Chars())
                {
                    if (ch is DriedRose.GhostHero)
                        ((DriedRose.GhostHero)ch).SayBoss();
                }
            }
        }

        private const string PUMPEDUP = "pumpedup";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(PUMPEDUP, pumpedUp);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            pumpedUp = bundle.GetInt(PUMPEDUP);
            if (state != SLEEPING)
                BossHealthBar.AssignBoss(this);
            if ((HP * 2 <= HT))
                BossHealthBar.Bleed(true);
        }
    }
}
