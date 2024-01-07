using System.Collections.Generic;
using Disciples.Common.Models;
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
    private readonly AnimationFrames _frames;

    private IImageSceneObject? _animationFrame;

    /// <inheritdoc />
    public AnimationComponent(GameObject gameObject,
        ISceneObjectContainer sceneObjectContainer,
        AnimationFrames frames,
        int layer,
        PointD? animationOffset = null
        ) : base(gameObject, sceneObjectContainer, layer, animationOffset)
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