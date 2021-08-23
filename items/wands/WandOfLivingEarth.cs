using System;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.effects;
using spdd.items.weapon.melee;
using spdd.mechanics;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.items.wands
{
    public class WandOfLivingEarth : DamageWand
    {
        public WandOfLivingEarth()
        {
            image = ItemSpriteSheet.WAND_LIVING_EARTH;
        }

        public override int Min(int lvl)
        {
            return 4;
        }

        public override int Max(int lvl)
        {
            return 6 + 2 * lvl;
        }

        protected override void OnZap(Ballistic bolt)
        {
            var ch = Actor.FindChar(bolt.collisionPos);
            int damage = DamageRoll();
            int armorToAdd = damage;

            EarthGuardian guardian = null;
            foreach (Mob m in Dungeon.level.mobs)
            {
                if (m is EarthGuardian)
                {
                    guardian = (EarthGuardian)m;
                    break;
                }
            }

            RockArmor buff = curUser.FindBuff<RockArmor>();
            if (ch == null)
            {
                armorToAdd = 0;
            }
            else
            {
                if (buff == null && guardian == null)
                {
                    buff = Buff.Affect<RockArmor>(curUser);
                }
                if (buff != null)
                {
                    buff.AddArmor(BuffedLvl(), armorToAdd);
                }
            }

            //shooting at the guardian
            if (guardian != null && guardian == ch)
            {
                guardian.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + BuffedLvl() / 2);
                guardian.SetInfo(curUser, BuffedLvl(), armorToAdd);
                ProcessSoulMark(guardian, ChargesPerCast());
                Sample.Instance.Play(Assets.Sounds.HIT_MAGIC, 1, 0.9f * Rnd.Float(0.87f, 1.15f));
            }
            //shooting the guardian at a location
            else if (guardian == null && buff != null && buff.armor >= buff.ArmorToGuardian())
            {
                //create a new guardian
                guardian = new EarthGuardian();
                guardian.SetInfo(curUser, BuffedLvl(), buff.armor);

                //if the collision pos is occupied (likely will be), then spawn the guardian in the
                //adjacent cell which is closes to the user of the wand.
                if (ch != null)
                {
                    ch.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Burst, 5 + BuffedLvl() / 2);

                    ProcessSoulMark(ch, ChargesPerCast());
                    ch.Damage(damage, this);

                    int closest = -1;
                    var passable = Dungeon.level.passable;

                    foreach (int n in PathFinder.NEIGHBORS9)
                    {
                        int c = bolt.collisionPos + n;
                        if (passable[c] && Actor.FindChar(c) == null && (closest == -1 || (Dungeon.level.TrueDistance(c, curUser.pos) < (Dungeon.level.TrueDistance(closest, curUser.pos)))))
                        {
                            closest = c;
                        }
                    }

                    if (closest == -1)
                    {
                        curUser.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + BuffedLvl() / 2);
                        return; //do not spawn guardian or detach buff
                    }
                    else
                    {
                        guardian.pos = closest;
                        GameScene.Add(guardian, 1);
                        Dungeon.level.OccupyCell(guardian);
                    }

                    if (ch.alignment == Character.Alignment.ENEMY || ch.FindBuff<Amok>() != null)
                    {
                        guardian.Aggro(ch);
                    }
                }
                else
                {
                    guardian.pos = bolt.collisionPos;
                    GameScene.Add(guardian, 1);
                    Dungeon.level.OccupyCell(guardian);
                }

                guardian.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + BuffedLvl() / 2);
                buff.Detach();
                Sample.Instance.Play(Assets.Sounds.HIT_MAGIC, 1, 0.9f * Rnd.Float(0.87f, 1.15f));
            }
            //shooting at a location/enemy with no guardian being shot
            else if (guardian == null && buff != null && buff.armor >= buff.ArmorToGuardian())
            {
                //create a new guardian
                guardian = new EarthGuardian();
                guardian.SetInfo(curUser, BuffedLvl(), buff.armor);

                //if the collision pos is occupied (likely will be), then spawn the guardian in the
                //adjacent cell which is closes to the user of the wand.
                if (ch != null)
                {
                    ch.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Burst, 5 + BuffedLvl() / 2);

                    ProcessSoulMark(ch, ChargesPerCast());
                    ch.Damage(damage, this);

                    int closest = -1;
                    var passable = Dungeon.level.passable;

                    var collisionPos = bolt.collisionPos;

                    foreach (int n in PathFinder.NEIGHBORS9)
                    {
                        int c = collisionPos + n;
                        if (passable[c] &&
                            Actor.FindChar(c) == null &&
                            (closest == -1 || (Dungeon.level.TrueDistance(c, curUser.pos) < (Dungeon.level.TrueDistance(closest, curUser.pos)))))
                        {
                            closest = c;
                        }
                    }

                    if (closest == -1)
                    {
                        curUser.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + BuffedLvl() / 2);
                        return; //do not spawn guardian or detach buff
                    }
                    else
                    {
                        guardian.pos = closest;
                        GameScene.Add(guardian, 1);
                        Dungeon.level.OccupyCell(guardian);
                    }

                    if (ch.alignment == Character.Alignment.ENEMY || ch.FindBuff<Amok>() != null)
                    {
                        guardian.Aggro(ch);
                    }
                }
                else
                {
                    guardian.pos = bolt.collisionPos;
                    GameScene.Add(guardian, 1);
                    Dungeon.level.OccupyCell(guardian);
                }

                guardian.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + BuffedLvl() / 2);
                buff.Detach();
                Sample.Instance.Play(Assets.Sounds.HIT_MAGIC, 1, 0.9f * Rnd.Float(0.87f, 1.15f));
            }
            //shooting at a location/enemy with no guardian being shot
            else
            {
                if (ch != null)
                {
                    ch.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Burst, 5 + BuffedLvl() / 2);

                    ProcessSoulMark(ch, ChargesPerCast());
                    ch.Damage(damage, this);
                    Sample.Instance.Play(Assets.Sounds.HIT_MAGIC, 1, 0.8f * Rnd.Float(0.87f, 1.15f));

                    if (guardian == null)
                    {
                        curUser.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + BuffedLvl() / 2);
                    }
                    else
                    {
                        guardian.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + BuffedLvl() / 2);
                        guardian.SetInfo(curUser, BuffedLvl(), armorToAdd);
                        if (ch.alignment == Character.Alignment.ENEMY || ch.FindBuff<Amok>() != null)
                        {
                            guardian.Aggro(ch);
                        }
                    }
                }
                else
                {
                    Dungeon.level.PressCell(bolt.collisionPos);
                }
            }
        }

        public override void Fx(Ballistic bolt, ICallback callback)
        {
            MagicMissile.BoltFromChar(curUser.sprite.parent,
                MagicMissile.EARTH,
                curUser.sprite,
                bolt.collisionPos,
                callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            EarthGuardian guardian = null;
            foreach (Mob m in Dungeon.level.mobs)
            {
                if (m is EarthGuardian)
                {
                    guardian = (EarthGuardian)m;
                    break;
                }
            }

            int armor = (int)Math.Round(damage * 0.25f, MidpointRounding.AwayFromZero);

            if (guardian != null)
            {
                guardian.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + BuffedLvl() / 2);
                guardian.SetInfo(Dungeon.hero, BuffedLvl(), armor);
            }
            else
            {
                attacker.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + BuffedLvl() / 2);
                Buff.Affect<RockArmor>(attacker).AddArmor(BuffedLvl(), armor);
            }
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            if (Rnd.Int(10) == 0)
            {
                var c1 = new Color(0xFF, 0xF5, 0x68, 0xFF);
                var c2 = new Color(0x80, 0x79, 0x1A, 0xFF);
                particle.SetColor(ColorMath.Random(c1, c2));
            }
            else
            {
                var c1 = new Color(0x80, 0x55, 0x00, 0xFF);
                var c2 = new Color(0x33, 0x25, 0x00, 0xFF);
                particle.SetColor(ColorMath.Random(c1, c2));
            }
            particle.am = 1f;
            particle.SetLifespan(2f);
            particle.SetSize(1f, 2f);
            particle.ShuffleXY(0.5f);
            float dst = Rnd.Float(11f);
            particle.x -= dst;
            particle.y += dst;
        }

        [SPDStatic]
        public class RockArmor : Buff
        {
            private int wandLevel;
            public int armor;

            public void AddArmor(int wandLevel, int toAdd)
            {
                this.wandLevel = Math.Max(this.wandLevel, wandLevel);
                armor += toAdd;
                armor = Math.Min(armor, 2 * ArmorToGuardian());
            }

            public int ArmorToGuardian()
            {
                return 8 + wandLevel * 4;
            }

            public int Absorb(int damage)
            {
                int block = damage - damage / 2;
                if (armor <= block)
                {
                    Detach();
                    return damage - armor;
                }
                else
                {
                    armor -= block;
                    return damage - block;
                }
            }

            public override int Icon()
            {
                return BuffIndicator.ARMOR;
            }

            public override float IconFadePercent()
            {
                return Math.Max(0, (ArmorToGuardian() - armor) / (float)ArmorToGuardian());
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", armor, ArmorToGuardian());
            }

            private const string WAND_LEVEL = "wand_level";
            private const string ARMOR = "armor";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(WAND_LEVEL, wandLevel);
                bundle.Put(ARMOR, armor);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                wandLevel = bundle.GetInt(WAND_LEVEL);
                armor = bundle.GetInt(ARMOR);
            }
        }

        [SPDStatic]
        public class EarthGuardian : NPC
        {
            public EarthGuardian()
            {
                InitInstance();

                spriteClass = typeof(EarthGuardianSprite);

                alignment = Alignment.ALLY;
                state = HUNTING;
                intelligentAlly = true;
                WANDERING = new EarthGuardianWandering(this);

                //before other mobs
                actPriority = MOB_PRIO + 1;

                HP = HT = 0;
            }

            private int wandLevel = -1;

            public void SetInfo(Hero hero, int wandLevel, int healthToAdd)
            {
                if (wandLevel > this.wandLevel)
                {
                    this.wandLevel = wandLevel;
                    HT = 16 + 8 * wandLevel;
                }
                HP = Math.Min(HT, HP + healthToAdd);
                //half of hero's evasion
                defenseSkill = (hero.lvl + 4) / 2;
            }

            public override int AttackSkill(Character target)
            {
                //same as the hero
                return 2 * defenseSkill + 5;
            }

            public override int AttackProc(Character enemy, int damage)
            {
                if (enemy is Mob)
                    ((Mob)enemy).Aggro(this);
                return base.AttackProc(enemy, damage);
            }

            public override int DamageRoll()
            {
                return Rnd.NormalIntRange(2, 4 + Dungeon.depth / 2);
            }

            public override int DrRoll()
            {
                if (Dungeon.IsChallenged(Challenges.NO_ARMOR))
                {
                    return Rnd.NormalIntRange(wandLevel, 2 + wandLevel);
                }
                else
                {
                    return Rnd.NormalIntRange(wandLevel, 3 + 3 * wandLevel);
                }
            }

            public override string Description()
            {
                if (Dungeon.IsChallenged(Challenges.NO_ARMOR))
                {
                    return Messages.Get(this, "desc", wandLevel, 2 + wandLevel);
                }
                else
                {
                    return Messages.Get(this, "desc", wandLevel, 3 + 3 * wandLevel);
                }
            }

            private void InitInstance()
            {
                immunities.Add(typeof(Corruption));
            }

            private const string DEFENSE = "defense";
            private const string WAND_LEVEL = "wand_level";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(DEFENSE, defenseSkill);
                bundle.Put(WAND_LEVEL, wandLevel);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                defenseSkill = bundle.GetInt(DEFENSE);
                wandLevel = bundle.GetInt(WAND_LEVEL);
            }

            public class EarthGuardianWandering : Mob.Wandering
            {
                public EarthGuardianWandering(Mob mob)
                    : base(mob)
                { }

                public override bool Act(bool enemyInFOV, bool justAlerted)
                {
                    if (!enemyInFOV)
                    {
                        var mob = (EarthGuardian)base.mob;

                        Buff.Affect<RockArmor>(Dungeon.hero).AddArmor(mob.wandLevel, mob.HP);
                        Dungeon.hero.sprite.CenterEmitter().Burst(MagicMissile.EarthParticle.Attract, 8 + mob.wandLevel / 2);
                        mob.Destroy();
                        mob.sprite.Die();
                        return true;
                    }
                    else
                    {
                        return base.Act(enemyInFOV, justAlerted);
                    }
                }
            }
        }
    }
}