namespace Disciples.Engine
{
    /// <summary>
    /// Изображение.
    /// </summary>
    public interface IBitmap
    {
        /// <summary>
        /// Ширина изображения.
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Высота изображения.
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Внутренний объект, содержащий информацию об изображении.
        /// </summary>
        /// <remarks>Тип зависит от используемой платформы - Avalonia, WPF и т.д.</remarks>
        object BitmapData { get; }
    }
}