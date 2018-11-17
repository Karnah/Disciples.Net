using ReactiveUI;

namespace Engine.Common.Models
{
    /// <summary>
    /// Объект, который отрисовывается на сцене.
    /// </summary>
    public abstract class VisualObject : ReactiveObject
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;

        protected VisualObject(int layer)
        {
            Layer = layer;
        }


        /// <summary>
        /// Слой на котором располагается объект на экране.
        /// </summary>
        public int Layer { get; }

        /// <summary>
        /// Координата X объекта (справа - налево).
        /// </summary>
        public double X {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        /// <summary>
        /// Координата Y объекта (сверху - вниз).
        /// </summary>
        public double Y {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        /// <summary>
        /// Ширина объекта.
        /// </summary>
        public double Width {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        /// <summary>
        /// Высота объекта.
        /// </summary>
        public double Height {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }


        /// <summary>
        /// Очистить занимаемые объектом ресурсы.
        /// </summary>
        public virtual void Destroy()
        { }
    }
}
