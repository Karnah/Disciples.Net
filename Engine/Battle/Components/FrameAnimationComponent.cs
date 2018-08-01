using System;
using System.Collections.Generic;

using Engine.Components;
using Engine.Interfaces;
using Engine.Models;

namespace Engine.Battle.Components
{
    public class FrameAnimationComponent : Component
    {
        private const int FrameChangeSpeed = 75;

        private readonly IMapVisual _mapVisual;
        private readonly IReadOnlyList<Frame> _frames;

        private BattleObjectComponent _battleObject;

        private VisualObject _visualObject;
        private int _frameIndex = 0;
        private long _ticksCount = 0;

        public FrameAnimationComponent(GameObject gameObject, IMapVisual mapVisual, IReadOnlyList<Frame> frames) : base(gameObject)
        {
            _mapVisual = mapVisual;
            _frames = frames;
        }


        public override void OnInitialize()
        {
            base.OnInitialize();

            _battleObject = GetComponent<BattleObjectComponent>();
            _visualObject = new VisualObject(GameObject, 1);
            _mapVisual.AddVisual(_visualObject);
        }

        public override void OnUpdate(long tickCount)
        {
            _ticksCount += tickCount;
            if (_ticksCount < FrameChangeSpeed)
                return;
            
            _frameIndex += (int) (_ticksCount / FrameChangeSpeed);
            _frameIndex %= _frames.Count;
            _ticksCount %= FrameChangeSpeed;

            var frame = _frames[_frameIndex];

            _visualObject.Bitmap = frame.Bitmap;

            var posX = _battleObject.Position.X + frame.OffsetX;
            if (Math.Abs(_visualObject.X - posX) > float.Epsilon) {
                _visualObject.X = posX;
            }

            var posY = _battleObject.Position.Y + frame.OffsetY;
            if (Math.Abs(_visualObject.Y - posY) > float.Epsilon) {
                _visualObject.Y = posY;
            }

            if (Math.Abs(_visualObject.Width - frame.Width) > float.Epsilon) {
                _visualObject.Width = frame.Width;
            }

            if (Math.Abs(_visualObject.Height - frame.Height) > float.Epsilon) {
                _visualObject.Height = frame.Height;
            }
        }

        public override void Destroy()
        {
            _mapVisual.RemoveVisual(_visualObject);
        }
    }
}
