namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// После завершения последнего действия выполняется небольшая задержка перед тем, как будет разблокирован интерфейс.
/// </summary>
internal class DelayBattleAction : BaseTimerBattleAction
{
    /// <summary>
    /// Создать объект типа <see cref="DelayBattleAction" />.
    /// </summary>
    public DelayBattleAction(long delay, Action? onCompletedAction = null) : base(delay, onCompletedAction)
    {
    }
}