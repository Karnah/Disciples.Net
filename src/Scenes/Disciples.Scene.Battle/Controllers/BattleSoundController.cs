using Disciples.Engine;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Контроллер звуков для битвы.
/// </summary>
internal class BattleSoundController : BaseSupportLoading
{
    private readonly ISoundController _soundController;
    private readonly ISoundProvider _soundProvider;
    private readonly IBattleResourceProvider _battleResourceProvider;

    private readonly Dictionary<BattleSoundType, List<IPlayingSound>> _playingSounds = new();

    private IPlayingSound _backgroundSound = null!;

    /// <summary>
    /// Создать объект типа <see cref="BattleSoundController" />.
    /// </summary>
    public BattleSoundController(ISoundController soundController, ISoundProvider soundProvider, IBattleResourceProvider battleResourceProvider)
    {
        _soundController = soundController;
        _soundProvider = soundProvider;
        _battleResourceProvider = battleResourceProvider;

        foreach (var battleSoundType in Enum.GetValues<BattleSoundType>())
            _playingSounds[battleSoundType] = new List<IPlayingSound>();
    }

    /// <summary>
    /// Проиграть звук атаки.
    /// </summary>
    public void PlayAttackTypeSound(UnitAttackType attackType)
    {
        var attackSound = _battleResourceProvider.GetAttackTypeSound(attackType);
        if (attackSound == null)
            return;

        PlaySound(attackSound, BattleSoundType.Attack);
    }

    /// <summary>
    /// Проиграть звук атаки.
    /// </summary>
    public void PlayRandomAttackSound(IReadOnlyList<RawSound> attackSounds)
    {
        var attackSound = attackSounds.TryGetRandomElement();
        if (attackSound == null)
            return;

        PlaySound(attackSound, BattleSoundType.Attack);
    }

    /// <summary>
    /// Проигрывать звук удаления призванного юнита с поля боя.
    /// </summary>
    public void PlayUnitUsummonSound()
    {
        PlaySound(_battleResourceProvider.UnitUnsummonSound, BattleSoundType.Attack);
    }

    /// <summary>
    /// Проиграть звук получения урона.
    /// </summary>
    public void PlayRandomHitSound(IReadOnlyList<RawSound> hitSounds)
    {
        var hitSound = hitSounds.TryGetRandomElement();
        if (hitSound == null)
            return;

        PlaySound(hitSound, BattleSoundType.Hit);
    }

    /// <summary>
    /// Проиграть звук получения урона.
    /// </summary>
    public void PlayRandomDamagedSound(IReadOnlyList<RawSound> damagedSounds)
    {
        var damagedSound = damagedSounds.TryGetRandomElement();
        if (damagedSound == null)
            return;

        PlaySound(damagedSound, BattleSoundType.Damaged);
    }

    /// <summary>
    /// Проиграть звук смерти юнита.
    /// </summary>
    public void PlayUnitDeathSound()
    {
        PlaySound(_battleResourceProvider.UnitDeathSound, BattleSoundType.Death);
    }

    /// <summary>
    /// Проиграть звук повышения уровня.
    /// </summary>
    public void PlayUnitLevelUpSound()
    {
        PlaySound(_battleResourceProvider.UnitLevelUpSound, BattleSoundType.LevelUp);
    }

    /// <summary>
    /// Обработать события перед обновлением сцены.
    /// </summary>
    public void BeforeSceneUpdate()
    {

    }

    /// <summary>
    /// Обработать завершение обновлении сцены.
    /// </summary>
    public void AfterSceneUpdate()
    {
        if (_backgroundSound.IsCompleted)
            _backgroundSound = GetBackgroundSound();
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _backgroundSound = GetBackgroundSound();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        _backgroundSound.Dispose();

        foreach (var playingSound in _playingSounds.Values.SelectMany(ps => ps))
            playingSound.Dispose();
    }

    /// <summary>
    /// Проиграть звук указанного типа.
    /// </summary>
    private void PlaySound(RawSound sound, BattleSoundType soundType)
    {
        var playingSounds = _playingSounds[soundType];

        // Проигрываем новый звук, только если предыдущие завершились или проигрываются достаточно давно.
        // Необходимо, чтобы однотипные звуки не накладывались друг на друга.
        // 0.5 выбран опытным путём, чтобы последовательно срабатывали звуки unsummon.
        var canPlayNewSound = playingSounds.All(ps => ps.IsCompleted || ps.PlayingPosition.Divide(ps.Duration) > 0.5);
        if (!canPlayNewSound)
            return;

        var completedSounds = playingSounds
            .Where(ps => ps.IsCompleted)
            .ToList();
        foreach (var completedSound in completedSounds)
        {
            completedSound.Dispose();
            playingSounds.Remove(completedSound);
        }

        playingSounds.Add(_soundController.PlaySound(sound));
    }

    /// <summary>
    /// Получить музыку, которая будет проигрываться в фоне.
    /// </summary>
    private IPlayingSound GetBackgroundSound()
    {
        // TODO Сделать так, чтобы не повторялись одна и та же музыка.
        var sounds = _soundProvider.GetBackgroundBattleSounds();
        var soundIndex = RandomGenerator.Get(sounds.Count);
        return _soundController.PlayBackground(sounds[soundIndex]);
    }
}