using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Platform.Factories;
using Disciples.Scene.Battle.Controllers;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle
{
    /// <summary>
    /// Сцена битвы двух отрядов.
    /// </summary>
    public class BattleScene : BaseSceneController<BattleSceneParameters>, IBattleSceneController
    {
        private readonly ITextProvider _textProvider;
        private readonly IUnitInfoProvider _unitInfoProvider;

        private readonly IBattleResourceProvider _battleResourceProvider;
        private readonly IBattleInterfaceProvider _battleInterfaceProvider;
        private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;

        private readonly BattleContext _battleContext;

        private IBattleController _battleController;
        private IBattleInterfaceController _battleInterfaceController;

        /// <inheritdoc />
        public BattleScene(
            IGameController gameController,
            ISceneFactory sceneFactory,
            IInterfaceProvider interfaceProvider,
            ITextProvider textProvider,
            IUnitInfoProvider unitInfoProvider,
            IBattleResourceProvider battleResourceProvider,
            IBattleInterfaceProvider battleInterfaceProvider,
            IBattleUnitResourceProvider battleUnitResourceProvider,
            BattleContext battleContext
            ) : base(gameController, sceneFactory, interfaceProvider)
        {
            _textProvider = textProvider;
            _unitInfoProvider = unitInfoProvider;
            _battleResourceProvider = battleResourceProvider;
            _battleInterfaceProvider = battleInterfaceProvider;
            _battleUnitResourceProvider = battleUnitResourceProvider;
            _battleContext = battleContext;
        }


        /// <inheritdoc />
        public override void InitializeParameters(BattleSceneParameters parameters)
        {
            base.InitializeParameters(parameters);

            _battleContext.AttackingSquad = parameters.AttackingSquad;
            _battleContext.DefendingSquad = parameters.DefendingSquad;

            _battleController = parameters.BattleController;
            _battleInterfaceController = parameters.BattleInterfaceController;
        }

        /// <inheritdoc />
        public override void BeforeSceneUpdate(UpdateSceneData data)
        {
            base.BeforeSceneUpdate(data);

            _battleContext.BeforeSceneUpdate(data);
            _battleInterfaceController.BeforeSceneUpdate();
            _battleController.BeforeSceneUpdate();
        }

        /// <inheritdoc />
        public override void AfterSceneUpdate(UpdateSceneData data)
        {
            base.AfterSceneUpdate(data);

            _battleController.AfterSceneUpdate();
            _battleInterfaceController.AfterSceneUpdate();
            _battleContext.AfterSceneUpdate();
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
            foreach (var component in components)
            {
                if (component.IsSharedBetweenScenes)
                    continue;

                component.Unload();
            }

            // TODO Инициализации.
            //_battleContext = new BattleContext();
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