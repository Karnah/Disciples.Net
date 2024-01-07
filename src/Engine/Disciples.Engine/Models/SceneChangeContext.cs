namespace Disciples.Engine.Models;

/// <summary>
/// Информация о смене сцены.
/// </summary>
public class SceneChangeContext
{
    /// <summary>
    /// Название предыдущей сцены.
    /// </summary>
    public string PreviousSceneName { get; init; } = null!;
}