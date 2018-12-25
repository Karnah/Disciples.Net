namespace Disciples.Engine.Battle.Enums
{
    /// <summary>
    /// Состояние сражения.
    /// </summary>
    public enum BattleState
    {
        /// <summary>
        /// Ожидание действия от игрока.
        /// </summary>
        WaitingAction,

        /// <summary>
        /// Задержка после того, как пользователь совершил действие.
        /// </summary>
        /// <remarks>Используется при ожидании и защите, чтобы ход не переходил слишком быстро.</remarks>
        Delay,

        /// <summary>
        /// Юнит замахивается для удара.
        /// </summary>
        AfterTouch,

        /// <summary>
        /// Юнит уже ударил.
        /// </summary>
        BeforeTouch,

        /// <summary>
        /// Сражение завершено.
        /// </summary>
        BattleEnd
    }
}