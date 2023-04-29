using Disciples.Resources.Sounds.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Информация о звуках юнита.
/// </summary>
internal class BattleUnitSounds
{
    /// <summary>
    /// Звуки атаки юнита.
    /// </summary>
    public IReadOnlyList<RawSound> AttackSounds { get; init; } = Array.Empty<RawSound>();

    /// <summary>
    /// Индекс анимации юнита, когда необходимо начать проигрывание звука при атаке.
    /// </summary>
    public int BeginAttackSoundFrameIndex { get; init; }

    /// <summary>
    /// Индекс анимации юнита, когда необходимо начать проигрывание звука удара при атаке.
    /// </summary>
    public int EndAttackSoundFrameIndex { get; init; }

    /// <summary>
    /// Звуки попадания по цели.
    /// </summary>
    public IReadOnlyList<RawSound> HitTargetSounds { get; init; } = Array.Empty<RawSound>();

    /// <summary>
    /// Когда начинать играть звук для файла из <see cref="HitTargetSounds" />.
    /// </summary>
    public int BeginAttackHitSoundFrameIndex { get; init; }

    /// <summary>
    /// Когда закончить играть звук для файла из <see cref="HitTargetSounds" />.
    /// </summary>
    public int EndAttackHitSoundFrameIndex { get; init; }

    /// <summary>
    /// Звуки получения повреждения.
    /// </summary>
    public IReadOnlyList<RawSound> DamagedSounds { get; init; } = Array.Empty<RawSound>();
}