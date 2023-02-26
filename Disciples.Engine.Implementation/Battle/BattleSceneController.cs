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
    public class BattleSceneController : BaseSceneController<BattleSceneParameters>, IBattleSceneController
    {
        private readonly ITextProvider _textProvider;
        private readonly IUnitInfoProvider _unitInfoProvider;

        private readonly IBattleResourceProvider _battleResourceProvider;
        private readonly IBattleInterfaceProvider _battleInterfaceProvider;
        private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;

        private IBattleController _battleController;
        private IBattleInterfaceController _battleInterfaceController;

        /// <inheritdoc />
        public BattleSceneController(
            IGameController gameController,
            ISceneFactory sceneFactory,
            IInterfaceProvider interfaceProvider,
            ITextProvider textProvider,
            IUnitInfoProvider unitInfoProvider,
            IBattleResourceProvider battleResourceProvider,
            IBattleInterfaceProvider battleInterfaceProvider,
            IBattleUnitResourceProvider battleUnitResourceProvider
            ) : base(gameController, sceneFactory, interfaceProvider)
        {
            _textProvider = textProvider;
            _unitInfoProvider = unitInfoProvider;
            _battleResourceProvider = battleResourceProvider;
            _battleInterfaceProvider = battleInterfaceProvider;
            _battleUnitResourceProvider = battleUnitResourceProvider;
        }


        /// <inheritdoc />
        public override bool IsSharedBetweenScenes => false;

        /// <inheritdoc />
        public override void InitializeParameters(ISceneContainer sceneContainer, BattleSceneParameters parameters)
        {
            base.InitializeParameters(sceneContainer, parameters);

            _battleController = parameters.BattleController;
            _battleController.InitializeParameters(new BattleSquadsData(parameters.AttackSquad, parameters.DefendSquad));

            _battleInterfaceController = parameters.BattleInterfaceController;
        }

        /// <inheritdoc />
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _textProvider.Load();
            _unitInfoProvider.Load();

            _battleResourceProvider.Load();
            _battleInterfaceProvider.Load();
            _battleUnitResourceProvider.Load();

            _battleController.Load();
            _battleInterfaceController.Load();
        }

        /// <inheritdoc />
        protected override void UnloadInternal()
        {
            base.UnloadInternal();

            var components = new ISupportLoading[] {
                _textProvider,
                _unitInfoProvider,
                _battleResourceProvider,
                _battleInterfaceProvider,
                _battleUnitResourceProvider,
                _battleController,
                _battleInterfaceController
            };

            // Деинициализируем те компоненты, которые существует в рамках одной сцены.
            foreach (var component in components) {
                if (component.IsSharedBetweenScenes)
                    continue;

                component.Unload();
            }
        }


        /// <inheritdoc />
        public BattleUnit AddBattleUnit(Unit unit, bool isAttacker)
        {
            var battleUnit = new BattleUnit(this, _battleUnitResourceProvider, unit, isAttacker);
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
            var unitPortrait = new UnitPortraitObject(_textProvider, this, _battleInterfaceProvider, unit, rightToLeft, x, y);
            GameController.CreateObject(unitPortrait);

            return unitPortrait;
        }

        /// <inheritdoc />
        public DetailUnitInfoObject ShowDetailUnitInfo(Unit unit)
        {
            var detailUnitInfoObject = new DetailUnitInfoObject(this, _battleInterfaceProvider, _textProvider, unit);
            GameController.CreateObject(detailUnitInfoObject);

            return detailUnitInfoObject;
        }
    }
}