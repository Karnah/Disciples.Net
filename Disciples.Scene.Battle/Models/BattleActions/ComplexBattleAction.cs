namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Действие, которое объединяет в себе другие действияю
/// </summary>
internal class ComplexBattleAction : IBattleAction
{
    /// <summary>
    /// Создать объект типа <see cref="ComplexBattleAction" />.
    /// </summary>
    public ComplexBattleAction(IReadOnlyList<IBattleAction> actions)
    {
        Actions = actions;
    }

    /// <inheritdoc />
    public bool IsCompleted => Actions.All(a => a.IsCompleted);

    /// <summary>
    /// Действия.
    /// </summary>
    public IReadOnlyList<IBattleAction> Actions { get; }

    /// <inheritdoc />
    public void UpdateTime(long ticks)
    {
        foreach (var battleAction in Actions)
        {
            battleAction.UpdateTime(ticks);
        }
    }
}