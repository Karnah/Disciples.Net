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
    CursorType DefaultCursorType { get; }

    /// <summary>
    /// Обработать загрузку сцены.
    /// </summary>
    void AfterSceneLoaded();

    /// <summary>
    /// Обновить сцену.
    /// </summary>
    void UpdateScene(UpdateSceneData data);
}

/// <summary>
/// Объект сцены с её параметрами.
/// </summary>
/// <typeparam name="TSceneParameters">Тип параметров сцены.</typeparam>
// Используется для передачи параметров в конструктор.
// ReSharper disable once UnusedTypeParameter
public interface IScene<TSceneParameters> : IScene
    where TSceneParameters : SceneParameters
{
}