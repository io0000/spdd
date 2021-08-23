using System;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.actors.mobs.npcs;
using spdd.effects;
using spdd.utils;
using spdd.sprites;
using spdd.scenes;
using spdd.mechanics;
using spdd.windows;
using spdd.items.weapon.melee;
using spdd.messages;

namespace spdd.items.wands
{
    public class WandOfWarding : Wand
    {
        public WandOfWarding()
        {
            image = ItemSpriteSheet.WAND_WARDING;
        }

        protected override int CollisionProperties(int target)
        {
            if (Dungeon.level.heroFOV[target])
            {
                return Ballistic.STOP_TARGET;
            }
            else
            {
                return Ballistic.PROJECTILE;
            }
        }

        private bool wardAvailable = true;

        public override bool TryToZap(Hero owner, int target)
        {
            int currentWardEnergy = 0;
            foreach (var chr in Actor.Chars())
            {
                if (chr is Ward)
                {
                    currentWardEnergy += ((Ward)chr).tier;
                }
            }

            int maxWardEnergy = 0;
            foreach (Buff buff in curUser.Buffs())
            {
                if (buff is Wand.Charger)
                {
                    if (((Charger)buff).Wand() is WandOfWarding)
                    {
                        maxWardEnergy += 2 + ((Charger)buff).Wand().GetLevel();
                    }
                }
            }

            wardAvailable = (currentWardEnergy < maxWardEnergy);

            var ch = Actor.FindChar(target);
            if (ch is Ward)
            {
                if (!wardAvailable && ((Ward)ch).tier <= 3)
                {
                    GLog.Warning(Messages.Get(this, "no_more_wards"));
                    return false;
                }
            }
            else
            {
                if ((currentWardEnergy + 1) > maxWardEnergy)
                {
                    GLog.Warning(Messages.Get(this, "no_more_wards"));
                    return false;
                }
            }

            return base.TryToZap(owner, target);
        }

        protected override void OnZap(Ballistic bolt)
        {
            int target = bolt.collisionPos;

            var ch = Actor.FindChar(target);
            if (ch != null && !(ch is Ward))
            {
                if (bolt.dist > 1)
                    target = bolt.path[bolt.dist - 1];

                ch = Actor.FindChar(target);
                if (ch != null && !(ch is Ward))
                {
                    GLog.Warning(Messages.Get(this, "bad_location"));
                    Dungeon.level.PressCell(bolt.collisionPos);
                    return;
                }
            }

            if (!Dungeon.level.passable[target])
            {
                GLog.Warning(Messages.Get(this, "bad_location"));
                Dungeon.level.PressCell(target);
            }
            else if (ch != null)
            {
                if (ch is Ward)
                {
                    if (wardAvailable)
                    {
                        ((Ward)ch).Upgrade(BuffedLvl());
                    }
                    else
                    {
                        ((Ward)ch).WandHeal(BuffedLvl());
                    }
                    ch.sprite.Emitter().Burst(MagicMissile.WardParticle.Up, ((Ward)ch).tier);
                }
                else
                {
                    GLog.Warning(Messages.Get(this, "bad_location"));
                    Dungeon.level.PressCell(target);
                }
            }
            else
            {
                Ward ward = new Ward();
                ward.pos = target;
                ward.wandLevel = BuffedLvl();
                GameScene.Add(ward, 1f);
                Dungeon.level.OccupyCell(ward);
                ward.sprite.Emitter().Burst(MagicMissile.WardParticle.Up, ward.tier);
                Dungeon.level.PressCell(target);
            }
        }

        public override void Fx(Ballistic bolt, ICallback callback)
        {
            MagicMissile m = MagicMissile.BoltFromChar(curUser.sprite.parent,
                    MagicMissile.WARD,
                    curUser.sprite,
                    bolt.collisionPos,
                    callback);

            if (bolt.dist > 10)
                m.SetSpeed(bolt.dist * 20);

            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            int level = Math.Max(0, staff.BuffedLvl());

            // lvl 0 - 20%
            // lvl 1 - 33%
            // lvl 2 - 43%
            if (Rnd.Int(level + 5) >= 4)
            {
                foreach (var ch in Actor.Chars())
                {
                    if (ch is Ward)
                    {
                        ((Ward)ch).WandHeal(staff.BuffedLvl());
                        ch.sprite.Emitter().Burst(MagicMissile.WardParticle.Up, ((Ward)ch).tier);
                    }
                }
            }
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            particle.SetColor(new Color(0x88, 0x22, 0xFF, 0xFF));
            particle.am = 0.3f;
            particle.SetLifespan(3f);
            particle.speed.Polar(Rnd.Float(PointF.PI2), 0.3f);
            particle.SetSize(1f, 2f);
            particle.RadiateXY(2.5f);
        }

        public override string StatsDesc()
        {
            if (levelKnown)
                return Messages.Get(this, "stats_desc", GetLevel() + 2);
            else
                return Messages.Get(this, "stats_desc", 2);
        }

        [SPDStatic]
        public class Ward : NPC
        {
            public int tier = 1;
            public int wandLevel = 1;

            public int totalZaps;

            public Ward()
            {
                InitInstance1();
                InitInstance2();

                spriteClass = typeof(WardSprite);

                alignment = Alignment.ALLY;

                properties.Add(Property.IMMOVABLE);
                properties.Add(Property.INORGANIC);

                viewDistance = 4;
                state = WANDERING;
            }

            public override string Name()
            {
                return Messages.Get(this, "name_" + tier);
            }

            public void Upgrade(int wandLevel)
            {
                if (this.wandLevel < wandLevel)
                {
                    this.wandLevel = wandLevel;
                }

                switch (tier)
                {
                    case 1:
                    case 2:
                    default:
                        break; //do nothing
                    case 3:
                        HT = 35;
                        HP = 15 + (5 - totalZaps) * 4;
                        break;
                    case 4:
                        HT = 54;
                        HP += 19;
                        break;
                    case 5:
                        HT = 84;
                        HP += 30;
                        break;
                    case 6:
                        WandHeal(wandLevel);
                        break;
                }

                if (tier < 6)
                {
                    ++tier;
                    ++viewDistance;
                    if (sprite != null)
                    {
                        ((WardSprite)sprite).UpdateTier(tier);
                        sprite.Place(pos);
                    }
                    GameScene.UpdateFog(pos, viewDistance + 1);
                }
            }

            public void WandHeal(int wandLevel)
            {
                if (this.wandLevel < wandLevel)
                {
                    this.wandLevel = wandLevel;
                }

                int heal;
                switch (tier)
                {
                    default:
                        return;
                    case 4:
                        heal = 9;
                        break;
                    case 5:
                        heal = 12;
                        break;
                    case 6:
                        heal = 16;
                        break;
                }

                HP = Math.Min(HT, HP + heal);
                if (sprite != null)
                    sprite.ShowStatus(CharSprite.POSITIVE, heal.ToString());
            }

            public override int DefenseSkill(Character enemy)
            {
                if (tier > 3)
                {
                    defenseSkill = 4 + Dungeon.depth;
                }
                return base.DefenseSkill(enemy);
            }

            public override int DrRoll()
            {
                if (tier > 3)
                {
                    return (int)Math.Round(Rnd.NormalIntRange(0, 3 + Dungeon.depth / 2) / (7f - tier), MidpointRounding.AwayFromZero);
                }
                else
                {
                    return 0;
                }
            }

            protected override float AttackDelay()
            {
                if (tier > 3)
                {
                    return 1f;
                }
                else
                {
                    return 2f;
                }
            }

            protected override bool CanAttack(Character enemy)
            {
                return new Ballistic(pos, enemy.pos, Ballistic.MAGIC_BOLT).collisionPos == enemy.pos;
            }

            protected override bool DoAttack(Character enemy)
            {
                bool visible = fieldOfView[pos] || fieldOfView[enemy.pos];
                if (visible)
                {
                    sprite.Zap(enemy.pos);
                }
                else
                {
                    Zap();
                }

                return !visible;
            }

            private void Zap()
            {
                Spend(1f);

                //always hits
                int dmg = Rnd.NormalIntRange(2 + wandLevel, 8 + 4 * wandLevel);
                enemy.Damage(dmg, typeof(WandOfWarding));
                if (enemy.IsAlive())
                {
                    Wand.ProcessSoulMark(enemy, wandLevel, 1);
                }

                if (!enemy.IsAlive() && enemy == Dungeon.hero)
                {
                    Dungeon.Fail(GetType());
                }

                ++totalZaps;
                switch (tier)
                {
                    case 1:
                    case 2:
                    case 3:
                    default:
                        if (totalZaps >= (2 * tier - 1))
                        {
                            Die(this);
                        }
                        break;
                    case 4:
                        Damage(5, this);
                        break;
                    case 5:
                        Damage(6, this);
                        break;
                    case 6:
                        Damage(7, this);
                        break;
                }
            }

            public void OnZapComplete()
            {
                Zap();
                Next();
            }

            public override bool GetCloser(int target)
            {
                return false;
            }

            public override bool GetFurther(int target)
            {
                return false;
            }

            // public CharSprite sprite()
            public override CharSprite GetSprite()
            {
                WardSprite sprite = (WardSprite)base.GetSprite();
                sprite.LinkVisuals(this);
                return sprite;
            }

            public override void UpdateSpriteState()
            {
                base.UpdateSpriteState();
                ((WardSprite)sprite).UpdateTier(tier);
                sprite.Place(pos);
            }

            public override void Destroy()
            {
                base.Destroy();
                Dungeon.Observe();
                GameScene.UpdateFog(pos, viewDistance + 1);
            }

            public override bool CanInteract(Character c)
            {
                return true;
            }

            public override bool Interact(Character c)
            {
                if (c != Dungeon.hero)
                {
                    return true;
                }

                var wnd = new WndOptions(
                                Messages.Get(this, "dismiss_title"),
                                Messages.Get(this, "dismiss_body"),
                                Messages.Get(this, "dismiss_confirm"),
                                Messages.Get(this, "dismiss_cancel"));

                wnd.selectAction = (index) =>
                {
                    if (index == 0)
                        Die(null);
                };

                GameScene.Show(wnd);

                return true;
            }

            public override string Description()
            {
                return Messages.Get(this, "desc_" + tier, 2 + wandLevel, 8 + 4 * wandLevel, tier);
            }

            private void InitInstance1()
            {
                immunities.Add(typeof(Corruption));
            }

            private const string TIER = "tier";
            private const string WAND_LEVEL = "wand_level";
            private const string TOTAL_ZAPS = "total_zaps";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(TIER, tier);
                bundle.Put(WAND_LEVEL, wandLevel);
                bundle.Put(TOTAL_ZAPS, totalZaps);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                tier = bundle.GetInt(TIER);
                viewDistance = 3 + tier;
                wandLevel = bundle.GetInt(WAND_LEVEL);
                totalZaps = bundle.GetInt(TOTAL_ZAPS);
            }

            private void InitInstance2()
            {
                properties.Add(Property.IMMOVABLE);
            }
        }
    }
}