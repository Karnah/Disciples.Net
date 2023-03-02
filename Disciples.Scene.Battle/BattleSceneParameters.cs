using Disciples.Engine.Common.Models;
using Disciples.Engine.Models;

namespace Disciples.Scene.Battle;

/// <summary>
/// Параметры, необходимые для инициализации сцены боя.
/// </summary>
public class BattleSceneParameters : SceneParameters
{
    /// <inheritdoc />
    public BattleSceneParameters(
        Squad attackingSquad,
        Squad defendingSquad)
    {
        AttackingSquad = attackingSquad;
        DefendingSquad = defendingSquad;
    }

    /// <summary>
    /// Атакующий отряд.
    /// </summary>
    public Squad AttackingSquad { get; }

    /// <summary>
    /// Защищающийся отряд.
    /// </summary>
    public Squad DefendingSquad { get; }
}