using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Images.Extensions;
using Disciples.Resources.Images.Models;

namespace Disciples.Resources.Images.Parsers;

/// <summary>
/// Парсер для элементов типа <see cref="TextListBoxSceneElement" />
/// </summary>
internal class TextListBoxSceneElementParser : BaseSceneElementParser
{
    /// <inheritdoc />
    public override string ElementTypeName => "TLBOX";

    /// <inheritdoc />
    public override SceneElement Parse(string line, int offsetX, int offsetY)
    {
        var elements = line.Split(',');
        if (elements.Length < 22)
            throw new ResourceException($"Невозможно распарсить строку в список строек {line}");

        return new TextListBoxSceneElement()
        {
            Name = elements[0],
            Position = elements.ParseBounds(1, offsetX, offsetY),
            ColumnCount = elements[5].ParseInt(),
            HorizontalSpacing = elements[6].ParseInt(),
            VerticalSpacing = elements[7].ParseInt(),
            ScrollUpButtonName = elements[8].ParseObjectName(),
            ScrollDownButtonName = elements[9].ParseObjectName(),
            ScrollLeftButtonName = elements[10].ParseObjectName(),
            ScrollRightButtonName = elements[11].ParseObjectName(),
            PageUpButtonName = elements[12].ParseObjectName(),
            PageDownButtonName = elements[13].ParseObjectName(),
            DoubleClickButtonName = elements[14].ParseObjectName(),
            SelectedTextStyle = elements[15].ParseTextStyle(),
            CommonTextStyle = elements[16].ParseTextStyle(),
            SelectionImageName = elements[17].ParseImageName(),
            UnselectedImageName = elements[18].ParseImageName(),
            BorderSize = elements[19].ParseInt(),
            ToolTipTextId = elements[20].ParseTextId(),
            ShouldCreateBackgroundImage = elements[21].ParseBoolean()
        };
    }
}