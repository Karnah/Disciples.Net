﻿using System;
using System.Collections.Generic;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Анимация.
/// </summary>
public class AnimationSceneElement : SceneElement
{
    /// <inheritdoc />
    public override SceneElementType Type => SceneElementType.Animation;

    /// <summary>
    /// Кадры анимации.
    /// </summary>
    public IReadOnlyList<Frame> Frames { get; init; } = Array.Empty<Frame>();

    /// <summary>
    /// Текстовая подсказка при наведении на элемент.
    /// </summary>
    public TextContainer? ToolTip { get; init; }
}