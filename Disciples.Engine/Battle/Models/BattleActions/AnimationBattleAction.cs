using System;
using Disciples.Engine.Common.Components;

namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Действие, продолжительность которого зависит от кадра анимации.
    /// </summary>
    public class AnimationBattleAction : IBattleAction
    {
        /// <inheritdoc />
        public AnimationBattleAction(BaseAnimationComponent animationComponent, int endFrameIndex)
        {
            if (endFrameIndex >= animationComponent.FramesCount)
                throw new ArgumentException("Кадр завершения больше количества кадров в анимации");

            AnimationComponent = animationComponent;
            EndFrameIndex = endFrameIndex;
        }

        /// <inheritdoc />
        public AnimationBattleAction(BaseAnimationComponent animationComponent) : this(animationComponent, animationComponent.FramesCount - 1)
        {
        }

        /// <inheritdoc />
        public bool IsEnded => AnimationComponent.FrameIndex >= EndFrameIndex;

        /// <summary>
        /// Компонент анимации от которого зависит продолжительность.
        /// </summary>
        public BaseAnimationComponent AnimationComponent { get; }

        /// <summary>
        /// Номер кадра анимации на котором завершится действие.
        /// </summary>
        public int EndFrameIndex { get; }
    }
}