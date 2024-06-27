using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Кадры анимации юнита.
/// </summary>
internal class UnitAnimationFrames
{
    /// <summary>
    /// Создать объект типа <see cref="UnitAnimationFrames" />.
    /// </summary>
    public UnitAnimationFrames(AnimationFrames smallUnitAnimationFrames, AnimationFrames bigUnitAnimationFrames)
    {
        SmallUnitAnimationFrames = smallUnitAnimationFrames;
        BigUnitAnimationFrames = bigUnitAnimationFrames;
    }

    /// <summary>
    /// Кадры анимации для маленького юнита.
    /// </summary>
    public AnimationFrames SmallUnitAnimationFrames { get; }

    /// <summary>
    /// Кадры анимации для большого юнита.
    /// </summary>
    public AnimationFrames BigUnitAnimationFrames { get; }

    /// <summary>
    /// Получить кадры анимации для указанного юнита.
    /// </summary>
    public AnimationFrames GetAnimationFrames(Unit unit)
    {
        return GetAnimationFrames(unit.UnitType.IsSmall);
    }

    /// <summary>
    /// Получить кадры анимации для юнита указанного размера.
    /// </summary>
    public AnimationFrames GetAnimationFrames(bool isSmallUnit)
    {
        return isSmallUnit
            ? SmallUnitAnimationFrames
            : BigUnitAnimationFrames;
    }
}
