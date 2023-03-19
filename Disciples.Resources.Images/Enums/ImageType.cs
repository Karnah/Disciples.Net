namespace Disciples.Resources.Images.Enums;

/// <summary>
/// Тип изображения в ресурсах.
/// </summary>
internal enum ImageType
{
    /// <summary>
    /// Представляет собой обычное изображение.
    /// </summary>
    Simple,

    /// <summary>
    /// Тень. Должна быть полупрозрачна.
    /// </summary>
    Shadow,

    /// <summary>
    /// Аура. Прозрачность достигается за счёт индексов кистей в палитре.
    /// </summary>
    Aura
}