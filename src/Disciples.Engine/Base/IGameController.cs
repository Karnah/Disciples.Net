using System;
using Disciples.Engine.Common;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Models;

namespace Disciples.Engine.Base;

/// <summary>
/// Контроллер игры.
/// </summary>
public interface IGameController
{
    /// <summary>
    /// Контейнер объектов для текущей сцены.
    /// </summary>
    IPlatformSceneObjectContainer? CurrentSceneContainer { get; }

    /// <summary>
    /// Событие смены текущей сцены.
    /// </summary>
    event EventHandler SceneChanged;

    /// <summary>
    /// Загрузить данные игры.
    /// </summary>
    public GameContext LoadGame(string savePath);

    /// <summary>
    /// Поменять сцену.
    /// </summary>
    /// <typeparam name="TScene">Тип новой сцены.</typeparam>
    /// <typeparam name="TSceneParameters">Тип параметров инициализации новой сцены.</typeparam>
    /// <param name="data">Данные для инициализации новой сцены.</param>
    void ChangeScene<TScene, TSceneParameters>(TSceneParameters data)
        where TScene : IScene, ISupportLoadingWithParameters<TSceneParameters>
        where TSceneParameters : SceneParameters;
}