using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;

namespace Disciples.Scene.LoadSaga.Controllers;

/// <summary>
/// Контроллер звук для сцены загрузки файла сохранения саги.
/// </summary>
internal class LoadSagaSoundController : BaseSupportLoading
{
    private readonly ISoundController _soundController;
    private readonly ISoundProvider _soundProvider;

    private IPlayingSound _backgroundSound = null!;

    /// <summary>
    /// Создать объект типа <see cref="LoadSagaSoundController" />.
    /// </summary>
    public LoadSagaSoundController(ISoundController soundController, ISoundProvider soundProvider)
    {
        _soundController = soundController;
        _soundProvider = soundProvider;
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
        return _soundController.PlayBackground(_soundProvider.MenuSound);
    }
}