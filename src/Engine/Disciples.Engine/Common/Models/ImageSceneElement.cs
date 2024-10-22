﻿using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Изображение.
/// </summary>
public class ImageSceneElement : SceneElement
{
    /// <inheritdoc />
    public override SceneElementType Type => SceneElementType.Image;

    /// <summary>
    /// Изображение.
    /// </summary>
    public IBitmap? ImageBitmap { get; init; }
}