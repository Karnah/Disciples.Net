using Disciples.Scene.Battle.Resources.ImageKeys.Extensions;

namespace Disciples.Scene.Battle.Resources.ImageKeys;

/// <summary>
/// Ключ для поиска изображения мёртвого тела на поле боя.
/// </summary>
internal class UnitDeadBodyResourceKey : BaseResourceKey
{
    /// <summary>
    /// Создать объект типа <see cref="UnitDeadBodyResourceKey" />.
    /// </summary>
    public UnitDeadBodyResourceKey(bool isSmall, int imageIndex)
    {
        if (imageIndex < 0 || imageIndex > 1)
            throw new ArgumentOutOfRangeException(nameof(imageIndex), "Индекс должен равняться 0 или 1");
        Key = $"DEAD_HUMAN_{isSmall.GetIsSmallResourceKey()}A{imageIndex:00}";
    }

    /// <inheritdoc />
    public override string Key { get; }
}