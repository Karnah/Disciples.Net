using Disciples.Common.Models;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Контекст битвы.
/// </summary>
internal class BattleContext : BaseSupportLoading
{
    /// <summary>
    /// Создать объект типа <see cref="BattleContext" />.
    /// </summary>
    public BattleContext()
    {
        UnitTurnQueue = new UnitTurnQueue();
        InputDeviceEvents = Array.Empty<InputDeviceEvent>();
    }

    /// <summary>
    /// Количество секунд, которое прошло с предыдущего обновления сцены.
    /// </summary>
    public long TicksCount { get; private set; }

    /// <summary>
    /// Позиция курсора.
    /// </summary>
    public PointD MousePosition { get; private set; }

    /// <summary>
    /// События от устройства ввода.
    /// </summary>
    public IReadOnlyList<InputDeviceEvent> InputDeviceEvents { get; private set; }

    /// <summary>
    /// Номер раунда битвы.
    /// </summary>
    public int RoundNumber { get; set; }

    /// <summary>
    /// Позиция отряда игрока.
    /// </summary>
    /// <remarks>
    /// Если атаковал игрок, то <see cref="BattleSquadPosition.Attacker" />.
    /// Если ИИ атаковал игрока, то <see cref="BattleSquadPosition.Defender" />.
    /// </remarks>
    public BattleSquadPosition PlayerSquadPosition => BattleSquadPosition.Attacker;

    /// <summary>
    /// Атакующий отряд
    /// </summary>
    public BattleSquadData AttackingBattleSquad { get; } = new();

    /// <summary>
    /// Отряд, который защищается.
    /// </summary>
    public BattleSquadData DefendingBattleSquad { get; } = new();

    /// <summary>
    /// Атакующий отряд
    /// </summary>
    public Squad AttackingSquad => AttackingBattleSquad.Squad;

    /// <summary>
    /// Отряд, который защищается.
    /// </summary>
    public Squad DefendingSquad => DefendingBattleSquad.Squad;

    /// <summary>
    /// Юнит, который выполняет свой ход.
    /// </summary>
    public BattleUnit CurrentBattleUnit { get; set; } = null!;

    /// <summary>
    /// Юнит, который выбран в качестве цели.
    /// </summary>
    public BattleUnit? TargetBattleUnit { get; set; }

    /// <summary>
    /// Состояние битвы.
    /// </summary>
    public BattleState BattleState { get; private set; }

    /// <summary>
    /// Признак того, что юнит атакует второй раз за текущий ход.
    /// </summary>
    /// <remarks>Актуально только для юнитов, которые бьют дважды за ход.</remarks>
    public bool IsSecondAttack { get; set; }

    /// <summary>
    /// Признак, что ходит юнит, который "ждал" в этом раунде.
    /// </summary>
    public bool IsWaitingUnitTurn => UnitTurnQueue.IsWaitingUnitTurn;

    /// <summary>
    /// Признак, что битва проходит в автоматическом режиме.
    /// </summary>
    public bool IsAutoBattle { get; set; }

    /// <summary>
    /// Признак, что битву необходимо завершить в автоматическом режиме.
    /// </summary>
    public bool IsInstantBattle { get; set; }

    /// <summary>
    /// Отряд, который победил в битве.
    /// </summary>
    public BattleSquadPosition? BattleWinnerSquad { get; private set; }

    /// <summary>
    /// Все юниты.
    /// </summary>
    public List<BattleUnit> BattleUnits { get; set; } = null!;

    /// <summary>
    /// Юниты, которые были превращены.
    /// </summary>
    public List<BattleUnit> TransformedUnits { get; } = new();

    /// <summary>
    /// Очередность хода юнитов.
    /// </summary>
    public UnitTurnQueue UnitTurnQueue { get; }

    /// <summary>
    /// Действие, которое выполняется юнитом в данный момент.
    /// </summary>
    public IBattleUnitActionController? UnitAction { get; private set; }

    /// <summary>
    /// Действие юнита, которое будет выполняться следующим.
    /// </summary>
    public IBattleUnitActionController? NextUnitAction { get; private set; }

    /// <summary>
    /// Завершившееся действие юнита.
    /// </summary>
    public IBattleUnitActionController? CompletedUnitAction { get; private set; }

    /// <summary>
    /// Обновить данные контекста на основе данных об обновлении сцены.
    /// </summary>
    public void BeforeSceneUpdate(UpdateSceneData updateSceneData)
    {
        TicksCount = updateSceneData.TicksCount;
        MousePosition = updateSceneData.MousePosition;
        InputDeviceEvents = updateSceneData.InputDeviceEvents;

        BattleState = GetNewBattleState(BattleState);
        CompletedUnitAction = null;

        UnitAction?.BeforeSceneUpdate();
    }

    /// <summary>
    /// Обновить данные после завершения обновления сцены.
    /// </summary>
    public void AfterSceneUpdate()
    {
        if (UnitAction != null)
        {
            UnitAction.AfterSceneUpdate();

            if (UnitAction.IsCompleted)
            {
                CompletedUnitAction = UnitAction;
                UnitAction = GetNextUnitAction();
                BattleState = UnitAction == null
                    ? BattleState.CompletedUnitAction
                    : BattleState.BeginNextUnitAction;
            }
        }
    }

    /// <summary>
    /// Получить игровой объекта юнита.
    /// </summary>
    public BattleUnit GetBattleUnit(Unit unit)
    {
        return BattleUnits.First(u => u.Unit == unit);
    }

    /// <summary>
    /// Получить отряд указанного юнита.
    /// </summary>
    public Squad GetBattleUnitSquad(BattleUnit unit)
    {
        return unit.IsAttacker
            ? AttackingSquad
            : DefendingSquad;
    }

    /// <summary>
    /// Установить действие.
    /// </summary>
    public void AddUnitAction(IBattleUnitActionController unitAction)
    {
        if (UnitAction == null)
        {
            // Если только идёт загрузка битвы, действие не будет инициализировано.
            if (IsLoaded)
            {
                unitAction.Initialize();
                if (unitAction.IsCompleted)
                    return;
            }

            BattleState = CompletedUnitAction == null
                ? BattleState.BeginUnitAction
                : BattleState.BeginNextUnitAction;
            UnitAction = unitAction;
        }
        else if (NextUnitAction == null)
        {
            BattleState = BattleState.ProcessingUnitAction;
            NextUnitAction = unitAction;
        }
        else
        {
            throw new Exception("Очередь действий заполнена");
        }
    }

    /// <summary>
    /// Установить, что битва завершена.
    /// </summary>
    public void SetBattleCompleted(BattleSquadPosition battleWinnerSquad)
    {
        BattleState = BattleState.CompletedBattle;
        BattleWinnerSquad = battleWinnerSquad;
    }

    /// <summary>
    /// Создать контекст атаки текущего юнита на другого.
    /// </summary>
    public AttackProcessorContext CreateAttackProcessorContext(BattleUnit targetBattleUnit)
    {
        var attackingSquad = CurrentBattleUnit.IsAttacker
            ? AttackingSquad
            : DefendingSquad;
        var targetSquad = targetBattleUnit.IsAttacker
            ? AttackingSquad
            : DefendingSquad;
        return new AttackProcessorContext(CurrentBattleUnit.Unit,
            targetBattleUnit.Unit,
            attackingSquad,
            targetSquad,
            UnitTurnQueue,
            RoundNumber);
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        UnitAction?.Initialize();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        foreach (var battleUnit in BattleUnits)
        {
            battleUnit.Destroy();
        }
    }

    /// <summary>
    /// Получить новое состояние битвы.
    /// </summary>
    private static BattleState GetNewBattleState(BattleState currentBattleState)
    {
        return currentBattleState switch
        {
            BattleState.WaitPlayerTurn => BattleState.WaitPlayerTurn,
            BattleState.BeginUnitAction => BattleState.ProcessingUnitAction,
            BattleState.BeginNextUnitAction => BattleState.ProcessingUnitAction,
            BattleState.ProcessingUnitAction => BattleState.ProcessingUnitAction,
            BattleState.CompletedUnitAction => BattleState.WaitPlayerTurn,
            BattleState.CompletedBattle => BattleState.WaitExit,
            BattleState.WaitExit => BattleState.WaitExit,
            _ => throw new ArgumentOutOfRangeException(nameof(currentBattleState), currentBattleState, null)
        };
    }

    /// <summary>
    /// Получить следующее действие юнита.
    /// </summary>
    private IBattleUnitActionController? GetNextUnitAction()
    {
        if (NextUnitAction == null)
            return null;

        var nextUnitAction = NextUnitAction;
        NextUnitAction = null;

        nextUnitAction.Initialize();

        // Некоторые действия заканчиваются мгновенно (например, промах при второй атаке).
        // В этом случае их никак фиксировать не нужно.
        if (nextUnitAction.IsCompleted)
            return null;

        return nextUnitAction;
    }
}