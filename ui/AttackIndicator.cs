using System.Collections.Generic;
using watabou.input;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.scenes;
using spdd.sprites;

namespace spdd.ui
{
    public class AttackIndicator : Tag
    {
        private const float ENABLED = 1.0f;
        private const float DISABLED = 0.3f;

        private static float delay;

        private static AttackIndicator instance;

        private CharSprite sprite;

        private Mob lastTarget;
        private List<Mob> candidates = new List<Mob>();

        public AttackIndicator()
            : base(DangerIndicator.COLOR)
        {
            instance = this;
            lastTarget = null;

            SetSize(24, 24);
            Visible(false);
            Enable(false);
        }

        public override GameAction KeyAction()
        {
            return SPDAction.TAG_ATTACK;
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();
        }

        protected override void Layout()
        {
            base.Layout();

            if (sprite != null)
            {
                sprite.x = x + (width - sprite.Width()) / 2 + 1;
                sprite.y = y + (height - sprite.Height()) / 2;
                PixelScene.Align(sprite);
            }
        }

        public override void Update()
        {
            base.Update();

            if (!bg.visible)
            {
                Enable(false);
                if (delay > 0f)
                    delay -= Game.elapsed;
                if (delay <= 0f)
                    active = false;
            }
            else
            {
                delay = 0.75f;
                active = true;

                if (Dungeon.hero.IsAlive())
                {
                    Enable(Dungeon.hero.ready);
                }
                else
                {
                    Visible(false);
                    Enable(false);
                }
            }
        }

        private void CheckEnemies()
        {
            candidates.Clear();
            int v = Dungeon.hero.VisibleEnemies();
            for (int i = 0; i < v; ++i)
            {
                var mob = Dungeon.hero.VisibleEnemy(i);
                if (Dungeon.hero.CanAttack(mob))
                    candidates.Add(mob);
            }

            if (!candidates.Contains(lastTarget))
            {
                if (candidates.Count == 0)
                {
                    lastTarget = null;
                }
                else
                {
                    active = true;
                    lastTarget = Rnd.Element(candidates);
                    UpdateImage();
                    Flash();
                }
            }
            else
            {
                if (!bg.visible)
                {
                    active = true;
                    Flash();
                }
            }

            Visible(lastTarget != null);
            Enable(bg.visible);
        }

        private void UpdateImage()
        {
            if (sprite != null)
            {
                sprite.KillAndErase();
                sprite = null;
            }

            sprite = (CharSprite)Reflection.NewInstance(lastTarget.spriteClass);
            active = true;
            sprite.LinkVisuals(lastTarget);
            sprite.Idle();
            sprite.paused = true;
            Add(sprite);

            Layout();
        }

        private bool enabled = true;

        private void Enable(bool value)
        {
            enabled = value;
            if (sprite != null)
                sprite.Alpha(value ? ENABLED : DISABLED);
        }

        private void Visible(bool value)
        {
            bg.visible = value;
            if (sprite != null)
                sprite.visible = value;
        }

        protected override void OnClick()
        {
            if (enabled)
            {
                if (Dungeon.hero.Handle(lastTarget.pos))
                    Dungeon.hero.Next();
            }
        }

        public static void Target(Character target)
        {
            instance.lastTarget = (Mob)target;
            instance.UpdateImage();

            TargetHealthIndicator.instance.Target(target);
        }

        public static void UpdateState()
        {
            instance.CheckEnemies();
        }
    }
}