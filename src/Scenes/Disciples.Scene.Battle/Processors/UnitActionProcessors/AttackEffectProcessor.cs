using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик эффекта возникшего вследствие атаки.
/// </summary>
internal class AttackEffectProcessor : IUnitEffectProcessor
{
    /// <summary>
    /// Создать объект типа <see cref="AttackEffectProcessor" />.
    /// </summary>
    public AttackEffectProcessor(IEffectAttackProcessor effectProcessor, CalculatedEffectResult effectResult)
    {
        EffectProcessor = effectProcessor;
        EffectResult = effectResult;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Attacked;

    /// <inheritdoc />
    public Unit TargetUnit => EffectResult.Context.TargetUnit;

    /// <summary>
    /// Обработчик эффекта.
    /// </summary>
    public IEffectAttackProcessor EffectProcessor { get; }

    /// <summary>
    /// Результат эффекта.
    /// </summary>
    public CalculatedEffectResult EffectResult { get; private set; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
        // Обработчики эффектов формируются сразу большим списком, без учёта друг-друга.
        // Таким образом, при вызове второго эффекта предварительно рассчитанный результат может быть не актуален.
        // Например, здоровье юнита опустилось ниже урона от атаки текущего эффекта.
        // Поэтому пересчитываем эффект здесь еще раз.
        // TODO Желательно избавиться от пересчёта.
        var newEffectResult = EffectProcessor.CalculateEffect(EffectResult.Context, EffectResult.Effect, EffectResult.IsForceCompleting);
        EffectResult = newEffectResult
                       ?? throw new InvalidOperationException($"Невозможно повторно обработать эффект {EffectResult.Effect.AttackType}");
        EffectProcessor.ProcessEffect(EffectResult);
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
    }
}