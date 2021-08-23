using System;
using System.Collections.Generic;
using System.Linq;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.items.armor.glyphs;
using spdd.items.artifacts;
using spdd.items.scrolls;
using spdd.levels;
using spdd.mechanics;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.actors.mobs
{
    public class DwarfKing : Mob
    {
        public DwarfKing()
        {
            spriteClass = typeof(KingSprite);

            HP = HT = 300;
            EXP = 40;
            defenseSkill = 22;

            properties.Add(Property.BOSS);
            properties.Add(Property.UNDEAD);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(15, 25);
        }

        public override int AttackSkill(Character target)
        {
            return 26;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 10);
        }

        private int phase = 1;
        private int summonsMade;

        private float summonCooldown;
        private float abilityCooldown;
        private const int MIN_COOLDOWN = 10;
        private const int MAX_COOLDOWN = 14;

        private int lastAbility;
        private const int NONE = 0;
        private const int LINK = 1;
        private const int TELE = 2;

        private const string PHASE = "phase";
        private const string SUMMONS_MADE = "summons_made";

        private const string SUMMON_CD = "summon_cd";
        private const string ABILITY_CD = "ability_cd";
        private const string LAST_ABILITY = "last_ability";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);

            bundle.Put(PHASE, phase);
            bundle.Put(SUMMONS_MADE, summonsMade);
            bundle.Put(SUMMON_CD, summonCooldown);
            bundle.Put(ABILITY_CD, abilityCooldown);
            bundle.Put(LAST_ABILITY, lastAbility);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);

            phase = bundle.GetInt(PHASE);
            summonsMade = bundle.GetInt(SUMMONS_MADE);
            summonCooldown = bundle.GetFloat(SUMMON_CD);
            abilityCooldown = bundle.GetFloat(ABILITY_CD);
            lastAbility = bundle.GetInt(LAST_ABILITY);

            if (phase == 2)
                properties.Add(Property.IMMOVABLE);
        }

        public override bool Act()
        {
            if (phase == 1)
            {
                if (summonCooldown <= 0 && SummonSubject(3))
                {
                    ++summonsMade;
                    summonCooldown += Rnd.NormalIntRange(MIN_COOLDOWN, MAX_COOLDOWN);
                }
                else if (summonCooldown > 0)
                {
                    --summonCooldown;
                }

                if (paralysed > 0)
                {
                    Spend(TICK);
                    return true;
                }

                if (abilityCooldown <= 0)
                {
                    if (lastAbility == NONE)
                    {
                        //50/50 either ability
                        lastAbility = Rnd.Int(2) == 0 ? LINK : TELE;
                    }
                    else if (lastAbility == LINK)
                    {
                        //more likely to use tele
                        lastAbility = Rnd.Int(8) == 0 ? LINK : TELE;
                    }
                    else
                    {
                        //more likely to use link
                        lastAbility = Rnd.Int(8) != 0 ? LINK : TELE;
                    }

                    if (lastAbility == LINK && LifeLinkSubject())
                    {
                        abilityCooldown += Rnd.NormalIntRange(MIN_COOLDOWN, MAX_COOLDOWN);
                        Spend(TICK);
                        return true;
                    }
                    else if (TeleportSubject())
                    {
                        lastAbility = TELE;
                        abilityCooldown += Rnd.NormalIntRange(MIN_COOLDOWN, MAX_COOLDOWN);
                        Spend(TICK);
                        return true;
                    }
                }
                else
                {
                    --abilityCooldown;
                }
            }
            else if (phase == 2)
            {
                if (summonsMade < 4)
                {
                    if (summonsMade == 0)
                    {
                        sprite.CenterEmitter().Start(Speck.Factory(Speck.SCREAM), 0.4f, 2);
                        Sample.Instance.Play(Assets.Sounds.CHALLENGE);
                        Yell(Messages.Get(this, "wave_1"));
                    }
                    SummonSubject(3, typeof(DKGhoul));
                    Spend(3 * TICK);
                    ++summonsMade;
                    return true;
                }
                else if (Shielding() <= 200 && summonsMade < 8)
                {
                    if (summonsMade == 4)
                    {
                        sprite.CenterEmitter().Start(Speck.Factory(Speck.SCREAM), 0.4f, 2);
                        Sample.Instance.Play(Assets.Sounds.CHALLENGE);
                        Yell(Messages.Get(this, "wave_2"));
                    }
                    if (summonsMade == 7)
                        SummonSubject(3, Rnd.Int(2) == 0 ? typeof(DKMonk) : typeof(DKWarlock));
                    else
                        SummonSubject(3, typeof(DKGhoul));

                    ++summonsMade;
                    Spend(TICK);
                    return true;
                }
                else if (Shielding() <= 100 && summonsMade < 12)
                {
                    sprite.CenterEmitter().Start(Speck.Factory(Speck.SCREAM), 0.4f, 2);
                    Sample.Instance.Play(Assets.Sounds.CHALLENGE);
                    Yell(Messages.Get(this, "wave_3"));
                    SummonSubject(4, typeof(DKWarlock));
                    SummonSubject(4, typeof(DKMonk));
                    SummonSubject(4, typeof(DKGhoul));
                    SummonSubject(4, typeof(DKGhoul));
                    summonsMade = 12;
                    Spend(TICK);
                    return true;
                }
                else
                {
                    Spend(TICK);
                    return true;
                }
            }
            else if (phase == 3 && Buffs<Summoning>().Count < 4)
            {
                if (SummonSubject(3))
                    ++summonsMade;
            }

            return base.Act();
        }

        private bool SummonSubject(int delay)
        {
            //4th summon is always a monk or warlock, otherwise ghoul
            if (summonsMade % 4 == 3)
            {
                return SummonSubject(delay, Rnd.Int(2) == 0 ? typeof(DKMonk) : typeof(DKWarlock));
            }
            else
            {
                return SummonSubject(delay, typeof(DKGhoul));
            }
        }

        private bool SummonSubject(int delay, Type type)
        {
            Summoning s = new Summoning();
            s.pos = ((NewCityBossLevel)Dungeon.level).GetSummoningPos();

            if (s.pos == -1)
                return false;

            s.summon = type;
            s.delay = delay;
            s.AttachTo(this);
            return true;
        }

        private HashSet<Mob> GetSubjects()
        {
            HashSet<Mob> subjects = new HashSet<Mob>();
            foreach (Mob m in Dungeon.level.mobs)
            {
                if (m.alignment == alignment && (m is Ghoul || m is Monk || m is Warlock))
                {
                    subjects.Add(m);
                }
            }
            return subjects;
        }

        private bool LifeLinkSubject()
        {
            Mob furthest = null;

            foreach (Mob m in GetSubjects())
            {
                bool alreadyLinked = false;
                foreach (LifeLink l in m.Buffs<LifeLink>())
                {
                    if (l.obj == Id())
                        alreadyLinked = true;
                }

                if (!alreadyLinked)
                {
                    if (furthest == null || Dungeon.level.Distance(pos, furthest.pos) < Dungeon.level.Distance(pos, m.pos))
                    {
                        furthest = m;
                    }
                }
            }

            if (furthest != null)
            {
                Buff.Append<LifeLink>(furthest, 100f).obj = Id();
                Buff.Append<LifeLink>(this, 100f).obj = furthest.Id();
                Yell(Messages.Get(this, "lifelink_" + Rnd.IntRange(1, 2)));
                sprite.parent.Add(new Beam.HealthRay(sprite.DestinationCenter(), furthest.sprite.DestinationCenter()));
                return true;
            }

            return false;
        }

        private bool TeleportSubject()
        {
            if (enemy == null)
                return false;

            Mob furthest = null;

            foreach (Mob m in GetSubjects())
            {
                if (furthest == null || Dungeon.level.Distance(pos, furthest.pos) < Dungeon.level.Distance(pos, m.pos))
                {
                    furthest = m;
                }
            }

            if (furthest != null)
            {
                float bestDist;
                int bestPos = pos;

                Ballistic trajectory = new Ballistic(enemy.pos, pos, Ballistic.STOP_TARGET);
                int targetCell = trajectory.path[trajectory.dist + 1];
                //if the position opposite the direction of the hero is open, go there
                if (Actor.FindChar(targetCell) == null && !Dungeon.level.solid[targetCell])
                {
                    bestPos = targetCell;
                }
                //Otherwise go to the neighbor cell that's open and is furthest
                else
                {
                    bestDist = Dungeon.level.TrueDistance(pos, enemy.pos);

                    foreach (int i in PathFinder.NEIGHBORS8)
                    {
                        if (Actor.FindChar(pos + i) == null &&
                            !Dungeon.level.solid[pos + i] &&
                            Dungeon.level.TrueDistance(pos + i, enemy.pos) > bestDist)
                        {
                            bestPos = pos + i;
                            bestDist = Dungeon.level.TrueDistance(pos + i, enemy.pos);
                        }
                    }
                }

                Actor.Add(new Pushing(this, pos, bestPos));
                pos = bestPos;

                //find closest cell that's adjacent to _enemy, place subject there
                bestDist = Dungeon.level.TrueDistance(enemy.pos, pos);
                bestPos = enemy.pos;
                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    if (Actor.FindChar(enemy.pos + i) == null &&
                        !Dungeon.level.solid[enemy.pos + i] &&
                        Dungeon.level.TrueDistance(enemy.pos + i, pos) < bestDist)
                    {
                        bestPos = enemy.pos + i;
                        bestDist = Dungeon.level.TrueDistance(enemy.pos + i, pos);
                    }
                }

                if (bestPos != enemy.pos)
                    ScrollOfTeleportation.Appear(furthest, bestPos);

                Yell(Messages.Get(this, "teleport_" + Rnd.IntRange(1, 2)));
                return true;
            }
            return false;
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
                    {
                        ((DriedRose.GhostHero)ch).SayBoss();
                        // break TODOÈ®ÀÎ
                    }
                }
            }
        }

        public override bool IsInvulnerable(Type effect)
        {
            return phase == 2 && !effect.Equals(typeof(KingDamager));
        }

        public override void Damage(int dmg, object src)
        {
            if (IsInvulnerable(src.GetType()))
            {
                base.Damage(dmg, src);
                return;
            }
            else if (phase == 3 && !(src is Viscosity.DeferedDamage))
            {
                var deferred = Buff.Affect<Viscosity.DeferedDamage>(this);
                deferred.Prolong(dmg);

                sprite.ShowStatus(CharSprite.WARNING, Messages.Get(typeof(Viscosity), "deferred", dmg));
                return;
            }

            int preHP = HP;
            base.Damage(dmg, src);

            LockedFloor lockedFloor = Dungeon.hero.FindBuff<LockedFloor>();
            if (lockedFloor != null && !IsImmune(src.GetType()))
                lockedFloor.AddTime(dmg / 3);

            if (phase == 1)
            {
                int dmgTaken = preHP - HP;
                abilityCooldown -= dmgTaken / 8f;
                summonCooldown -= dmgTaken / 8f;
                if (HP <= 50)
                {
                    HP = 50;
                    sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "invulnerable"));
                    ScrollOfTeleportation.Appear(this, NewCityBossLevel.throne);
                    properties.Add(Property.IMMOVABLE);
                    phase = 2;
                    summonsMade = 0;
                    sprite.Idle();
                    Buff.Affect<DKBarrior>(this).SetShield(HT);
                    foreach (var s in Buffs<Summoning>())
                    {
                        s.Detach();
                    }

                    foreach (Mob m in Dungeon.level.mobs.ToArray())
                    {
                        if (m is Ghoul || m is Monk || m is Warlock)
                            m.Die(null);
                    }
                }
            }
            else if (phase == 2 && Shielding() == 0)
            {
                properties.Remove(Property.IMMOVABLE);
                phase = 3;
                summonsMade = 1; //monk/warlock on 3rd summon
                sprite.CenterEmitter().Start(Speck.Factory(Speck.SCREAM), 0.4f, 2);
                Sample.Instance.Play(Assets.Sounds.CHALLENGE);
                Yell(Messages.Get(this, "enraged", Dungeon.hero.Name()));
            }
            else if (phase == 3 && preHP > 20 && HP < 20)
            {
                Yell(Messages.Get(this, "losing"));
            }
        }

        public override bool IsAlive()
        {
            return base.IsAlive() || phase != 3;
        }

        public override void Die(object cause)
        {
            GameScene.BossSlain();

            base.Die(cause);

            if (Dungeon.level.solid[pos])
            {
                Heap h = Dungeon.level.heaps[pos];
                if (h != null)
                {
                    foreach (Item i in h.items)
                    {
                        Dungeon.level.Drop(i, pos + Dungeon.level.Width());
                    }
                    h.Destroy();
                }
                Dungeon.level.Drop(new ArmorKit(), pos + Dungeon.level.Width()).sprite.Drop(pos);
            }
            else
            {
                Dungeon.level.Drop(new ArmorKit(), pos).sprite.Drop();
            }

            BadgesExtensions.ValidateBossSlain();

            Dungeon.level.Unseal();

            foreach (Mob m in GetSubjects())
            {
                m.Die(null);
            }

            var beacon = Dungeon.hero.belongings.GetItem<LloydsBeacon>();
            if (beacon != null)
                beacon.Upgrade();

            Yell(Messages.Get(this, "defeated"));
        }

        public override bool IsImmune(Type effect)
        {
            //immune to damage amplification from doomed in 2nd phase or later, but it can still be applied
            if (phase > 1 && effect.Equals(typeof(Doom)) && FindBuff<Doom>() != null)
            {
                return true;
            }

            return base.IsImmune(effect);
        }

        [SPDStatic]
        public class DKGhoul : Ghoul
        {
            public DKGhoul()
            {
                state = HUNTING;
            }

            public override bool Act()
            {
                partnerID = -2; //no partners
                return base.Act();
            }
        }

        [SPDStatic]
        public class DKMonk : Monk
        {
            public DKMonk()
            {
                state = HUNTING;
            }
        }

        [SPDStatic]
        public class DKWarlock : Warlock
        {
            public DKWarlock()
            {
                state = HUNTING;
            }
        }

        [SPDStatic]
        public class Summoning : Buff
        {
            public int delay;
            public int pos;
            public Type summon; // Class<?extends Mob>

            private Emitter particles;

            public int GetPos()
            {
                return pos;
            }

            public override bool Act()
            {
                --delay;

                if (delay <= 0)
                {
                    if (summon.Equals(typeof(DKWarlock)))
                    {
                        particles.Burst(ShadowParticle.Curse, 10);
                        Sample.Instance.Play(Assets.Sounds.CURSED);
                    }
                    else if (summon.Equals(typeof(DKMonk)))
                    {
                        particles.Burst(ElmoParticle.Factory, 10);
                        Sample.Instance.Play(Assets.Sounds.BURNING);
                    }
                    else
                    {
                        particles.Burst(Speck.Factory(Speck.BONE), 10);
                        Sample.Instance.Play(Assets.Sounds.BONES);
                    }
                    particles = null;

                    if (Actor.FindChar(pos) != null)
                    {
                        List<int> candidates = new List<int>();
                        foreach (int i in PathFinder.NEIGHBORS8)
                        {
                            if (Dungeon.level.passable[pos + i] && Actor.FindChar(pos + i) == null)
                                candidates.Add(pos + i);
                        }
                        if (candidates.Count > 0)
                            pos = Rnd.Element(candidates);
                    }

                    if (Actor.FindChar(pos) == null)
                    {
                        Mob m = (Mob)Reflection.NewInstance(summon);
                        m.pos = pos;
                        m.maxLvl = -2;
                        GameScene.Add(m);
                        m.state = m.HUNTING;
                        if (((DwarfKing)target).phase == 2)
                            Buff.Affect<KingDamager>(m);
                    }
                    else
                    {
                        var ch = Actor.FindChar(pos);
                        ch.Damage(Rnd.NormalIntRange(20, 40), summon);
                        if (((DwarfKing)target).phase == 2)
                            target.Damage(target.HT / 12, new KingDamager());
                    }

                    Detach();
                }

                Spend(TICK);
                return true;
            }

            public override void Fx(bool on)
            {
                if (on && particles == null)
                {
                    particles = CellEmitter.Get(pos);

                    if (summon.Equals(typeof(DKWarlock)))
                    {
                        particles.Pour(ShadowParticle.Up, 0.1f);
                    }
                    else if (summon.Equals(typeof(DKMonk)))
                    {
                        particles.Pour(ElmoParticle.Factory, 0.1f);
                    }
                    else
                    {
                        particles.Pour(Speck.Factory(Speck.RATTLE), 0.1f);
                    }
                }
                else if (!on && particles != null)
                {
                    particles.on = false;
                }
            }

            private const string DELAY = "delay";
            private const string POS = "pos";
            private const string SUMMON = "summon";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(DELAY, delay);
                bundle.Put(POS, pos);
                bundle.Put(SUMMON, summon);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                delay = bundle.GetInt(DELAY);
                pos = bundle.GetInt(POS);
                summon = bundle.GetClass(SUMMON);
            }
        }

        [SPDStatic]
        public class KingDamager : Buff
        {
            public override bool Act()
            {
                if (target.alignment != Alignment.ENEMY)
                    Detach();

                Spend(TICK);
                return true;
            }

            public override void Detach()
            {
                base.Detach();
                foreach (Mob m in Dungeon.level.mobs)
                {
                    if (m is DwarfKing)
                        m.Damage(m.HT / 12, this);
                }
            }
        }

        [SPDStatic]
        public class DKBarrior : Barrier
        {
            public override bool Act()
            {
                IncShield();
                return base.Act();
            }

            public override int Icon()
            {
                return BuffIndicator.NONE;
            }
        }
    }
}