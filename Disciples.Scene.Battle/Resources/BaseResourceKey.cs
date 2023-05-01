namespace Disciples.Scene.Battle.Resources;

/// <summary>
/// Ключ для поиска в ресурсах.
/// </summary>
internal abstract class BaseResourceKey
{
    /// <summary>
    /// Получить значение ключа.
    /// </summary>
    public abstract string Key { get; }
}