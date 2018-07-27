using System;
using System.Collections.Generic;
using System.Linq;

using Engine.Battle.Enums;
using Engine.Battle.Models;
using Engine.Battle.Providers;
using Engine.Components;
using Engine.Interfaces;
using Engine.Models;

namespace Engine.Battle.Components
{
    public class BattleUnitAnimationComponent : Component
    {
        private const int FrameChangeSpeed = 75;

        private readonly IMapVisual _mapVisual;
        private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
        private readonly string _unitId;
        private BattleObjectComponent _battleObject;

        private long _ticksCount = 0;
        private BattleAction _action;

        private BattleUnitAnimation _unitAnimation;

        private IReadOnlyList<Frame> _shadowFrames;
        private IReadOnlyList<Frame> _unitFrames;
        private IReadOnlyList<Frame> _auraFrames;

        private VisualObject _shadowVisual;
        private VisualObject _unitVisual;
        private VisualObject _auraVisual;


        public BattleUnitAnimationComponent(
            GameObject gameObject,
            IMapVisual mapVisual,
            IBattleUnitResourceProvider battleUnitResourceProvider,
            string unitId) : base(
            gameObject)
        {
            _mapVisual = mapVisual;
            _battleUnitResourceProvider = battleUnitResourceProvider;
            _unitId = unitId;
        }


        public int FrameIndex { get; private set; }

        public int FramesCount { get; private set; }


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

            UpdateBitmap(_shadowVisual, _shadowFrames, FrameIndex);
            UpdateBitmap(_unitVisual, _unitFrames, FrameIndex);
            UpdateBitmap(_auraVisual, _auraFrames, FrameIndex);
        }


        private static void UpdateBitmap(VisualObject visual, IReadOnlyList<Frame> frames, int frameIndex)
        {
            if (visual == null)
                return;

            if (frames?.Any() != true)
                return;

            visual.Bitmap = frames[frameIndex].Bitmap;
        }


        private void UpdateSource()
        {
            _action = _battleObject.Action;
            FrameIndex = 1;

            var frames = _unitAnimation.BattleUnitFrameses[_action];
            _shadowFrames = frames.ShadowFrames;
            _unitFrames = frames.UnitFrames;
            _auraFrames = frames.AuraFrames;

            UpdatePosition(ref _shadowVisual, _shadowFrames);
            UpdatePosition(ref _unitVisual, _unitFrames);
            UpdatePosition(ref _auraVisual, _auraFrames);

            FramesCount = _unitFrames.Count;
        }

        private void UpdatePosition(ref VisualObject visual, IReadOnlyList<Frame> frames)
        {
            if (frames == null) {
                if (visual != null) {
                    // todo Здесь происходит удаление.
                    // Возможно, просто достаточно перенести в невидимую область
                    _mapVisual.RemoveVisual(visual);
                    visual = null;
                }
            }
            else {
                if (visual == null) {
                    visual = new VisualObject();
                    _mapVisual.AddVisual(visual);
                }

                var frame = frames[0];
                visual.Bitmap = frame.Bitmap;

                // Предполагается, что размеры и смещение всех фреймов в анимации одинаково
                // Поэтому это условие проверяется только при изменении действия юнита
                var posX = _battleObject.Position.X + frame.OffsetX;
                if (Math.Abs(visual.X - posX) > float.Epsilon) {
                    visual.X = posX;
                }

                var posY = _battleObject.Position.Y + frame.OffsetY;
                if (Math.Abs(visual.Y - posY) > float.Epsilon) {
                    visual.Y = posY;
                }

                if (Math.Abs(visual.Width - frame.Width) > float.Epsilon) {
                    visual.Width = frame.Width;
                }

                if (Math.Abs(visual.Height - frame.Height) > float.Epsilon) {
                    visual.Height = frame.Height;
                }
            }
        }
    }
}
