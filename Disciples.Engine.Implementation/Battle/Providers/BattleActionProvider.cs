using System;
using System.Collections.Generic;
using System.Linq;

using Disciples.Engine.Base;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Models.BattleActions;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Battle.Providers
{
    /// <inheritdoc cref="IBattleActionProvider" />
    public class BattleActionProvider : BaseSupportLoading, IBattleActionProvider
    {
        private readonly IGameController _gameController;
        private LinkedList<IBattleAction> _battleActions;

        /// <inheritdoc />
        public BattleActionProvider(IGameController gameController)
        {
            _gameController = gameController;
        }


        /// <inheritdoc />
        public bool IsInterfaceActive => !_battleActions.Any();

        /// <inheritdoc />
        public IReadOnlyList<IBattleAction> BattleActions => _battleActions.ToList();

        /// <inheritdoc />
        public override bool OneTimeLoading => false;


        /// <inheritdoc />
        public event EventHandler<BattleActionEventArgs> BattleActionBegin;

        /// <inheritdoc />
        public event EventHandler<BattleActionEventArgs> BattleActionEnd;


        /// <inheritdoc />
        public void RegisterBattleAction(IBattleAction battleAction)
        {
            _battleActions.AddLast(battleAction);
            BattleActionBegin?.Invoke(this, new BattleActionEventArgs(battleAction));
        }

        /// <inheritdoc />
        protected override void LoadInternal()
        {
            _battleActions = new LinkedList<IBattleAction>();
            _gameController.SceneEndUpdating += CheckActionsState;
        }

        /// <inheritdoc />
        protected override void UnloadInternal()
        {
            _gameController.SceneEndUpdating -= CheckActionsState;

            foreach (var battleAction in _battleActions) {
                BattleActionEnd?.Invoke(this, new BattleActionEventArgs(battleAction));
            }
            _battleActions = null;
        }


        /// <summary>
        /// Проверить состояние всех действий.
        /// </summary>
        private void CheckActionsState(object sender, SceneUpdatingEventArgs e)
        {
            if (!_battleActions.Any())
                return;

            // WaitAction завершаются, когда все прочие были завершены.
            var isAllWaited = _battleActions.All(ba => ba is WaitAction);

            for (var battleActionNode = _battleActions.First; battleActionNode != null;) {
                var nextNode = battleActionNode.Next;
                var battleAction = battleActionNode.Value;

                // Если действие завязано на времени, то обновляем счётчик.
                if (battleAction is BaseTimerBattleAction timerBattleAction) {
                    timerBattleAction.UpdateTime(e.TicksCount);
                }

                // Проверяем завершение анимации.
                if (isAllWaited || battleAction.IsEnded) {
                    _battleActions.Remove(battleActionNode);

                    BattleActionEnd?.Invoke(this, new BattleActionEventArgs(battleAction));
                }

                battleActionNode = nextNode;
            }
        }
    }
}