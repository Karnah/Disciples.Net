using Disciples.Engine.Common.Enums;
using Disciples.Scene.Battle.Resources.Extensions;

namespace Disciples.Scene.Battle.Resources;

/// <summary>
/// Ключ для поиска анимация применяемого на юнита эффекта на поле боя.
/// </summary>
internal class UnitBattleEffectAnimationResourceKey : BaseImageKey
{
    /// <summary>
    /// Создать объект типа <see cref="UnitBattleEffectAnimationResourceKey" />.
    /// </summary>
    public UnitBattleEffectAnimationResourceKey(UnitBattleEffectType effectType, bool isSmall)
    {
        Key = $"{effectType.GetResourceKey()}{isSmall.GetIsSmallResourceKey()}";
    }

    /// <inheritdoc />
    public override string Key { get; }
}