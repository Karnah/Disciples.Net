using Disciples.Common.Models;

namespace Disciples.Engine;

/// <summary>
/// Изображение.
/// </summary>
public interface IBitmap
{
    /// <summary>
    /// Размеры изображения в ресурсах.
    /// </summary>
    SizeD OriginalSize { get; }

    /// <summary>
    /// Реальный размер изображения <see cref="BitmapData" />.
    /// </summary>
    SizeD ActualSize { get; }

    /// <summary>
    /// Смещение <see cref="BitmapData" /> относительно размеров <see cref="OriginalSize" />.
    /// </summary>
    PointD Offset { get; }

    /// <summary>
    /// Внутренний объект, содержащий информацию об изображении.
    /// </summary>
    /// <remarks>Тип зависит от используемой платформы - Avalonia, WPF и т.д.</remarks>
    object BitmapData { get; }
}