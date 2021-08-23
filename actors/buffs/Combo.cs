using System;
using watabou.noosa;
using watabou.utils;
using watabou.noosa.audio;
using spdd.ui;
using spdd.utils;
using spdd.actors.hero;
using spdd.sprites;
using spdd.items;
using spdd.items.wands;
using spdd.scenes;
using spdd.mechanics;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Combo : Buff, ActionIndicator.IAction
    {
        private int count;
        private float comboTime;
        private int misses;

        public Combo()
        {
            finisher = new ComboCellSelector(this);
        }

        public override int Icon()
        {
            return BuffIndicator.COMBO;
        }

        public override void TintIcon(Image icon)
        {
            if (count >= 10)
                icon.Hardlight(1f, 0f, 0f);
            else if (count >= 8)
                icon.Hardlight(1f, 0.8f, 0f);
            else if (count >= 6)
                icon.Hardlight(1f, 1f, 0f);
            else if (count >= 4)
                icon.Hardlight(0.8f, 1f, 0f);
            else if (count >= 2)
                icon.Hardlight(0f, 1f, 0f);
            else
                icon.ResetColor();
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (4 - comboTime) / 4f);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public void Hit(Character enemy)
        {
            ++count;

            comboTime = 4f;
            misses = 0;

            if (count >= 2)
            {
                ActionIndicator.SetAction(this);
                BadgesExtensions.ValidateMasteryCombo(count);

                GLog.Positive(Messages.Get(this, "combo", count));
            }

            BuffIndicator.RefreshHero(); //refresh the buff visually on-hit
        }

        public void Miss(Character enemy)
        {
            ++misses;
            comboTime = 4f;
            if (misses >= 2)
                Detach();
        }

        public override void Detach()
        {
            base.Detach();
            ActionIndicator.ClearAction(this);
        }

        public override bool Act()
        {
            comboTime -= TICK;
            Spend(TICK);
            if (comboTime <= 0)
                Detach();

            return true;
        }

        public override string Desc()
        {
            string desc = Messages.Get(this, "desc");

            if (count >= 10)
                desc += "\n\n" + Messages.Get(this, "fury_desc");
            else if (count >= 8)
                desc += "\n\n" + Messages.Get(this, "crush_desc");
            else if (count >= 6)
                desc += "\n\n" + Messages.Get(this, "slam_desc");
            else if (count >= 4)
                desc += "\n\n" + Messages.Get(this, "cleave_desc");
            else if (count >= 2)
                desc += "\n\n" + Messages.Get(this, "clobber_desc");

            return desc;
        }

        private const string COUNT = "count";
        private const string TIME = "combotime";
        private const string MISSES = "misses";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(COUNT, count);
            bundle.Put(TIME, comboTime);
            bundle.Put(MISSES, misses);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            count = bundle.GetInt(COUNT);
            if (count >= 2)
                ActionIndicator.SetAction(this);
            comboTime = bundle.GetFloat(TIME);
            misses = bundle.GetInt(MISSES);
        }

        // ActionIndicator.IAction
        public Image GetIcon()
        {
            Image icon = null;
            if (((Hero)target).belongings.weapon != null)
            {
                icon = new ItemSprite(((Hero)target).belongings.weapon.image, null);
            }
            else
            {
                var item = new Item();
                item.image = ItemSpriteSheet.WEAPON_HOLDER;
                icon = new ItemSprite(item);
            }

            if (count >= 10)
                icon.Tint(new Color(0xFF, 0x00, 0x00, 0xFF));
            else if (count >= 8)
                icon.Tint(new Color(0xFF, 0xCC, 0x00, 0xFF));
            else if (count >= 6)
                icon.Tint(new Color(0xFF, 0xFF, 0x00, 0xFF));
            else if (count >= 4)
                icon.Tint(new Color(0xCC, 0xFF, 0x00, 0xFF));
            else
                icon.Tint(new Color(0x00, 0xFF, 0x00, 0xFF));

            return icon;
        }

        // ActionIndicator.IAction
        public void DoAction()
        {
            GameScene.SelectCell(finisher);
        }

        private enum FinisherType
        {
            CLOBBER, CLEAVE, SLAM, CRUSH, FURY
        }

        private CellSelector.IListener finisher;

        class ComboCellSelector : CellSelector.IListener
        {
            private FinisherType type;
            private Combo combo;

            public ComboCellSelector(Combo combo)
            {
                this.combo = combo;
            }

            public void OnSelect(int? cell)
            {
                if (cell == null)
                    return;

                var target = combo.target;
                int c = cell.Value;

                var enemy = Actor.FindChar(c);

                if (enemy == null || 
                    !Dungeon.level.heroFOV[c] || 
                    !((Hero)target).CanAttack(enemy) || 
                    target.IsCharmedBy(enemy))
                {
                    GLog.Warning(Messages.Get(typeof(Combo), "bad_target"));
                }
                else
                {
                    var callback = new ActionCallback();
                    callback.action = () =>
                    {
                        int count = combo.count;

                        if (count >= 10) 
                            type = FinisherType.FURY;
                        else if (count >= 8) 
                            type = FinisherType.CRUSH;
                        else if (count >= 6) 
                            type = FinisherType.SLAM;
                        else if (count >= 4) 
                            type = FinisherType.CLEAVE;
                        else 
                            type = FinisherType.CLOBBER;

                        DoAttack(enemy);
                    };
                    target.sprite.Attack(c, callback);
                }
            }

            private void DoAttack(Character enemy)
            {
                AttackIndicator.Target(enemy);

                var target = combo.target;

                if (enemy.DefenseSkill(target) >= Character.INFINITE_EVASION)
                {
                    enemy.sprite.ShowStatus(CharSprite.NEUTRAL, enemy.DefenseVerb());
                    Sample.Instance.Play(Assets.Sounds.MISS);
                    combo.Detach();
                    ActionIndicator.ClearAction(combo);
                    ((Hero)target).SpendAndNext(((Hero)target).AttackDelay());
                    return;
                }
                else if (enemy.IsInvulnerable(target.GetType()))
                {
                    enemy.sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(typeof(Character), "invulnerable"));
                    Sample.Instance.Play(Assets.Sounds.MISS);
                    combo.Detach();
                    ActionIndicator.ClearAction(combo);
                    ((Hero)target).SpendAndNext(((Hero)target).AttackDelay());
                    return;
                }

                int dmg = target.DamageRoll();

                //variance in damage dealt
                switch (type)
                {
                    case FinisherType.CLOBBER:
                        dmg = (int)Math.Round(dmg * 0.6f, MidpointRounding.AwayFromZero);
                        break;
                    case FinisherType.CLEAVE:
                        dmg = (int)Math.Round(dmg * 1.5f, MidpointRounding.AwayFromZero);
                        break;
                    case FinisherType.SLAM:
                        dmg += target.DrRoll();
                        break;
                    case FinisherType.CRUSH:
                        //rolls 4 times, takes the highest roll
                        for (int i = 1; i < 4; ++i)
                        {
                            int dmgReroll = target.DamageRoll();
                            if (dmgReroll > dmg)
                                dmg = dmgReroll;
                        }
                        dmg = (int)Math.Round(dmg * 2.5f, MidpointRounding.AwayFromZero);
                        break;
                    case FinisherType.FURY:
                        dmg = (int)Math.Round(dmg * 0.6f, MidpointRounding.AwayFromZero);
                        break;
                }

                dmg = enemy.DefenseProc(target, dmg);
                dmg -= enemy.DrRoll();

                if (enemy.FindBuff<Vulnerable>() != null)
                    dmg = (int)(dmg * 1.33f);

                dmg = target.AttackProc(enemy, dmg);
                bool wasAlly = enemy.alignment == target.alignment;
                enemy.Damage(dmg, this);

                //special effects
                switch (type)
                {
                    case FinisherType.CLOBBER:
                        if (enemy.IsAlive())
                        {
                            //trace a ballistica to our target (which will also extend past them
                            Ballistic trajectory = new Ballistic(target.pos, enemy.pos, Ballistic.STOP_TARGET);
                            //trim it to just be the part that goes past them
                            trajectory = new Ballistic(trajectory.collisionPos,
                                trajectory.path[trajectory.path.Count - 1],
                                Ballistic.PROJECTILE);
                            //knock them back along that ballistica
                            WandOfBlastWave.ThrowChar(enemy, trajectory, 2, true, false);
                            Buff.Prolong<Vertigo>(enemy, Rnd.NormalIntRange(1, 4));
                        }
                        break;
                    case FinisherType.SLAM:
                        var shield = Buff.Affect<BrokenSeal.WarriorShield>(target);
                        if (shield != null)
                        {
                            shield.Supercharge(dmg / 2);
                        }
                        break;
                    default:
                        //nothing
                        break;
                }

                if (target.FindBuff<FireImbue>() != null)
                    target.FindBuff<FireImbue>().Proc(enemy);
                if (target.FindBuff<EarthImbue>() != null)
                    target.FindBuff<EarthImbue>().Proc(enemy);
                if (target.FindBuff<FrostImbue>() != null)
                    target.FindBuff<FrostImbue>().Proc(enemy);

                target.HitSound(Rnd.Float(0.87f, 1.15f));
                if (type != FinisherType.FURY)
                    Sample.Instance.Play(Assets.Sounds.HIT_STRONG);
                enemy.sprite.BloodBurstA(target.sprite.Center(), dmg);
                enemy.sprite.Flash();

                if (!enemy.IsAlive())
                {
                    GLog.Information(Messages.Capitalize(Messages.Get(typeof(Character), "defeat", enemy.Name())));
                }

                Hero hero = (Hero)target;

                //Post-attack behaviour
                switch (type)
                {
                    case FinisherType.CLEAVE:
                        //combo isn't reset, but rather increments with a cleave kill, and grants more time.
                        //this includes corrupting kills (which is why we check alignment
                        if (!enemy.IsAlive() || (!wasAlly && enemy.alignment == target.alignment))
                        {
                            combo.Hit(enemy);
                            combo.comboTime = 12f;
                        }
                        else
                        {
                            combo.Detach();
                            ActionIndicator.ClearAction(combo);
                        }
                        hero.SpendAndNext(hero.AttackDelay());
                        break;

                    case FinisherType.FURY:
                        --combo.count;
                        //fury attacks as many times as you have combo count
                        if (combo.count > 0 && enemy.IsAlive())
                        {
                            var callback = new ActionCallback();
                            callback.action = () => DoAttack(enemy);
                            target.sprite.Attack(enemy.pos, callback);
                        } 
                        else
                        {
                            combo.Detach();
                            Sample.Instance.Play(Assets.Sounds.HIT_STRONG);
                            ActionIndicator.ClearAction(combo);
                            hero.SpendAndNext(hero.AttackDelay());
                        }
                        break;

                    default:
                        combo.Detach();
                        ActionIndicator.ClearAction(combo);
                        hero.SpendAndNext(hero.AttackDelay());
                        break;
                }
            }

            // Interface CellSelector
            public string Prompt()
            {
                Type t = typeof(Combo);

                if (combo.count >= 10)
                    return Messages.Get(t, "fury_prompt");
                else if (combo.count >= 8)
                    return Messages.Get(t, "crush_prompt");
                else if (combo.count >= 6)
                    return Messages.Get(t, "slam_prompt");
                else if (combo.count >= 4)
                    return Messages.Get(t, "cleave_prompt");
                else
                    return Messages.Get(t, "clobber_prompt");
            }
        }
    }
}