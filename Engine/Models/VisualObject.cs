using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Engine.Models
{
    public class VisualObject : ReactiveObject
    {
        private double _width;
        private double _height;
        private double _x;
        private double _y;
        private Bitmap _bitmap;

        public VisualObject(GameObject gameObject, int layer)
        {
            GameObject = gameObject;
            Layer = layer;
        }


        public GameObject GameObject { get; }

        public int Layer { get; }


        public double Width {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

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
    }
}
