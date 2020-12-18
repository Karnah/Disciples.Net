using System;
using Disciples.Engine.Battle.Models.BattleActions;

namespace Disciples.Engine.Battle.Models
{
    /// <summary>
    /// Аргументы события начала или завершения действия.
    /// </summary>
    public class BattleActionEventArgs : EventArgs
    {
        /// <inheritdoc />
        public BattleActionEventArgs(IBattleAction battleAction)
        {
            BattleAction = battleAction;
        }

        /// <summary>
        /// Действие.
        /// </summary>
        public IBattleAction BattleAction { get; }
    }
}