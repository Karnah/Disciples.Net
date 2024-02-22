using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик атаки юнита.
/// </summary>
internal class UnitSuccessAttackProcessor : IAttackUnitActionProcessor
{
    private readonly bool _shouldProcessOnBeginAction;

    /// <summary>
    /// Создать объект типа <see cref="UnitSuccessAttackProcessor" />.
    /// </summary>
    public UnitSuccessAttackProcessor(IAttackTypeProcessor attackTypeProcessor, IReadOnlyList<CalculatedAttackResult> attackResults, bool canUseSecondaryAttack)
    {
        AttackTypeProcessor = attackTypeProcessor;
        AttackResults = attackResults;
        SecondaryAttackUnits = canUseSecondaryAttack
            ? attackResults.Select(ar => ar.Context.TargetUnit).ToArray()
            : Array.Empty<Unit>();

        _shouldProcessOnBeginAction = GetShouldProcessOnBeginAction(attackTypeProcessor.AttackType);
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Attacked;

    /// <inheritdoc />
    public Unit TargetUnit => AttackResults[0].Context.TargetUnit;

    /// <summary>
    /// Обработчик типа атаки.
    /// </summary>
    public IAttackTypeProcessor AttackTypeProcessor { get; }

    /// <summary>
    /// Вычисленные результаты атаки.
    /// </summary>
    public IReadOnlyList<CalculatedAttackResult> AttackResults { get; }

    /// <summary>
    /// Юниты, которые были исцелены с помощью вампиризма.
    /// </summary>
    /// <remarks>
    /// Формировании списка происходит только после вызова <see cref="ProcessBeginAction" />.
    /// Для атак вампиризма обработка происходит именно в этом методе из-за <see cref="GetShouldProcessOnBeginAction" />.
    /// 
    /// НЕ МЕНЯТЬ ЭТО ПОВЕДЕНИЕ!
    /// 
    /// Если потребуется, то можно создать отдельный класс для хранения вычисленных значений исцеления,
    /// Но пока это не имеет особого смысла.
    /// </remarks>
    public IReadOnlyList<Unit> DrainLifeHealUnits { get; private set; } = Array.Empty<Unit>();

    /// <inheritdoc />
    public IReadOnlyList<Unit> SecondaryAttackUnits { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
        if (_shouldProcessOnBeginAction)
            ProcessAttack();
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
        if (!_shouldProcessOnBeginAction)
            ProcessAttack();
    }

    /// <summary>
    /// Обработать результат атаки.
    /// </summary>
    private void ProcessAttack()
    {
        foreach (var attackResult in AttackResults)
        {
            AttackTypeProcessor.ProcessAttack(attackResult);
        }

        if (AttackTypeProcessor is IDrainLifeAttackProcessor drainLifeAttackProcessor)
            DrainLifeHealUnits = drainLifeAttackProcessor.ProcessDrainLifeHeal(AttackResults);
    }

    /// <summary>
    /// Получить признак, что обработку нужно выполнять в начале действия.
    /// </summary>
    private static bool GetShouldProcessOnBeginAction(UnitAttackType attackType)
    {
        switch (attackType)
        {
            case UnitAttackType.Damage:
            case UnitAttackType.DrainLife:
            case UnitAttackType.Heal:
            case UnitAttackType.Fear:
            case UnitAttackType.DrainLifeOverflow:
            case UnitAttackType.Paralyze:
            case UnitAttackType.IncreaseDamage:
            case UnitAttackType.Petrify:
            case UnitAttackType.ReduceDamage:
            case UnitAttackType.ReduceInitiative:
            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.Cure:
            case UnitAttackType.GiveAdditionalAttack:
            case UnitAttackType.Blister:
            case UnitAttackType.GiveProtection:
            case UnitAttackType.ReduceArmor:
                return true;


            case UnitAttackType.Revive:
            case UnitAttackType.Summon:
            case UnitAttackType.ReduceLevel:
            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.TransformEnemy:
                return false;

            default:
                throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null);
        }
    }
}