using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc cref="IBattleGameObjectContainer" />
internal class BattleGameObjectContainer : IBattleGameObjectContainer
{
    private readonly IGameObjectContainer _gameObjectContainer;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly ITextProvider _textProvider;
    private readonly Lazy<IBattleInterfaceController> _battleInterfaceController;

    public BattleGameObjectContainer(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        IBattleInterfaceProvider battleInterfaceProvider,
        ITextProvider textProvider,
        Lazy<IBattleInterfaceController> battleInterfaceController)
    {
        _gameObjectContainer = gameObjectContainer;
        _sceneObjectContainer = sceneObjectContainer;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _battleInterfaceProvider = battleInterfaceProvider;
        _textProvider = textProvider;
        _battleInterfaceController = battleInterfaceController;
    }

    /// <inheritdoc />
    public BattleUnit AddBattleUnit(Unit unit, bool isAttacker)
    {
        var battleUnit = new BattleUnit(_sceneObjectContainer, _battleUnitResourceProvider, 
            _battleInterfaceController.Value.BattleUnitSelected,
            _battleInterfaceController.Value.BattleUnitUnselected,
            _battleInterfaceController.Value.BattleUnitLeftMouseButtonClicked,
            _battleInterfaceController.Value.BattleUnitRightMouseButtonPressed,
            unit,
            isAttacker);
        return AddObject(battleUnit);
    }

    /// <inheritdoc />
    public BattleUnitInfoGameObject AddBattleUnitInfo(int x, int y, int layer)
    {
        var battleUnitInfoObject = new BattleUnitInfoGameObject(_sceneObjectContainer, x, y, layer);
        return AddObject(battleUnitInfoObject);
    }

    /// <inheritdoc />
    public UnitPortraitObject AddUnitPortrait(Unit unit, bool rightToLeft, double x, double y)
    {
        var unitPortrait = new UnitPortraitObject(_textProvider, _sceneObjectContainer, _battleInterfaceProvider, _battleUnitResourceProvider,
            _battleInterfaceController.Value.UnitPortraitSelected,
            _battleInterfaceController.Value.UnitPortraitLeftMouseButtonClicked,
            _battleInterfaceController.Value.UnitPortraitRightMouseButtonPressed,
            unit, rightToLeft, x, y);
        return AddObject(unitPortrait);
    }

    /// <inheritdoc />
    public DetailUnitInfoObject ShowDetailUnitInfo(Unit unit)
    {
        var detailUnitInfoObject = new DetailUnitInfoObject(_sceneObjectContainer, _battleInterfaceProvider, _battleUnitResourceProvider, _textProvider, unit);
        return AddObject(detailUnitInfoObject);
    }

    #region IGameObjectContainer

    /// <inheritdoc />
    public IReadOnlyCollection<GameObject> GameObjects => _gameObjectContainer.GameObjects;

    /// <inheritdoc />
    public AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true)
    {
        return _gameObjectContainer.AddAnimation(frames, x, y, layer, repeat);
    }

    /// <inheritdoc />
    public ButtonObject AddButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer,
        KeyboardButton? hotkey = null)
    {
        return _gameObjectContainer.AddButton(buttonStates, buttonPressedAction, x, y, layer, hotkey);
    }

    /// <inheritdoc />
    public ToggleButtonObject AddToggleButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer,
        KeyboardButton? hotkey = null)
    {
        return _gameObjectContainer.AddToggleButton(buttonStates, buttonPressedAction, x, y, layer, hotkey);
    }

    /// <inheritdoc />
    public TGameObject AddObject<TGameObject>(TGameObject gameObject) where TGameObject : GameObject
    {
        return _gameObjectContainer.AddObject(gameObject);
    }

    /// <inheritdoc />
    public void UpdateGameObjects(long ticksCount)
    {
        _gameObjectContainer.UpdateGameObjects(ticksCount);
    }

    #endregion
}