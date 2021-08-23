using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.items.rings;
using spdd.mechanics;
using spdd.sprites;
using spdd.effects;
using spdd.utils;
using spdd.ui;
using spdd.scenes;
using spdd.actors;
using spdd.messages;
using spdd.tiles;

namespace spdd.items.artifacts
{
    public class EtherealChains : Artifact
    {
        public const string AC_CAST = "CAST";

        public EtherealChains()
        {
            image = ItemSpriteSheet.ARTIFACT_CHAINS;

            levelCap = 5;
            exp = 0;

            charge = 5;

            defaultAction = AC_CAST;
            usesTargeting = true;

            caster = new Caster(this);
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            if (IsEquipped(hero) && charge > 0 && !cursed)
                actions.Add(AC_CAST);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_CAST))
            {
                curUser = hero;

                //tt
                //charge += 20;

                if (!IsEquipped(hero))
                {
                    GLog.Information(Messages.Get(typeof(Artifact), "need_to_equip"));
                    QuickSlotButton.Cancel();
                }
                else if (charge < 1)
                {
                    GLog.Information(Messages.Get(this, "no_charge"));
                    QuickSlotButton.Cancel();
                }
                else if (cursed)
                {
                    GLog.Warning(Messages.Get(this, "cursed"));
                    QuickSlotButton.Cancel();
                }
                else
                {
                    GameScene.SelectCell(caster);
                }
            }
        }


        // [FIXED] 
        // A와 B가 다른 결과가 나옴
        //    A. 퀵슬롯 touch -> 퀵슬롯 touch
        //    B. 퀵슬롯 touch -> 대상 touch
        //    퀵슬롯에서는 AutoAim() 함수를 사용
        //    AutoAim() 함수 내부에서 Ballistic를 사용할 때 검사옵션이 ( STOP_TARGET | STOP_CHARS | STOP_TERRAIN ) 라서 문제
        //    위와 같은 이유로 ThrowPos를 override해서 문제로직을 skip
        public override int ThrowPos(Hero user, int dst)
        {
            //return new Ballistic(user.pos, dst, Ballistica.PROJECTILE).collisionPos;
            // PROJECTILE - STOP_TARGET | STOP_CHARS | STOP_TERRAIN
            return dst;
        }

        public void OnSelect(int target)
        {
            if (Dungeon.level.visited[target] || Dungeon.level.mapped[target])
            {
                //chains cannot be used to go where it is impossible to walk to
                PathFinder.BuildDistanceMap(target, BArray.Or(Dungeon.level.passable, Dungeon.level.avoid, null));
                if (PathFinder.distance[curUser.pos] == int.MaxValue)
                {
                    GLog.Warning(Messages.Get(typeof(EtherealChains), "cant_reach"));
                    return;
                }

                Ballistic chain = new Ballistic(curUser.pos, target, Ballistic.STOP_TARGET);
                var collisionPos = chain.collisionPos;

                var ch = Actor.FindChar(collisionPos);
                if (ch != null)
                {
                    ChainEnemy(chain, curUser, ch);
                }
                else
                {
                    ChainLocation(chain, curUser);
                }

                ThrowSound();
                Sample.Instance.Play(Assets.Sounds.CHAINS);
            }
        }

        private Caster caster;

        public class Caster : CellSelector.IListener
        {
            EtherealChains chains;

            public Caster(EtherealChains chains)
            {
                this.chains = chains;
            }

            public void OnSelect(int? target)
            {
                if (target == null)
                    return;
                chains.OnSelect(target.Value);
            }

            public string Prompt()
            {
                return Messages.Get(typeof(EtherealChains), "prompt");
            }
        }

        //pulls an enemy to a position along the chain's path, as close to the hero as possible
        private void ChainEnemy(Ballistic chain, Hero hero, Character enemy)
        {
            if (enemy.Properties().Contains(Character.Property.IMMOVABLE))
            {
                GLog.Warning(Messages.Get(this, "cant_pull"));
                return;
            }

            int bestPos = -1;
            foreach (int i in chain.SubPath(1, chain.dist))
            {
                //prefer to the earliest point on the path
                if (!Dungeon.level.solid[i] &&
                    Actor.FindChar(i) == null &&
                    (!Character.HasProp(enemy, Character.Property.LARGE) || Dungeon.level.openSpace[i]))
                {
                    bestPos = i;
                    break;
                }
            }

            if (bestPos == -1)
            {
                GLog.Information(Messages.Get(this, "does_nothing"));
                return;
            }

            int pulledPos = bestPos;

            int chargeUse = Dungeon.level.Distance(enemy.pos, pulledPos);
            if (chargeUse > charge)
            {
                GLog.Warning(Messages.Get(this, "no_charge"));
                return;
            }
            else
            {
                charge -= chargeUse;
                UpdateQuickslot();
            }

            var callbackChain = new ActionCallback();
            callbackChain.action = () =>
            {
                var callbackPushing = new ActionCallback();
                callbackPushing.action = () => Dungeon.level.OccupyCell(enemy);

                Actor.Add(new Pushing(enemy, enemy.pos, pulledPos, callbackPushing));
                enemy.pos = pulledPos;
                Dungeon.Observe();
                GameScene.UpdateFog();
                hero.SpendAndNext(1f);
            };

            hero.Busy();
            hero.sprite.parent.Add(new Chains(
                hero.sprite.Center(),
                enemy.sprite.Center(),
                callbackChain));
        }

        private void ChainLocation(Ballistic chain, Hero hero)
        {
            var collisionPos = chain.collisionPos;

            //don't pull if the collision spot is in a wall
            if (Dungeon.level.solid[collisionPos])
            {
                GLog.Information(Messages.Get(this, "inside_wall"));
                return;
            }

            //don't pull if there are no solid objects next to the pull location
            bool solidFound = false;
            foreach (int i in PathFinder.NEIGHBORS8)
            {
                if (Dungeon.level.solid[collisionPos + i])
                {
                    solidFound = true;
                    break;
                }
            }
            if (!solidFound)
            {
                GLog.Information(Messages.Get(typeof(EtherealChains), "nothing_to_grab"));
                return;
            }

            int newHeroPos = collisionPos;

            int chargeUse = Dungeon.level.Distance(hero.pos, newHeroPos);
            if (chargeUse > charge)
            {
                GLog.Warning(Messages.Get(typeof(EtherealChains), "no_charge"));
                return;
            }
            else
            {
                charge -= chargeUse;
                UpdateQuickslot();
            }

            var callback = new ActionCallback();
            callback.action = () =>
            {
                var callbackPushing = new ActionCallback();
                callbackPushing.action = () => Dungeon.level.OccupyCell(hero);

                Actor.Add(new Pushing(hero, hero.pos, newHeroPos, callbackPushing));
                hero.SpendAndNext(1f);
                hero.pos = newHeroPos;
                Dungeon.Observe();
                GameScene.UpdateFog();
            };

            var chains = new Chains(hero.sprite.Center(),
                DungeonTilemap.RaisedTileCenterToWorld(newHeroPos),
                callback);

            hero.Busy();
            hero.sprite.parent.Add(chains);
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new ChainsRecharge(this);
        }

        public override void Charge(Hero target)
        {
            int chargeTarget = 5 + (GetLevel() * 2);
            if (charge < chargeTarget * 2)
            {
                partialCharge += 0.5f;
                if (partialCharge >= 1)
                {
                    --partialCharge;
                    ++charge;
                    UpdateQuickslot();
                }
            }
        }

        public override string Desc()
        {
            string desc = base.Desc();

            if (IsEquipped(Dungeon.hero))
            {
                desc += "\n\n";
                if (cursed)
                    desc += Messages.Get(this, "desc_cursed");
                else
                    desc += Messages.Get(this, "desc_equipped");
            }
            return desc;
        }

        public bool ChainsRechargeAct(ArtifactBuff buff)
        {
            var target = buff.target;

            int chargeTarget = 5 + (GetLevel() * 2);
            var lockedFloor = target.FindBuff<LockedFloor>();
            if (charge < chargeTarget && !cursed && (lockedFloor == null || lockedFloor.RegenOn()))
            {
                //gains a charge in 40 - 2*missingCharge turns
                float chargeGain = (1 / (40f - (chargeTarget - charge) * 2f));
                chargeGain *= RingOfEnergy.ArtifactChargeMultiplier(target);
                partialCharge += chargeGain;
            }
            else if (cursed && Rnd.Int(100) == 0)
            {
                Buff.Prolong<Cripple>(target, 10f);
            }

            if (partialCharge >= 1)
            {
                --partialCharge;
                ++charge;
            }

            UpdateQuickslot();

            buff.Spend(Actor.TICK);

            return true;
        }

        public void GainExp(float levelPortion)
        {
            if (cursed || levelPortion == 0)
                return;

            exp += (int)Math.Round(levelPortion * 100, MidpointRounding.AwayFromZero);

            //past the soft charge cap, gaining  charge from leveling is slowed.
            if (charge > 5 + (GetLevel() * 2))
                levelPortion *= (5 + ((float)GetLevel() * 2)) / charge;

            partialCharge += levelPortion * 10f;

            if (exp > 100 + GetLevel() * 100 && GetLevel() < levelCap)
            {
                exp -= 100 + GetLevel() * 100;
                GLog.Positive(Messages.Get(typeof(ChainsRecharge), "levelup"));
                Upgrade();
            }
        }

        public class ChainsRecharge : ArtifactBuff
        {
            public ChainsRecharge(Artifact artifact)
                : base(artifact)
            { }

            public override bool Act()
            {
                var chains = (EtherealChains)artifact;

                return chains.ChainsRechargeAct(this);
            }

            public void GainExp(float levelPortion)
            {
                var chains = (EtherealChains)artifact;

                chains.GainExp(levelPortion);
            }
        }
    }
}