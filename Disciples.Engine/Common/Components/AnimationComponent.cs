using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.Components;

/// <summary>
/// Компонент для создания анимации.
/// </summary>
public class AnimationComponent : BaseAnimationComponent
{
    private readonly IReadOnlyList<Frame> _frames;

    private IImageSceneObject? _animationFrame;

    /// <inheritdoc />
    public AnimationComponent(
        GameObject gameObject,
        ISceneController sceneController,
        IReadOnlyList<Frame> frames,
        int layer
    ) : base(gameObject, sceneController, layer)
    {
        _frames = frames;

        FramesCount = _frames.Count;
    }


    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        UpdatePosition(ref _animationFrame, _frames);
    }

    /// <inheritdoc />
    protected override void OnAnimationFrameChange()
    {
        UpdateFrame(_animationFrame, _frames);
    }

    /// <inheritdoc />
    protected override IReadOnlyList<IImageSceneObject?> GetAnimationHosts()
    {
        return new[] { _animationFrame };
    }
}