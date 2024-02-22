using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors;

/// <summary>
/// Обработчик битвы.
/// </summary>
internal class BattleProcessor
{
    /// <summary>
    /// Разброс инициативы при вычислении очередности.
    /// </summary>
    private const int INITIATIVE_RANGE = 5;

    private readonly IReadOnlyDictionary<UnitAttackType, IAttackTypeProcessor> _attackTypeProcessors;

    /// <summary>
    /// Создать объект типа <see cref="BattleProcessor" />.
    /// </summary>
    public BattleProcessor(IReadOnlyDictionary<UnitAttackType, IAttackTypeProcessor> attackTypeProcessors)
    {
        _attackTypeProcessors = attackTypeProcessors;
    }

    #region Очерёдность ходов

    /// <summary>
    /// Получить очередность ходов юнитов.
    /// </summary>
    /// <param name="attackingSquad">Атакующий отряд.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    public LinkedList<UnitTurnOrder> GetTurnOrder(Squad attackingSquad, Squad defendingSquad)
    {
        return new LinkedList<UnitTurnOrder>(
            attackingSquad.Units
                .Concat(defendingSquad.Units)
                .Where(u => !u.IsDeadOrRetreated)
                .Select(u => new UnitTurnOrder(u, u.Initiative + RandomGenerator.Get(0, INITIATIVE_RANGE))));
    }

    #endregion

    #region Проверка возможности атаковать

    /// <summary>
    /// Проверить, может ли атаковать один юнит другого.
    /// </summary>
    public bool CanAttack(AttackProcessorContext attackProcessorContext)
    {
        var attackingUnit = attackProcessorContext.CurrentUnit;
        var mainAttack = attackingUnit.UnitType.MainAttack;
        var mainAttackProcessor = _attackTypeProcessors[mainAttack.AttackType];
        if (mainAttackProcessor.CanAttack(attackProcessorContext, mainAttack, attackingUnit.MainAttackPower))
            return true;

        var secondaryAttack = attackingUnit.UnitType.SecondaryAttack;
        if (mainAttackProcessor.CanMainAttackBeSkipped && secondaryAttack != null)
        {
            var secondaryAttackProcessor = _attackTypeProcessors[secondaryAttack.AttackType];
            if (secondaryAttackProcessor.CanAttack(attackProcessorContext, mainAttack, attackingUnit.MainAttackPower))
                return true;
        }

        return false;
    }

    #endregion

    #region Рассчет атаки

    /// <summary>
    /// Выполнить атаку юнита с помощью основной атаки.
    /// Атака выполняется на весь отряд, если атака по площади.
    /// </summary>
    public IReadOnlyList<IAttackUnitActionProcessor> ProcessMainAttack(AttackProcessorContext attackProcessorContext)
    {
        var attackingUnit = attackProcessorContext.CurrentUnit;
        var basePower = attackingUnit.MainAttackBasePower;
        var power = attackingUnit.MainAttackPower;
        var attack = attackingUnit.UnitType.MainAttack;
        var accuracy = attackingUnit.MainAttackAccuracy;
        var attackProcessor = _attackTypeProcessors[attack.AttackType];
        var canUseSecondAttack = attackingUnit.UnitType.SecondaryAttack != null;

        IReadOnlyList<Unit> targetUnits = attackingUnit.UnitType.MainAttack.Reach == UnitAttackReach.All
            ? attackProcessorContext.TargetUnitSquad.Units
            : new[] { attackProcessorContext.TargetUnit };
        var calculatedAttacks = new List<CalculatedAttackResult>();
        var resultProcessors = new List<IAttackUnitActionProcessor>();
        foreach (var targetUnit in targetUnits)
        {
            var targetContext = new AttackProcessorContext(attackingUnit, targetUnit,
                attackProcessorContext.CurrentUnitSquad, attackProcessorContext.TargetUnitSquad,
                attackProcessorContext.UnitTurnQueue, attackProcessorContext.RoundNumber);

            if (!attackProcessor.CanAttack(targetContext, attack, power))
            {
                if (attackProcessor.CanMainAttackBeSkipped && canUseSecondAttack)
                    resultProcessors.Add(new SkipAttackProcessor(targetUnit));

                continue;
            }

            var accuracyOrProtectionProcessor = ProcessAccuracyAndProtections(attack, targetUnit, accuracy);
            if (accuracyOrProtectionProcessor != null)
            {
                resultProcessors.Add(accuracyOrProtectionProcessor);
                continue;
            }

            var calculatedAttack = attackProcessor.CalculateAttackResult(targetContext, attack, power, basePower);
            calculatedAttacks.Add(calculatedAttack);
        }

        if (calculatedAttacks.Count > 0)
            resultProcessors.Add(new UnitSuccessAttackProcessor(attackProcessor, calculatedAttacks, canUseSecondAttack));

        return resultProcessors;
    }

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью второстепенной атаки.
    /// </summary>
    public IAttackUnitActionProcessor? ProcessSecondaryAttack(AttackProcessorContext attackProcessorContext)
    {
        var attackingUnit = attackProcessorContext.CurrentUnit;
        var power = attackingUnit.SecondaryAttackPower;
        var attack = attackingUnit.UnitType.SecondaryAttack!;
        var accuracy = attackingUnit.SecondaryAttackAccuracy!.Value;
        var attackProcessor = _attackTypeProcessors[attack.AttackType];

        if (!attackProcessor.CanAttack(attackProcessorContext, attack, power))
            return null;

        var accuracyOrProtectionProcessor = ProcessAccuracyAndProtections(attack, attackProcessorContext.TargetUnit, accuracy);
        if (accuracyOrProtectionProcessor != null)
            return accuracyOrProtectionProcessor;

        var calculatedAttack = attackProcessor.CalculateAttackResult(attackProcessorContext, attack, power, power);
        return new UnitSuccessAttackProcessor(attackProcessor, new[] { calculatedAttack }, false);
    }

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью основной атаки.
    /// </summary>
    public IAttackUnitActionProcessor? ProcessSingleMainAttack(AttackProcessorContext attackProcessorContext)
    {
        var attackingUnit = attackProcessorContext.CurrentUnit;
        var basePower = attackingUnit.MainAttackBasePower;
        var power = attackingUnit.MainAttackPower;
        var attack = attackingUnit.UnitType.MainAttack;
        var accuracy = attackingUnit.MainAttackAccuracy;
        var attackProcessor = _attackTypeProcessors[attack.AttackType];
        var canUseSecondAttack = attackingUnit.UnitType.SecondaryAttack != null;

        if (!attackProcessor.CanAttack(attackProcessorContext, attack, power))
        {
            if (attackProcessor.CanMainAttackBeSkipped && canUseSecondAttack)
                return new SkipAttackProcessor(attackProcessorContext.TargetUnit);

            return null;
        }

        var accuracyOrProtectionProcessor = ProcessAccuracyAndProtections(attack, attackProcessorContext.TargetUnit, accuracy);
        if (accuracyOrProtectionProcessor != null)
            return accuracyOrProtectionProcessor;

        var calculatedAttack = attackProcessor.CalculateAttackResult(attackProcessorContext, attack, power, basePower);
        return new UnitSuccessAttackProcessor(attackProcessor, new[] { calculatedAttack }, canUseSecondAttack);
    }

    /// <summary>
    /// Обработать защиты и точность юнита.
    /// </summary>
    private static IAttackUnitActionProcessor? ProcessAccuracyAndProtections(UnitAttack unitAttack, Unit targetUnit, int accuracy)
    {
        var attackType = unitAttack.AttackType;
        var attackSource = unitAttack.AttackSource;

        var attackTypeProtection = targetUnit
            .AttackTypeProtections
            .FirstOrDefault(atp => atp.UnitAttackType == attackType);
        if (attackTypeProtection?.ProtectionCategory == ProtectionCategory.Immunity)
            return new ImmunityAttackProcessor(targetUnit);

        var attackSourceProtection = targetUnit
            .AttackSourceProtections
            .FirstOrDefault(asp => asp.UnitAttackSource == attackSource);
        if (attackSourceProtection?.ProtectionCategory == ProtectionCategory.Immunity)
            return new ImmunityAttackProcessor(targetUnit);

        var chanceOfAttack = RandomGenerator.Get(0, 100);
        if (chanceOfAttack >= accuracy)
            return new MissAttackProcessor(targetUnit);

        if (attackTypeProtection?.ProtectionCategory == ProtectionCategory.Ward ||
            attackSourceProtection?.ProtectionCategory == ProtectionCategory.Ward)
        {
            return new WardAttackProcessor(targetUnit, attackType, attackSource);
        }

        return null;
    }

    #endregion

    #region Обработка эффектов

    /// <summary>
    /// Получить все обработчики эффектов, возможные для текущего юнита.
    /// </summary>
    /// <remarks>
    /// Длительность эффекта может быть привязана как к ходу юнита-цели, так и к юниту, который наложил цель.
    /// Поэтому в этом методе проверяются все юниты, которые есть на поле боя.
    /// </remarks>
    public IReadOnlyList<IUnitEffectProcessor> GetEffectProcessors(Unit currentUnit, Squad currentUnitSquad, Squad otherUnitSquad, UnitTurnQueue unitTurnQueue, int roundNumber)
    {
        var processors = new List<IUnitEffectProcessor>();

        foreach (var targetUnit in currentUnitSquad.Units)
        {
            var context = new AttackProcessorContext(currentUnit, targetUnit,
                currentUnitSquad, currentUnitSquad,
                unitTurnQueue, roundNumber);
            var targetUnitBattleEffects = context.TargetUnit.Effects.GetBattleEffects();
            processors.AddRange(GetAttackEffectProcessors(context, targetUnitBattleEffects, false));
        }

        foreach (var targetUnit in otherUnitSquad.Units)
        {
            var context = new AttackProcessorContext(currentUnit, targetUnit,
                currentUnitSquad, otherUnitSquad,
                unitTurnQueue, roundNumber);
            var targetUnitBattleEffects = context.TargetUnit.Effects.GetBattleEffects();
            processors.AddRange(GetAttackEffectProcessors(context, targetUnitBattleEffects, false));
        }

        if (currentUnit.Effects.IsDefended)
            processors.Add(new DefendCompletedProcessor(currentUnit));

        // Отступление возможно только в том случае, если юнит можешь совершить ход.
        if (currentUnit.Effects.IsRetreating && !currentUnit.Effects.IsParalyzed)
        {
            var context = new AttackProcessorContext(currentUnit, currentUnit,
                currentUnitSquad, currentUnitSquad,
                unitTurnQueue, roundNumber);
            processors.Add(new UnitRetreatedProcessor(currentUnit, GetForceCompleteEffectProcessors(context)));
        }

        return processors;
    }

    /// <summary>
    /// Получить обработчики для принудительного завершения всех эффектов юнита.
    /// </summary>
    public IReadOnlyList<IUnitEffectProcessor> GetForceCompleteEffectProcessors(AttackProcessorContext attackProcessorContext)
    {
        var processors = new List<IUnitEffectProcessor>();

        var targetUnit = attackProcessorContext.TargetUnit;
        var targetUnitBattleEffects = targetUnit.Effects.GetBattleEffects();
        processors.AddRange(GetAttackEffectProcessors(attackProcessorContext, targetUnitBattleEffects, true));

        if (targetUnit.Effects.IsDefended)
            processors.Add(new DefendCompletedProcessor(targetUnit));

        if (targetUnit.Effects.IsRetreating)
            processors.Add(new UnitInterruptRetreatingProcessor(targetUnit));

        return processors;
    }

    /// <summary>
    /// Получить обработчики для принудительного завершения эффектов.
    /// </summary>
    public IReadOnlyList<IUnitEffectProcessor> GetForceCompleteEffectProcessors(AttackProcessorContext attackProcessorContext, IReadOnlyList<UnitBattleEffect> battleEffects)
    {
        return GetAttackEffectProcessors(attackProcessorContext, battleEffects, true).ToArray();
    }

    /// <summary>
    /// Получить обработчик эффектов конкретного юнита.
    /// </summary>
    private IEnumerable<AttackEffectProcessor> GetAttackEffectProcessors(
        AttackProcessorContext context,
        IReadOnlyList<UnitBattleEffect> battleEffects,
        bool isForceCompleting)
    {
        return battleEffects
            .Select(ube =>
            {
                // Если на одном юните наложено два однотипных эффекта, то эта штука не работает.
                var effectProcessor = (IEffectAttackProcessor)_attackTypeProcessors[ube.AttackType];
                var calculatedEffect = effectProcessor.CalculateEffect(context, ube, isForceCompleting);
                return calculatedEffect == null
                    ? null
                    : new AttackEffectProcessor(effectProcessor, calculatedEffect);
            })
            .Where(p => p != null)
            .Select(p => p!);
    }

    #endregion


    #region Проверка окончания битвы

    // TODO Если в отряде есть целитель, то перед завершением битвы ему даётся ход.

    /// <summary>
    /// Получить победителя битвы.
    /// <see langword="null" />, если битва еще не завершена.
    /// </summary>
    /// <param name="attackingSquad">Атакующий отряд.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    public Squad? GetBattleWinnerSquad(Squad attackingSquad, Squad defendingSquad)
    {
        if (attackingSquad.Units.All(u => u.IsDeadOrRetreated))
            return defendingSquad;

        if (defendingSquad.Units.All(u => u.IsDeadOrRetreated))
            return attackingSquad;

        return null;
    }

    #endregion
}