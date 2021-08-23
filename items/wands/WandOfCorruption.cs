using System;
using System.Linq;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.actors.buffs;
using spdd.effects;
using spdd.sprites;
using spdd.mechanics;
using spdd.utils;
using spdd.items.weapon.melee;
using spdd.messages;

namespace spdd.items.wands
{
    public class WandOfCorruption : Wand
    {
        public WandOfCorruption()
        {
            image = ItemSpriteSheet.WAND_CORRUPTION;
        }

        //Note that some debuffs here have a 0% chance to be applied.
        // This is because the wand of corruption considers them to be a certain level of harmful
        // for the purposes of reducing resistance, but does not actually apply them itself

        private const float MINOR_DEBUFF_WEAKEN = 1 / 4f;
        private static Dictionary<Type, float> MINOR_DEBUFFS = new Dictionary<Type, float>();

        static WandOfCorruption()
        {
            MINOR_DEBUFFS.Add(typeof(Weakness), 2f);
            MINOR_DEBUFFS.Add(typeof(Vulnerable), 2f);
            MINOR_DEBUFFS.Add(typeof(Cripple), 1f);
            MINOR_DEBUFFS.Add(typeof(Blindness), 1f);
            MINOR_DEBUFFS.Add(typeof(Terror), 1f);

            MINOR_DEBUFFS.Add(typeof(Chill), 0f);
            MINOR_DEBUFFS.Add(typeof(Ooze), 0f);
            MINOR_DEBUFFS.Add(typeof(Roots), 0f);
            MINOR_DEBUFFS.Add(typeof(Vertigo), 0f);
            MINOR_DEBUFFS.Add(typeof(Drowsy), 0f);
            MINOR_DEBUFFS.Add(typeof(Bleeding), 0f);
            MINOR_DEBUFFS.Add(typeof(Burning), 0f);
            MINOR_DEBUFFS.Add(typeof(Poison), 0f);

            MAJOR_DEBUFFS.Add(typeof(Amok), 3f);
            MAJOR_DEBUFFS.Add(typeof(Slow), 2f);
            MAJOR_DEBUFFS.Add(typeof(Hex), 2f);
            MAJOR_DEBUFFS.Add(typeof(Paralysis), 1f);

            MAJOR_DEBUFFS.Add(typeof(Charm), 0f);
            MAJOR_DEBUFFS.Add(typeof(MagicalSleep), 0f);
            MAJOR_DEBUFFS.Add(typeof(SoulMark), 0f);
            MAJOR_DEBUFFS.Add(typeof(Corrosion), 0f);
            MAJOR_DEBUFFS.Add(typeof(Frost), 0f);
            MAJOR_DEBUFFS.Add(typeof(Doom), 0f);
        }

        private const float MAJOR_DEBUFF_WEAKEN = 1 / 2f;
        private static Dictionary<Type, float> MAJOR_DEBUFFS = new Dictionary<Type, float>();

        protected override void OnZap(Ballistic bolt)
        {
            var ch = Actor.FindChar(bolt.collisionPos);

            if (ch != null)
            {
                if (!(ch is Mob))
                    return;

                Mob enemy = (Mob)ch;

                float corruptingPower = 3 + BuffedLvl() / 2f;

                //base enemy resistance is usually based on their exp, but in special cases it is based on other criteria
                float enemyResist = 1 + enemy.EXP;
                if (ch is Mimic || ch is Statue)
                {
                    enemyResist = 1 + Dungeon.depth;
                }
                else if (ch is Piranha || ch is Bee)
                {
                    enemyResist = 1 + Dungeon.depth / 2f;
                }
                else if (ch is Wraith)
                {
                    //divide by 5 as wraiths are always at full HP and are therefore ~5x harder to corrupt
                    enemyResist = (1f + Dungeon.depth / 3f) / 5f;
                }
                else if (ch is Yog.BurningFist || ch is Yog.RottingFist)
                {
                    enemyResist = 1 + 30;
                }
                //else if (ch is Yog.Larva || ch is King.Undead)
                else if (ch is Yog.Larva)
                {
                    enemyResist = 1 + 5;
                }
                else if (ch is Swarm)
                {
                    //child swarms don't give exp, so we force this here.
                    enemyResist = 1 + 3;
                }

                //100% health: 5x resist   75%: 3.25x resist   50%: 2x resist   25%: 1.25x resist
                enemyResist *= (1 + 4 * (float)Math.Pow(enemy.HP / (float)enemy.HT, 2));

                //debuffs placed on the enemy reduce their resistance
                foreach (Buff buff in enemy.Buffs())
                {
                    if (MAJOR_DEBUFFS.ContainsKey(buff.GetType()))
                        enemyResist *= (1f - MAJOR_DEBUFF_WEAKEN);
                    else if (MINOR_DEBUFFS.ContainsKey(buff.GetType()))
                        enemyResist *= (1f - MINOR_DEBUFF_WEAKEN);
                    else if (buff.type == Buff.BuffType.NEGATIVE)
                        enemyResist *= (1f - MINOR_DEBUFF_WEAKEN);
                }

                //cannot re-corrupt or doom an enemy, so give them a major debuff instead
                if (enemy.FindBuff<Corruption>() != null || enemy.FindBuff<Doom>() != null)
                {
                    corruptingPower = enemyResist - 0.001f;
                }

                if (corruptingPower > enemyResist)
                {
                    CorruptEnemy(enemy);
                }
                else
                {
                    float debuffChance = corruptingPower / enemyResist;
                    if (Rnd.Float() < debuffChance)
                    {
                        DebuffEnemy(enemy, MAJOR_DEBUFFS);
                    }
                    else
                    {
                        DebuffEnemy(enemy, MINOR_DEBUFFS);
                    }
                }

                ProcessSoulMark(ch, ChargesPerCast());
                Sample.Instance.Play(Assets.Sounds.HIT_MAGIC, 1, 0.8f * Rnd.Float(0.87f, 1.15f));
            }
            else
            {
                Dungeon.level.PressCell(bolt.collisionPos);
            }
        }

        private void DebuffEnemy(Mob enemy, Dictionary<Type, float> category)
        {
            //do not consider buffs which are already assigned, or that the enemy is immune to.
            Dictionary<Type, float> debuffs = new Dictionary<Type, float>(category);
            foreach (Buff existing in enemy.Buffs())
            {
                if (debuffs.ContainsKey(existing.GetType()))
                {
                    debuffs[existing.GetType()] = 0f;
                }
            }

            foreach (var toAssign in debuffs.Keys.ToList())
            {
                if (debuffs[toAssign] > 0 && enemy.IsImmune(toAssign))
                {
                    debuffs[toAssign] = 0.0f;
                }
            }

            //all buffs with a > 0 chance are flavor buffs
            Type debuffCls = Rnd.Chances(debuffs);

            if (debuffCls != null)
            {
                Buff.Append(enemy, debuffCls, 6 + BuffedLvl() * 3);
            }
            else
            {
                //if no debuff can be applied (all are present), then go up one tier
                if (category == MINOR_DEBUFFS)
                    DebuffEnemy(enemy, MAJOR_DEBUFFS);
                else if (category == MAJOR_DEBUFFS)
                    CorruptEnemy(enemy);
            }
        }

        private void CorruptEnemy(Mob enemy)
        {
            //cannot re-corrupt or doom an enemy, so give them a major debuff instead
            if (enemy.FindBuff<Corruption>() != null || enemy.FindBuff<Doom>() != null)
            {
                GLog.Warning(Messages.Get(this, "already_corrupted"));
                return;
            }

            if (!enemy.IsImmune(typeof(Corruption)))
            {
                enemy.HP = enemy.HT;
                foreach (Buff buff in enemy.Buffs())
                {
                    if (buff.type == Buff.BuffType.NEGATIVE && !(buff is SoulMark))
                    {
                        buff.Detach();
                    }
                    else if (buff is PinCushion)
                    {
                        buff.Detach();
                    }
                }
                if (enemy.alignment == Character.Alignment.ENEMY)
                {
                    enemy.RollToDropLoot();
                }

                Buff.Affect<Corruption>(enemy);

                ++Statistics.enemiesSlain;
                BadgesExtensions.ValidateMonstersSlain();
                Statistics.qualifiedForNoKilling = false;
                if (enemy.EXP > 0 && curUser.lvl <= enemy.maxLvl)
                {
                    curUser.sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(enemy, "exp", enemy.EXP));
                    curUser.EarnExp(enemy.EXP, enemy.GetType());
                }
                else
                {
                    curUser.EarnExp(0, enemy.GetType());
                }
            }
            else
            {
                Buff.Affect<Doom>(enemy);
            }
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            // lvl 0 - 25%
            // lvl 1 - 40%
            // lvl 2 - 50%
            if (Rnd.Int(BuffedLvl() + 4) >= 3)
            {
                Buff.Prolong<Amok>(defender, 4 + BuffedLvl() * 2);
            }
        }

        public override void Fx(Ballistic bolt, ICallback callback)
        {
            MagicMissile.BoltFromChar(curUser.sprite.parent,
                    MagicMissile.SHADOW,
                    curUser.sprite,
                    bolt.collisionPos,
                    callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            particle.SetColor(new Color(0x00, 0x00, 0x00, 0xFF));
            particle.am = 0.6f;
            particle.SetLifespan(2f);
            particle.speed.Set(0, 5);
            particle.SetSize(0.5f, 2f);
            particle.ShuffleXY(1f);
        }
    }
}