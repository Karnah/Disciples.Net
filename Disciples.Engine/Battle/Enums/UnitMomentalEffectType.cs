namespace Disciples.Engine.Battle.Enums
{
    /// <summary>
    /// Тип моментального эффекта, который произошел с юнитом.
    /// </summary>
    public enum UnitMomentalEffectType
    {
        /// <summary>
        /// Юнит получил повреждения.
        /// </summary>
        Damaged,

        /// <summary>
        /// Юнит был исцелён.
        /// </summary>
        Healed,

        /// <summary>
        /// По юниту промахнулись.
        /// </summary>
        Miss,

        /// <summary>
        /// Юнит защитился.
        /// </summary>
        Defended,

        /// <summary>
        /// Юнит ждёт.
        /// </summary>
        Waiting
    }
}