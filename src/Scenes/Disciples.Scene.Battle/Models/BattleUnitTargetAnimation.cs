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
    public BattleUnitTargetAnimation(AnimationFrames? attackerDirectionFrames, AnimationFrames? defenderDirectionFrames, AnimationFrames? areaFrames)
    {
        AttackerDirectionFrames = attackerDirectionFrames;
        DefenderDirectionFrames = defenderDirectionFrames;
        AreaFrames = areaFrames;
    }

    /// <summary>
    /// Кадры анимации, который применяются к юниту/отряду <see cref="BattleSquadPosition.Attacker" />.
    /// </summary>
    public AnimationFrames? AttackerDirectionFrames { get; }

    /// <summary>
    /// Кадры анимации, который применяются к юниту/отряду <see cref="BattleSquadPosition.Defender" />.
    /// </summary>
    public AnimationFrames? DefenderDirectionFrames { get; }

    /// <summary>
    /// Кадры анимации, применяемые к области.
    /// </summary>
    public AnimationFrames? AreaFrames { get; }
}