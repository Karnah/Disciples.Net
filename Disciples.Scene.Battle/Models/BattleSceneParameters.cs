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
            Squad attackSquad,
            Squad defendSquad)
        {
            BattleController = battleController;
            BattleInterfaceController = battleInterfaceController;
            AttackSquad = attackSquad;
            DefendSquad = defendSquad;
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
        public Squad AttackSquad { get; }

        /// <summary>
        /// Защищающийся отряд.
        /// </summary>
        public Squad DefendSquad { get; }
    }
}