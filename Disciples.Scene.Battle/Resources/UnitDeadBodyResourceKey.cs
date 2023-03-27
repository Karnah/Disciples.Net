using Disciples.Scene.Battle.Resources.Extensions;

namespace Disciples.Scene.Battle.Resources;

/// <summary>
/// Ключ для поиска изображения мёртвого тела на поле боя.
/// </summary>
internal class UnitDeadBodyResourceKey : BaseImageKey
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