using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Resources.Extensions;

namespace Disciples.Scene.Battle.Resources;

/// <summary>
/// Ключ для поиска звука типа атаки.
/// </summary>
internal class UnitAttackTypeSoundResourceKey : BaseResourceKey
{
    /// <summary>
    /// Создать объект типа <see cref="UnitAttackTypeSoundResourceKey" />.
    /// </summary>
    public UnitAttackTypeSoundResourceKey(UnitAttackType attackType)
    {
        Key = attackType.GetResourceKey();
    }

    /// <inheritdoc />
    public override string Key { get; }
}