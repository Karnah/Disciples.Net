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
    public BattleUnitTargetAnimation(bool isSingle, IReadOnlyList<Frame> attackerDirectionFrames, IReadOnlyList<Frame> defenderDirectionFrames)
    {
        IsSingle = isSingle;
        AttackerDirectionFrames = attackerDirectionFrames;
        DefenderDirectionFrames = defenderDirectionFrames;
    }

    /// <summary>
    /// Анимация применяется к одному юниту, а не к площади.
    /// </summary>
    public bool IsSingle { get; }

    /// <summary>
    /// Кадры анимации, который применяются к юниту/отряду <see cref="BattleSquadPosition.Attacker" />.
    /// </summary>
    public IReadOnlyList<Frame> AttackerDirectionFrames { get; }

    /// <summary>
    /// Кадры анимации, который применяются к юниту/отряду <see cref="BattleSquadPosition.Defender" />.
    /// </summary>
    public IReadOnlyList<Frame> DefenderDirectionFrames { get; }
}