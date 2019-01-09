using System;
using System.Collections.Generic;

using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Models;

namespace Disciples.Engine.Base
{
    /// <summary>
    /// Контроллер игры.
    /// </summary>
    public interface IGameController
    {
        /// <summary>
        /// Все объекты, размещённые на сцене.
        /// </summary>
        IReadOnlyCollection<GameObject> GameObjects { get; }

        /// <summary>
        /// Контейнер объектов для текущей сцены.
        /// </summary>
        ISceneContainer CurrentSceneContainer { get; }


        /// <summary>
        /// Событие смены текущей сцены.
        /// </summary>
        event EventHandler SceneChanged;

        /// <summary>
        /// Событие, которое возникает после того, как сцена была обновлена.
        /// </summary>
        event EventHandler<SceneUpdatingEventArgs> SceneEndUpdating;

        // Сделано, так как EventHandler не поддерживает приоритет в вызове, а делать на Action не хочется.
        /// <summary>
        /// Событие возникает после того, как вся логика была выполнена и можно перерисовать сцену.
        /// </summary>
        event EventHandler<SceneUpdatingEventArgs> SceneRedraw;

        /// <summary>
        /// Событие действия над игровым объектом.
        /// </summary>
        event EventHandler<GameObjectActionEventArgs> GameObjectAction;


        /// <summary>
        /// Инициализировать новый объект и разместить его на сцене.
        /// </summary>
        void CreateObject(GameObject gameObject);

        /// <summary>
        /// Убрать объект со сцены.
        /// </summary>
        void DestroyObject(GameObject gameObject);


        /// <summary>
        /// Поменять сцену.
        /// </summary>
        /// <typeparam name="TSceneController">Тип контроллера новой сцены.</typeparam>
        /// <typeparam name="TData">Тип параметров инициализации новой сцены.</typeparam>
        /// <param name="sceneController">Контроллер новой сцены.</param>
        /// <param name="data">Данные для инициализации новой сцены.</param>
        void ChangeScene<TSceneController, TData>(TSceneController sceneController, TData data)
            where TSceneController : ISceneController<TData>;
    }
}