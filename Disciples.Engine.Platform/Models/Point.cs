namespace Disciples.Engine.Platform.Models
{
    /// <summary>
    /// Координаты точки.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Создать координаты точки.
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }


        /// <summary>
        /// Координата X.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Координата Y.
        /// </summary>
        public double Y { get; }
    }
}