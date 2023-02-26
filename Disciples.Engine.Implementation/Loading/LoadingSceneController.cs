using Disciples.Engine.Base;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Loading;
using Disciples.Engine.Models;
using Disciples.Engine.Platform.Factories;

namespace Disciples.Engine.Implementation.Loading
{
    /// <inheritdoc cref="ILoadingSceneController" />
    public class LoadingSceneController : BaseSceneController<SceneParameters>, ILoadingSceneController
    {
        /// <summary>
        /// Наименование картинки, которая содержит фон загрузки.
        /// </summary>
        private const string LOADING_IMAGE_NAME = "LOADING";

        private IImageSceneObject _loadingSceneObject;

        public LoadingSceneController(
            IGameController gameController,
            ISceneFactory sceneFactory,
            IInterfaceProvider interfaceProvider
            ) : base(gameController, sceneFactory, interfaceProvider)
        { }


        /// <inheritdoc />
        protected override void LoadInternal()
        {
            base.LoadInternal();

            var loadingImage = InterfaceProvider.GetImage(LOADING_IMAGE_NAME);
            _loadingSceneObject = AddImage(loadingImage, 0, 0, 0);
        }

        /// <inheritdoc />
        protected override void UnloadInternal()
        {
            base.UnloadInternal();

            RemoveSceneObject(_loadingSceneObject);
            _loadingSceneObject = null;
        }
    }
}