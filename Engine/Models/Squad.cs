namespace Engine.Models
{
    public class Squad
    {
        public Squad(Unit[] units)
        {
            Units = units;
        }


        public Unit[] Units { get; }
    }
}
