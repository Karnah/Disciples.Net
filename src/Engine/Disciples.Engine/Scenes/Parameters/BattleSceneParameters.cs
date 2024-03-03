using Disciples.Engine.Common;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Models;

namespace Disciples.Engine.Scenes.Parameters;

/// <summary>
/// Параметры, необходимые для инициализации сцены боя <see cref="IBattleScene" />.
/// </summary>
public class BattleSceneParameters : SceneParameters
{
    /// <summary>
    /// Создать объект типа <see cref="BattleSceneParameters" />.
    /// </summary>
    public BattleSceneParameters(GameContext gameContext, Squad attackingSquad, Squad defendingSquad)
    {
        GameContext = gameContext;
        AttackingSquad = attackingSquad;
        DefendingSquad = defendingSquad;
    }

    /// <summary>
    /// Данные игры.
    /// </summary>
    public GameContext GameContext { get; }

    /// <summary>
    /// Атакующий отряд.
    /// </summary>
    public Squad AttackingSquad { get; }

    /// <summary>
    /// Защищающийся отряд.
    /// </summary>
    public Squad DefendingSquad { get; }
}