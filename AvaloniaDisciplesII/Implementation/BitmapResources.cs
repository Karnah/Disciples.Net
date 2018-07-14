using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using Engine;
using Engine.Enums;
using Engine.Interfaces;
using Engine.Models;
using ResourceManager;
using ResourceManager.Models;

using Action = Engine.Enums.Action;

namespace AvaloniaDisciplesII.Implementation
{
    public class BitmapResources : IBitmapResources
    {
        private readonly Dictionary<string, IReadOnlyList<Frame>> _resources;
        private readonly ImagesExtractor _extractor;

        public BitmapResources()
        {
            _resources = new Dictionary<string, IReadOnlyList<Frame>>();
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\BatUnits.ff");
        }

        // g000uu0015 - ид в верхнем регистре
        // HHIT - ограбает | HMOVE - атакует | IDLE - ждёт | STIL - замер | TUCH - бьёт 1 врага | HEFF - бьёт площадь
        // A - объект или аура | S - тень
        // 1 - объект | 2 -аура
        // A - юго-восток, лицом | D - северо-запад, спиной | B - симметрично
        // 00
        public IReadOnlyList<Frame> GetBitmapResources(string name, string code, Action action, Direction direction)
        {
            var fileName = $"{name.ToUpper()}{ConvertAction(action)}{code}{ConvertDirection(direction)}00";
            if (_resources.ContainsKey(fileName) == false) {
                _resources[fileName] = CacheBitmaps(fileName);
            }

            return _resources[fileName];
        }

        private IReadOnlyList<Frame> CacheBitmaps(string fileName)
        {
            var frames = _extractor.GetAnimationFrames(fileName);
            if (frames == null)
                return null;

            var result = new List<Frame>(frames.Count);

            int minRow = int.MaxValue, maxRow = int.MinValue;
            int minColumn = int.MaxValue, maxColumn = int.MinValue;
            foreach (var frame in frames) {
                minRow = Math.Min(minRow, frame.MinRow);
                maxRow = Math.Max(maxRow, frame.MaxRow);

                minColumn = Math.Min(minColumn, frame.MinColumn);
                maxColumn = Math.Max(maxColumn, frame.MaxColumn);
            }

            var bounds = new OpacityBounds(minRow, maxRow, minColumn, maxColumn);
            foreach (var frame in frames) {
                result.Add(ConvertImageToFrame(frame, bounds));
            }

            return result;
        }


        private static string ConvertAction(Action action)
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

        private static string ConvertDirection(Direction direction)
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

        private static Frame ConvertImageToFrame(Image image, OpacityBounds opacityBounds)
        {
            var frame = new Frame();

            frame.OffsetX = opacityBounds.MinColumn * GameInfo.Scale;
            frame.OffsetY = opacityBounds.MinRow * GameInfo.Scale;

            var width = opacityBounds.MaxColumn - opacityBounds.MinColumn + 1;
            var height = opacityBounds.MaxRow - opacityBounds.MinRow + 1;

            var bitmap = new WritableBitmap(width, height, PixelFormat.Rgba8888);
            using (var l = bitmap.Lock()) {
                for (int row = opacityBounds.MinRow; row <= opacityBounds.MaxRow; ++row) {
                    var begin = (row * image.Width + opacityBounds.MinColumn) << 2;
                    var length = width << 2;

                    Marshal.Copy(image.Data, begin,
                        new IntPtr(l.Address.ToInt64() + (row - opacityBounds.MinRow) * length), length);
                }
            }

            frame.Width = width * GameInfo.Scale;
            frame.Height = height * GameInfo.Scale;

            frame.Bitmap = bitmap;

            return frame;
        }


        private class OpacityBounds
        {
            public OpacityBounds(int minRow, int maxRow, int minColumn, int maxColumn)
            {
                MinRow = minRow;
                MaxRow = maxRow;
                MinColumn = minColumn;
                MaxColumn = maxColumn;
            }


            public int MinRow { get; }

            public int MaxRow { get; }

            public int MinColumn { get; }

            public int MaxColumn { get; }
        }
    }
}
