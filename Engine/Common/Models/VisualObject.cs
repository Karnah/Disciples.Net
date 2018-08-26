using Avalonia.Media;
using Avalonia.Media.Imaging;

using ReactiveUI;

using Engine.Common.GameObjects;

namespace Engine.Common.Models
{
    public class VisualObject : ReactiveObject
    {
        private double _width;
        private double _height;
        private double _x;
        private double _y;
        private Bitmap _bitmap;
        private Transform _transform; 

        public VisualObject(GameObject gameObject, int layer)
        {
            GameObject = gameObject;
            Layer = layer;
        }


        public GameObject GameObject { get; }

        public int Layer { get; }


        // todo Ширину можно извлекать из изображения
        public double Width {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        // todo Высоту можно извлекать из изображения
        public double Height {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }

        public double X {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        public double Y {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        public Bitmap Bitmap {
            get => _bitmap;
            set => this.RaiseAndSetIfChanged(ref _bitmap, value);
        }

        public Transform Transform {
            get => _transform;
            set => this.RaiseAndSetIfChanged(ref _transform, value);
        }
    }
}
