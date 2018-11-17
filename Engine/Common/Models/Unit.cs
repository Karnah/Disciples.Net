using ReactiveUI;

namespace Engine.Common.Models
{
    /// <summary>
    /// Информация о конкретном юните.
    /// </summary>
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
            Effects = new UnitEffects();
        }


        /// <summary>
        /// Уникальный идентификатор юнита.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Тип юнита.
        /// </summary>
        public UnitType UnitType { get; }

        /// <summary>
        /// Игрок, который управляет юнитом.
        /// </summary>
        public Player Player { get; }


        /// <summary>
        /// На какой линии располагается юнит в отряде.
        /// </summary>
        public int SquadLinePosition { get; set; }

        /// <summary>
        /// На какой позиции находится юнит в отряде.
        /// </summary>
        public int SquadFlankPosition { get; set; }


        /// <summary>
        /// Уровень юнита.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Накопленный за уровень опыт.
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// Количество оставшихся очков здоровья.
        /// </summary>
        public int HitPoints {
            get => _hitPoints;
            private set => this.RaiseAndSetIfChanged(ref _hitPoints, value);
        }

        /// <summary>
        /// Мёртв ли юнит.
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// Эффекты, воздействующие на юнита.
        /// </summary>
        public UnitEffects Effects { get; }


        /// <summary>
        /// Изменить текущее количество очков здоровья у юнита.
        /// </summary>
        /// <param name="value">На какое количество необходимо изменить.</param>
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
