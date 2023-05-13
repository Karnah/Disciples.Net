using Disciples.Engine.Base;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Scene.LoadSaga.GameObjects;
using Disciples.Scene.LoadSaga.Models;

namespace Disciples.Scene.LoadSaga.Controllers;

/// <summary>
/// Контейнер для игровых объектов загрузки саги.
/// </summary>
internal class LoadSagaGameObjectContainer : BaseSceneGameObjectContainer
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly Lazy<LoadSagaInterfaceController> _interfaceController;

    /// <summary>
    /// Создать объект типа <see cref="LoadSagaGameObjectContainer" />.
    /// </summary>
    public LoadSagaGameObjectContainer(IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        Lazy<LoadSagaInterfaceController> interfaceController
        ) : base(gameObjectContainer)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _interfaceController = interfaceController;
    }

    /// <summary>
    /// Добавить информацию о сейве.
    /// </summary>
    public SagaSaveObject AddSave(Save save, int x, int y)
    {
        var sagaSaveObject = new SagaSaveObject(_sceneObjectContainer, x, y,
            _interfaceController.Value.OnSaveLeftMouseButtonPressed,
            _interfaceController.Value.OnSaveLeftMouseButtonDoubleClicked,
            save);
        AddObject(sagaSaveObject);

        return sagaSaveObject;
    }
}