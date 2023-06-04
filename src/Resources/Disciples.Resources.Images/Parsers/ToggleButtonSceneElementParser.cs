using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Images.Extensions;
using Disciples.Resources.Images.Models;

namespace Disciples.Resources.Images.Parsers;

/// <summary>
/// Парсер для элементов типа <see cref="ToggleButtonSceneElement" />
/// </summary>
internal class ToggleButtonSceneElementParser : BaseSceneElementParser
{
    /// <inheritdoc />
    public override string ElementTypeName => "TOGGLE";

    /// <inheritdoc />
    public override SceneElement Parse(string line, int offsetX, int offsetY)
    {
        var elements = line.Split(',');
        if (elements.Length < 13)
            throw new ResourceException($"Невозможно распарсить строку в кнопку-переключатель {line}");

        return new ToggleButtonSceneElement
        {
            Name = elements[0],
            Position = elements.ParseBounds(1, offsetX, offsetY),
            ActiveStateImageName = elements[5].ParseImageName(),
            HoverStateImageName = elements[6].ParseImageName(),
            PressedStateImageName = elements[7].ParseImageName(),
            DisabledStateImageName = elements[8].ParseImageName(),
            CheckedActiveStateImageName = elements[9].ParseImageName(),
            CheckedHoverStateImageName = elements[10].ParseImageName(),
            CheckedPressedStateImageName = elements[11].ParseImageName(),
            ToolTipTextId = elements[12].ParseTextId(),
            HotKeys = elements.Skip(13).Select(int.Parse).ToArray()
        };
    }
}