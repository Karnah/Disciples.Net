namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// После завершения последнего действия выполняется небольшая задержка перед тем, как будет разблокирован интерфейс.
/// </summary>
internal class DelayBattleAction : BaseTimerBattleAction
{
    /// <summary>
    /// Задержка после завершения всех действий, прежде чем ход перейдёт к следующему юниту.
    /// </summary>
    private const long ACTION_DELAY = 250;

    /// <summary>
    /// Создать объект типа <see cref="DelayBattleAction" />.
    /// </summary>
    public DelayBattleAction() : base(ACTION_DELAY)
    {
    }
}