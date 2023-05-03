using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Resources.ImageKeys.Extensions;

namespace Disciples.Scene.Battle.Resources.ImageKeys;

/// <summary>
/// Ключ для поиска анимация применяемого на юнита эффекта на поле боя.
/// </summary>
internal class UnitBattleEffectAnimationResourceKey : BaseResourceKey
{
    /// <summary>
    /// Создать объект типа <see cref="UnitBattleEffectAnimationResourceKey" />.
    /// </summary>
    public UnitBattleEffectAnimationResourceKey(UnitAttackType effectAttackType, bool isSmall)
    {
        Key = $"{effectAttackType.GetResourceKey()}{isSmall.GetIsSmallResourceKey()}";
    }

    /// <inheritdoc />
    public override string Key { get; }
}