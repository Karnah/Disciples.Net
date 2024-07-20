using System;
using Avalonia;
using Avalonia.Controls;

namespace Disciples.Avalonia.Models;

/// <summary>
/// Информация об игре.
/// </summary>
public class AvaloniaGameInfo
{
    /// <summary>
    /// Признак, что игра запущена в ОС Windows.
    /// </summary>
    public bool IsWindows { get; } = OperatingSystem.IsWindows();

    /// <summary>
    /// Признак, что игра запущена в ОС Linux.
    /// </summary>
    public bool IsLinux { get; } = OperatingSystem.IsLinux();

    /// <summary>
    /// Позиция игрового поля относительно экрана.
    /// </summary>
    public Matrix FieldTransform { get; set; }

    /// <summary>
    /// Окно с которым происходят все события.
    /// </summary>
    public Window OverlapWindow { get; set; } = null!;
}