using Disciples.Engine.Common;
using Disciples.Engine.Common.Models;

namespace Disciples.Scene.LoadSaga.Models;

/// <summary>
/// Сохранённая игра пользователя.
/// </summary>
internal class Save : TextListBoxItem
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