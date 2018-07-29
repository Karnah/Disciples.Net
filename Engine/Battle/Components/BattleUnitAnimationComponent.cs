using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;
using Avalonia.Media.Imaging;

using Engine.Battle.Enums;
using Engine.Battle.Models;
using Engine.Battle.Providers;
using Engine.Components;
using Engine.Models;

namespace Engine.Battle.Components
{
    public class BattleUnitAnimationComponent : Component
    {
        private const int FrameChangeSpeed = 75;

        private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
        private readonly string _unitId;
        private BattleObjectComponent _battleObject;

        private long _ticksCount = 0;
        private BattleAction _action;

        private BattleUnitAnimation _unitAnimation;

        private IReadOnlyList<Frame> _shadowFrames;
        private IReadOnlyList<Frame> _unitFrames;
        private IReadOnlyList<Frame> _auraFrames;

        private Bitmap _bitmap;
        private double _x;
        private double _y;
        private double _width;
        private double _height;


        public BattleUnitAnimationComponent(
            GameObject gameObject,
            IBattleUnitResourceProvider battleUnitResourceProvider,
            string unitId) : base(
            gameObject)
        {
            _battleUnitResourceProvider = battleUnitResourceProvider;
            _unitId = unitId;
        }


        public int FrameIndex { get; private set; }

        public int FramesCount { get; private set; }


        public Bitmap Bitmap {
            get => _bitmap;
            private set => this.RaiseAndSetIfChanged(ref _bitmap, value);
        }

        public double X {
            get => _x;
            private set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        public double Y {
            get => _y;
            private set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        public double Width {
            get => _width;
            private set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        public double Height {
            get => _height;
            private set => this.RaiseAndSetIfChanged(ref _height, value);
        }



        public override void OnInitialize()
        {
            base.OnInitialize();

            _battleObject = GetComponent<BattleObjectComponent>();
            _unitAnimation = _battleUnitResourceProvider.GetBattleUnitAnimation(_unitId, _battleObject.Direction);

            UpdateSource();
        }

        public override void OnUpdate(long tickCount)
        {
            if (_battleObject.Action != _action) {
                UpdateSource();
                return;
            }

            _ticksCount += tickCount;
            if (_ticksCount < FrameChangeSpeed)
                return;

            FrameIndex += (int) (_ticksCount / FrameChangeSpeed);
            // todo Хреновая реализация контроллера анимации.
            // Предполагается, что любая анимация будет выполняться только 1 раз, кроме анимации ожидания
            if (FrameIndex >= FramesCount && _action != BattleAction.Waiting) {
                _battleObject.Action = BattleAction.Waiting;
                return;
            }

            FrameIndex %= FramesCount;
            _ticksCount %= FrameChangeSpeed;

            Bitmap = UnionFrames(
                GetFrame(_shadowFrames, FrameIndex),
                GetFrame(_unitFrames, FrameIndex),
                GetFrame(_auraFrames, FrameIndex));
        }


        private static Frame GetFrame(IReadOnlyList<Frame> frames, int frameIndex)
        {
            if (frames?.Any() != true)
                return null;

            if (frameIndex >= frames.Count)
                return null;

            return frames[frameIndex];
        }

        private void UpdateSource()
        {
            _action = _battleObject.Action;
            FrameIndex = 1;

            var frames = _unitAnimation.BattleUnitFrameses[_action];
            _shadowFrames = frames.ShadowFrames;
            _unitFrames = frames.UnitFrames;
            _auraFrames = frames.AuraFrames;

            var shadowFrame = GetFrame(_shadowFrames, FrameIndex);
            var unitFrame = GetFrame(_unitFrames, FrameIndex);
            var auraFrame = GetFrame(_auraFrames, FrameIndex);

            UpdatePosition(shadowFrame, unitFrame, auraFrame);
            Bitmap = UnionFrames(shadowFrame, unitFrame, auraFrame);

            FramesCount = _unitFrames.Count;
        }

        private void UpdatePosition(params Frame[] frames)
        {
            double offsetX = double.MaxValue,
                offsetY = double.MaxValue,
                width = 0,
                height = 0;

            foreach (var frame in frames) {
                if (frame == null)
                    continue;

                offsetX = Math.Min(offsetX, frame.OffsetX);
                offsetY = Math.Min(offsetY, frame.OffsetY);
            }

            foreach (var frame in frames) {
                if (frame == null)
                    continue;

                width = Math.Max(width, frame.OffsetX - offsetX + frame.Width);
                height = Math.Max(height, frame.OffsetY - offsetY + frame.Height);
            }

            var posX = _battleObject.Position.X + offsetX;
            if (Math.Abs(X - posX) > float.Epsilon) {
                X = posX;
            }

            var posY = _battleObject.Position.Y + offsetY;
            if (Math.Abs(Y - posY) > float.Epsilon) {
                Y = posY;
            }

            if (Math.Abs(Width - width) > float.Epsilon) {
                Width = width;
            }

            if (Math.Abs(Height - height) > float.Epsilon) {
                Height = height;
            }
        }

        private Bitmap UnionFrames(params Frame[] frames)
        {
            var canvas = new Canvas {
                Width = Width,
                Height = Height
            };

            var offsetX = X - _battleObject.Position.X;
            var offsetY = Y - _battleObject.Position.Y;

            foreach (var frame in frames) {
                if (frame == null)
                    continue;

                var image = new Image {
                    Source = frame.Bitmap,
                    Width = frame.Width,
                    Height = frame.Height,
                    Stretch = Stretch.Fill
                };

                Canvas.SetLeft(image, frame.OffsetX - offsetX);
                Canvas.SetTop(image, frame.OffsetY - offsetY);

                canvas.Children.Add(image);
            }

            using (var bitmap = new RenderTargetBitmap(
                (int)canvas.Width,
                (int)canvas.Height)) {
                var size = new Size(canvas.Width, canvas.Height);
                canvas.Measure(size);
                canvas.Arrange(new Rect(size));
                bitmap.Render(canvas);

                var copy = new Bitmap(bitmap.PlatformImpl);
                return copy;
            }
        }
    }
}
