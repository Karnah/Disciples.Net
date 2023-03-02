using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Событие начала дополнительной атаки указанного.
/// </summary>
internal class BeginSecondaryAttackBattleAction : BaseEventBattleAction
{
    /// <summary>
    /// Создать объект типа <see cref="BeginSecondaryAttackBattleAction" />.
    /// </summary>
    /// <param name="attacker">Юнит, который атаковал.</param>
    /// <param name="target">Юнит, который являлся целью атаки.</param>
    public BeginSecondaryAttackBattleAction(BattleUnit attacker, BattleUnit target)
    {
        Attacker = attacker;
        Target = target;
    }

    /// <summary>
    /// Создать объект типа <see cref="BeginSecondaryAttackBattleAction" />.
    /// </summary>
    /// <param name="attacker">Юнит, который атаковал.</param>
    /// <param name="target">Юнит, который являлся целью атаки.</param>
    /// <param name="power">Сила атаки.</param>
    public BeginSecondaryAttackBattleAction(BattleUnit attacker, BattleUnit target, int power)
    {
        Attacker = attacker;
        Target = target;
        Power = power;
    }

    /// <summary>
    /// Юнит, который атаковал.
    /// </summary>
    public BattleUnit Attacker { get; }

    /// <summary>
    /// Юнит, который являлся целью атаки.
    /// </summary>
    public BattleUnit Target { get; }

    /// <summary>
    /// Сила атаки.
    /// </summary>
    /// <remarks>
    /// Нужна в тех случаях, когда сила зависит от первой атаки. Например, если восстанавливаем здоровье за счёт урона.
    /// </remarks>
    public int? Power { get; }
}