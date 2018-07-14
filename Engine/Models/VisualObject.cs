using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Engine.Models
{
    public class VisualObject : ReactiveObject
    {
        private double _x;
        private double _y;
        private Bitmap _bitmap;


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
