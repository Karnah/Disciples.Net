using Disciples.Resources.Images.Enums;

namespace Disciples.Resources.Images.Models;

/// <summary>
/// Кнопка-переключатель из двух состояний.
/// </summary>
public class ToggleButtonSceneElement : SceneElement
{
    /// <inheritdoc />
    public override SceneElementType Type => SceneElementType.ToggleButton;

    /// <summary>
    /// Название изображения с активным состоянием кнопки.
    /// </summary>
    public string? ActiveStateImageName { get; init; }

    /// <summary>
    /// Название изображения с состоянием кнопки, когда навели курсором.
    /// </summary>
    public string? HoverStateImageName { get; init; }

    /// <summary>
    /// Название изображения с состоянием кнопки, когда она нажата.
    /// </summary>
    public string? PressedStateImageName { get; init; }

    /// <summary>
    /// Название изображения с состоянием кнопки, когда она заблокирована.
    /// </summary>
    public string? DisabledStateImageName { get; init; }

    /// <summary>
    /// Название изображения с активным состоянием кнопки, когда переключатель включен.
    /// </summary>
    public string? CheckedActiveStateImageName { get; init; }

    /// <summary>
    /// Название изображения с состоянием кнопки, когда навели курсором и переключатель включен.
    /// </summary>
    public string? CheckedHoverStateImageName { get; init; }

    /// <summary>
    /// Название изображения с состоянием кнопки, когда она нажата и переключатель включен.
    /// </summary>
    public string? CheckedPressedStateImageName { get; init; }

    /// <summary>
    /// Идентификатор текстовой подсказки при наведении на элемент.
    /// </summary>
    public string? ToolTipTextId { get; init; }

    /// <summary>
    /// Список "горячих клавиш" для кнопки.
    /// </summary>
    public IReadOnlyList<int> HotKeys { get; init; } = Array.Empty<int>();
}