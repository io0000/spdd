using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.mechanics;
using spdd.scenes;
using spdd.effects;
using spdd.ui;
using spdd.messages;

namespace spdd.items.spells
{
    public abstract class TargetedSpell : Spell
    {
        protected int collisionProperties = Ballistic.PROJECTILE;

        protected override void OnCast(Hero hero)
        {
            GameScene.SelectCell(targeter);
        }

        protected abstract void AffectTarget(Ballistic bolt, Hero hero);

        protected virtual void Fx(Ballistic bolt, ICallback callback)
        {
            MagicMissile.BoltFromChar(curUser.sprite.parent,
                    MagicMissile.MAGIC_MISSILE,
                    curUser.sprite,
                    bolt.collisionPos,
                    callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        private Targeter targeter = new Targeter();

        class Targeter : CellSelector.IListener
        {
            public void OnSelect(int? t)
            {
                if (t != null)
                {
                    int target = t.Value;
                    //FIXME this safety check shouldn't be necessary
                    //it would be better to eliminate the curItem static variable.
                    TargetedSpell curSpell;
                    if (curItem is TargetedSpell)
                    {
                        curSpell = (TargetedSpell)curItem;
                    }
                    else
                    {
                        return;
                    }

                    Ballistic shot = new Ballistic(curUser.pos, target, curSpell.collisionProperties);
                    int cell = shot.collisionPos;

                    curUser.sprite.Zap(cell);

                    //attempts to target the cell aimed at if something is there, otherwise targets the collision pos.
                    if (Actor.FindChar(target) != null)
                        QuickSlotButton.Target(Actor.FindChar(target));
                    else
                        QuickSlotButton.Target(Actor.FindChar(cell));

                    curUser.Busy();

                    var callback = new ActionCallback();
                    callback.action = () =>
                    {
                        curSpell.AffectTarget(shot, curUser);
                        curSpell.Detach(curUser.belongings.backpack);
                        Invisibility.Dispel();
                        UpdateQuickslot();
                        curUser.SpendAndNext(1f);
                    };

                    curSpell.Fx(shot, callback);
                }
            }

            public string Prompt()
            {
                return Messages.Get(typeof(TargetedSpell), "prompt");
            }
        }
    }
}