namespace Disciples.Resources.Sounds.Models;

/// <summary>
/// Информация о звуках юнита.
/// </summary>
internal class SoundInfoRecord
{
    /// <summary>
    /// Идентификатор юнита.
    /// </summary>
    public int UnitTypeId { get; init; }

    /// <summary>
    /// Названия списка с атаками.
    /// </summary>
    public string AttackListName { get; init; } = null!;

    /// <summary>
    /// Индекс анимации юнита, когда необходимо начать проигрывание звука при атаке.
    /// </summary>
    public int BeginAttackSoundFrameIndex { get; init; }

    /// <summary>
    /// Индекс анимации юнита, когда необходимо начать проигрывание звука удара при атаке.
    /// </summary>
    public int EndAttackSoundFrameIndex { get; init; }

    /// <summary>
    /// Названия списка со звуками попадания по цели.
    /// </summary>
    public string HitTargetListName { get; init; } = null!;

    /// <summary>
    /// Когда начинать играть звук для файла из <see cref="HitTargetListName" />.
    /// </summary>
    public int BeginAttackHitSoundFrameIndex { get; init; }

    /// <summary>
    /// Когда закончить играть звук для файла из <see cref="HitTargetListName" />.
    /// </summary>
    public int EndAttackHitSoundFrameIndex { get; init; }

    /// <summary>
    /// Названия списка со звуками получения повреждения.
    /// </summary>
    public string DamagedListName { get; init; } = null!;

    /// <summary>
    /// Названия списка со звуками передвижения по глобальной карте.
    /// </summary>
    public string GlobalMapWalkListName { get; init; } = null!;
}