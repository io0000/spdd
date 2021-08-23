using System;
using watabou.noosa;
using watabou.noosa.particles;
using watabou.noosa.ui;
using watabou.utils;
using spdd.actors.mobs;
using spdd.effects.particles;

namespace spdd.ui
{
    public class BossHealthBar : Component
    {
        private Image bar;

        private Image rawShielding;
        private Image shieldedHP;
        private Image hp;

        private static Mob boss;

        private Image skull;
        private Emitter blood;

        private static string asset = Assets.Interfaces.BOSSHP;

        private static BossHealthBar instance;
        private static bool bleeding;

        public BossHealthBar()
        {
            visible = active = (boss != null);
            instance = this;
        }

        protected override void CreateChildren()
        {
            bar = new Image(asset, 0, 0, 64, 16);
            Add(bar);

            width = bar.width;
            height = bar.height;

            rawShielding = new Image(asset, 15, 25, 47, 4);
            rawShielding.Alpha(0.5f);
            Add(rawShielding);

            shieldedHP = new Image(asset, 15, 25, 47, 4);
            Add(shieldedHP);

            hp = new Image(asset, 15, 19, 47, 4);
            Add(hp);

            skull = new Image(asset, 5, 18, 6, 6);
            Add(skull);

            blood = new Emitter();
            blood.Pos(skull);
            blood.Pour(BloodParticle.Factory, 0.3f);
            blood.autoKill = false;
            blood.on = false;
            Add(blood);
        }

        protected override void Layout()
        {
            bar.x = x;
            bar.y = y;

            hp.x = shieldedHP.x = rawShielding.x = bar.x + 15;
            hp.y = shieldedHP.y = rawShielding.y = bar.y + 6;

            skull.x = bar.x + 5;
            skull.y = bar.y + 5;
        }

        public override void Update()
        {
            base.Update();
            if (boss != null)
            {
                if (!boss.IsAlive() || !Dungeon.level.mobs.Contains(boss))
                {
                    boss = null;
                    visible = active = false;
                }
                else
                {
                    float health = boss.HP;
                    float shield = boss.Shielding();
                    float max = boss.HT;

                    hp.scale.x = Math.Max(0, (health - shield) / max);
                    shieldedHP.scale.x = health / max;
                    rawShielding.scale.x = shield / max;

                    if (hp.scale.x < 0.25f)
                        Bleed(true);

                    if (bleeding != blood.on)
                    {
                        if (bleeding)
                            skull.Tint(new Color(0xcc, 0x00, 0x00, 0xFF), 0.6f);
                        else
                            skull.ResetColor();
                        blood.on = bleeding;
                    }
                }
            }
        }

        public static void AssignBoss(Mob boss)
        {
            BossHealthBar.boss = boss;
            Bleed(false);
            if (instance != null)
            {
                instance.visible = instance.active = true;
            }
        }

        public static bool IsAssigned()
        {
            return boss != null && boss.IsAlive() && Dungeon.level.mobs.Contains(boss);
        }

        public static void Bleed(bool value)
        {
            bleeding = value;
        }
    }
}