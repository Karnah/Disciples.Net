using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Resources.ImageKeys.Extensions;

namespace Disciples.Scene.Battle.Resources.ImageKeys;

/// <summary>
/// Ключ для поиска анимация повышения уровня юнита на поле боя.
/// </summary>
internal class UnitLevelUpAnimationResourceKey : BaseResourceKey
{
    /// <summary>
    /// Создать объект типа <see cref="UnitLevelUpAnimationResourceKey" />.
    /// </summary>
    public UnitLevelUpAnimationResourceKey(RaceType raceType, bool isSmall)
    {
        Key = $"UPGRADE{raceType.GetResourceKey()}{isSmall.GetIsSmallResourceKey()}";
    }

    /// <inheritdoc />
    public override string Key { get; }
}