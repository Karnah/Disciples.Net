using ReactiveUI;

using Disciples.Engine;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Avalonia.SceneObjects
{
    /// <inheritdoc cref="IImageSceneObject" />
    public class ImageSceneObject : BaseSceneObject, IImageSceneObject
    {
        private IBitmap _bitmap;
        private bool _isReflected;

        /// <inheritdoc />
        public ImageSceneObject(int layer) : base(layer)
        { }


        /// <inheritdoc />
        public IBitmap Bitmap
        {
            get => _bitmap;
            set => this.RaiseAndSetIfChanged(ref _bitmap, value);
        }

        /// <inheritdoc />
        public bool IsReflected
        {
            get => _isReflected;
            set => this.RaiseAndSetIfChanged(ref _isReflected, value);
        }
    }
}