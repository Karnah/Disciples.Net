using Disciples.Common.Models;
using Disciples.Engine.Common;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;
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
    /// Позиция отряда игрока.
    /// </summary>
    /// <remarks>
    /// Если атаковал игрок, то <see cref="BattleSquadPosition.Attacker" />.
    /// Если ИИ атаковал игрока, то <see cref="BattleSquadPosition.Defender" />.
    /// </remarks>
    public BattleSquadPosition PlayerSquadPosition => BattleSquadPosition.Attacker;

    /// <summary>
    /// Контекст игры.
    /// </summary>
    public GameContext GameContext { get; set; } = null!;

    /// <summary>
    /// Атакующий отряд
    /// </summary>
    public BattleSquadData AttackingBattleSquad { get; } = new();

    /// <summary>
    /// Отряд, который защищается.
    /// </summary>
    public BattleSquadData DefendingBattleSquad { get; } = new();

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
    public BattleState BattleState { get; set; }

    /// <summary>
    /// Событие, которое произошло в битве.
    /// </summary>
    public BattleActionEvent BattleActionEvent { get; set; }

    /// <summary>
    /// Признак того, что юнит атакует второй раз за текущий ход.
    /// </summary>
    /// <remarks>Актуально только для юнитов, которые бьют дважды за ход.</remarks>
    public bool IsSecondAttack { get; set; }

    /// <summary>
    /// Признак, что битва проходит в автоматическом режиме.
    /// </summary>
    public bool IsAutoBattle { get; set; }

    /// <summary>
    /// Признак, что битву необходимо завершить в автоматическом режиме.
    /// </summary>
    public bool IsInstantBattleRequested { get; set; }

    /// <summary>
    /// Отряд, который победил в битве.
    /// </summary>
    public BattleSquadPosition? WinnerSquadPosition { get; set; }

    /// <summary>
    /// Все юниты.
    /// </summary>
    public List<BattleUnit> BattleUnits { get; set; } = null!;

    /// <summary>
    /// Действие, которое выполняется в данный момент.
    /// </summary>
    public IBattleActionController? Action { get; private set; }

    /// <summary>
    /// Действие, которое будет выполняться следующим.
    /// </summary>
    public IBattleActionController? NextAction { get; private set; }

    /// <summary>
    /// Завершившееся действие.
    /// </summary>
    public IBattleActionController? CompletedAction { get; private set; }

    /// <summary>
    /// Обновить данные контекста на основе данных об обновлении сцены.
    /// </summary>
    public void BeforeSceneUpdate(UpdateSceneData updateSceneData)
    {
        TicksCount = updateSceneData.TicksCount;
        MousePosition = updateSceneData.MousePosition;
        InputDeviceEvents = updateSceneData.InputDeviceEvents;

        BattleActionEvent = BattleActionEvent.None;
        CompletedAction = null;

        Action?.BeforeSceneUpdate();
    }

    /// <summary>
    /// Обновить данные после завершения обновления сцены.
    /// </summary>
    public void AfterSceneUpdate()
    {
        if (Action == null)
            return;

        Action.AfterSceneUpdate();

        if (Action.IsCompleted)
        {
            CompletedAction = Action;
            Action = NextAction == null
                ? null
                : InitializeAction(NextAction);
            NextAction = null;
        }
    }

    /// <summary>
    /// Получить игровой объекта юнита.
    /// </summary>
    public BattleUnit GetBattleUnit(Unit unit)
    {
        return BattleUnits.First(u => u.Unit.Id == unit.Id);
    }

    /// <summary>
    /// Установить действие.
    /// </summary>
    public void AddAction(IBattleActionController battleAction)
    {
        if (Action == null)
        {
            Action = InitializeAction(battleAction);
        }
        else if (NextAction == null)
        {
            NextAction = battleAction;
        }
        else
        {
            throw new InvalidOperationException("Очередь действий заполнена");
        }
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        Action?.Initialize();
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
    /// Инициализировать действие.
    /// </summary>
    private IBattleActionController? InitializeAction(IBattleActionController action)
    {
        // Нельзя инициализировать действие, если только идёт загрузка битвы.
        // В этот момент могли еще не все контроллеры инициализированы.
        if (!IsLoaded)
            return action;

        action.Initialize();

        // Если действие совершается мгновенно, то его дальше обрабатывать никак не нужно.
        return action.IsCompleted
            ? null
            : action;
    }
}