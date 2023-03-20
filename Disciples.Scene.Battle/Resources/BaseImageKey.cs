namespace Disciples.Scene.Battle.Resources;

/// <summary>
/// Ключ для поиска в ресурсах изображений.
/// </summary>
internal abstract class BaseImageKey
{
    /// <summary>
    /// Получить значение ключа.
    /// </summary>
    public abstract string Key { get; }
}