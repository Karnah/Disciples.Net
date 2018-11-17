using System.Collections.Generic;

using Engine.Common.Components;
using Engine.Common.Controllers;
using Engine.Common.Models;

namespace Engine.Common.GameObjects
{
    /// <summary>
    /// Игровой объект анимации.
    /// </summary>
    public class AnimationObject : GameObject
    {
        public AnimationObject(IMapVisual mapVisual, IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true)
            : base(x, y)
        {
            AnimationComponent = new AnimationComponent(this, mapVisual, frames, layer);

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
        public override void OnUpdate(long ticksCount)
        {
            // Если анимация не зациклена, то объект уничтожает сам себя
            // Такая проверка ужасна, так как можно переписать анимацию так, чтобы пропускались фреймы, но сейчас это работает.
            if (Repeat == false && AnimationComponent.FrameIndex == AnimationComponent.FramesCount - 1) {
                this.Destroy();
                return;
            }

            base.OnUpdate(ticksCount);
        }
    }
}
