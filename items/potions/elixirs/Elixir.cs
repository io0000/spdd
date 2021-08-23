namespace spdd.items.potions.elixirs
{
    //public abstract class Elixir : Potion

    public class Elixir : Potion
    {
        //public abstract void Apply(Hero hero);

        public override bool IsKnown()
        {
            return true;
        }
    }
}