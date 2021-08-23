using System.Collections.Generic;
using System.IO;
using watabou.noosa;
using spdd.actors;
using spdd.actors.hero;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items
{
    public class Amulet : Item
    {
        private const string AC_END = "END";

        public Amulet()
        {
            image = ItemSpriteSheet.AMULET;

            unique = true;
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            actions.Add(AC_END);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action == AC_END)
                ShowAmuletScene(false);
        }

        public override bool DoPickUp(Hero hero)
        {
            if (!base.DoPickUp(hero))
                return false;

            if (Statistics.amuletObtained)
                return true;

            Statistics.amuletObtained = true;
            BadgesExtensions.ValidateVictory();
            hero.Spend(-TIME_TO_PICK_UP);

            Actor.AddDelayed(new AmuletActor(this), -5);

            return true;
        }

        private class AmuletActor : Actor
        {
            Amulet amulet;

            public AmuletActor(Amulet amulet)
            {
                this.amulet = amulet;
            }
            public override bool Act()
            {
                Actor.Remove(this);
                amulet.ShowAmuletScene(true);
                return false;
            }
        }

        private void ShowAmuletScene(bool showText)
        {
            try
            {
                Dungeon.SaveAll();
                AmuletScene.noText = !showText;
                Game.SwitchScene(typeof(AmuletScene));
            }
            catch (IOException e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
            }
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override bool IsUpgradable()
        {
            return false;
        }
    }
}