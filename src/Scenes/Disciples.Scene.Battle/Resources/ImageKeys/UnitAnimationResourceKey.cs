using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Resources.Enum;
using Disciples.Scene.Battle.Resources.ImageKeys.Extensions;

namespace Disciples.Scene.Battle.Resources.ImageKeys;

/// <summary>
/// Ключ для поиска анимация юнита на поле боя.
/// </summary>
internal class UnitAnimationResourceKey : BaseResourceKey
{
    /// <summary>
    /// Создать объект типа <see cref="UnitAnimationResourceKey" />.
    /// </summary>
    public UnitAnimationResourceKey(string unitTypeId, BattleUnitState unitState, BattleDirection direction, UnitAnimationType animationType)
    {
        Key = $"{unitTypeId}{unitState.GetResourceKey()}{animationType.GetResourceKey()}{direction.GetResourceKey()}00";
    }

    /// <inheritdoc />
    public override string Key { get; }
}