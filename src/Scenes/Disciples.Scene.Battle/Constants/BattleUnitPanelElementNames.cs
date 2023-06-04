namespace Disciples.Scene.Battle.Constants;

/// <summary>
/// Имена элементов на боковых панелях с юнитами на сцене.
/// </summary>
internal static class BattleUnitPanelElementNames
{
    /// <summary>
    /// Кнопка для инвертирования отряда на правой панели юнитов.
    /// </summary>
    /// <remarks>
    /// Кнопка для инвертирования левой отсутствует в ресурсах.
    /// </remarks>
    public const string RIGHT_UNIT_PANEL_REFLECT_TOGGLE_BUTTON = "TOG_RIGHTUNITS";

    /// <summary>
    /// Область, при наведении в которую выводится левая панель с юнитами.
    /// </summary>
    /// <remarks>
    /// Область для правой панели отсутствует в ресурсах.
    /// </remarks>
    public const string LEFT_PANEL_DISPLAY_PLACEHOLDER = "IMG_PUNITGROUP_SPOT";

    /// <summary>
    /// Изображение с левой панелью юнитов.
    /// </summary>
    public const string LEFT_UNIT_PANEL_IMAGE = "IMG_LUNITGROUPBG";

    /// <summary>
    /// Паттерн для поиска плейсхолдеров для портретов юнита на левой панели.
    /// </summary>
    /// <remarks>
    /// Пример: IMG_LSLOT1.
    /// </remarks>
    public const string LEFT_UNIT_PANEL_PORTRAIT_PATTERN_PLACEHOLDER = "IMG_LSLOT";

    /// <summary>
    /// Паттерн для поиска плейсхолдеров для ХП юнита на левой панели.
    /// </summary>
    /// <remarks>
    /// Пример: IMG_LMETER1.
    /// </remarks>
    public const string LEFT_UNIT_PANEL_HP_PATTERN_PLACEHOLDER = "IMG_LMETER";

    /// <summary>
    /// Изображение с правой панелью юнитов.
    /// </summary>
    public const string RIGHT_UNIT_PANEL_IMAGE = "IMG_RUNITGROUPBG";

    /// <summary>
    /// Паттерн для поиска плейсхолдеров для портретов юнита на левой панели.
    /// </summary>
    /// <remarks>
    /// Пример: IMG_RSLOT1.
    /// </remarks>
    public const string RIGHT_UNIT_PANEL_PORTRAIT_PATTERN_PLACEHOLDER = "IMG_RSLOT";

    /// <summary>
    /// Паттерн для поиска плейсхолдеров для ХП юнита на левой панели.
    /// </summary>
    /// <remarks>
    /// Пример: IMG_RMETER1.
    /// </remarks>
    public const string RIGHT_UNIT_PANEL_HP_PATTERN_PLACEHOLDER = "IMG_RMETER";
}