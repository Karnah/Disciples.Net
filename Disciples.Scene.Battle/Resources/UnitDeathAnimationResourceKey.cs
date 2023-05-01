using Disciples.Engine.Common.Enums;
using Disciples.Scene.Battle.Resources.Extensions;

namespace Disciples.Scene.Battle.Resources;

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