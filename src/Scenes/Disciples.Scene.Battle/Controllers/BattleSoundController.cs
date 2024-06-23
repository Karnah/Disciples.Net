using Disciples.Engine;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;
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

    private IPlayingSound _backgroundSound = null!;

    /// <summary>
    /// Создать объект типа <see cref="BattleSoundController" />.
    /// </summary>
    public BattleSoundController(ISoundController soundController, ISoundProvider soundProvider, IBattleResourceProvider battleResourceProvider)
    {
        _soundController = soundController;
        _soundProvider = soundProvider;
        _battleResourceProvider = battleResourceProvider;
    }

    /// <summary>
    /// Проигрывать звук удаления призванного юнита с поля боя.
    /// </summary>
    public IPlayingSound PlayUnitUsummonSound()
    {
        // TODO Контроль удаления и отсутствие повторных вызовов.
        return _soundController.PlaySound(_battleResourceProvider.UnitUnsummonSound);
    }

    /// <summary>
    /// Проиграть звук.
    /// </summary>
    public IPlayingSound PlaySound(RawSound sound)
    {
        return _soundController.PlaySound(sound);
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