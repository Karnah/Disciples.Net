using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Действие атаки одного юнита на другого.
/// </summary>
internal class MainAttackBattleAction : AnimationBattleAction
{
    /// <inheritdoc />
    public MainAttackBattleAction(BattleUnit attacker, BattleUnit target)
        : base(attacker.AnimationComponent, attacker.SoundComponent.Sounds.EndAttackSoundFrameIndex - 1)
    {
        Attacker = attacker;
        Target = target;
    }

    /// <summary>
    /// Юнит, который атаковал.
    /// </summary>
    public BattleUnit Attacker { get; }

    /// <summary>
    /// Юнит, который являлся целью атаки.
    /// </summary>
    public BattleUnit Target { get; }
}