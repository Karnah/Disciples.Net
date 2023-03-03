using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models.BattleActions;

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
        Actions = new BattleActionContainer();
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => false;

    /// <summary>
    /// Количество секунд, которое прошло с предыдущего обновления сцены.
    /// </summary>
    public long TicksCount { get; private set; }

    /// <summary>
    /// События от устройства ввода.
    /// </summary>
    public IReadOnlyList<InputDeviceEvent> InputDeviceEvents { get; private set; }

    /// <summary>
    /// Атакующий отряд
    /// </summary>
    public Squad AttackingSquad { get; set; } = null!;

    /// <summary>
    /// Отряд, который защищается.
    /// </summary>
    public Squad DefendingSquad { get; set; } = null!;

    /// <summary>
    /// Юнит, который выполняет свой ход.
    /// </summary>
    public BattleUnit CurrentBattleUnit { get; set; } = null!;

    /// <summary>
    /// Признак того, что юнит атакует второй раз за текущий ход.
    /// </summary>
    /// <remarks>Актуально только для юнитов, которые бьют дважды за ход.</remarks>
    public bool IsSecondAttack { get; set; }

    /// <summary>
    /// Признак, что ходит юнит, который "ждал" в этом раунде.
    /// </summary>
    public bool IsWaitingUnitTurn { get; set; }

    /// <summary>
    /// Все юниты.
    /// </summary>
    public IReadOnlyList<BattleUnit> BattleUnits { get; set; } = null!;

    /// <summary>
    /// Список всех действий на поле боя.
    /// </summary>
    public BattleActionContainer Actions { get; }

    /// <summary>
    /// Обновить данные контекста на основе данных об обновлении сцены.
    /// </summary>
    public void BeforeSceneUpdate(UpdateSceneData updateSceneData)
    {
        TicksCount = updateSceneData.TicksCount;
        InputDeviceEvents = updateSceneData.InputDeviceEvents;
        Actions.BeforeSceneUpdate(TicksCount);
    }

    /// <summary>
    /// Обновить данные после завершения обновления сцены.
    /// </summary>
    public void AfterSceneUpdate()
    {
        Actions.AfterSceneUpdate();
    }

    /// <summary>
    /// Получить игровой объекта юнита.
    /// </summary>
    /// <remarks>
    /// TODO Вообще метод можно удалить, если сделать отдельную реализацию BattleUnitPortrait.
    /// </remarks>
    public BattleUnit GetBattleUnit(Unit unit)
    {
        return BattleUnits.First(u => u.Unit == unit);
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        foreach (var battleUnit in BattleUnits)
        {
            battleUnit.Destroy();
        }
    }
}