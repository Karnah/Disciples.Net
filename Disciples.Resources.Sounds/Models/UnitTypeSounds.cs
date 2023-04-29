namespace Disciples.Resources.Sounds.Models;

/// <summary>
/// Звуки юнита.
/// </summary>
public class UnitTypeSounds
{
    /// <summary>
    /// Идентификатор типа юнита.
    /// </summary>
    public string UnitTypeId { get; init; } = null!;

    /// <summary>
    /// Названия звуков атаки.
    /// </summary>
    public IReadOnlyList<string> AttackSounds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Индекс анимации юнита, когда необходимо начать проигрывание звука при атаке.
    /// </summary>
    public int BeginAttackSoundFrameIndex { get; init; }

    /// <summary>
    /// Индекс анимации юнита, когда необходимо начать проигрывание звука удара при атаке.
    /// </summary>
    public int EndAttackSoundFrameIndex { get; init; }

    /// <summary>
    /// Названия звуков попадания по цели.
    /// </summary>
    public IReadOnlyList<string> HitTargetSounds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Когда начинать играть звук для файла из <see cref="HitTargetSounds" />.
    /// </summary>
    public int BeginAttackHitSoundFrameIndex { get; init; }

    /// <summary>
    /// Когда закончить играть звук для файла из <see cref="HitTargetSounds" />.
    /// </summary>
    public int EndAttackHitSoundFrameIndex { get; init; }

    /// <summary>
    /// Названия звуков получения повреждения.
    /// </summary>
    public IReadOnlyList<string> DamagedSounds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Названия звуков передвижения по глобальной карте.
    /// </summary>
    public IReadOnlyList<string> GlobalMapWalkSounds { get; init; } = Array.Empty<string>();
}