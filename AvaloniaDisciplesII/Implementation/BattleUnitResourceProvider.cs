using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using Engine;
using Engine.Battle.Enums;
using Engine.Battle.Models;
using Engine.Battle.Providers;
using Engine.Models;
using ResourceProvider;
using ResourceProvider.Models;

namespace AvaloniaDisciplesII.Implementation
{
    public class BattleUnitResourceProvider : IBattleUnitResourceProvider
    {
        private readonly SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation> _unitsAnimations;
        private readonly ImagesExtractor _extractor;

        public BattleUnitResourceProvider()
        {
            _unitsAnimations = new SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation>();
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\BatUnits.ff");
        }


        public BattleUnitAnimation GetBattleUnitAnimation(string unitId, BattleDirection direction)
        {
            if (_unitsAnimations.ContainsKey((unitId, direction)) == false) {
                _unitsAnimations[(unitId, direction)] = GetUnitAnimation(unitId, direction);
            }

            return _unitsAnimations[(unitId, direction)];
        }


        private BattleUnitAnimation GetUnitAnimation(string unitId, BattleDirection direction)
        {
            var unitFrames = new Dictionary<BattleAction, BattleUnitFrames>();
            foreach (BattleAction action in Enum.GetValues(typeof(BattleAction))) {
                unitFrames.Add(action, GetUnitFrames(unitId, direction, action));
            }

            // todo Добавить анимацию для атаки цели
            return new BattleUnitAnimation(unitFrames, null);
        }


        // g000uu0015 - ид в верхнем регистре
        // HHIT - ограбает | HMOVE - атакует | IDLE - ждёт | STIL - замер | TUCH - бьёт 1 врага | HEFF - бьёт площадь
        // A - объект или аура | S - тень
        // 1 - объект | 2 -аура
        // A - юго-восток, лицом | D - северо-запад, спиной | B - симметрично
        // 00
        private BattleUnitFrames GetUnitFrames(string unitId, BattleDirection direction, BattleAction action)
        {
            var shadowImagesName = $"{unitId.ToUpper()}{ConvertAction(action)}S1{ConvertDirection(direction)}00";
            var shadowFrames = GetAnimationFrames(shadowImagesName);

            var unitImagesName = $"{unitId.ToUpper()}{ConvertAction(action)}A1{ConvertDirection(direction)}00";
            var unitFrames = GetAnimationFrames(unitImagesName);

            var auraImagesName = $"{unitId.ToUpper()}{ConvertAction(action)}A2{ConvertDirection(direction)}00";
            var auraFrames = GetAnimationFrames(auraImagesName);

            return new BattleUnitFrames(shadowFrames, unitFrames, auraFrames);
        }

        private IReadOnlyList<Frame> GetAnimationFrames(string fileName)
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

            // todo Здесь можно огрести, если фреймы будут иметь различные размеры
            var bounds = new OpacityBounds(minRow, maxRow, minColumn, maxColumn);
            foreach (var frame in frames) {
                result.Add(ConvertImageToFrame(frame, bounds));
            }

            return result;
        }


        private static string ConvertAction(BattleAction action)
        {
            switch (action) {
                case BattleAction.Waiting:
                    return "IDLE";
                case BattleAction.Attacking:
                    return "HMOV";
                case BattleAction.TakingDamage:
                    return "HHIT";
                case BattleAction.Paralized:
                    return "STIL";
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private static string ConvertDirection(BattleDirection direction)
        {
            switch (direction) {
                case BattleDirection.Attacker:
                    return "A";
                case BattleDirection.Defender:
                    return "D";
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
