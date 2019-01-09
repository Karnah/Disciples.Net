using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Battle.Models
{
    /// <summary>
    /// Параметры, необходимые для инициализации сцены боя.
    /// </summary>
    public class BattleSquadsData
    {
        /// <inheritdoc />
        public BattleSquadsData(Squad attackSquad, Squad defendSquad)
        {
            AttackSquad = attackSquad;
            DefendSquad = defendSquad;
        }


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