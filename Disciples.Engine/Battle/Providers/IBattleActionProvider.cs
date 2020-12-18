using System;
using System.Collections.Generic;

using Disciples.Engine.Base;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Models.BattleActions;

namespace Disciples.Engine.Battle.Providers
{
    /// <summary>
    /// Шина событий действий на поле боя.
    /// </summary>
    public interface IBattleActionProvider : ISupportLoading
    {
        /// <summary>
        /// Проверить, что не существуют ли действия, блокирующие команды пользователя.
        /// </summary>
        bool IsInterfaceActive { get; }

        /// <summary>
        /// Действия на поле боя.
        /// </summary>
        IReadOnlyList<IBattleAction> BattleActions { get; }


        /// <summary>
        /// Событие начала действия.
        /// </summary>
        event EventHandler<BattleActionEventArgs> BattleActionBegin;

        /// <summary>
        /// Событие завершения действия.
        /// </summary>
        event EventHandler<BattleActionEventArgs> BattleActionEnd;


        /// <summary>
        /// Зарегистрировать новое действие на поле битвы.
        /// </summary>
        void RegisterBattleAction(IBattleAction battleAction);
    }
}