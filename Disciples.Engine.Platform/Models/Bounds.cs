namespace Disciples.Engine.Platform.Models
{
    /// <summary>
    /// Информация о границах.
    /// </summary>
    public class Bounds
    {
        /// <inheritdoc />
        public Bounds(int minRow, int maxRow, int minColumn, int maxColumn)
        {
            MinRow = minRow;
            MaxRow = maxRow;
            MinColumn = minColumn;
            MaxColumn = maxColumn;
        }


        /// <summary>
        /// Минимальная строка.
        /// </summary>
        public int MinRow { get; }

        /// <summary>
        /// Максимальная строка.
        /// </summary>
        public int MaxRow { get; }

        /// <summary>
        /// Минимальная колонка.
        /// </summary>
        public int MinColumn { get; }

        /// <summary>
        /// Максимальная колонка.
        /// </summary>
        public int MaxColumn { get; }
    }
}