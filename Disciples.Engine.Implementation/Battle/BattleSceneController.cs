using System;
using Disciples.Engine.Base;
using Disciples.Engine.Battle;
using Disciples.Engine.Battle.Controllers;
using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Platform.Factories;

namespace Disciples.Engine.Implementation.Battle
{
    /// <inheritdoc cref="IBattleSceneController" />
    public class BattleSceneController : BaseSceneController<BattleInitializeData>, IBattleSceneController
    {
        private readonly ITextProvider _textProvider;
        private readonly IUnitInfoProvider _unitInfoProvider;

        private readonly Lazy<IBattleResourceProvider> _battleResourceProvider;
        private readonly Lazy<IBattleInterfaceProvider> _battleInterfaceProvider;
        private readonly Lazy<IBattleUnitResourceProvider> _battleUnitResourceProvider;
        private readonly Lazy<IBattleActionProvider> _battleActionProvider;
        private readonly Lazy<IBattleController> _battleController;
        private readonly Lazy<IBattleInterfaceController> _battleInterfaceController;

        /// <inheritdoc />
        public BattleSceneController(
            IGameController gameController,
            ISceneFactory sceneFactory,
            IInterfaceProvider interfaceProvider,
            ITextProvider textProvider,
            IUnitInfoProvider unitInfoProvider,
            Lazy<IBattleResourceProvider> battleResourceProvider,
            Lazy<IBattleInterfaceProvider> battleInterfaceProvider,
            Lazy<IBattleUnitResourceProvider> battleUnitResourceProvider,
            Lazy<IBattleActionProvider> battleActionProvider,
            Lazy<IBattleController> battleController,
            Lazy<IBattleInterfaceController> battleInterfaceController
            ) : base(gameController, sceneFactory, interfaceProvider)
        {
            _textProvider = textProvider;
            _unitInfoProvider = unitInfoProvider;
            _battleResourceProvider = battleResourceProvider;
            _battleInterfaceProvider = battleInterfaceProvider;
            _battleUnitResourceProvider = battleUnitResourceProvider;
            _battleActionProvider = battleActionProvider;
            _battleController = battleController;
            _battleInterfaceController = battleInterfaceController;
        }


        /// <inheritdoc />
        public override bool OneTimeLoading => false;


        /// <inheritdoc />
        protected override void LoadInternal(ISceneContainer sceneContainer, BattleInitializeData data)
        {
            base.LoadInternal(sceneContainer, data);

            _textProvider.Load();
            _unitInfoProvider.Load();

            _battleResourceProvider.Value.Load();
            _battleInterfaceProvider.Value.Load();
            _battleUnitResourceProvider.Value.Load();
            _battleActionProvider.Value.Load();

            _battleController.Value.Load(new BattleSquadsData(data.AttackSquad, data.DefendSquad));
            _battleInterfaceController.Value.Load();
        }

        /// <inheritdoc />
        protected override void UnloadInternal()
        {
            base.UnloadInternal();

            var components = new ISupportUnloading[] {
                _textProvider,
                _unitInfoProvider,
                _battleResourceProvider.Value,
                _battleInterfaceProvider.Value,
                _battleUnitResourceProvider.Value,
                _battleActionProvider.Value,
                _battleController.Value,
                _battleInterfaceController.Value
            };

            // Деинициализируем те компоненты, которые существует в рамках одной сцены.
            foreach (var component in components) {
                if (component.OneTimeLoading)
                    continue;

                component.Unload();
            }
        }


        /// <inheritdoc />
        public BattleUnit AddBattleUnit(Unit unit, bool isAttacker)
        {
            var battleUnit = new BattleUnit(this, _battleUnitResourceProvider.Value, unit, isAttacker);
            GameController.CreateObject(battleUnit);

            return battleUnit;
        }

        /// <inheritdoc />
        public BattleUnitInfoGameObject AddBattleUnitInfo(int x, int y, int layer)
        {
            var battleUnitInfoObject = new BattleUnitInfoGameObject(this, x, y, layer);
            GameController.CreateObject(battleUnitInfoObject);

            return battleUnitInfoObject;
        }

        /// <inheritdoc />
        public UnitPortraitObject AddUnitPortrait(Unit unit, bool rightToLeft, double x, double y)
        {
            var unitPortrait = new UnitPortraitObject(_textProvider, this, _battleActionProvider.Value, _battleInterfaceProvider.Value, unit, rightToLeft, x, y);
            GameController.CreateObject(unitPortrait);

            return unitPortrait;
        }

        /// <inheritdoc />
        public DetailUnitInfoObject ShowDetailUnitInfo(Unit unit)
        {
            var detailUnitInfoObject = new DetailUnitInfoObject(this, _battleInterfaceProvider.Value, _textProvider, unit);
            GameController.CreateObject(detailUnitInfoObject);

            return detailUnitInfoObject;
        }
    }
}