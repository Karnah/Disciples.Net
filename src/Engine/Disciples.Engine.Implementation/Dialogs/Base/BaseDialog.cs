using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Dialogs.Base;

/// <summary>
/// Базовый класс для диалога.
/// </summary>
public abstract class BaseDialog : IDialog
{
    private readonly IGameObjectContainer _gameObjectContainer;
    private readonly ISceneInterfaceController _sceneInterfaceController;
    private readonly IInterfaceProvider _interfaceProvider;

    /// <summary>
    /// Создать объект типа <see cref="BaseDialog" />.
    /// </summary>
    protected BaseDialog(IGameObjectContainer gameObjectContainer, ISceneInterfaceController sceneInterfaceController, IInterfaceProvider interfaceProvider)
    {
        _gameObjectContainer = gameObjectContainer;
        _sceneInterfaceController = sceneInterfaceController;
        _interfaceProvider = interfaceProvider;
    }

    /// <inheritdoc />
    public bool IsClosed { get; private set; }

    /// <summary>
    /// Имя диалога.
    /// </summary>
    protected abstract string DialogName { get; }

    /// <summary>
    /// Объекты диалога.
    /// </summary>
    protected IReadOnlyList<GameObject> DialogGameObjects { get; private set; } = null!;

    /// <inheritdoc />
    public void Open()
    {
        // Деактивируем все объекты, которые были размещены до открытия диалога.
        // Это необходимо для того, чтобы все события ввода обрабатывались только на объектах самого диалога.
        foreach (var gameObject in _gameObjectContainer.GameObjects)
            gameObject.IsDeactivated = true;

        var sceneInterface = _interfaceProvider.GetSceneInterface(DialogName);
        DialogGameObjects = _sceneInterfaceController.AddSceneGameObjects(sceneInterface, Layers.DialogLayers);

        OpenInternal(DialogGameObjects);
    }

    /// <inheritdoc />
    public abstract void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents);

    /// <summary>
    /// Открыть диалог.
    /// </summary>
    protected abstract void OpenInternal(IReadOnlyList<GameObject> dialogGameObjects);

    /// <summary>
    /// Закрыть диалог.
    /// </summary>
    protected virtual void Close()
    {
        IsClosed = true;

        foreach (var gameObject in DialogGameObjects)
            gameObject.Destroy();

        foreach (var gameObject in _gameObjectContainer.GameObjects)
            gameObject.IsDeactivated = false;
    }
}