using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Images.Extensions;
using Disciples.Resources.Images.Models;

namespace Disciples.Resources.Images.Parsers;

/// <summary>
/// Парсер для элементов типа <see cref="TextBlockSceneElement" />
/// </summary>
internal class TextBlockSceneElementParser : BaseSceneElementParser
{
    /// <inheritdoc />
    public override string ElementTypeName => "TEXT";

    /// <inheritdoc />
    public override SceneElement Parse(string line, int offsetX, int offsetY)
    {
        var elements = line.Split(',');
        if (elements.Length < 8)
            throw new ResourceException($"Невозможно распарсить строку в строку {line}");

        return new TextBlockSceneElement
        {
            Name = elements[0],
            Position = elements.ParseBounds(1, offsetX, offsetY),
            TextStyle = elements[5].ParseTextStyle(),
            TextId = elements[6].ParseTextId(),
            ToolTipTextId = elements[7].ParseTextId(),
        };
    }
}