using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Models;

namespace Disciples.Engine.Base
{
    /// <summary>
    /// Объект сцены.
    /// </summary>
    /// <typeparam name="TSceneParameters">Параметры для инициализации сцены.</typeparam>
    public interface IScene<in TSceneParameters> : ISupportLoading
        where TSceneParameters : SceneParameters
    {
        /// <summary>
        /// Инициализировать параметры сцены.
        /// </summary>
        public void InitializeParameters(ISceneContainer sceneContainer, TSceneParameters parameters);
    }
}
