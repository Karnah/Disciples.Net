using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Images.Extensions;
using Disciples.Resources.Images.Models;

namespace Disciples.Resources.Images.Parsers;

/// <summary>
/// Парсер для элементов типа <see cref="ButtonSceneElement" />
/// </summary>
internal class ButtonSceneElementParser : BaseSceneElementParser
{
    /// <inheritdoc />
    public override string ElementTypeName => "BUTTON";

    /// <inheritdoc />
    public override SceneElement Parse(string line, int offsetX, int offsetY)
    {
        var elements = line.Split(',');
        if (elements.Length < 11)
            throw new ResourceException($"Невозможно распарсить строку в кнопку {line}");

        return new ButtonSceneElement
        {
            Name = elements[0],
            Position = elements.ParseBounds(1, offsetX, offsetY),
            ActiveStateImageName = elements[5].ParseImageName(),
            HoverStateImageName = elements[6].ParseImageName(),
            PressedStateImageName = elements[7].ParseImageName(),
            DisabledStateImageName = elements[8].ParseImageName(),
            ToolTipTextId = elements[9].ParseTextId(),
            IsRepeat = elements[10].ParseBoolean(),
            HotKeys = elements.Skip(11).Select(int.Parse).ToArray()
        };
    }
}