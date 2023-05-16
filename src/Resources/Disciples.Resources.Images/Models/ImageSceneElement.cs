using Disciples.Resources.Images.Enums;

namespace Disciples.Resources.Images.Models;

/// <summary>
/// Изображение.
/// </summary>
public class ImageSceneElement : SceneElement
{
    /// <inheritdoc />
    public override SceneElementType Type => SceneElementType.Image;

    /// <summary>
    /// Имя изображения в ресурсах.
    /// </summary>
    public string? ImageName { get; init; }

    /// <summary>
    /// Идентификатор текстовой подсказки при наведении на элемент.
    /// </summary>
    public string? ToolTipTextId { get; init; }
}