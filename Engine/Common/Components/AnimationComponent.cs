using System;
using System.Collections.Generic;

using Engine.Common.Controllers;
using Engine.Common.GameObjects;
using Engine.Common.Models;

namespace Engine.Common.Components
{
    public class AnimationComponent : Component
    {
        /// <summary>
        /// Промежуток времени в мс через которое происходит смена кадра в анимации
        /// </summary>
        private const int FRAME_CHANGE_SPEED = 75;

        private readonly IMapVisual _mapVisual;
        private readonly IReadOnlyList<Frame> _frames;
        private readonly int _layer;

        private VisualObject _visualObject;
        private long _ticksCount = 0;

        public AnimationComponent(GameObject gameObject, IMapVisual mapVisual, IReadOnlyList<Frame> frames, int layer) : base(gameObject)
        {
            _mapVisual = mapVisual;
            _frames = frames;
            _layer = layer;
        }


        public int FrameIndex { get; private set; }

        public int FramesCount => _frames.Count;


        public override void OnInitialize()
        {
            base.OnInitialize();

            _visualObject = new VisualObject(GameObject, _layer);
            _mapVisual.AddVisual(_visualObject);
        }

        public override void OnUpdate(long tickCount)
        {
            _ticksCount += tickCount;
            if (_ticksCount < FRAME_CHANGE_SPEED)
                return;
            
            ++FrameIndex;
            FrameIndex %= _frames.Count;
            _ticksCount %= FRAME_CHANGE_SPEED;

            var frame = _frames[FrameIndex];

            _visualObject.Bitmap = frame.Bitmap;

            var posX = GameObject.X + frame.OffsetX;
            if (Math.Abs(_visualObject.X - posX) > float.Epsilon) {
                _visualObject.X = posX;
            }

            var posY = GameObject.Y + frame.OffsetY;
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
