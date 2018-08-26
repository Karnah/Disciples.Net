using ReactiveUI;

namespace Engine.Common.Models
{
    public class Unit : ReactiveObject
    {
        private int _hitPoints;

        public Unit(string id, UnitType unitType, Player player, int squadLinePosition, int squadFlankPosition)
        {
            Id = id;
            UnitType = unitType;
            Player = player;

            SquadLinePosition = squadLinePosition;
            SquadFlankPosition = squadFlankPosition;

            Level = UnitType.Level;
            Experience = 0;
            HitPoints = UnitType.HitPoints;
        }


        public string Id { get; }

        public UnitType UnitType { get; }

        public Player Player { get; }


        public int SquadLinePosition { get; set; }

        public int SquadFlankPosition { get; set; }


        public int Level { get; set; }

        public int Experience { get; set; }

        public int HitPoints {
            get => _hitPoints;
            private set => this.RaiseAndSetIfChanged(ref _hitPoints, value);
        }

        public bool IsDead { get; set; }



        public void ChangeHitPoints(int value)
        {
            var hitpoints = HitPoints + value;
            if (hitpoints > UnitType.HitPoints) {
                HitPoints = UnitType.HitPoints;
            }
            else if (hitpoints < 0) {
                HitPoints = 0;
            }
            else {
                HitPoints = hitpoints;
            }
        }
    }
}
