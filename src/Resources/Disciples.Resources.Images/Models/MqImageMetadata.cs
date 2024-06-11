using System.Drawing;

namespace Disciples.Resources.Images.Models;

/// <summary>
/// Дополнительные данные для изображения.
/// </summary>
internal class MqImageMetadata
{
    /// <summary>
    /// Создать объект типа <see cref="MqImageMetadata" />.
    /// </summary>
    public MqImageMetadata(int transparentColorIndex, int opacityAlgorithm, int sizeX, int sizeY, IReadOnlyList<Color> palette)
    {
        TransparentColorIndex = transparentColorIndex;
        OpacityAlgorithm = opacityAlgorithm;
        SizeX = sizeX;
        SizeY = sizeY;
        Palette = palette;
    }

    /// <summary>
    /// Идентификатор прозрачного цвета в палитре.
    /// </summary>
    public int TransparentColorIndex { get; }

    /// <summary>
    /// Алгоритм прозрачности.
    /// </summary>
    /// <remarks>
    /// Если <see cref="OpacityAlgorithm" /> меньше или равно 255, то значение прозрачности.
    /// Иначе обрабатывается отдельно.
    /// </remarks>
    public int OpacityAlgorithm { get; }

    /// <summary>
    /// Размер базового изображения.
    /// </summary>
    public int SizeX { get; }

    /// <summary>
    /// Размер базового изображения.
    /// </summary>
    public int SizeY { get; }

    /// <summary>
    /// Палитра цветов.
    /// </summary>
    public IReadOnlyList<Color> Palette { get; }
}
