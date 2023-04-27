using Disciples.Engine;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Контроллер звуков для битвы.
/// </summary>
internal class BattleSoundController : BaseSupportLoading
{
    private readonly ISoundController _soundController;
    private readonly ISoundProvider _soundProvider;
    private readonly BattleSoundsExtractor _soundsExtractor;

    private IPlayingSound _backgroundSound = null!;

    /// <summary>
    /// Создать объект типа <see cref="BattleSoundController" />.
    /// </summary>
    public BattleSoundController(ISoundController soundController, ISoundProvider soundProvider, BattleSoundsExtractor soundsExtractor)
    {
        _soundController = soundController;
        _soundProvider = soundProvider;
        _soundsExtractor = soundsExtractor;
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => false;

    /// <summary>
    /// Проиграть звук атаки юнита.
    /// </summary>
    public IPlayingSound? PlayUnitAttack(string unitTypeId)
    {
        // TODO Использовать маппинг.
        var soundName = unitTypeId switch
        {
            "G000UU0154" => "UNIT154",
            "G000UU0002" => "ANIM2A", // ANIM2B, ANIM2C
            "G000UU0003" => "ANIM03A", // ANIM03B, ANIM03C
            _ => null
        };

        if (string.IsNullOrEmpty(soundName))
            return null;

        var rawSound = _soundsExtractor.GetSound(soundName);
        if (rawSound == null)
            return null;

        return _soundController.PlaySound(rawSound);
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