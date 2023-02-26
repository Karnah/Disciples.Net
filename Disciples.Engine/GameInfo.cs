namespace Disciples.Engine
{
    /// <summary>
    /// Информация об игре.
    /// </summary>
    public static class GameInfo
    {
        /// <summary>
        /// Оригинальная ширина экрана.
        /// </summary>
        public const double OriginalWidth = 800;

        /// <summary>
        /// Оригинальная высота экрана.
        /// </summary>
        public const double OriginalHeight = 600;


        /// <summary>
        /// Текущая ширина экрана.
        /// </summary>
        public static double Width { get; set; } = 1440;

        /// <summary>
        /// Текущая высота экрана.
        /// </summary>
        public static double Height { get; set; } = 1080;


        /// <summary>
        /// Масштаб экрана относительного базового разрешения.
        /// </summary>
        public static double Scale { get; set; } = Height / OriginalHeight;
    }
}