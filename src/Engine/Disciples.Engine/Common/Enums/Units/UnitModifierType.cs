namespace Disciples.Engine.Common.Enums.Units;

/// <summary>
/// Тип модификатора характеристики или способности юнита.
/// </summary>
public enum UnitModifierType
{
    /// <summary>
    /// Модификатор для усиления и ослабления живучести (защиты от атак, броня и здоровья).
    /// </summary>
    Protection = 0,

    /// <summary>
    /// Модификаторы для лидера отряда (его способности, скорость передвижения).
    /// </summary>
    LeaderAbility = 1,

    /// <summary>
    /// Модификаторы для усиления и ослабления урона/инициативы/меткости.
    /// </summary>
    Attack = 3
}