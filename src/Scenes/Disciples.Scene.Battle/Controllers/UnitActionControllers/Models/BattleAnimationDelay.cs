using Disciples.Engine.Common.Components;

namespace Disciples.Scene.Battle.Controllers.UnitActionControllers.Models;

/// <summary>
/// Ожидание определённого кадра анимации.
/// </summary>
internal class BattleAnimationDelay : IBattleActionDelay
{
    private readonly Action? _onCompleted;

    /// <summary>
    /// Создать объект типа <see cref="BattleAnimationDelay" />.
    /// </summary>
    public BattleAnimationDelay(BaseAnimationComponent animationComponent, int endFrameIndex, Action? onCompleted = null)
    {
        AnimationComponent = animationComponent;
        _onCompleted = onCompleted;

        // Бага ресурсов: иногда endFrameIndex > FramesCount.
        EndFrameIndex = Math.Min(endFrameIndex, animationComponent.FramesCount - 1);
    }

    /// <inheritdoc />
    public BattleAnimationDelay(BaseAnimationComponent animationComponent, Action? onCompleted = null) : this(animationComponent, GetAnimationComponentEndIndex(animationComponent), onCompleted)
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

    /// <inheritdoc />
    public void UpdateTime(long ticks)
    {
    }

    /// <inheritdoc />
    public void ProcessCompleted()
    {
        _onCompleted?.Invoke();
    }

    /// <summary>
    /// Получить индекс последнего кадра в анимации.
    /// </summary>
    private static int GetAnimationComponentEndIndex(BaseAnimationComponent animationComponent)
    {
        return animationComponent.FramesCount - 1;
    }
}