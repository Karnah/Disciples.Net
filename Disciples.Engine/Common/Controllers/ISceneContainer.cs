using System.Collections.Generic;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.Controllers;

/// <summary>
/// Контейнер объектов на сцене.
/// </summary>
public interface ISceneContainer
{
    /// <summary>
    /// Список всех объектов на сцене.
    /// </summary>
    IReadOnlyList<ISceneObject> SceneObjects { get; }


    /// <summary>
    /// Добавить объект на сцену.
    /// </summary>
    void AddSceneObject(ISceneObject sceneObject);

    /// <summary>
    /// Удалить объект со сцены.
    /// </summary>
    void RemoveSceneObject(ISceneObject sceneObject);

    /// <summary>
    /// Обновить список всех объектов на сцене.
    /// </summary>
    /// <remarks>
    /// Используется, чтобы создавать/удалять объекты за один проход, а не каждый раз при вызове метода.
    /// </remarks>
    void UpdateContainer();
}