namespace Disciples.Engine.Common.Models
{
    /// <summary>
    /// Отряд юнитов.
    /// </summary>
    public class Squad
    {
        /// <inheritdoc />
        public Squad(Unit[] units)
        {
            Units = units;
        }


        /// <summary>
        /// Юниты в отряде.
        /// </summary>
        public Unit[] Units { get; }
    }
}