using Avalonia;
using Avalonia.Controls;

namespace Disciples.Avalonia.Models;

/// <summary>
/// Информация об игре.
/// </summary>
public class AvaloniaGameInfo
{
    /// <summary>
    /// Позиция игрового поля относительно экрана.
    /// </summary>
    public Matrix FieldTransform { get; set; }

    /// <summary>
    /// Окно с которым происходят все события.
    /// </summary>
    public Window OverlapWindow { get; set; } = null!;
}