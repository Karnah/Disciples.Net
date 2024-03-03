namespace Disciples.Engine.Settings;

/// <summary>
/// Настройки игры.
/// </summary>
public class GameSettings
{
    /// <summary>
    /// Строка подключения к БД.
    /// </summary>
    public string DatabaseConnection { get; init; } = null!;

    /// <summary>
    /// Путь, где лежат сейвы пользователя.
    /// </summary>
    public string SavesFolder { get; init; } = null!;

    /// <summary>
    /// Выключить сломанные анимации перехода между страницами.
    /// </summary>
    /// <remarks>
    /// Контролы, которые показывают анимацию перехода между страницами, перед показом всегда отображает черный экран.
    /// Из-за этого такие переходы выглядят очень дёрганными.
    /// </remarks>
    public bool IsBrokenTransitionAnimationsDisabled { get; init; }

    /// <summary>
    /// Скрыть кнопки, обработчики которых еще не реализованы.
    /// </summary>
    public bool IsUselessButtonsHidden { get; init; }
}