using System.Collections.Generic;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.Controllers
{
    /// <summary>
    /// Интерфейс для объектов, которые отрисовываются на сцене.
    /// </summary>
    public interface IScene
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
    }
}