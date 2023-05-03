using Disciples.Engine.Common.Enums;
using Disciples.Scene.Battle.Resources.ImageKeys.Extensions;

namespace Disciples.Scene.Battle.Resources.ImageKeys;

/// <summary>
/// Ключ для поиска анимации смерти юнита на поле боя.
/// </summary>
internal class UnitDeathAnimationResourceKey : BaseResourceKey
{
    /// <summary>
    /// Создать объект типа <see cref="UnitDeathAnimationResourceKey" />.
    /// </summary>
    public UnitDeathAnimationResourceKey(UnitDeathAnimationType animationType)
    {
        Key = animationType.GetResourceKey();
    }

    /// <inheritdoc />
    public override string Key { get; }
}