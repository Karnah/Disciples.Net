using Disciples.Engine.Common.Enums;

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
    public AnimationFrames Frames { get; init; } = null!;
}