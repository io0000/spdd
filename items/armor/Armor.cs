using System;
using System.Linq;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.particles;
using spdd.actors.hero;
using spdd.sprites;
using spdd.utils;
using spdd.levels;
using spdd.actors;
using spdd.effects;
using spdd.items.armor.glyphs;
using spdd.items.armor.curses;
using spdd.actors.buffs;
using spdd.messages;

namespace spdd.items.armor
{
    public static class ArmorAugmentExtensions
    {
        public static int EvasionFactor(this Armor.Augment augment, int level)
        {
            float evasionFactor = 0.0f;
            switch (augment)
            {
                case Armor.Augment.EVASION:
                    evasionFactor = 2.0f;
                    break;
                case Armor.Augment.DEFENSE:
                    evasionFactor = -2.0f;
                    break;
                case Armor.Augment.NONE:
                    evasionFactor = 0.0f;
                    break;
            }

            return (int)Math.Round((2 + level) * evasionFactor, MidpointRounding.AwayFromZero);
        }

        public static int DefenseFactor(this Armor.Augment augment, int level)
        {
            float defenceFactor = 0.0f;
            switch (augment)
            {
                case Armor.Augment.EVASION:
                    defenceFactor = -1.0f;
                    break;
                case Armor.Augment.DEFENSE:
                    defenceFactor = 1.0f;
                    break;
                case Armor.Augment.NONE:
                    defenceFactor = 0.0f;
                    break;
            }

            return (int)Math.Round((2 + level) * defenceFactor, MidpointRounding.AwayFromZero);
        }
    }

    public class Armor : EquipableItem
    {
        protected const string AC_DETACH = "DETACH";

        public enum Augment
        {
            EVASION,
            DEFENSE,
            NONE
        }

        public Augment augment = Augment.NONE;

        public Glyph glyph;
        public bool curseInfusionBonus;

        private BrokenSeal seal;

        public int tier;

        private const int USES_TO_ID = 10;
        private int usesLeftToID = USES_TO_ID;
        private float availableUsesToID = USES_TO_ID / 2f;

        public Armor(int tier)
        {
            this.tier = tier;
        }

        private const string USES_LEFT_TO_ID = "uses_left_to_id";
        private const string AVAILABLE_USES = "available_uses";
        private const string GLYPH = "glyph";
        private const string CURSE_INFUSION_BONUS = "curse_infusion_bonus";
        private const string SEAL = "seal";
        private const string AUGMENT = "augment";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(USES_LEFT_TO_ID, usesLeftToID);
            bundle.Put(AVAILABLE_USES, availableUsesToID);
            bundle.Put(GLYPH, glyph);
            bundle.Put(CURSE_INFUSION_BONUS, curseInfusionBonus);
            bundle.Put(SEAL, seal);
            bundle.Put(AUGMENT, augment.ToString());    // enum
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            usesLeftToID = bundle.GetInt(USES_LEFT_TO_ID);
            availableUsesToID = bundle.GetInt(AVAILABLE_USES);
            Inscribe((Glyph)bundle.Get(GLYPH));
            curseInfusionBonus = bundle.GetBoolean(CURSE_INFUSION_BONUS);
            seal = (BrokenSeal)bundle.Get(SEAL);

            augment = bundle.GetEnum<Augment>(AUGMENT);
        }

        public override void Reset()
        {
            base.Reset();
            usesLeftToID = USES_TO_ID;
            availableUsesToID = USES_TO_ID / 2f;
            //armor can be kept in bones between runs, the seal cannot.
            seal = null;
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            if (seal != null)
                actions.Add(AC_DETACH);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_DETACH) && seal != null)
            {
                var sealBuff = hero.FindBuff<BrokenSeal.WarriorShield>();
                if (sealBuff != null)
                    sealBuff.SetArmor(null);

                if (seal.GetLevel() > 0)
                    Degrade();

                GLog.Information(Messages.Get(typeof(Armor), "detach_seal"));
                hero.sprite.Operate(hero.pos);
                if (!seal.Collect())
                    Dungeon.level.Drop(seal, hero.pos);

                seal = null;
            }
        }

        public override bool DoEquip(Hero hero)
        {
            Detach(hero.belongings.backpack);

            if (hero.belongings.armor == null || hero.belongings.armor.DoUnequip(hero, true, false))
            {
                hero.belongings.armor = this;

                cursedKnown = true;
                if (cursed)
                {
                    EquipCursed(hero);
                    GLog.Negative(Messages.Get(typeof(Armor), "equip_cursed"));
                }

                ((HeroSprite)hero.sprite).UpdateArmor();
                Activate(hero);

                hero.SpendAndNext(Time2Equip(hero));
                return true;

            }
            else
            {
                Collect(hero.belongings.backpack);
                return false;
            }
        }

        public override void Activate(Character ch)
        {
            if (seal != null)
                Buff.Affect<BrokenSeal.WarriorShield>(ch).SetArmor(this);
        }

        public void AffixSeal(BrokenSeal seal)
        {
            this.seal = seal;
            if (seal.GetLevel() > 0)
            {
                //doesn't trigger upgrading logic such as affecting curses/glyphs
                SetLevel(GetLevel() + 1);
                BadgesExtensions.ValidateItemLevelAquired(this);
            }

            if (IsEquipped(Dungeon.hero))
                Buff.Affect<BrokenSeal.WarriorShield>(Dungeon.hero).SetArmor(this);
        }

        public BrokenSeal CheckSeal()
        {
            return seal;
        }

        protected override float Time2Equip(Hero hero)
        {
            return 2 / hero.Speed();
        }

        public override bool DoUnequip(Hero hero, bool collect, bool single)
        {
            if (base.DoUnequip(hero, collect, single))
            {
                hero.belongings.armor = null;
                ((HeroSprite)hero.sprite).UpdateArmor();

                var sealBuff = hero.FindBuff<BrokenSeal.WarriorShield>();
                if (sealBuff != null)
                    sealBuff.SetArmor(null);

                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool IsEquipped(Hero hero)
        {
            return hero.belongings.armor == this;
        }

        public int DRMax()
        {
            return DRMax(BuffedLvl());
        }

        public virtual int DRMax(int lvl)
        {
            int max = tier * (2 + lvl) + augment.DefenseFactor(lvl);
            if (lvl > max)
            {
                return ((lvl - max) + 1) / 2;
            }
            else
            {
                return max;
            }
        }

        public int DRMin()
        {
            return DRMin(BuffedLvl());
        }

        public int DRMin(int lvl)
        {
            int max = DRMax(lvl);
            if (lvl >= max)
            {
                return (lvl - max);
            }
            else
            {
                return lvl;
            }
        }

        public float EvasionFactor(Character owner, float evasion)
        {
            if (HasGlyph(typeof(Stone), owner) && !((Stone)glyph).TestingEvasion())
                return 0;

            if (owner is Hero)
            {
                int aEnc = STRReq() - ((Hero)owner).GetSTR();
                if (aEnc > 0)
                    evasion /= (float)Math.Pow(1.5f, aEnc);

                Momentum momentum = owner.FindBuff<Momentum>();
                if (momentum != null)
                    evasion += momentum.EvasionBonus(Math.Max(0, -aEnc));
            }

            return evasion + augment.EvasionFactor(BuffedLvl());
        }

        public float SpeedFactor(Character owner, float speed)
        {
            if (owner is Hero)
            {
                int aEnc = STRReq() - ((Hero)owner).GetSTR();
                if (aEnc > 0)
                    speed /= (float)Math.Pow(1.2f, aEnc);
            }

            if (HasGlyph(typeof(Swiftness), owner))
            {
                bool enemyNear = false;
                PathFinder.BuildDistanceMap(owner.pos, Dungeon.level.passable, 2);
                foreach (var ch in Actor.Chars())
                {
                    if (PathFinder.distance[ch.pos] != int.MaxValue && owner.alignment != ch.alignment)
                    {
                        enemyNear = true;
                        break;
                    }
                }
                if (!enemyNear)
                    speed *= (1.2f + 0.04f * BuffedLvl());
            }
            else if (HasGlyph(typeof(Flow), owner) && Dungeon.level.water[owner.pos])
            {
                speed *= (2f + 0.25f * BuffedLvl());
            }

            if (HasGlyph(typeof(Bulk), owner) &&
                (Dungeon.level.map[owner.pos] == Terrain.DOOR ||
                Dungeon.level.map[owner.pos] == Terrain.OPEN_DOOR))
            {
                speed /= 3f;
            }

            return speed;
        }

        public float StealthFactor(Character owner, float stealth)
        {
            if (HasGlyph(typeof(Obfuscation), owner))
            {
                stealth += 1 + BuffedLvl() / 3f;
            }

            return stealth;
        }

        public override int GetLevel()
        {
            return base.GetLevel() + (curseInfusionBonus ? 1 : 0);
        }

        //other things can equip these, for now we assume only the hero can be affected by levelling debuffs
        public override int BuffedLvl()
        {
            if (IsEquipped(Dungeon.hero) || Dungeon.hero.belongings.Contains(this))
            {
                return base.BuffedLvl();
            }
            else
            {
                return GetLevel();
            }
        }

        public override Item Upgrade()
        {
            return Upgrade(false);
        }

        public Item Upgrade(bool inscribe)
        {
            if (inscribe && (glyph == null || glyph.Curse()))
            {
                Inscribe(Glyph.Random());
            }
            else if (!inscribe && GetLevel() >= 4 && Rnd.Float(10) < Math.Pow(2, GetLevel() - 4))
            {
                Inscribe(null);
            }

            cursed = false;

            if (seal != null && seal.GetLevel() == 0)
                seal.Upgrade();

            return base.Upgrade();
        }

        public int Proc(Character attacker, Character defender, int damage)
        {
            if (glyph != null && defender.FindBuff<MagicImmune>() == null)
            {
                damage = glyph.Proc(this, attacker, defender, damage);
            }

            if (!levelKnown && defender == Dungeon.hero && availableUsesToID >= 1)
            {
                --availableUsesToID;
                --usesLeftToID;
                if (usesLeftToID <= 0)
                {
                    Identify();
                    GLog.Positive(Messages.Get(typeof(Armor), "identify"));
                    BadgesExtensions.ValidateItemLevelAquired(this);
                }
            }

            return damage;
        }

        public override void OnHeroGainExp(float levelPercent, Hero hero)
        {
            if (!levelKnown && IsEquipped(hero) && availableUsesToID <= USES_TO_ID / 2f)
            {
                //gains enough uses to ID over 0.5 levels
                availableUsesToID = Math.Min(USES_TO_ID / 2f, availableUsesToID + levelPercent * USES_TO_ID);
            }
        }

        public override string Name()
        {
            return glyph != null && (cursedKnown || !glyph.Curse()) ? glyph.Name(base.Name()) : base.Name();
        }

        public override string Info()
        {
            string info = Desc();

            if (levelKnown)
            {
                info += "\n\n" + Messages.Get(typeof(Armor), "curr_absorb", DRMin(), DRMax(), STRReq());

                if (STRReq() > Dungeon.hero.GetSTR())
                {
                    info += " " + Messages.Get(typeof(Armor), "too_heavy");
                }
            }
            else
            {
                info += "\n\n" + Messages.Get(typeof(Armor), "avg_absorb", DRMin(0), DRMax(0), STRReq(0));

                if (STRReq(0) > Dungeon.hero.GetSTR())
                {
                    info += " " + Messages.Get(typeof(Armor), "probably_too_heavy");
                }
            }

            switch (augment)
            {
                case Augment.EVASION:
                    info += "\n\n" + Messages.Get(typeof(Armor), "evasion");
                    break;
                case Augment.DEFENSE:
                    info += "\n\n" + Messages.Get(typeof(Armor), "defense");
                    break;
                case Augment.NONE:
                    break;
            }

            if (glyph != null && (cursedKnown || !glyph.Curse()))
            {
                info += "\n\n" + Messages.Get(typeof(Armor), "inscribed", glyph.Name());
                info += " " + glyph.Desc();
            }

            if (cursed && IsEquipped(Dungeon.hero))
            {
                info += "\n\n" + Messages.Get(typeof(Armor), "cursed_worn");
            }
            else if (cursedKnown && cursed)
            {
                info += "\n\n" + Messages.Get(typeof(Armor), "cursed");
            }
            else if (seal != null)
            {
                info += "\n\n" + Messages.Get(typeof(Armor), "seal_attached");
            }
            else if (!IsIdentified() && cursedKnown)
            {
                info += "\n\n" + Messages.Get(typeof(Armor), "not_cursed");
            }

            return info;
        }

        public override Emitter Emitter()
        {
            if (seal == null)
                return base.Emitter();
            var emitter = new Emitter();
            emitter.Pos(ItemSpriteSheet.film.Width(image) / 2f + 2f, ItemSpriteSheet.film.Height(image) / 3f);
            emitter.fillTarget = false;
            emitter.Pour(Speck.Factory(Speck.RED_LIGHT), 0.6f);
            return emitter;
        }

        public override Item Random()
        {
            //+0: 75% (3/4)
            //+1: 20% (4/20)
            //+2: 5%  (1/20)
            int n = 0;
            if (Rnd.Int(4) == 0)
            {
                ++n;
                if (Rnd.Int(5) == 0)
                    ++n;
            }
            SetLevel(n);

            //30% chance to be cursed
            //15% chance to be inscribed
            float effectRoll = Rnd.Float();
            if (effectRoll < 0.3f)
            {
                Inscribe(Glyph.RandomCurse());
                cursed = true;
            }
            else if (effectRoll >= 0.85f)
            {
                Inscribe();
            }

            return this;
        }

        public int STRReq()
        {
            return STRReq(GetLevel());
        }

        public virtual int STRReq(int lvl)
        {
            lvl = Math.Max(0, lvl);

            //strength req decreases at +1,+3,+6,+10,etc.
            //return (8 + Math.Round(tier * 2)) - (int)(Math.Sqrt(8 * lvl + 1) - 1) / 2;
            return (8 + (tier * 2)) - (int)(Math.Sqrt(8 * lvl + 1) - 1) / 2;
        }

        public override int Value()
        {
            if (seal != null)
                return 0;

            int price = 20 * tier;
            if (HasGoodGlyph())
                price = (int)(price * 1.5f);

            if (cursedKnown && (cursed || HasCurseGlyph()))
                price /= 2;

            if (levelKnown && GetLevel() > 0)
                price *= (GetLevel() + 1);

            if (price < 1)
                price = 1;

            return price;
        }

        public Armor Inscribe(Glyph glyph)
        {
            if (glyph == null || !glyph.Curse())
                curseInfusionBonus = false;
            this.glyph = glyph;
            UpdateQuickslot();
            return this;
        }

        public Armor Inscribe()
        {
            Type oldGlyphClass = glyph != null ? glyph.GetType() : null;
            Glyph gl = Glyph.Random(oldGlyphClass);

            return Inscribe(gl);
        }

        public bool HasGlyph(Type type, Character owner)
        {
            return glyph != null && glyph.GetType() == type && owner.FindBuff<MagicImmune>() == null;
        }

        //these are not used to process specific glyph effects, so magic immune doesn't affect them
        public bool HasGoodGlyph()
        {
            return glyph != null && !glyph.Curse();
        }

        public bool HasCurseGlyph()
        {
            return glyph != null && glyph.Curse();
        }

        public override ItemSprite.Glowing Glowing()
        {
            return glyph != null && (cursedKnown || !glyph.Curse()) ?
                glyph.Glowing() : null;
        }

        public abstract class Glyph : IBundlable
        {
            private static Type[] common = new Type[]
            {
                typeof(Obfuscation),
                typeof(Swiftness),
                typeof(Viscosity),
                typeof(Potential)
            };

            private static Type[] uncommon = new Type[]
            {
                typeof(Brimstone),
                typeof(Stone),
                typeof(Entanglement),
                typeof(Repulsion),
                typeof(Camouflage),
                typeof(Flow)
            };

            private static Type[] rare = new Type[]
            {
                typeof(Affection),
                typeof(AntiMagic),
                typeof(Thorns)
            };

            private static float[] typeChances =
            {
                50, //12.5% each
                40, //6.67% each
                10  //3.33% each
            };

            private static Type[] curses = new Type[]
            {
                typeof(AntiEntropy),
                typeof(curses.Corrosion),
                typeof(Displacement),
                typeof(Metabolism),
                typeof(Multiplicity),
                typeof(Stench),
                typeof(Overgrowth),
                typeof(Bulk)
            };

            public abstract int Proc(Armor armor, Character attacker, Character defender, int damage);

            public string Name()
            {
                if (!Curse())
                    return Name(Messages.Get(this, "glyph"));
                else
                    return Name(Messages.Get(typeof(Item), "curse"));
            }

            public string Name(string armorName)
            {
                return Messages.Get(this, "name", armorName);
            }

            public string Desc()
            {
                return Messages.Get(this, "desc");
            }

            public virtual bool Curse()
            {
                return false;
            }

            public void RestoreFromBundle(Bundle bundle)
            { }

            public void StoreInBundle(Bundle bundle)
            { }

            public abstract ItemSprite.Glowing Glowing();

            public static Glyph Random(params Type[] toIgnore)
            {
                switch (Rnd.Chances(typeChances))
                {
                    case 0:
                    default:
                        return RandomCommon(toIgnore);
                    case 1:
                        return RandomUncommon(toIgnore);
                    case 2:
                        return RandomRare(toIgnore);
                }
            }

            public static Glyph RandomCommon(params Type[] toIgnore)
            {
                return RandomHelper(common.ToList(), toIgnore.ToList());
            }

            public static Glyph RandomUncommon(params Type[] toIgnore)
            {
                return RandomHelper(uncommon.ToList(), toIgnore.ToList());
            }

            public static Glyph RandomRare(params Type[] toIgnore)
            {
                return RandomHelper(rare.ToList(), toIgnore.ToList());
            }

            public static Glyph RandomHelper(List<Type> list, List<Type> toIgnore)
            {
                var glyphs = list.Except(toIgnore).ToList();

                if (glyphs.Count == 0)
                {
                    return Random();
                }
                else
                {
                    return (Glyph)Reflection.NewInstance(Rnd.Element(glyphs));
                }
            }

            public static Glyph RandomCurse(params Type[] toIgnore)
            {
                return RandomHelper(curses.ToList(), toIgnore.ToList());
            }
        }
    }
}