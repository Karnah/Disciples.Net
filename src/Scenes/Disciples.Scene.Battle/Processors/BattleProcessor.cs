using System.Diagnostics.CodeAnalysis;
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
    /// <param name="roundNumber">Номер раунда.</param>
    public LinkedList<UnitTurnOrder> GetTurnOrder(Squad attackingSquad, Squad defendingSquad, int roundNumber)
    {
        var allAvailableUnits = attackingSquad.Units
            .Concat(defendingSquad.Units)
            .Where(u => !u.IsDeadOrRetreated)
            .ToArray();
        var turnOrders = allAvailableUnits
            .Select(u => new UnitTurnOrder(u, u.Initiative + RandomGenerator.Get(0, INITIATIVE_RANGE)));

        // На первом ходу доппельгангер получает дополнительный ход, раньше всех других юнитов.
        // TODO В этот ход доппельгангер не может отступить и ждать, т.е. должен рассматриваться именно как дополнительный ход,
        // А не просто добавление в очередь второй раз.
        if (roundNumber == 1)
        {
            turnOrders = turnOrders.Concat(allAvailableUnits
                .Where(u => u.UnitType.MainAttack.AttackType == UnitAttackType.Doppelganger)
                .Select(u => new UnitTurnOrder(u, int.MaxValue)));
        }

        return new LinkedList<UnitTurnOrder>(turnOrders);
    }

    #endregion

    #region Проверка возможности атаковать

    /// <summary>
    /// Проверить, может ли атаковать один юнит другого.
    /// </summary>
    public bool CanAttack(AttackProcessorContext attackProcessorContext)
    {
        return CanAttack(attackProcessorContext, out _, out _);
    }

    /// <summary>
    /// Проверить, можно ли атаковать указанного юнита.
    /// </summary>
    /// <param name="attackProcessorContext">Контекст атаки.</param>
    /// <param name="mainAttack">Информация о первой атаке.</param>
    /// <param name="secondaryAttack">Информация о второй атаке.</param>
    public bool CanAttack(AttackProcessorContext attackProcessorContext,
        CalculatedUnitAttack mainAttack,
        CalculatedUnitAttack? secondaryAttack)
    {
        var mainAttackTypeProcessor = _attackTypeProcessors[mainAttack.AttackType];
        if (mainAttackTypeProcessor.CanAttack(attackProcessorContext, mainAttack))
            return true;

        if (secondaryAttack != null && mainAttackTypeProcessor.CanMainAttackBeSkipped)
        {
            var secondaryAttackTypeProcessor = _attackTypeProcessors[secondaryAttack.AttackType];
            if (secondaryAttackTypeProcessor.CanAttack(attackProcessorContext, secondaryAttack))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Получить юнитов, которые будут являться целями атаки.
    /// </summary>
    public IReadOnlyList<Unit> GetMainAttackTargetUnits(AttackProcessorContext attackProcessorContext)
    {
        return GetMainAttackTargetUnits(attackProcessorContext, out _, out _);
    }

    /// <summary>
    /// Получить юнитов, которые будут являться целями атаки.
    /// </summary>
    private IReadOnlyList<Unit> GetMainAttackTargetUnits(
        AttackProcessorContext attackProcessorContext,
        out CalculatedUnitAttack? unitAttack,
        out bool isAlternativeAttackUsed)
    {
        if (!CanAttack(attackProcessorContext, out unitAttack, out isAlternativeAttackUsed))
            return Array.Empty<Unit>();

        if (unitAttack.Reach != UnitAttackReach.All)
            return new[] { attackProcessorContext.TargetUnit };

        var mainAttack = unitAttack;
        var secondaryAttack = attackProcessorContext.CurrentUnit.SecondaryAttack;
        return attackProcessorContext
            .TargetUnitSquad
            .Units
            .Where(u =>
                {
                    if (u == attackProcessorContext.TargetUnit)
                        return true;

                    var innerContext = new AttackProcessorContext(attackProcessorContext.CurrentUnit, u,
                        attackProcessorContext.CurrentUnitSquad, attackProcessorContext.TargetUnitSquad,
                        attackProcessorContext.UnitTurnQueue, attackProcessorContext.RoundNumber);
                    return CanAttack(innerContext, mainAttack, secondaryAttack);
                })
            .ToArray();
    }

    /// <summary>
    /// Проверить, можно ли атаковать указанного юнита.
    /// </summary>
    private bool CanAttack(AttackProcessorContext attackProcessorContext,
        [NotNullWhen(true)] out CalculatedUnitAttack? unitAttack,
        out bool isAlternativeAttackUsed)
    {
        var attackingUnit = attackProcessorContext.CurrentUnit;
        var mainAttack = attackingUnit.MainAttack;
        var secondaryAttack = attackingUnit.SecondaryAttack;
        isAlternativeAttackUsed = false;

        if (CanAttack(attackProcessorContext, mainAttack, secondaryAttack))
        {
            unitAttack = mainAttack;
            return true;
        }

        // BUG Доппельгангер может атаковать альтернативной атакой только в том случае,
        // Если не может ни в кого превратиться на поле боя.
        var alternativeAttack = attackingUnit.AlternativeAttack;
        if (alternativeAttack != null && CanAttack(attackProcessorContext, alternativeAttack, secondaryAttack))
        {
            unitAttack = alternativeAttack;
            isAlternativeAttackUsed = true;
            return true;
        }

        unitAttack = null;
        return false;
    }

    #endregion

    #region Рассчет атаки

    /// <summary>
    /// Выполнить атаку юнита с помощью основной атаки.
    /// Атака выполняется на весь отряд, если атака по площади.
    /// </summary>
    public MainAttackResult ProcessMainAttack(AttackProcessorContext attackProcessorContext)
    {
        var targetUnits = GetMainAttackTargetUnits(attackProcessorContext,
            out var attack,
            out var isAlternativeAttackUsed);
        if (attack == null)
            throw new InvalidOperationException("Невозможно атаковать указанного юнита");

        var attackingUnit = attackProcessorContext.CurrentUnit;
        var attackProcessor = _attackTypeProcessors[attack.AttackType];
        var calculatedAttacks = new List<CalculatedAttackResult>();
        var resultProcessors = new List<IAttackUnitActionProcessor>();
        var canUseSecondAttack = attackingUnit.UnitType.SecondaryAttack != null;
        var secondaryUnits = new List<Unit>();
        foreach (var targetUnit in targetUnits)
        {
            var targetContext = new AttackProcessorContext(attackingUnit, targetUnit,
                attackProcessorContext.CurrentUnitSquad, attackProcessorContext.TargetUnitSquad,
                attackProcessorContext.UnitTurnQueue, attackProcessorContext.RoundNumber);

            // Проверяем, можно ли атаковать основной атакой.
            // Если нет, то точно можно атаковать второй, это особенность метода GetMainAttackTargetUnits.
            if (!attackProcessor.CanAttack(targetContext, attack))
            {
                resultProcessors.Add(new SkipAttackProcessor(targetUnit));
                secondaryUnits.Add(targetUnit);
                continue;
            }

            var accuracyOrProtectionProcessor = ProcessAccuracyAndProtections(attackingUnit, targetUnit, attack);
            if (accuracyOrProtectionProcessor != null)
            {
                resultProcessors.Add(accuracyOrProtectionProcessor);
                continue;
            }

            var calculatedAttack = attackProcessor.CalculateAttackResult(targetContext, attack);
            calculatedAttacks.Add(calculatedAttack);

            if (canUseSecondAttack)
                secondaryUnits.Add(targetUnit);
        }

        if (calculatedAttacks.Count > 0)
            resultProcessors.Add(new UnitSuccessAttackProcessor(attackProcessor, calculatedAttacks, isAlternativeAttackUsed));

        return new MainAttackResult(isAlternativeAttackUsed, resultProcessors, secondaryUnits);
    }

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью второстепенной атаки.
    /// </summary>
    public IAttackUnitActionProcessor? ProcessSecondaryAttack(AttackProcessorContext attackProcessorContext)
    {
        var attackingUnit = attackProcessorContext.CurrentUnit;
        var attack = attackingUnit.SecondaryAttack!;

        var attackProcessor = _attackTypeProcessors[attack.AttackType];
        if (!attackProcessor.CanAttack(attackProcessorContext, attack))
            return null;

        var accuracyOrProtectionProcessor = ProcessAccuracyAndProtections(attackingUnit, attackProcessorContext.TargetUnit, attack);
        if (accuracyOrProtectionProcessor != null)
            return accuracyOrProtectionProcessor;

        var calculatedAttack = attackProcessor.CalculateAttackResult(attackProcessorContext, attack);
        return new UnitSuccessAttackProcessor(attackProcessor, new[] { calculatedAttack }, false);
    }

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью основной атаки.
    /// </summary>
    public IAttackUnitActionProcessor? ProcessSingleMainAttack(AttackProcessorContext attackProcessorContext)
    {
        if (!CanAttack(attackProcessorContext, out var attack, out var isAlternativeAttackUsed))
            return null;

        // Если можно атаковать юнита, но нельзя атаковать первой,
        // Значит точно можно атаковать второй атакой.
        var attackProcessor = _attackTypeProcessors[attack.AttackType];
        if (!attackProcessor.CanAttack(attackProcessorContext, attack))
            return new SkipAttackProcessor(attackProcessorContext.TargetUnit);

        var accuracyOrProtectionProcessor = ProcessAccuracyAndProtections(attackProcessorContext.CurrentUnit, attackProcessorContext.TargetUnit, attack);
        if (accuracyOrProtectionProcessor != null)
            return accuracyOrProtectionProcessor;


        var calculatedAttack = attackProcessor.CalculateAttackResult(attackProcessorContext, attack);
        return new UnitSuccessAttackProcessor(attackProcessor, new[] { calculatedAttack }, isAlternativeAttackUsed);
    }

    /// <summary>
    /// Обработать защиты и точность юнита.
    /// </summary>
    private static IAttackUnitActionProcessor? ProcessAccuracyAndProtections(Unit attackingUnit, Unit targetUnit, CalculatedUnitAttack unitAttack)
    {
        var attackType = unitAttack.AttackType;
        var attackSource = unitAttack.AttackSource;

        // Если атака по союзнику или доппельгангер, то все проверки.
        // Точность для таких атак всегда равна 100%.
        if (attackingUnit.Player.Id == targetUnit.Player.Id ||
            attackType == UnitAttackType.Doppelganger)
        {
            return null;
        }


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
        if (chanceOfAttack >= unitAttack.TotalAccuracy)
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