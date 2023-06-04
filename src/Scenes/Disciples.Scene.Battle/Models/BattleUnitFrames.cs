using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Кадры для анимации юнита во время битвы.
/// </summary>
internal class BattleUnitFrames
{
    /// <summary>
    /// Создать объект типа <see cref="BattleUnitFrames" />.
    /// </summary>
    public BattleUnitFrames(AnimationFrames? shadowFrames,
        AnimationFrames unitFrames,
        AnimationFrames? auraFrames)
    {
        ShadowFrames = shadowFrames;
        UnitFrames = unitFrames;
        AuraFrames = auraFrames;
    }


    /// <summary>
    /// Кадры для анимации тени.
    /// </summary>
    public AnimationFrames? ShadowFrames { get; }

    /// <summary>
    /// Кадры для анимации самого юнита.
    /// </summary>
    public AnimationFrames UnitFrames { get; }

    /// <summary>
    /// Кадры для анимации ауры юнита.
    /// </summary>
    public AnimationFrames? AuraFrames { get; }
}