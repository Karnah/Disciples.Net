using System.Windows;
using System.Windows.Controls;

namespace Disciples.WPF.Models;

/// <summary>
/// Информация об игре.
/// </summary>
public class WpfGameInfo
{
    /// <summary>
    /// Игровое поле.
    /// </summary>
    public Grid GameFiled { get; set; } = null!;

    /// <summary>
    /// Окно с которым происходят все события.
    /// </summary>
    public Window OverlapWindow { get; set; } = null!;
}