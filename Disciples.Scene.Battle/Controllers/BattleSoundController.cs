﻿using Disciples.Engine;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Контроллер звуков для битвы.
/// </summary>
internal class BattleSoundController : BaseSupportLoading
{
    private readonly ISoundController _soundController;
    private readonly ISoundProvider _soundProvider;

    private IPlayingSound _backgroundSound = null!;

    /// <summary>
    /// Создать объект типа <see cref="BattleSoundController" />.
    /// </summary>
    public BattleSoundController(
        ISoundController soundController,
        ISoundProvider soundProvider)
    {
        _soundController = soundController;
        _soundProvider = soundProvider;
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => false;

    /// <summary>
    /// Проиграть случайный звук из списка.
    /// </summary>
    public IPlayingSound? PlayRandomSound(IReadOnlyList<RawSound> sounds)
    {
        if (sounds.Count == 0)
            return null;

        var randomSoundIndex = RandomGenerator.Get(sounds.Count);
        return _soundController.PlaySound(sounds[randomSoundIndex]);
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
        // TODO Проверять это раз в секунду, не чаще.
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
        _backgroundSound.Stop();
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