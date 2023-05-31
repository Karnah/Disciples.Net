using System.Linq;
using Avalonia.Media;
using Disciples.Engine.Models;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Получить цвет фона для текста.
/// </summary>
public class TextBackgroundConverter : BaseTextConverter<IBrush?>
{
    /// <inheritdoc />
    protected override IBrush? Convert(TextContainer textContainer, TextStyle textStyle)
    {
        return GetBrush(
            textContainer.TextPieces.FirstOrDefault()?.Style.BackgroundColor ??
            textStyle.BackgroundColor);
    }
}