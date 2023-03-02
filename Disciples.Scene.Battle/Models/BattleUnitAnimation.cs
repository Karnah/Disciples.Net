using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Вся информация об анимации юнита.
/// </summary>
public class BattleUnitAnimation
{
    /// <summary>
    /// Создать объект типа <see cref="BattleUnitAnimation" />.
    /// </summary>
    public BattleUnitAnimation(
        Dictionary<BattleAction, BattleUnitFrames> battleUnitFrames,
        BattleUnitTargetAnimation? targetAnimation,
        IReadOnlyList<Frame> deathFrames)
    {
        BattleUnitFrames = battleUnitFrames;
        TargetAnimation = targetAnimation;
        DeathFrames = deathFrames;
    }


    /// <summary>
    /// Кадры анимации для каждого состояния юнита.
    /// </summary>
    public Dictionary<BattleAction, BattleUnitFrames> BattleUnitFrames { get; }

    /// <summary>
    /// Информации об анимации, которая применяются в юниту-цели.
    /// </summary>
    public BattleUnitTargetAnimation? TargetAnimation { get; }

    /// <summary>
    /// Анимация смерти юнита.
    /// </summary>
    public IReadOnlyList<Frame> DeathFrames { get; }
}