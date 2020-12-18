using System;
using System.Collections.Generic;

using Disciples.Engine.Base;
using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Battle.Components
{
    /// <summary>
    /// Компонент для создания анимации юнита.
    /// </summary>
    public class BattleUnitAnimationComponent : BaseAnimationComponent
    {
        private readonly BattleUnit _battleUnit;
        private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;

        /// <summary>
        /// Анимация какого действия отображается в данный момент.
        /// </summary>
        private BattleAction _action;

        /// <summary>
        /// Кадры анимации тени юнита.
        /// </summary>
        private IReadOnlyList<Frame> _shadowFrames;
        /// <summary>
        /// Кадры анимации юнита.
        /// </summary>
        private IReadOnlyList<Frame> _unitFrames;
        /// <summary>
        /// Кадры анимации ауры юнита.
        /// </summary>
        private IReadOnlyList<Frame> _auraFrames;

        /// <summary>
        /// Изображение, которые отрисовывает кадры анимации тени юнита.
        /// </summary>
        private IImageSceneObject _shadowAnimationHost;
        /// <summary>
        /// Изображение, которые отрисовывает кадры анимации юнита.
        /// </summary>
        private IImageSceneObject _unitAnimationHost;
        /// <summary>
        /// Изображение, которые отрисовывает кадры анимации ауры юнита.
        /// </summary>
        private IImageSceneObject _auraAnimationHost;

        /// <inheritdoc />
        public BattleUnitAnimationComponent(
            BattleUnit battleUnit,
            ISceneController sceneController,
            IBattleUnitResourceProvider battleUnitResourceProvider
            ) : base(battleUnit, sceneController, GetLayer(battleUnit))
        {
            _battleUnit = battleUnit;
            _battleUnitResourceProvider = battleUnitResourceProvider;
        }


        /// <summary>
        /// Вычислить на каком слое располагается юнит визуально.
        /// </summary>
        private static int GetLayer(BattleUnit battleUnit)
        {
            var battleLine = battleUnit.IsAttacker
                ? battleUnit.Unit.SquadLinePosition
                : 3 - battleUnit.Unit.SquadLinePosition;
            var flankPosition = 2 - battleUnit.Unit.SquadFlankPosition;


            return battleLine * 100 + flankPosition * 10 + 5;
        }


        /// <summary>
        /// Вся информация об анимации юнита.
        /// </summary>
        public BattleUnitAnimation BattleUnitAnimation { get; private set; }


        /// <inheritdoc />
        public override void OnInitialize()
        {
            base.OnInitialize();

            _battleUnit.UnitStateChanged += OnUnitStateChanged;

            BattleUnitAnimation = _battleUnitResourceProvider.GetBattleUnitAnimation(_battleUnit.Unit.UnitType.UnitTypeId, _battleUnit.Direction);

            // Чтобы юниты не двигались синхронно в начале боя, первый кадр выбирается случайно.
            var frameIndex = RandomGenerator.Next(BattleUnitAnimation.BattleUnitFrames[_battleUnit.Action].UnitFrames.Count);
            UpdateSource(frameIndex);
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            _battleUnit.UnitStateChanged -= OnUnitStateChanged;
        }


        /// <inheritdoc />
        protected override void OnAnimationFrameChange()
        {
            UpdateFrame(_shadowAnimationHost, _shadowFrames);
            UpdateFrame(_unitAnimationHost, _unitFrames);
            UpdateFrame(_auraAnimationHost, _auraFrames);
        }

        /// <inheritdoc />
        protected override IReadOnlyList<IImageSceneObject> GetAnimationHosts()
        {
            return new[] {
                _shadowAnimationHost,
                _unitAnimationHost,
                _auraAnimationHost
            };
        }


        /// <summary>
        /// Обработать событие изменения состояния юнита.
        /// </summary>
        private void OnUnitStateChanged(object sender, EventArgs e)
        {
            UpdateSource();
        }

        /// <summary>
        /// Обновить анимацию юнита.
        /// </summary>
        private void UpdateSource(int startFrameIndex = 0)
        {
            _action = _battleUnit.Action;

            var frames = BattleUnitAnimation.BattleUnitFrames[_action];
            _shadowFrames = frames.ShadowFrames;
            _unitFrames = frames.UnitFrames;
            _auraFrames = frames.AuraFrames;

            FrameIndex = startFrameIndex;
            FramesCount = _unitFrames.Count;

            UpdatePosition(ref _shadowAnimationHost, _shadowFrames, Layer - 1);
            UpdatePosition(ref _unitAnimationHost, _unitFrames, Layer);
            UpdatePosition(ref _auraAnimationHost, _auraFrames, Layer + 1);
        }
    }
}