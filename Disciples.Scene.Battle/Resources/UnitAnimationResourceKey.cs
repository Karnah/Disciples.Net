using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Resources.Enum;
using Disciples.Scene.Battle.Resources.Extensions;

namespace Disciples.Scene.Battle.Resources;

/// <summary>
/// Ключ для поиска анимация юнита на поле боя.
/// </summary>
internal class UnitAnimationResourceKey : BaseImageKey
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