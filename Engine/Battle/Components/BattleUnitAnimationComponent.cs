using System;
using System.Collections.Generic;
using System.Linq;

using Engine.Battle.Enums;
using Engine.Battle.GameObjects;
using Engine.Battle.Models;
using Engine.Battle.Providers;
using Engine.Common.Components;
using Engine.Common.Controllers;
using Engine.Common.Models;

namespace Engine.Battle.Components
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

        private VisualObject _shadowVisual;
        private VisualObject _unitVisual;
        private VisualObject _auraVisual;


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
        /// Вычислить на каком слое располгается юнит визуально
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

        private void UpdatePosition(ref VisualObject visual, IReadOnlyList<Frame> frames, int layer)
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
                    visual = new VisualObject(GameObject, layer);
                    _mapVisual.AddVisual(visual);
                }

                var frame = frames[FrameIndex];
                visual.Bitmap = frame.Bitmap;

                // Предполагается, что размеры и смещение всех фреймов в анимации одинаково
                // Поэтому это условие проверяется только при изменении действия юнита
                var posX = _battleUnit.X + frame.OffsetX;
                if (Math.Abs(visual.X - posX) > float.Epsilon) {
                    visual.X = posX;
                }

                var posY = _battleUnit.Y + frame.OffsetY;
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
