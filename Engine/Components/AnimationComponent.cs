using System;
using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Engine.Enums;
using Engine.Interfaces;
using Engine.Models;

using Action = Engine.Enums.Action;

namespace Engine.Components
{
    public class AnimationComponent : Component
    {
        private const int FrameChangeSpeed = 75;

        private readonly IMapVisual _mapVisual;
        private readonly IBitmapResources _bitmapResources;
        private readonly string _name;
        private readonly string _code;
        private MapObject _mapObject;

        private long _ticksCount = 0;
        private IReadOnlyList<Bitmap> _animationFrames;
        private int _frameIndex = 0;
        private Action _action;
        private Direction _direction;
        private VisualObject _visual;


        public AnimationComponent(GameObject gameObject, IMapVisual mapVisual, IBitmapResources bitmapResources, string name, string code) : base(
            gameObject)
        {
            _mapVisual = mapVisual;
            _bitmapResources = bitmapResources;
            _name = name;
            _code = code;
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            _mapObject = GetComponent<MapObject>();
            UpdateSource();
        }

        public override void OnUpdate(long tickCount)
        {
            if (_mapObject.Action != _action || _mapObject.Direction != _direction)
            {
                UpdateSource();
                return;
            }

            if (_animationFrames == null)
                return;

            if (Math.Abs(_visual.X - _mapObject.Position.X) > float.Epsilon)
                _visual.X = _mapObject.Position.X;

            if (Math.Abs(_visual.Y - _mapObject.Position.Y) > float.Epsilon)
                _visual.Y = _mapObject.Position.Y;

            _ticksCount += tickCount;
            if (_ticksCount < FrameChangeSpeed)
                return;

            _frameIndex += (int)(_ticksCount / FrameChangeSpeed);
            // todo Хреновая реализация контроллера анимации. 
            // Предполагается, что любая анимация будет выполняться только 1 раз, кроме анимации ожидания
            if (_frameIndex >= _animationFrames.Count && _action != Action.Waiting)
            {
                _mapObject.Action = Action.Waiting;
                return;
            }

            _frameIndex %= _animationFrames.Count;
            _ticksCount %= FrameChangeSpeed;
            _visual.Bitmap = _animationFrames[_frameIndex];
        }

        private void UpdateSource()
        {
            _action = _mapObject.Action;
            _direction = _mapObject.Direction;
            _animationFrames = _bitmapResources.GetBitmapResources(_name, _code, _action, _direction);

            if (_animationFrames == null)
            {
                if (_visual != null)
                {
                    // todo Здесь происходит удаление.
                    // Возможно, просто достаточно перенести в невидимую область
                    _mapVisual.RemoveVisual(_visual);
                    _visual = null;
                }

            }
            else
            {
                if (_visual == null)
                {
                    _visual = new VisualObject();
                    _mapVisual.AddVisual(_visual);
                }

                _visual.X = _mapObject.Position.X;
                _visual.Y = _mapObject.Position.Y;

                _ticksCount = 0;
                _frameIndex = 0;
                _visual.Bitmap = _animationFrames[_frameIndex++];
            }
        }
    }
}
