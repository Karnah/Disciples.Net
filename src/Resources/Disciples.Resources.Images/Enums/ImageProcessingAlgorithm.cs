namespace Disciples.Resources.Images.Enums;

/// <summary>
/// Алгоритм обработки изображения.
/// </summary>
public enum ImageProcessingAlgorithm
{
    /// <summary>
    /// Режим по умолчанию.
    /// Альфа-канал настраивается согласно метаданным и стандартным кистям.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Режим тени.
    /// Все непрозрачные пиксели становятся полупрозрачными.
    /// </summary>
    Shadow = 1
}
