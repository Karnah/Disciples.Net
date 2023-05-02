using Disciples.Resources.Sounds.Models;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Действие для ожидания момента, когда можно будет начать проигрывать звук атаки юнита.
/// </summary>
internal class MainAttackSoundBattleAction : AnimationBattleAction
{
    /// <summary>
    /// Создать объект типа <see cref="MainAttackSoundBattleAction" />.
    /// </summary>
    public MainAttackSoundBattleAction(BattleUnit battleUnit)
        : base(battleUnit.AnimationComponent, battleUnit.SoundComponent.Sounds.BeginAttackSoundFrameIndex - 1)
    {
        AttackSounds = battleUnit.SoundComponent.Sounds.AttackSounds;
    }

    /// <summary>
    /// Звуки атаки юнита.
    /// </summary>
    public IReadOnlyList<RawSound> AttackSounds { get; }
}