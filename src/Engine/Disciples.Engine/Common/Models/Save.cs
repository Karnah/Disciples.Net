namespace Disciples.Engine.Common.Models;

/// <summary>
/// Сохранённая игра пользователя.
/// </summary>
public class Save : TextListBoxItem
{
    /// <summary>
    /// Имя сейва.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Путь до сейва.
    /// </summary>
    public string Path { get; init; } = null!;

    /// <summary>
    /// Данные игры.
    /// </summary>
    /// <remarks>
    /// TODO Использовать более лёгкую модель.
    /// </remarks>
    public GameContext GameContext { get; init; } = null!;
}