namespace Disciples.Engine.Common.Models
{
    /// <summary>
    /// Информация о конкретном юните.
    /// </summary>
    public class Unit
    {
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
        /// Имя юнита.
        /// todo Герои могут иметь собственные имена.
        /// </summary>
        public string Name => UnitType.Name;

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
        public int HitPoints { get; set; }

        /// <summary>
        /// Максимальное количество очков здоровья.
        /// todo Рассчитывать, зависит от уровня и эффектов типа эликсира.
        /// </summary>
        public int MaxHitPoints => UnitType.HitPoints;

        /// <summary>
        /// Базовая броня юнита.
        /// </summary>
        public int BaseArmor => UnitType.Armor;

        /// <summary>
        /// Модификатор брони.
        /// todo Рассчитывать, зависит от эффектов.
        /// </summary>
        public int ArmorModifier => 0;

        /// <summary>
        /// Текущая броня юнита.
        /// </summary>
        public int Armor => BaseArmor + ArmorModifier;

        /// <summary>
        /// Базовое значение силы первой атаки.
        /// todo Рассчитывать, зависит от уровня.
        /// </summary>
        public int BaseFirstAttackPower => UnitType.MainAttack.HealPower > 0
            ? UnitType.MainAttack.HealPower
            : UnitType.MainAttack.DamagePower;

        /// <summary>
        /// Модификатор значения силы первой атаки.
        /// todo Рассчитывать, зависит от эффектов.
        /// </summary>
        public int FirstAttackPowerModifier => 0;

        /// <summary>
        /// Текущее значение силы первой атаки.
        /// </summary>
        public int FirstAttackPower => BaseFirstAttackPower + FirstAttackPowerModifier;

        /// <summary>
        /// Базовое значение силы второй атаки.
        /// todo Рассчитывать, зависит от уровня.
        /// </summary>
        /// <remarks>
        /// На вторую атаку модификаторы не распространяются.
        /// </remarks>
        public int? SecondAttackPower => UnitType.SecondaryAttack?.HealPower > 0
            ? UnitType.SecondaryAttack?.HealPower
            : UnitType.SecondaryAttack?.DamagePower;

        /// <summary>
        /// Базовое значение точности первой атаки.
        /// todo Рассчитывать, зависит от уровня.
        /// </summary>
        public int BaseFirstAttackAccuracy => UnitType.MainAttack.Accuracy;

        /// <summary>
        /// Модификатор точности первой атаки.
        /// todo Рассчитывать, зависит эффектов.
        /// </summary>
        public int FirstAttackAccuracyModifier => 0;

        /// <summary>
        /// Текущее значение точность первой атаки.
        /// </summary>
        public int MainAttackAccuracy => BaseFirstAttackAccuracy + FirstAttackAccuracyModifier;

        /// <summary>
        /// Значение точности второй атаки.
        /// todo Рассчитывать, зависит от уровня.
        /// </summary>
        /// <remarks>
        /// На вторую атаку модификаторы не распространяются.
        /// </remarks>
        public int? SecondaryAttackAccuracy => UnitType.SecondaryAttack?.Accuracy;

        /// <summary>
        /// Базовая инициатива.
        /// </summary>
        public int BaseInitiative => UnitType.MainAttack.Initiative;

        /// <summary>
        /// Модификатор инициативы.
        /// todo Рассчитывать, зависит от эффектов.
        /// </summary>
        public int InitiativeModifier => 0;

        /// <summary>
        /// Текущее значение инициативы.
        /// </summary>
        public int Initiative => BaseInitiative + InitiativeModifier;

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