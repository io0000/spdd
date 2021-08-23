using watabou.utils;
using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;
using spdd.ui;
using spdd.actors;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class CapeOfThorns : Artifact
    {
        public CapeOfThorns()
        {
            image = ItemSpriteSheet.ARTIFACT_CAPE;

            levelCap = 10;

            charge = 0;
            chargeCap = 100;
            cooldown = 0;

            defaultAction = "NONE"; //so it can be quickslotted
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new CapeOfThorns.Thorns(this);
        }

        public override void Charge(Hero target)
        {
            if (cooldown == 0)
            {
                charge += 4;
                UpdateQuickslot();
            }

            if (charge >= chargeCap)
                target.FindBuff<Thorns>().Proc(0, null, null);
        }

        public override string Desc()
        {
            var desc = Messages.Get(this, "desc");
            if (IsEquipped(Dungeon.hero))
            {
                desc += "\n\n";
                if (cooldown == 0)
                    desc += Messages.Get(this, "desc_inactive");
                else
                    desc += Messages.Get(this, "desc_active");
            }

            return desc;
        }

        public class Thorns : ArtifactBuff
        {
            public Thorns(Artifact artifact)
                : base(artifact)
            { }

            public override bool Act()
            {
                if (artifact.cooldown > 0)
                {
                    --artifact.cooldown;
                    if (artifact.cooldown == 0)
                        GLog.Warning(Messages.Get(this, "inert"));

                    Item.UpdateQuickslot();
                }
                Spend(TICK);
                return true;
            }

            public int Proc(int damage, Character attacker, Character defender)
            {
                if (artifact.cooldown == 0)
                {
                    artifact.charge += (int)(damage * (0.5 + artifact.GetLevel() * 0.05));
                    if (artifact.charge >= artifact.chargeCap)
                    {
                        artifact.charge = 0;
                        artifact.cooldown = 10 + artifact.GetLevel();
                        GLog.Positive(Messages.Get(this, "radiating"));
                    }
                }

                if (artifact.cooldown != 0)
                {
                    int deflected = Rnd.NormalIntRange(0, damage);
                    damage -= deflected;

                    if (attacker != null && Dungeon.level.Adjacent(attacker.pos, defender.pos))
                        attacker.Damage(deflected, this);

                    artifact.exp += deflected;

                    if (artifact.exp >= (artifact.GetLevel() + 1) * 5 && artifact.GetLevel() < artifact.levelCap)
                    {
                        artifact.exp -= (artifact.GetLevel() + 1) * 5;
                        artifact.Upgrade();
                        GLog.Positive(Messages.Get(this, "levelup"));
                    }
                }

                Item.UpdateQuickslot();
                return damage;
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", DispTurns(artifact.cooldown));
            }

            public override int Icon()
            {
                if (artifact.cooldown == 0)
                    return BuffIndicator.NONE;
                else
                    return BuffIndicator.THORNS;
            }

            public override void Detach()
            {
                artifact.cooldown = 0;
                artifact.charge = 0;
                base.Detach();
            }
        }
    }
}