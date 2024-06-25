using System.Diagnostics.CodeAnalysis;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;
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

    private readonly IUnitInfoProvider _unitInfoProvider;
    private readonly IReadOnlyDictionary<UnitAttackType, IAttackTypeProcessor> _attackTypeProcessors;

    /// <summary>
    /// Создать объект типа <see cref="BattleProcessor" />.
    /// </summary>
    public BattleProcessor(IUnitInfoProvider unitInfoProvider,
        IReadOnlyDictionary<UnitAttackType, IAttackTypeProcessor> attackTypeProcessors)
    {
        _unitInfoProvider = unitInfoProvider;
        _attackTypeProcessors = attackTypeProcessors;

        UnitTurnQueue = new UnitTurnQueue();
    }

    /// <summary>
    /// Юнит, чей ход сейчас.
    /// </summary>
    public Unit CurrentUnit { get; private set; } = null!;

    /// <summary>
    /// Атакующий отряд.
    /// </summary>
    public Squad AttackingSquad { get; private set; } = null!;

    /// <summary>
    /// Защищающийся отряд.
    /// </summary>
    public Squad DefendingSquad { get; private set; } = null!;

    /// <summary>
    /// Очередность хода юнитов.
    /// </summary>
    public UnitTurnQueue UnitTurnQueue { get; }

    /// <summary>
    /// Номер раунда.
    /// </summary>
    public int RoundNumber { get; private set; }

    /// <summary>
    /// Отряд, который победил в битве.
    /// </summary>
    public Squad? WinnerSquad { get; private set; }

    /// <summary>
    /// Инициализировать обработчик битвы.
    /// </summary>
    /// <remarks>
    /// TODO Пробовал передавать BattleSceneParameters в параметрах конструктора, но есть проблема в DryIoc.
    /// BattleUnitActionFactory не может отрезолвить контроллеры с BattleProcessor, так как не находит зарегистрированного BattleSceneParameters,
    /// Хотя в рамках скоупа этот объект уже создан.
    /// Попытка зарегистрировать параметры в GameController также не помогла.
    /// </remarks>
    public void Initialize(Squad attackingSquad, Squad defendingSquad)
    {
        AttackingSquad = attackingSquad;
        DefendingSquad = defendingSquad;
    }

    #region Очерёдность ходов и проверка на завершение битвы

    /// <summary>
    /// Получить следующего юнита, кто будет выполнять ход.
    /// </summary>
    /// <returns>
    /// Юнит, который будет выполнять ход.
    /// <see langword="null" />, если битва закончена.
    /// </returns>
    public Unit? GetNextUnit()
    {
        var nextUnit = UnitTurnQueue.GetNextUnit();
        if (WinnerSquad == null)
        {
            CurrentUnit = nextUnit ?? NextRound();
            return CurrentUnit;
        }

        // Если уже определён победитель, то его юниты еще могут иметь право на ход в рамках текущего раунда.
        // Лекари, если есть юниты для лечения.
        // Воскрешатели, если есть кого воскрешать.
        while (nextUnit != null)
        {
            CurrentUnit = nextUnit;

            if (WinnerSquad.Units.Any(CanAttack))
                return nextUnit;

            nextUnit = UnitTurnQueue.GetNextUnit();
        }

        // Все юниты в раунде ходили, битва завершена окончательно.
        return null;
    }

    /// <summary>
    /// Проверить, закончена ли битва и получить победителя.
    /// </summary>
    public Squad? CheckAndSetBattleWinnerSquad()
    {
        if (AttackingSquad.Units.All(u => u.IsInactive))
        {
            WinnerSquad = DefendingSquad;
            return WinnerSquad;
        }

        if (DefendingSquad.Units.All(u => u.IsInactive))
        {
            WinnerSquad = AttackingSquad;
            return WinnerSquad;
        }

        return null;
    }

    /// <summary>
    /// Завершить битву.
    /// </summary>
    public IReadOnlyList<UnitCompleteBattleProcessor> CompleteBattle()
    {
        var processors = new List<UnitCompleteBattleProcessor>();

        var winnerSquad = WinnerSquad ?? throw new InvalidOperationException("Битва еще не закончена");
        foreach (var unit in winnerSquad.Units)
        {
            // TODO Убежавшим тоже юнитам начисляется опыт.
            // Интересно, они могут повысить уровень?
            if (unit.IsInactive)
                continue;

            var completeEffectProcessors = GetForceCompleteEffectProcessors(unit);

            var nextLevelExperience = unit.NextLevelExperience - unit.Experience;
            if (unit.BattleExperience < nextLevelExperience)
            {
                processors.Add(new UnitCompleteBattleProcessor(unit, null, winnerSquad, completeEffectProcessors));
                continue;
            }

            // Обрабатываем ситуацию, когда юниту достаточно опыта, чтобы перейти на следующий уровень.
            var upgradeUnitsTypes = _unitInfoProvider.GetUpgradeUnitsTypes(unit.UnitType.Id);

            // TODO Вообще повышать нужно исходя из того, какое строение построено.
            // TODO Если есть юнит для повышения, но не построено здание, то по умолчанию юнит остаётся с NextLevelExperience - 1.
            var upgradeUnit = upgradeUnitsTypes.Count > 0
                ? new Unit(unit.Id, upgradeUnitsTypes.GetRandomElement(), unit.Player, unit.Squad, unit.Position)
                : Unit.CreateNextLevelUnit(unit);
            processors.Add(new UnitCompleteBattleProcessor(unit, upgradeUnit, winnerSquad, completeEffectProcessors));
            unit.BattleExperience = 0;
        }

        return processors;
    }

    /// <summary>
    /// Обработать наступление следующего раунда.
    /// </summary>
    /// <returns>
    /// Юнит, который будет ходить первым в раунде.
    /// </returns>
    private Unit NextRound()
    {
        var allAvailableUnits = AttackingSquad.Units
            .Concat(DefendingSquad.Units)
            .Where(u => !u.IsInactive)
            .ToArray();
        var turnOrders = allAvailableUnits
            .Select(u => new UnitTurnOrder(u, u.Initiative + RandomGenerator.Get(0, INITIATIVE_RANGE)));

        // На первом ходу доппельгангер получает дополнительный ход, раньше всех других юнитов.
        // TODO В этот ход доппельгангер не может отступить и ждать, т.е. должен рассматриваться именно как дополнительный ход,
        // А не просто добавление в очередь второй раз.
        if (RoundNumber == 0)
        {
            turnOrders = turnOrders.Concat(allAvailableUnits
                .Where(u => u.UnitType.MainAttack.AttackType == UnitAttackType.Doppelganger)
                .Select(u => new UnitTurnOrder(u, int.MaxValue)));
        }

        RoundNumber++;
        return UnitTurnQueue.NextRound(new LinkedList<UnitTurnOrder>(turnOrders));
    }

    #endregion

    #region Проверка возможности атаковать

    /// <summary>
    /// Проверить, может ли атаковать один юнит другого.
    /// </summary>
    public bool CanAttack(Unit targetUnit)
    {
        return CanAttack(targetUnit, out _, out _);
    }

    /// <summary>
    /// Проверить, можно ли атаковать указанного юнита.
    /// </summary>
    /// <param name="targetUnit">Цель атаки.</param>
    /// <param name="mainAttack">Информация о первой атаке.</param>
    /// <param name="secondaryAttack">Информация о второй атаке.</param>
    public bool CanAttack(Unit targetUnit,
        CalculatedUnitAttack mainAttack,
        CalculatedUnitAttack? secondaryAttack)
    {
        var isBattleCompleted = WinnerSquad != null;
        var attackProcessorContext = GetAttackProcessorContext(targetUnit);
        var mainAttackTypeProcessor = _attackTypeProcessors[mainAttack.AttackType];
        var isMainAttackAllowed = !isBattleCompleted || mainAttackTypeProcessor.CanAttackAfterBattle;
        if (isMainAttackAllowed && mainAttackTypeProcessor.CanAttack(attackProcessorContext, mainAttack))
            return true;

        if (secondaryAttack != null && mainAttackTypeProcessor.CanMainAttackBeSkipped)
        {
            var secondaryAttackTypeProcessor = _attackTypeProcessors[secondaryAttack.AttackType];
            var isSecondaryAttackAllowed = !isBattleCompleted || secondaryAttackTypeProcessor.CanAttackAfterBattle;
            if (isSecondaryAttackAllowed && secondaryAttackTypeProcessor.CanAttack(attackProcessorContext, secondaryAttack))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Получить юнитов, которые будут являться целями атаки.
    /// </summary>
    /// <param name="targetUnit">Юнит, который выбран первоначальной целью.</param>
    public IReadOnlyList<Unit> GetMainAttackTargetUnits(Unit targetUnit)
    {
        return GetMainAttackTargetUnits(targetUnit, out _, out _);
    }

    /// <summary>
    /// Получить юнитов, которые будут являться целями атаки.
    /// </summary>
    private IReadOnlyList<Unit> GetMainAttackTargetUnits(
        Unit targetUnit,
        out CalculatedUnitAttack? unitAttack,
        out bool isAlternativeAttackUsed)
    {
        if (!CanAttack(targetUnit, out unitAttack, out isAlternativeAttackUsed))
            return Array.Empty<Unit>();

        if (unitAttack.Reach != UnitAttackReach.All)
            return new[] { targetUnit };

        var mainAttack = unitAttack;
        var secondaryAttack = CurrentUnit.SecondaryAttack;
        return GetUnitSquad(targetUnit)
            .Units
            .Where(u => u == targetUnit || CanAttack(u, mainAttack, secondaryAttack))
            .ToArray();
    }

    /// <summary>
    /// Получить юнитов, которые будут вызваны при атаке.
    /// </summary>
    private IReadOnlyList<Unit> GetSummonAttackTargetUnits(
        UnitSquadPosition targetPosition,
        out CalculatedUnitAttack? unitAttack,
        out bool isAlternativeAttackUsed)
    {
        var mainAttack = CurrentUnit.MainAttack;
        if (mainAttack.AttackType != UnitAttackType.Summon)
            throw new InvalidOperationException($"Unit {CurrentUnit.Id} is not summoner");

        unitAttack = mainAttack;
        isAlternativeAttackUsed = false;

        var summonProcessor = (SummonAttackProcessor)_attackTypeProcessors[unitAttack.AttackType];
        var context = GetProcessorContext();
        return summonProcessor.GetSummonedUnits(context, targetPosition);
    }

    /// <summary>
    /// Проверить, можно ли атаковать указанного юнита.
    /// </summary>
    private bool CanAttack(Unit targetUnit,
        [NotNullWhen(true)] out CalculatedUnitAttack? unitAttack,
        out bool isAlternativeAttackUsed)
    {
        var attackingUnit = CurrentUnit;
        var mainAttack = attackingUnit.MainAttack;
        var secondaryAttack = attackingUnit.SecondaryAttack;
        isAlternativeAttackUsed = false;

        if (CanAttack(targetUnit, mainAttack, secondaryAttack))
        {
            unitAttack = mainAttack;
            return true;
        }

        // BUG Доппельгангер может атаковать альтернативной атакой только в том случае,
        // Если не может ни в кого превратиться на поле боя.
        var alternativeAttack = attackingUnit.AlternativeAttack;
        if (alternativeAttack != null && CanAttack(targetUnit, alternativeAttack, secondaryAttack))
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
    /// Получить позиции, куда может быть призван юнит.
    /// </summary>
    public IReadOnlyList<UnitSquadPosition> GetSummonPositions()
    {
        if (CurrentUnit.UnitType.MainAttack.AttackType != UnitAttackType.Summon)
            throw new InvalidOperationException("Текущий юнит не имеет атаки призыва");

        var summonProcessor = (SummonAttackProcessor)_attackTypeProcessors[CurrentUnit.UnitType.MainAttack.AttackType];
        var context = GetProcessorContext();
        return summonProcessor.GetSummonPositions(context);
    }

    /// <summary>
    /// Выполнить юнита с помощью основной атаки.
    /// Атака выполняется на весь отряд, если атака по площади.
    /// </summary>
    public MainAttackResult ProcessMainAttack(Squad targetSquad, UnitSquadPosition targetPosition)
    {
        IReadOnlyList<Unit> squadTargetUnits;
        CalculatedUnitAttack? attack;
        bool isAlternativeAttackUsed;

        // Если юнит призыватель, то проверяем возможность призыва.
        if (CurrentUnit.UnitType.MainAttack.AttackType == UnitAttackType.Summon &&
            targetSquad.Player == CurrentUnit.Player)
        {
            squadTargetUnits = GetSummonAttackTargetUnits(targetPosition,
                out attack,
                out isAlternativeAttackUsed);
        }
        else
        {
            var targetUnit = targetSquad.GetUnits(targetPosition).SingleOrDefault();
            if (targetUnit == null)
                throw new InvalidOperationException("Невозможно атаковать указанную позицию");

            squadTargetUnits = GetMainAttackTargetUnits(targetUnit,
                out attack,
                out isAlternativeAttackUsed);
        }

        if (attack == null)
            throw new InvalidOperationException("Невозможно атаковать указанного юнита");

        var attackingUnit = CurrentUnit;
        var attackProcessor = _attackTypeProcessors[attack.AttackType];
        var calculatedAttacks = new List<CalculatedAttackResult>();
        var resultProcessors = new List<IAttackUnitActionProcessor>();
        var canUseSecondAttack = attackingUnit.UnitType.SecondaryAttack != null;
        var secondaryUnits = new List<Unit>();
        foreach (var squadTargetUnit in squadTargetUnits)
        {
            var targetContext = GetAttackProcessorContext(squadTargetUnit);

            // Проверяем, можно ли атаковать основной атакой.
            // Если нет, то точно можно атаковать второй, это особенность метода GetMainAttackTargetUnits.
            if (!attackProcessor.CanAttack(targetContext, attack))
            {
                resultProcessors.Add(new SkipAttackProcessor(squadTargetUnit));
                secondaryUnits.Add(squadTargetUnit);
                continue;
            }

            var accuracyOrProtectionProcessor = ProcessAccuracyAndProtections(attackingUnit, squadTargetUnit, attack);
            if (accuracyOrProtectionProcessor != null)
            {
                resultProcessors.Add(accuracyOrProtectionProcessor);
                continue;
            }

            var calculatedAttack = attackProcessor.CalculateAttackResult(targetContext, attack);
            calculatedAttacks.Add(calculatedAttack);

            if (canUseSecondAttack)
                secondaryUnits.Add(squadTargetUnit);
        }

        if (calculatedAttacks.Count > 0)
            resultProcessors.Add(new UnitSuccessAttackProcessor(attackProcessor, calculatedAttacks, isAlternativeAttackUsed));

        return new MainAttackResult(isAlternativeAttackUsed, resultProcessors, secondaryUnits);
    }

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью второстепенной атаки.
    /// </summary>
    public IAttackUnitActionProcessor? ProcessSecondaryAttack(Unit targetUnit)
    {
        var attackingUnit = CurrentUnit;
        var attack = attackingUnit.SecondaryAttack!;

        var attackProcessorContext = GetAttackProcessorContext(targetUnit);
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
    public IAttackUnitActionProcessor? ProcessSingleMainAttack(Unit targetUnit)
    {
        if (!CanAttack(targetUnit, out var attack, out var isAlternativeAttackUsed))
            return null;

        // Если можно атаковать юнита, но нельзя атаковать первой,
        // Значит точно можно атаковать второй атакой.
        var attackProcessorContext = GetAttackProcessorContext(targetUnit);
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
    public IReadOnlyList<IUnitEffectProcessor> GetCurrentUnitEffectProcessors()
    {
        var processors = new List<IUnitEffectProcessor>();

        // Длительность эффекта может быть привязана как к ходу юнита-цели, так и к юниту, который наложил цель.
        // Поэтому проверяем всех юнитов, которые есть на поле боя.
        var allUnits = AttackingSquad
            .Units
            .Concat(DefendingSquad.Units);
        foreach (var targetUnit in allUnits)
        {
            var context = GetAttackProcessorContext(targetUnit);
            var targetUnitBattleEffects = context.TargetUnit.Effects.GetBattleEffects();
            processors.AddRange(GetAttackEffectProcessors(context, targetUnitBattleEffects, false));
        }

        if (CurrentUnit.Effects.IsDefended)
            processors.Add(new DefendCompletedProcessor(CurrentUnit));

        // Отступление возможно только в том случае, если юнит можешь совершить ход.
        if (CurrentUnit.Effects is { IsRetreating: true, IsParalyzed: false })
        {
            processors.Add(new UnitRetreatedProcessor(
                CurrentUnit,
                GetForceCompleteEffectProcessors(CurrentUnit),
                GetUnsommonProcessors(CurrentUnit)));
        }

        return processors;
    }

    /// <summary>
    /// Получить обработчики для изгнания всех вызванных юнитов указанным.
    /// </summary>
    private IReadOnlyList<IUnitEffectProcessor> GetUnsommonProcessors(Unit summonerUnit)
    {
        var currentUnitSquad = GetUnitSquad(summonerUnit);
        var summonedTargetUnits = currentUnitSquad
            .Units
            .Where(u => u != summonerUnit &&
                        u.Effects.IsSummoned &&
                        u.Effects.GetBattleEffects(UnitAttackType.Summon)[0].DurationControlUnit == summonerUnit);
        return summonedTargetUnits.SelectMany(GetForceCompleteEffectProcessors).ToList();
    }

    /// <summary>
    /// Получить обработчики для принудительного завершения всех эффектов юнита.
    /// </summary>
    public IReadOnlyList<IUnitEffectProcessor> GetForceCompleteEffectProcessors(Unit targetUnit)
    {
        var processors = new List<IUnitEffectProcessor>();

        var attackProcessorContext = GetAttackProcessorContext(targetUnit);
        var targetUnitBattleEffects = targetUnit.Effects.GetBattleEffects();

        // Если юнит призван, то обрабатываем только один эффект - его уничтожение.
        var isSummoned = targetUnit.Effects.IsSummoned;
        if (isSummoned)
        {
            var summonedEffect = targetUnitBattleEffects.First(e => e.AttackType == UnitAttackType.Summon);
            processors.AddRange(GetAttackEffectProcessors(attackProcessorContext, new []{ summonedEffect }, true));
            return processors;
        }

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
    public IReadOnlyList<IUnitEffectProcessor> GetForceCompleteEffectProcessors(Unit targetUnit, IReadOnlyList<UnitBattleEffect> battleEffects)
    {
        var attackProcessorContext = GetAttackProcessorContext(targetUnit);
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

    #region Отдельные обработчики

    /// <summary>
    /// Получить обработчик защиты юнита.
    /// </summary>
    public DefendProcessor ProcessDefend()
    {
        return new DefendProcessor(CurrentUnit);
    }

    /// <summary>
    /// Получить обработчик побега юнита.
    /// </summary>
    public UnitRetreatingProcessor ProcessRetreat()
    {
        return new UnitRetreatingProcessor(CurrentUnit);
    }

    /// <summary>
    /// Получить обработчик ожидания юнита.
    /// </summary>
    public UnitWaitingProcessor ProcessWait()
    {
        return new UnitWaitingProcessor(CurrentUnit, UnitTurnQueue);
    }

    /// <summary>
    /// Получить обработчик смерти юнита.
    /// </summary>
    public UnitDeathProcessor ProcessDeath(Unit targetUnit)
    {
        if (targetUnit.HitPoints > 0)
            throw new ArgumentException($"{targetUnit.Id} еще живой", nameof(targetUnit));

        return new UnitDeathProcessor(targetUnit,
            GetUnitEnemySquad(targetUnit),
            GetForceCompleteEffectProcessors(targetUnit),
            GetUnsommonProcessors(targetUnit));
    }

    #endregion

    #region Контексты и вспомогательные методы.

    /// <summary>
    /// Получить контекст битвы.
    /// </summary>
    public BattleProcessorContext GetProcessorContext()
    {
        return new BattleProcessorContext(
            CurrentUnit,
            AttackingSquad, DefendingSquad,
            UnitTurnQueue, RoundNumber);
    }

    /// <summary>
    /// Получить контекст атаки на указанного юнита.
    /// </summary>
    public AttackProcessorContext GetAttackProcessorContext(Unit targetUnit)
    {
        return new AttackProcessorContext(
            CurrentUnit, targetUnit,
            AttackingSquad, DefendingSquad,
            UnitTurnQueue, RoundNumber);
    }

    /// <summary>
    /// Получить отряд указанного юнита.
    /// </summary>
    public Squad GetUnitSquad(Unit targetUnit)
    {
        return targetUnit.Player == AttackingSquad.Player
            ? AttackingSquad
            : DefendingSquad;
    }

    /// <summary>
    /// Получить вражеский отряд для указанного юнита.
    /// </summary>
    public Squad GetUnitEnemySquad(Unit targetUnit)
    {
        return targetUnit.Player == AttackingSquad.Player
            ? DefendingSquad
            : AttackingSquad;
    }

    #endregion
}