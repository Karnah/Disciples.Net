using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Models;

namespace Disciples.Engine.Base
{
    /// <summary>
    /// Объект сцены.
    /// </summary>
    public interface IScene : ISupportLoading
    {
        /// <summary>
        /// Контейнер объектов сцены.
        /// </summary>
        ISceneContainer SceneContainer { get; }

        /// <summary>
        /// Выполнить действия перед обновлением объектов на сцене.
        /// </summary>
        void BeforeSceneUpdate(UpdateSceneData data);

        /// <summary>
        /// Выполнить действия после обновления объектов на сцене.
        /// </summary>
        void AfterSceneUpdate(UpdateSceneData data);
    }
}
