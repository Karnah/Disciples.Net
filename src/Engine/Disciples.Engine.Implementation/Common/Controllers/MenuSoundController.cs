using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <summary>
/// Контроллер звук для сцен меню.
/// </summary>
public class MenuSoundController : BaseSupportLoading
{
    private readonly ISoundController _soundController;
    private readonly ISoundProvider _soundProvider;

    private IPlayingSound? _backgroundSound;

    /// <summary>
    /// Создать объект типа <see cref="MenuSoundController" />.
    /// </summary>
    public MenuSoundController(ISoundController soundController, ISoundProvider soundProvider)
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
        if (_backgroundSound?.IsCompleted != false)
            _backgroundSound = GetBackgroundSound();
    }

    /// <summary>
    /// Остановить проигрывание музыки в главном меню.
    /// </summary>
    public void Stop()
    {
        _backgroundSound?.Stop();
        _backgroundSound = null;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        if (_backgroundSound?.IsCompleted != false)
            _backgroundSound = GetBackgroundSound();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Получить музыку, которая будет проигрываться в фоне.
    /// </summary>
    private IPlayingSound GetBackgroundSound()
    {
        return _soundController.PlayBackground(_soundProvider.MenuSound);
    }
}