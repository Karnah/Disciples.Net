using System;
using System.Collections.Generic;

using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.Components
{
    /// <summary>
    /// Компонент для создания анимации.
    /// </summary>
    public class AnimationComponent : BaseComponent
    {
        /// <summary>
        /// Промежуток времени в мс через которое происходит смена кадра в анимации.
        /// </summary>
        private const int FRAME_CHANGE_SPEED = 75;

        private readonly IVisualSceneController _visualSceneController;
        private readonly IReadOnlyList<Frame> _frames;
        private readonly int _layer;

        private IImageSceneObject _animationFrame;
        private long _ticksCount = 0;

        /// <inheritdoc />
        public AnimationComponent(
            GameObject gameObject,
            IVisualSceneController visualSceneController,
            IReadOnlyList<Frame> frames,
            int layer
            ) : base(gameObject)
        {
            _visualSceneController = visualSceneController;
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

            _animationFrame = _visualSceneController.AddImage(_layer);
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

            _animationFrame.Bitmap = frame.Bitmap;

            var posX = GameObject.X + frame.OffsetX;
            if (Math.Abs(_animationFrame.X - posX) > float.Epsilon) {
                _animationFrame.X = posX;
            }

            var posY = GameObject.Y + frame.OffsetY;
            if (Math.Abs(_animationFrame.Y - posY) > float.Epsilon) {
                _animationFrame.Y = posY;
            }

            if (Math.Abs(_animationFrame.Width - frame.Width) > float.Epsilon) {
                _animationFrame.Width = frame.Width;
            }

            if (Math.Abs(_animationFrame.Height - frame.Height) > float.Epsilon) {
                _animationFrame.Height = frame.Height;
            }
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            _visualSceneController.RemoveSceneObject(_animationFrame);
        }
    }
}