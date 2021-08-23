using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.hero;
using spdd.actors.mobs.npcs;
using spdd.effects;
using spdd.messages;
using spdd.scenes;
using spdd.ui;
using spdd.utils;

namespace spdd.actors.buffs
{
    public class Preparation : Buff, ActionIndicator.IAction
    {
        public Preparation()
        {
            //always acts after other buffs, so invisibility effects can process first
            actPriority = BUFF_PRIO - 1;
            attack = new CellSelctorPreparation(this);
        }

        public class AttackLevel
        {
            public static AttackLevel[] values = new AttackLevel[4] {
                new AttackLevel(1,  0.15f, 0.05f, 1, 1, 0),      // LVL_1
                new AttackLevel(3,  0.30f, 0.15f, 1, 3, 1),      // LVL_2
                new AttackLevel(6,  0.45f, 0.30f, 2, 5, 2),      // LVL_3
                new AttackLevel(11, 0.60f, 0.50f, 3, 7, 3)       // LVL_4
            };

            public int turnsReq;
            public float baseDmgBonus, kOThreshold;
            public int damageRolls, blinkDistance;
            public int ordinal;

            private AttackLevel(int turnsReq, float baseDmgBonus, float kOThreshold, int damageRolls, int blinkDistance, int ordinal)
            {
                this.turnsReq = turnsReq;
                this.baseDmgBonus = baseDmgBonus;
                this.kOThreshold = kOThreshold;
                this.damageRolls = damageRolls;
                this.blinkDistance = blinkDistance;
                this.ordinal = ordinal;
            }

            public bool CanKO(Character defender)
            {
                var properties = defender.Properties();

                if (properties.Contains(Character.Property.MINIBOSS) ||
                    properties.Contains(Character.Property.BOSS))
                {
                    return (defender.HP / (float)defender.HT) < (kOThreshold / 5f);
                }
                else
                {
                    return (defender.HP / (float)defender.HT) < kOThreshold;
                }
            }

            public int DamageRoll(Character attacker)
            {
                int dmg = attacker.DamageRoll();
                for (int i = 1; i < damageRolls; ++i)
                {
                    int newDmg = attacker.DamageRoll();
                    if (newDmg > dmg)
                        dmg = newDmg;
                }
                return (int)Math.Round(dmg * (1.0f + baseDmgBonus), MidpointRounding.AwayFromZero);
            }

            public static AttackLevel GetLvl(int turnsInvis)
            {
                //List<AttackLevel> values = Arrays.asList(values());
                //Collections.reverse(values);
                //for (AttackLevel lvl : values)
                //{
                //    if (turnsInvis >= lvl.turnsReq)
                //    {
                //        return lvl;
                //    }
                //}
                //return LVL_1;
                for (int i = values.Length - 1; i >= 0; --i)
                {
                    if (turnsInvis >= values[i].turnsReq)
                        return values[i];
                }

                return values[0];
            }
        }

        private int turnsInvis;

        public override bool Act()
        {
            if (target.invisible > 0)
            {
                ++turnsInvis;
                if (AttackLevel.GetLvl(turnsInvis).blinkDistance > 0 && target == Dungeon.hero)
                {
                    ActionIndicator.SetAction(this);
                }
                Spend(TICK);
            }
            else
            {
                Detach();
            }
            return true;
        }

        public override void Detach()
        {
            base.Detach();
            ActionIndicator.ClearAction(this);
        }

        public int DamageRoll(Character attacker)
        {
            return AttackLevel.GetLvl(turnsInvis).DamageRoll(attacker);
        }

        public bool CanKO(Character defender)
        {
            return AttackLevel.GetLvl(turnsInvis).CanKO(defender);
        }

        public override int Icon()
        {
            return BuffIndicator.PREPARATION;
        }

        public override void TintIcon(Image icon)
        {
            switch (AttackLevel.GetLvl(turnsInvis).ordinal)
            {
                case 0: //LVL_1:
                    icon.Hardlight(0f, 1f, 0.0f);
                    break;
                case 1: //LVL_2:
                    icon.Hardlight(1f, 1f, 0.0f);
                    break;
                case 2: //LVL_3:
                    icon.Hardlight(1f, 0.6f, 0.0f);
                    break;
                case 3: // LVL_4:
                    icon.Hardlight(1f, 0.0f, 0.0f);
                    break;
            }
        }

        public override float IconFadePercent()
        {
            var level = AttackLevel.GetLvl(turnsInvis);
            var levelOrdinal = level.ordinal;
            if (levelOrdinal == 3) //AttackLevel.LVL_4)
            {
                return 0;
            }
            else
            {
                float turnsForCur = level.turnsReq;
                float turnsForNext = AttackLevel.values[levelOrdinal + 1].turnsReq;
                turnsForNext -= turnsForCur;
                float turnsToNext = turnsInvis - turnsForCur;
                return Math.Min(1, (turnsForNext - turnsToNext) / (turnsForNext));
            }
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            string desc = Messages.Get(this, "desc");

            AttackLevel lvl = AttackLevel.GetLvl(turnsInvis);

            desc += "\n\n" + Messages.Get(this, "desc_dmg",
                    (int)(lvl.baseDmgBonus * 100),
                    (int)(lvl.kOThreshold * 100),
                    (int)(lvl.kOThreshold * 20));

            if (lvl.damageRolls > 1)
            {
                desc += " " + Messages.Get(this, "desc_dmg_likely");
            }

            if (lvl.blinkDistance > 0)
            {
                desc += "\n\n" + Messages.Get(this, "desc_blink", lvl.blinkDistance);
            }

            desc += "\n\n" + Messages.Get(this, "desc_invis_time", turnsInvis);

            if (lvl.ordinal != AttackLevel.values.Length - 1)
            {
                AttackLevel next = AttackLevel.values[lvl.ordinal + 1];
                desc += "\n" + Messages.Get(this, "desc_invis_next", next.turnsReq);
            }

            return desc;
        }

        private const string TURNS = "turnsInvis";

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            turnsInvis = bundle.GetInt(TURNS);
            if (AttackLevel.GetLvl(turnsInvis).blinkDistance > 0)
            {
                ActionIndicator.SetAction(this);
            }
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(TURNS, turnsInvis);
        }

        // ActionIndicator.IAction
        public Image GetIcon()
        {
            Image actionIco = Effects.Get(Effects.Type.WOUND);
            TintIcon(actionIco);
            return actionIco;
        }

        //ActionIndicator.IAction
        public void DoAction()
        {
            GameScene.SelectCell(attack);
        }

        private CellSelector.IListener attack;

        private class CellSelctorPreparation : CellSelector.IListener
        {
            private Preparation prep;

            public CellSelctorPreparation(Preparation prep)
            {
                this.prep = prep;
            }

            public void OnSelect(int? target)
            {
                if (target == null)
                    return;
                int cell = target.Value;

                Character enemy = Actor.FindChar(cell);
                if (enemy == null ||
                    Dungeon.hero.IsCharmedBy(enemy) ||
                    enemy is NPC)
                {
                    GLog.Warning(Messages.Get(typeof(Preparation), "no_target"));
                }
                else
                {
                    //just attack them then!
                    if (Dungeon.hero.CanAttack(enemy))
                    {
                        Dungeon.hero.curAction = new HeroAction.Attack(enemy);
                        Dungeon.hero.Next();
                        return;
                    }

                    AttackLevel lvl = AttackLevel.GetLvl(prep.turnsInvis);

                    bool[] passable = (bool[])Dungeon.level.passable.Clone();
                    //need to consider enemy cell as passable in case they are on a trap or chasm
                    passable[cell] = true;
                    PathFinder.BuildDistanceMap(Dungeon.hero.pos, passable, lvl.blinkDistance + 1);
                    if (PathFinder.distance[cell] == int.MaxValue)
                    {
                        GLog.Warning(Messages.Get(typeof(Preparation), "out_of_reach"));
                        return;
                    }

                    //we can move through enemies when determining blink distance,
                    // but not when actually jumping to a location
                    foreach (var ch in Actor.Chars())
                    {
                        if (ch != Dungeon.hero)
                            passable[ch.pos] = false;
                    }

                    PathFinder.Path path = PathFinder.Find(Dungeon.hero.pos, cell, passable);
                    int attackPos = path == null ? -1 : path[path.Count - 2];

                    if (attackPos == -1 ||
                        Dungeon.level.Distance(attackPos, Dungeon.hero.pos) > lvl.blinkDistance)
                    {
                        GLog.Warning(Messages.Get(typeof(Preparation), "out_of_reach"));
                        return;
                    }

                    Dungeon.hero.pos = attackPos;
                    Dungeon.level.OccupyCell(Dungeon.hero);
                    //prevents the hero from being interrupted by seeing new enemies
                    Dungeon.Observe();
                    Dungeon.hero.CheckVisibleMobs();

                    Dungeon.hero.sprite.Place(Dungeon.hero.pos);
                    Dungeon.hero.sprite.TurnTo(Dungeon.hero.pos, cell);
                    CellEmitter.Get(Dungeon.hero.pos).Burst(Speck.Factory(Speck.WOOL), 6);
                    Sample.Instance.Play(Assets.Sounds.PUFF);

                    Dungeon.hero.curAction = new HeroAction.Attack(enemy);
                    Dungeon.hero.Next();
                }
            }

            public string Prompt()
            {
                return Messages.Get(typeof(Preparation),
                    "prompt",
                    AttackLevel.GetLvl(prep.turnsInvis).blinkDistance);
            }
        } // CellSelctorPreparation
    } // Preparation
}
