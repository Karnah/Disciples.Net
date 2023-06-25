﻿using System.Windows;

namespace Disciples.WPF;

/// <summary>
/// Окно, которое перекрывает основное.
/// </summary>
/// <remarks>
/// Необходимо, так как используется нативный контрол для отображения видео.
/// Этот контрол не отлавливает события мыши. Поэтому сверху располагается прозрачное окно, которое эти события перехватывает.
/// </remarks>
public partial class OverlapWindow : Window
{
    /// <summary>
    /// Создать объект типа <see cref="OverlapWindow" />.
    /// </summary>
    public OverlapWindow()
    {
        InitializeComponent();
    }
}