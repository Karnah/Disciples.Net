namespace Disciples.Scene.Battle.Resources;

/// <summary>
/// Статический ключ.
/// </summary>
internal class StaticResourceKey : BaseResourceKey
{
    /// <summary>
    /// Создать объект типа <see cref="StaticResourceKey" />.
    /// </summary>
    public StaticResourceKey(string key)
    {
        Key = key;
    }

    /// <inheritdoc />
    public override string Key { get; }
}