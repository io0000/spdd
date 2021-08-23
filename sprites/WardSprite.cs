using System;
using watabou.noosa;
using watabou.noosa.tweeners;
using spdd.actors;
using spdd.effects;
using spdd.items.wands;
using spdd.tiles;

namespace spdd.sprites
{
    public class WardSprite : MobSprite
    {
        private Animation[] tierIdles = new Animation[7];

        public WardSprite()
        {
            Texture(Assets.Sprites.WARDS);

            tierIdles[1] = new Animation(1, true);
            tierIdles[1].Frames(texture.UvRect(0, 0, 9, 10));

            tierIdles[2] = new Animation(1, true);
            tierIdles[2].Frames(texture.UvRect(10, 0, 21, 12));

            tierIdles[3] = new Animation(1, true);
            tierIdles[3].Frames(texture.UvRect(22, 0, 37, 16));

            tierIdles[4] = new Animation(1, true);
            tierIdles[4].Frames(texture.UvRect(38, 0, 44, 13));

            tierIdles[5] = new Animation(1, true);
            tierIdles[5].Frames(texture.UvRect(45, 0, 51, 15));

            tierIdles[6] = new Animation(1, true);
            tierIdles[6].Frames(texture.UvRect(52, 0, 60, 15));
        }

        public override void Zap(int pos)
        {
            Idle();
            Flash();

            Emitter().Burst(MagicMissile.WardParticle.Up, 2);
            if (Actor.FindChar(pos) != null)
            {
                parent.Add(new Beam.DeathRay(Center(), Actor.FindChar(pos).sprite.Center()));
            }
            else
            {
                parent.Add(new Beam.DeathRay(Center(), DungeonTilemap.RaisedTileCenterToWorld(pos)));
            }

            ((WandOfWarding.Ward)ch).OnZapComplete();
        }

        public override void TurnTo(int from, int to)
        {
            //do nothing
        }

        public override void Die()
        {
            base.Die();
            //cancels die animation and fades out immediately
            Play(idle, true);
            Emitter().Burst(MagicMissile.WardParticle.Up, 10);
            parent.Add(new WardAlphaTweener(this, 0.0f, 2.0f));
        }

        public class WardAlphaTweener : AlphaTweener
        {
            public WardAlphaTweener(Visual image, float alpha, float time)
                : base(image, alpha, time)
            { }

            public override void OnComplete()
            {
                target.KillAndErase();  // _target : WardSprite
                parent.Erase(this);
            }
        }

        public override void ResetColor()
        {
            base.ResetColor();
            if (ch is WandOfWarding.Ward)
            {
                WandOfWarding.Ward ward = (WandOfWarding.Ward)ch;
                if (ward.tier <= 3)
                {
                    Brightness(Math.Max(0.2f, 1f - (ward.totalZaps / (float)(2 * ward.tier - 1))));
                }
            }
        }

        public override void LinkVisuals(Character ch)
        {
            if (ch == null)
                return;

            UpdateTier(((WandOfWarding.Ward)ch).tier);
        }

        public void UpdateTier(int tier)
        {
            idle = tierIdles[tier];
            run = idle.Clone();
            attack = idle.Clone();
            die = idle.Clone();

            //always render first
            if (parent != null)
            {
                parent.SendToBack(this);
            }

            ResetColor();
            if (ch != null)
                Place(ch.pos);
            Idle();

            if (tier <= 3)
            {
                shadowWidth = shadowHeight = 1f;
                perspectiveRaise = (16 - Height()) / 32f; //center of the cell
            }
            else
            {
                shadowWidth = 1.2f;
                shadowHeight = 0.25f;
                perspectiveRaise = 6 / 16f; //6 pixels
            }
        }

        private float baseY = float.NaN;

        public override void Place(int cell)
        {
            base.Place(cell);
            baseY = y;
        }

        public override void Update()
        {
            base.Update();
            //if tier is greater than 3
            if (perspectiveRaise >= 6 / 16f && !paused)
            {
                if (float.IsNaN(baseY))
                    baseY = y;

                y = baseY + (float)Math.Sin(Game.timeTotal);
                shadowOffset = 0.25f - 0.8f * (float)Math.Sin(Game.timeTotal);
            }
        }
    }
}