using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

/// <summary>
/// Дополнительный интерфейс для атак, которые накладывают эффекты.
/// </summary>
internal interface IEffectAttackProcessor : IAttackTypeProcessor
{
    /// <summary>
    /// Проверить, можно ли вылечить эффект.
    /// </summary>
    bool CanCure(UnitBattleEffect battleEffect);

    /// <summary>
    /// Вычислить результат срабатывания эффекта.
    /// </summary>
    /// <param name="context">Контекст обработчика.</param>
    /// <param name="battleEffect">Эффект.</param>
    /// <param name="isForceCompleting">
    /// Признак, что эффект завершается принудительно.
    /// Такое необходимо при смерти юнита, отступлении или атаке <see cref="UnitAttackType.Cure" />.
    /// </param>
    CalculatedEffectResult? CalculateEffect(AttackProcessorContext context, UnitBattleEffect battleEffect, bool isForceCompleting);

    /// <summary>
    /// Обработать срабатывание эффекта.
    /// </summary>
    void ProcessEffect(CalculatedEffectResult effectResult);
}