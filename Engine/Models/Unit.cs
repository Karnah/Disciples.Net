namespace Engine.Models
{
    public class Unit
    {
        public Unit(string id, UnitType unitType, int squadLinePosition, int squadFlankPosition)
        {
            Id = id;
            UnitType = unitType;
            SquadLinePosition = squadLinePosition;
            SquadFlankPosition = squadFlankPosition;
        }


        public string Id { get; }

        public UnitType UnitType { get; }


        public int SquadLinePosition { get; }

        public int SquadFlankPosition { get; }
    }
}
