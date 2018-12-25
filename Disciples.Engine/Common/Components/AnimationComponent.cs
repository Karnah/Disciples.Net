using System;
using System.Collections.Generic;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.VisualObjects;

namespace Disciples.Engine.Common.Components
{
    public class AnimationComponent : Component
    {
        /// <summary>
        /// Промежуток времени в мс через которое происходит смена кадра в анимации.
        /// </summary>
        private const int FRAME_CHANGE_SPEED = 75;

        private readonly IMapVisual _mapVisual;
        private readonly IReadOnlyList<Frame> _frames;
        private readonly int _layer;

        private ImageVisualObject _imageVisualObject;
        private long _ticksCount = 0;

        public AnimationComponent(
            GameObject gameObject,
            IMapVisual mapVisual,
            IReadOnlyList<Frame> frames,
            int layer) : base(gameObject)
        {
            _mapVisual = mapVisual;
            _frames = frames;
            _layer = layer;
        }


        /// <summary>
        /// Индекс текущего кадра анимации.
        /// </summary>
        public int FrameIndex { get; private set; }

        /// <summary>
        /// Количество кадров в анимации.
        /// </summary>
        public int FramesCount => _frames.Count;


        /// <inheritdoc />
        public override void OnInitialize()
        {
            base.OnInitialize();

            _imageVisualObject = new ImageVisualObject(_layer);
            _mapVisual.AddVisual(_imageVisualObject);
        }

        /// <inheritdoc />
        public override void OnUpdate(long tickCount)
        {
            _ticksCount += tickCount;
            if (_ticksCount < FRAME_CHANGE_SPEED)
                return;
            
            ++FrameIndex;
            FrameIndex %= _frames.Count;
            _ticksCount %= FRAME_CHANGE_SPEED;

            var frame = _frames[FrameIndex];

            _imageVisualObject.Bitmap = frame.Bitmap;

            var posX = GameObject.X + frame.OffsetX;
            if (Math.Abs(_imageVisualObject.X - posX) > float.Epsilon) {
                _imageVisualObject.X = posX;
            }

            var posY = GameObject.Y + frame.OffsetY;
            if (Math.Abs(_imageVisualObject.Y - posY) > float.Epsilon) {
                _imageVisualObject.Y = posY;
            }

            if (Math.Abs(_imageVisualObject.Width - frame.Width) > float.Epsilon) {
                _imageVisualObject.Width = frame.Width;
            }

            if (Math.Abs(_imageVisualObject.Height - frame.Height) > float.Epsilon) {
                _imageVisualObject.Height = frame.Height;
            }
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            _mapVisual.RemoveVisual(_imageVisualObject);
        }
    }
}
