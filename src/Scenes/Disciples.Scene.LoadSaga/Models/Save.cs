namespace Disciples.Scene.LoadSaga.Models;

/// <summary>
/// Сохранённая игра пользователя.
/// </summary>
internal class Save
{
    /// <summary>
    /// Имя сейва.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Путь до сейва.
    /// </summary>
    public string Path { get; init; } = null!;
}