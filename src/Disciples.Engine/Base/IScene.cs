using Disciples.Engine.Common.Enums;
using Disciples.Engine.Models;

namespace Disciples.Engine.Base;

/// <summary>
/// Объект сцены.
/// </summary>
public interface IScene : ISupportLoading
{
    /// <summary>
    /// Контейнер игровых объектов.
    /// </summary>
    IGameObjectContainer GameObjectContainer { get; }

    /// <summary>
    /// Контейнер объектов сцены.
    /// </summary>
    ISceneObjectContainer SceneObjectContainer { get; }

    /// <summary>
    /// Состояние курсора на сцене по умолчанию.
    /// </summary>
    CursorState DefaultCursorState { get; }

    /// <summary>
    /// Обработать загрузку сцены.
    /// </summary>
    void AfterSceneLoaded();

    /// <summary>
    /// Обновить сцену.
    /// </summary>
    void UpdateScene(UpdateSceneData data);
}