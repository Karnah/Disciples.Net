using Disciples.Engine.Base;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Base;

/// <summary>
/// Базовый класс для всех сцен.
/// </summary>
public abstract class BaseScene : BaseSupportLoading, IScene
{
    /// <summary>
    /// Создать объект типа <see cref="BaseScene" />.
    /// </summary>
    protected BaseScene(IGameObjectContainer gameObjectContainer, ISceneObjectContainer sceneObjectContainer)
    {
        GameObjectContainer = gameObjectContainer;
        SceneObjectContainer = sceneObjectContainer;
    }

    /// <inheritdoc />
    public IGameObjectContainer GameObjectContainer { get; }

    /// <inheritdoc />
    public ISceneObjectContainer SceneObjectContainer { get; }

    /// <inheritdoc />
    public void AfterSceneLoaded()
    {
        AfterSceneLoadedInternal();
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
    }

    /// <summary>
    /// Обработать загрузку сцены.
    /// </summary>
    protected virtual void AfterSceneLoadedInternal()
    {

    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <inheritdoc />
    public void UpdateScene(UpdateSceneData data)
    {
        BeforeSceneUpdate(data);
        GameObjectContainer.UpdateGameObjects(data.TicksCount);

        // При обработке событий от пользователя, может поменяться сцена.
        // В таком случае дальше обновлять не нужно.
        // TODO Вообще, это может стрельнуть где угодно. Нужно посмотреть, куда это лучше вынести.
        if (IsLoaded)
            AfterSceneUpdate();

        SceneObjectContainer.PlatformSceneObjectContainer.UpdateContainer();
    }

    /// <summary>
    /// Выполнить действия до обновления игровых объектов.
    /// </summary>
    protected abstract void BeforeSceneUpdate(UpdateSceneData data);

    /// <summary>
    /// Выполнить действия после игровых объектов.
    /// </summary>
    protected abstract void AfterSceneUpdate();
}