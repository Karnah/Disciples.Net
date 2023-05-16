using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Images.Extensions;
using Disciples.Resources.Images.Models;

namespace Disciples.Resources.Images.Parsers;

/// <summary>
/// Парсер для элементов типа <see cref="ImageSceneElement" />
/// </summary>
internal class ImageSceneElementParser : BaseSceneElementParser
{
    /// <inheritdoc />
    public override string ElementTypeName => "IMAGE";

    /// <inheritdoc />
    public override SceneElement Parse(string line)
    {
        var elements = line.Split(',');
        if (elements.Length < 7)
            throw new ResourceException($"Невозможно распарсить строку в изображение {line}");

        return new ImageSceneElement
        {
            Name = elements[0],
            Position = elements.ParseBounds(1),
            ImageName = elements[5].ParseImageName(),
            ToolTipTextId = elements[6].ParseTextId(),
        };
    }
}