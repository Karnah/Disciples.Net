using ReactiveUI;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Avalonia.Common.SceneObjects
{
    /// <inheritdoc cref="ISceneObject" />
    public abstract class BaseSceneObject : ReactiveObject, ISceneObject
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;

        /// <inheritdoc />
        protected BaseSceneObject(int layer)
        {
            Layer = layer;
        }


        /// <inheritdoc />
        public double X {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        /// <inheritdoc />
        public double Y {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        /// <inheritdoc />
        public double Width {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        /// <inheritdoc />
        public double Height {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }

        /// <inheritdoc />
        public int Layer { get; }


        /// <inheritdoc />
        public virtual void Destroy()
        { }
    }
}