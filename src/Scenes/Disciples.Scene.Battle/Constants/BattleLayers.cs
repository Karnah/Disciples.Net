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
    /// Базовый слой для юнитов.
    /// </summary>
    /// <remarks>
    /// Юниты используют много слоёв, в зависимости от положения в отряде.
    /// </remarks>
    public const int UNIT_BASE_LAYER = 100;

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
    public const int INTERFACE_ANIMATION_LAYER = 1010;
}