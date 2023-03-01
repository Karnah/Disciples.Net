using Disciples.Engine.Common.Models;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Controllers;

namespace Disciples.Scene.Battle.Models
{
    /// <summary>
    /// Параметры, необходимые для инициализации сцены боя.
    /// </summary>
    public class BattleSceneParameters : SceneParameters
    {
        /// <inheritdoc />
        public BattleSceneParameters(
            IBattleController battleController,
            IBattleInterfaceController battleInterfaceController,
            Squad attackingSquad,
            Squad defendingSquad)
        {
            BattleController = battleController;
            BattleInterfaceController = battleInterfaceController;
            AttackingSquad = attackingSquad;
            DefendingSquad = defendingSquad;
        }


        /// <summary>
        /// Контроллер управления битвой.
        /// </summary>
        public IBattleController BattleController { get; }

        /// <summary>
        /// Контроллер управления интерфейсом.
        /// </summary>
        public IBattleInterfaceController BattleInterfaceController { get; }

        /// <summary>
        /// Атакующий отряд.
        /// </summary>
        public Squad AttackingSquad { get; }

        /// <summary>
        /// Защищающийся отряд.
        /// </summary>
        public Squad DefendingSquad { get; }
    }
}