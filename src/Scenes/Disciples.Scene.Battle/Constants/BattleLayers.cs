namespace Disciples.Scene.Battle.Constants;

/// <summary>
/// Номера слоёв.
/// </summary>
internal static class BattleLayers
{
    /// <summary>
    /// Слой для расположения фона.
    /// </summary>
    public const int BACKGROUND_LAYER = 0;

    /// <summary>
    /// Слой, на котором располагаются анимации выделения юнита.
    /// </summary>
    public const int UNIT_SELECTION_ANIMATION_LAYER = 1;

    /// <summary>
    /// Базовый слой для юнитов из атакующего отряда.
    /// </summary>
    /// <remarks>
    /// Юниты используют много слоёв, в зависимости от положения в отряде.
    /// </remarks>
    public const int ATTACKER_UNIT_BASE_LAYER = 100;

    /// <summary>
    /// Слой, который перекрывает всех атакующих юнитов.
    /// </summary>
    public const int ABOVE_ALL_ATTACKER_UNITS_LAYER = 400;

    /// <summary>
    /// Базовый слой для юнитов из защищающегося отряда.
    /// </summary>
    /// <remarks>
    /// Юниты используют много слоёв, в зависимости от положения в отряде.
    /// </remarks>
    public const int DEFENDER_UNIT_BASE_LAYER = 500;

    /// <summary>
    /// Слой, который перекрывает всех защищающихся юнитов.
    /// </summary>
    public const int ABOVE_ALL_DEFENDER_UNITS_LAYER = 900;

    /// <summary>
    /// Слой для расположения изображений панелей.
    /// </summary>
    public const int PANEL_LAYER = 1000;

    /// <summary>
    /// Слой для расположения игровых объектов (кнопки, портреты), связанных с интерфейсом.
    /// </summary>
    public const int INTERFACE_LAYER = 1010;

    /// <summary>
    /// Слой для расположения анимаций, связанных с интерфейсом.
    /// </summary>
    public const int INTERFACE_ANIMATION_LAYER = 1020;
}