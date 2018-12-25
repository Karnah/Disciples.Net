using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.VisualObjects;

namespace Disciples.Engine.Battle.Components
{
    public class BattleUnitAnimationComponent : Component
    {
        private const int FRAME_CHANGE_SPEED = 75;

        private readonly BattleUnit _battleUnit;
        private readonly IMapVisual _mapVisual;
        private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
        private readonly string _unitId;

        private long _ticksCount = 0;
        private BattleAction _action;

        private IReadOnlyList<Frame> _shadowFrames;
        private IReadOnlyList<Frame> _unitFrames;
        private IReadOnlyList<Frame> _auraFrames;

        private ImageVisualObject _shadowVisual;
        private ImageVisualObject _unitVisual;
        private ImageVisualObject _auraVisual;


        public BattleUnitAnimationComponent(
            BattleUnit battleUnit,
            IMapVisual mapVisual,
            IBattleUnitResourceProvider battleUnitResourceProvider,
            string unitId) : base(battleUnit)
        {
            _battleUnit = battleUnit;
            _mapVisual = mapVisual;
            _battleUnitResourceProvider = battleUnitResourceProvider;
            _unitId = unitId;

            Layer = GetLayer();
        }


        /// <summary>
        /// Вычислить на каком слое располагается юнит визуально.
        /// </summary>
        private int GetLayer()
        {
            var battleLine = _battleUnit.IsAttacker
                ? _battleUnit.Unit.SquadLinePosition
                : 3 - _battleUnit.Unit.SquadLinePosition;
            var flankPosition = 2 - _battleUnit.Unit.SquadFlankPosition;


            return battleLine * 100 + flankPosition * 10 + 5;
        }


        public BattleUnitAnimation BattleUnitAnimation { get; private set; }

        public int FrameIndex { get; private set; }

        public int FramesCount { get; private set; }

        public int Layer { get; }


        public override void OnInitialize()
        {
            base.OnInitialize();

            BattleUnitAnimation = _battleUnitResourceProvider.GetBattleUnitAnimation(_unitId, _battleUnit.Direction);

            // Чтобы юниты не двигались синхронно в начале боя, первый кадр выбирается случайно
            FrameIndex = RandomGenerator.Next(BattleUnitAnimation.BattleUnitFrames[_battleUnit.Action].UnitFrames.Count);
            UpdateSource();
        }

        public override void OnUpdate(long tickCount)
        {
            if (_battleUnit.Action != _action) {
                FrameIndex = 0;
                UpdateSource();
                return;
            }

            _ticksCount += tickCount;
            if (_ticksCount < FRAME_CHANGE_SPEED)
                return;

            ++FrameIndex;
            // todo Хреновая реализация контроллера анимации.
            // Предполагается, что любая анимация будет выполняться только 1 раз, кроме анимации ожидания и смерти
            if (FrameIndex >= FramesCount && _action != BattleAction.Waiting && _action != BattleAction.Dead) {
                _battleUnit.Action = BattleAction.Waiting;
                return;
            }

            FrameIndex %= FramesCount;
            _ticksCount %= FRAME_CHANGE_SPEED;

            UpdateBitmap(_shadowVisual, _shadowFrames, FrameIndex);
            UpdateBitmap(_unitVisual, _unitFrames, FrameIndex);
            UpdateBitmap(_auraVisual, _auraFrames, FrameIndex);
        }


        private static void UpdateBitmap(ImageVisualObject imageVisual, IReadOnlyList<Frame> frames, int frameIndex)
        {
            if (imageVisual == null)
                return;

            if (frames?.Any() != true)
                return;

            imageVisual.Bitmap = frames[frameIndex].Bitmap;
        }


        private void UpdateSource()
        {
            _action = _battleUnit.Action;

            var frames = BattleUnitAnimation.BattleUnitFrames[_action];
            _shadowFrames = frames.ShadowFrames;
            _unitFrames = frames.UnitFrames;
            _auraFrames = frames.AuraFrames;

            UpdatePosition(ref _shadowVisual, _shadowFrames, Layer - 1);
            UpdatePosition(ref _unitVisual, _unitFrames, Layer);
            UpdatePosition(ref _auraVisual, _auraFrames, Layer + 1);

            FramesCount = _unitFrames.Count;
        }

        private void UpdatePosition(ref ImageVisualObject imageVisual, IReadOnlyList<Frame> frames, int layer)
        {
            if (frames == null) {
                if (imageVisual != null) {
                    // todo Здесь происходит удаление.
                    // Возможно, просто достаточно перенести в невидимую область
                    _mapVisual.RemoveVisual(imageVisual);
                    imageVisual = null;
                }
            }
            else {
                if (imageVisual == null) {
                    imageVisual = new ImageVisualObject(layer);
                    _mapVisual.AddVisual(imageVisual);
                }

                var frame = frames[FrameIndex];
                imageVisual.Bitmap = frame.Bitmap;

                // Предполагается, что размеры и смещение всех кадров в анимации одинаково,
                // Поэтому это условие проверяется только при изменении действия юнита.
                var posX = _battleUnit.X + frame.OffsetX;
                if (Math.Abs(imageVisual.X - posX) > float.Epsilon) {
                    imageVisual.X = posX;
                }

                var posY = _battleUnit.Y + frame.OffsetY;
                if (Math.Abs(imageVisual.Y - posY) > float.Epsilon) {
                    imageVisual.Y = posY;
                }

                if (Math.Abs(imageVisual.Width - frame.Width) > float.Epsilon) {
                    imageVisual.Width = frame.Width;
                }

                if (Math.Abs(imageVisual.Height - frame.Height) > float.Epsilon) {
                    imageVisual.Height = frame.Height;
                }
            }
        }
    }
}
