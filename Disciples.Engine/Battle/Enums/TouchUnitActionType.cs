namespace Disciples.Engine.Battle.Enums
{
    /// <summary>
    /// Тип воздействия на юнита.
    /// </summary>
    public enum TouchUnitActionType
    {
        /// <summary>
        /// Попадание атаки в этого юнита.
        /// </summary>
        Attack,

        /// <summary>
        /// Промах.
        /// </summary>
        Miss,

        /// <summary>
        /// Защита.
        /// </summary>
        Defend,

        /// <summary>
        /// Ожидание.
        /// </summary>
        Waiting,

        /// <summary>
        /// Смерть.
        /// </summary>
        Death,

        /// <summary>
        /// Наложение эффекта.
        /// </summary>
        /// <remarks>
        /// Отравления, усиления, проклятия и т.д.
        /// </remarks>
        Effect
    }
}