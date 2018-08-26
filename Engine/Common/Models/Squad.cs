namespace Engine.Common.Models
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
