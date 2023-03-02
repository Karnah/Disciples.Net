using System.Collections.Generic;

using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Игровой объект анимации.
/// </summary>
public class AnimationObject : GameObject
{
    public AnimationObject(
        ISceneObjectContainer sceneObjectContainer,
        IReadOnlyList<Frame> frames,
        double x,
        double y,
        int layer,
        bool repeat = true
    ) : base(x, y)
    {
        AnimationComponent = new AnimationComponent(this, sceneObjectContainer, frames, layer);

        Components = new IComponent[] {
            AnimationComponent
        };

        Repeat = repeat;
    }


    /// <summary>
    /// Компонент анимации.
    /// </summary>
    public AnimationComponent AnimationComponent { get; }

    /// <summary>
    /// Зациклена ли анимация.
    /// </summary>
    public bool Repeat { get; }


    /// <inheritdoc />
    public override bool IsInteractive => false;


    /// <inheritdoc />
    public override void Update(long ticksCount)
    {
        // Если анимация не зациклена, то объект уничтожает сам себя.
        // Такая проверка ужасна, так как можно переписать анимацию так, чтобы пропускались фреймы, но сейчас это работает.
        if (Repeat == false && AnimationComponent.FrameIndex == AnimationComponent.FramesCount - 1) {
            this.Destroy();
            return;
        }

        base.Update(ticksCount);
    }
}