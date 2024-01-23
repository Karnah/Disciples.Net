using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Класс анимации, которая применяется к атакуемому юниту (например, удары магов или лечение целителей).
/// </summary>
internal class BattleUnitTargetAnimation
{
    /// <summary>
    /// Создать объект типа <see cref="BattleUnitTargetAnimation" />.
    /// </summary>
    public BattleUnitTargetAnimation(AnimationFrames? attackerUnitFrames, AnimationFrames? defenderUnitFrames, AnimationFrames? attackerAreaFrames, AnimationFrames? defenderAreaFrames)
    {
        AttackerUnitFrames = attackerUnitFrames;
        DefenderUnitFrames = defenderUnitFrames;
        AttackerAreaFrames = attackerAreaFrames;
        DefenderAreaFrames = defenderAreaFrames;
    }

    /// <summary>
    /// Кадры анимации, который применяются к юниту из отряда <see cref="BattleSquadPosition.Attacker" />.
    /// </summary>
    public AnimationFrames? AttackerUnitFrames { get; }

    /// <summary>
    /// Кадры анимации, который применяются к юниту из отряда <see cref="BattleSquadPosition.Defender" />.
    /// </summary>
    public AnimationFrames? DefenderUnitFrames { get; }

    /// <summary>
    /// Кадры анимации, который применяются к области <see cref="BattleSquadPosition.Attacker" />.
    /// </summary>
    public AnimationFrames? AttackerAreaFrames { get; }

    /// <summary>
    /// Кадры анимации, который применяются к области <see cref="BattleSquadPosition.Defender" />.
    /// </summary>
    public AnimationFrames? DefenderAreaFrames { get; }
}