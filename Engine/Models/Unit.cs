namespace Engine.Models
{
    public class Unit
    {
        public Unit(string id, UnitType unitType, Player player, int squadLinePosition, int squadFlankPosition)
        {
            Id = id;
            UnitType = unitType;
            Player = player;

            SquadLinePosition = squadLinePosition;
            SquadFlankPosition = squadFlankPosition;
        }


        public string Id { get; }

        public UnitType UnitType { get; }

        public Player Player { get; }


        public int SquadLinePosition { get; }

        public int SquadFlankPosition { get; }
    }
}
