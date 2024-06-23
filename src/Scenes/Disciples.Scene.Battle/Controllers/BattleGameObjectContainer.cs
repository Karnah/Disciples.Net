using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc cref="IBattleGameObjectContainer" />
/// <remarks>
/// TODO переписать на DI.
/// </remarks>
internal class BattleGameObjectContainer : BaseSceneGameObjectContainer, IBattleGameObjectContainer
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly ITextProvider _textProvider;
    private readonly BattleContext _battleContext;
    private readonly Lazy<IBattleInterfaceController> _battleInterfaceController;

    public BattleGameObjectContainer(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        IBattleInterfaceProvider battleInterfaceProvider,
        ITextProvider textProvider,
        BattleContext battleContext,
        Lazy<IBattleInterfaceController> battleInterfaceController
        ) : base(gameObjectContainer)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _battleInterfaceProvider = battleInterfaceProvider;
        _textProvider = textProvider;
        _battleContext = battleContext;
        _battleInterfaceController = battleInterfaceController;
    }

    /// <inheritdoc />
    public BattleUnit AddBattleUnit(Unit unit, BattleSquadPosition unitSquadPosition)
    {
        var battleUnit = new BattleUnit(_sceneObjectContainer, _battleUnitResourceProvider,
            _battleInterfaceController.Value.BattleUnitSelected,
            _battleInterfaceController.Value.BattleUnitUnselected,
            _battleInterfaceController.Value.BattleUnitLeftMouseButtonClicked,
            _battleInterfaceController.Value.BattleUnitRightMouseButtonPressed,
            unit,
            unitSquadPosition,
            _battleInterfaceController.Value.GetBattleUnitPosition(unitSquadPosition, unit.SquadPosition));
        return AddObject(battleUnit);
    }

    /// <inheritdoc />
    public SummonPlaceholder AddSummonPlaceholder(BattleUnitPosition position, RectangleD bounds)
    {
        var summonPlaceholder = new SummonPlaceholder(_sceneObjectContainer, _battleUnitResourceProvider,
            _battleInterfaceController.Value.SummonPlaceholderLeftMouseButtonClicked,
            _battleInterfaceController.Value.SummonPlaceholderRightMouseButtonPressed,
            position,
            bounds);
        return AddObject(summonPlaceholder);
    }

    /// <inheritdoc />
    public UnitPortraitObject AddUnitPortrait(Unit unit, BattleSquadPosition unitSquadPosition, RectangleD portraitBounds, RectangleD hitPointsBounds)
    {
        var unitPortrait = new UnitPortraitObject(_textProvider, _sceneObjectContainer, _battleInterfaceProvider, _battleUnitResourceProvider,
            _battleInterfaceController.Value.UnitPortraitSelected,
            _battleInterfaceController.Value.UnitPortraitLeftMouseButtonClicked,
            _battleInterfaceController.Value.UnitPortraitRightMouseButtonPressed,
            unit, unitSquadPosition, portraitBounds, hitPointsBounds);
        return AddObject(unitPortrait);
    }

    /// <inheritdoc />
    public BottomUnitPortraitObject AddBottomUnitPortrait(bool isLeft,
        SceneElement portraitSceneElement,
        SceneElement leaderPanelSceneElement,
        SceneElement unitInfoSceneElement)
    {
        var bottomUnitPortrait = new BottomUnitPortraitObject(_sceneObjectContainer, _textProvider, _battleUnitResourceProvider, _battleContext,
            isLeft, portraitSceneElement, leaderPanelSceneElement, unitInfoSceneElement,
            _battleInterfaceController.Value.BottomUnitPortraitRightMouseButtonPressed);
        return AddObject(bottomUnitPortrait);
    }
}