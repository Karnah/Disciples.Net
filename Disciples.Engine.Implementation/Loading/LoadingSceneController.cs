using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Loading;
using Disciples.Engine.Platform.Factories;

namespace Disciples.Engine.Implementation.Loading
{
    /// <inheritdoc cref="ILoadingSceneController" />
    public class LoadingSceneController : BaseSceneController<object>, ILoadingSceneController
    {
        private IImageSceneObject _loadingSceneObject;

        public LoadingSceneController(
            IGameController gameController,
            ISceneFactory sceneFactory,
            IInterfaceProvider interfaceProvider
            ) : base(gameController, sceneFactory, interfaceProvider)
        { }


        /// <inheritdoc />
        protected override void LoadInternal(ISceneContainer sceneContainer, object data)
        {
            base.LoadInternal(sceneContainer, data);

            var loadingImage = InterfaceProvider.GetImage("LOADING");
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