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
}