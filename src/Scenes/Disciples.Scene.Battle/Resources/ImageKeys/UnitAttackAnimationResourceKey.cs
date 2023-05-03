using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Resources.Enum;
using Disciples.Scene.Battle.Resources.ImageKeys.Extensions;

namespace Disciples.Scene.Battle.Resources.ImageKeys;

/// <summary>
/// Ключ для поиска анимация атаки юнита на поле боя.
/// </summary>
internal class UnitAttackAnimationResourceKey : BaseResourceKey
{
    /// <summary>
    /// Создать объект типа <see cref="UnitAttackAnimationResourceKey" />.
    /// </summary>
    public UnitAttackAnimationResourceKey(string unitTypeId, UnitTargetAnimationType animationType, BattleSquadPosition? squadPosition)
    {
        Key = $"{unitTypeId}{animationType.GetResourceKey()}1{squadPosition.GetResourceKey()}00";
    }

    /// <inheritdoc />
    public override string Key { get; }
}