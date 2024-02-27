using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

/// <summary>
/// Контроллер атаки определённого типа.
/// </summary>
internal interface IAttackTypeProcessor
{
    /// <summary>
    /// Тип атаки, который обрабатывает контроллер данного типа.
    /// </summary>
    UnitAttackType AttackType { get; }

    /// <summary>
    /// Если <see cref="UnitType.MainAttack"/> имеет тип <see cref="AttackType" /> и <see cref="CanAttack" /> <see langword="false" />,
    /// То можно будет использовать <see cref="UnitType.SecondaryAttack" />.
    /// </summary>
    /// <remarks>
    /// Для атак на союзников есть особые условия. Первая атака может быть невозможна, но вторая да, и она должна быть выполнена.
    /// Например, Иерофант: он не может лечить мёртвого (первая атака), но может воскресить второй.
    /// Также архидруидресса: она может не иметь возможности усилить, но может второй снять дебаффы.
    /// </remarks>
    bool CanMainAttackBeSkipped { get; }

    /// <summary>
    /// Проверить, можно ли выполнить атаку.
    /// </summary>
    bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack);

    /// <summary>
    /// Вычислить атаку юнита.
    /// </summary>
    CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context, CalculatedUnitAttack unitAttack);

    /// <summary>
    /// Обработать результат атаки.
    /// </summary>
    void ProcessAttack(CalculatedAttackResult attackResult);
}