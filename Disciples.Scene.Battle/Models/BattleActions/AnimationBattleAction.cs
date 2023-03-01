using Disciples.Engine.Common.Components;

namespace Disciples.Scene.Battle.Models.BattleActions
{
    /// <summary>
    /// Действие, продолжительность которого зависит от кадра анимации.
    /// </summary>
    public class AnimationBattleAction : IBattleAction
    {
        /// <summary>
        /// Создать объект типа <see cref="AnimationBattleAction" />.
        /// </summary>
        public AnimationBattleAction(BaseAnimationComponent animationComponent, int endFrameIndex)
        {
            if (endFrameIndex >= animationComponent.FramesCount)
                throw new ArgumentException("Кадр завершения больше количества кадров в анимации");

            AnimationComponent = animationComponent;
            EndFrameIndex = endFrameIndex;
        }

        /// <inheritdoc />
        public AnimationBattleAction(BaseAnimationComponent animationComponent) : this(animationComponent, GetAnimationComponentEndIndex(animationComponent))
        {
        }

        /// <inheritdoc />
        public bool IsCompleted => AnimationComponent.FrameIndex >= EndFrameIndex;

        /// <summary>
        /// Компонент анимации от которого зависит продолжительность.
        /// </summary>
        public BaseAnimationComponent AnimationComponent { get; }

        /// <summary>
        /// Номер кадра анимации на котором завершится действие.
        /// </summary>
        public int EndFrameIndex { get; }

        /// <summary>
        /// Получить индекс последнего кадра в анимации.
        /// </summary>
        private static int GetAnimationComponentEndIndex(BaseAnimationComponent animationComponent)
        {
            // Обязательно обновляем состояние компонента, чтобы он был в актуальном состоянии.
            animationComponent.Update(0);

            return animationComponent.FramesCount - 1;
        }
    }
}