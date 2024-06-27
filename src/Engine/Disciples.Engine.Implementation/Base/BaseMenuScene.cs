using System.IO;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Base;

/// <summary>
/// Базовый класс для сцен меню.
/// </summary>
public abstract class BaseMenuScene : BaseScene
{
    private readonly IInterfaceProvider _interfaceProvider;

    private VideoGameObject? _transitionVideo;

    /// <summary>
    /// Создать объект типа <see cref="BaseMenuScene" />.
    /// </summary>
    protected BaseMenuScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IDialogController dialogController,
        IInterfaceProvider interfaceProvider
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController)
    {
        _interfaceProvider = interfaceProvider;
    }

    /// <summary>
    /// Имя для анимации перехода на указанную сцену.
    /// </summary>
    protected virtual string? TransitionAnimationName => null;

    /// <inheritdoc />
    protected override GameObject? MainInputGameObject => _transitionVideo;

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        base.LoadInternal();

        if (TransitionAnimationName != null)
        {
            // BUG: При переходе между страницами в начале возникает черный экран, а только потом начинает проигрываться анимация.
            // Связано это с особенностью воспроизведения видео на нативных контролах. Не смог побороть ни для Avalonia, ни для WPF.
            var transitionAnimation = _interfaceProvider.GetSceneTransitionAnimation(TransitionAnimationName);
            var stream = new MemoryStream(transitionAnimation.Data);
            _transitionVideo = GameObjectContainer.AddVideo(stream, GameInfo.SceneBounds, Layers.SceneLayers.AboveAllLayer);
        }
    }

    /// <inheritdoc />
    protected override void BeforeSceneUpdate(UpdateSceneData data)
    {
        base.BeforeSceneUpdate(data);

        if (_transitionVideo?.IsDestroyed == true)
            _transitionVideo = null;
    }
}