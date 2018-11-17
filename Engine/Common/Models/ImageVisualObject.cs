using Avalonia.Media;
using Avalonia.Media.Imaging;

using ReactiveUI;

namespace Engine.Common.Models
{
    public class ImageVisualObject : VisualObject
    {
        private Bitmap _bitmap;
        private Transform _transform;

        public ImageVisualObject(int layer) : base(layer)
        {
        }

        /// <summary>
        /// Изображение.
        /// </summary>
        public Bitmap Bitmap {
            get => _bitmap;
            set => this.RaiseAndSetIfChanged(ref _bitmap, value);
        }

        /// <summary>
        /// Трансформация изображения.
        /// </summary>
        public Transform Transform {
            get => _transform;
            set => this.RaiseAndSetIfChanged(ref _transform, value);
        }
    }
}
