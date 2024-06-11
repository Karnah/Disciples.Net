namespace Disciples.Resources.Images.Models;

/// <summary>
/// Фрагмент кадра изображения.
/// </summary>
internal class MqImagePart
{
    /// <summary>
    /// Создать объект типа <see cref="MqImagePart" />.
    /// </summary>
    public MqImagePart(int sourceX, int sourceY, int destX, int destY, int width, int height)
    {
        SourceX = sourceX;
        SourceY = sourceY;
        DestX = destX;
        DestY = destY;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Координата X в новом изображении.
    /// </summary>
    public int SourceX { get; }

    /// <summary>
    /// Координата Y в новом изображении.
    /// </summary>
    public int SourceY { get; }

    /// <summary>
    /// Координата X в базовом изображении.
    /// </summary>
    public int DestX { get; }

    /// <summary>
    /// Координата Y в базовом изображении.
    /// </summary>
    public int DestY { get; }

    /// <summary>
    /// Ширина части изображения.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Высота части изображения.
    /// </summary>
    public int Height { get; }
}