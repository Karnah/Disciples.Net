using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using DII.ResourceExtractor;
using Inftastructure.Enums;
using Inftastructure.Interfaces;

using Action = Inftastructure.Enums.Action;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Animation.Implementation
{
    public class BitmapResources : IBitmapResources
    {
        private readonly Dictionary<string, IReadOnlyList<Bitmap>> _resources;
        private readonly ImagesExtractor _extractor;

        public BitmapResources()
        {
            _resources = new Dictionary<string, IReadOnlyList<Bitmap>>();
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\BatUnits.ff");
        }

        // g000uu0015 - ид в верхнем регистре
        // HHIT - ограбает | HMOVE - атакует | IDLE - ждёт | STIL - замер | TUCH - бьёт 1 врага | HEFF - бьёт площадь
        // A - объект или аура | S - тень
        // 1 - объект | 2 -аура
        // A - юго-восток, лицом | D - северо-запад, спиной | B - симметрично
        // 00
        public IReadOnlyList<Bitmap> GetBitmapResources(string name, string code, Action action, Direction direction)
        {
            var fileName = $"{name.ToUpper()}{ConvertAction(action)}{code}{ConvertDirection(direction)}00";
            if (_resources.ContainsKey(fileName) == false) {
                _resources[fileName] = CacheBitmaps(fileName);
            }

            return _resources[fileName];
        }

        private IReadOnlyList<Bitmap> CacheBitmaps(string fileName)
        {
            var frames = _extractor.GetAnimationFrames(fileName);
            if (frames == null)
                return null;

            var result = new List<Bitmap>(frames.Length);

            foreach (var frame in frames) {
                int width = 800,
                    height = 600;
                var bitmap = new WritableBitmap(width, height, PixelFormat.Rgba8888);

                using (var l = bitmap.Lock()) {
                    for (int row = 0; row < height; ++row) {
                        Marshal.Copy(frame, row * l.RowBytes, new IntPtr(l.Address.ToInt64() + row * l.RowBytes), l.RowBytes);
                    }
                }

                result.Add(bitmap);
            }

            return result;
        }


        private string ConvertAction(Action action)
        {
            switch (action) {
                case Action.Waiting:
                    return "IDLE";
                case Action.Attacking:
                    return "HMOV";
                case Action.TakingDamage:
                    return "HHIT";
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private string ConvertDirection(Direction direction)
        {
            switch (direction) {
                case Direction.Northwest:
                    return "D";
                case Direction.Southeast:
                    return "A";
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
