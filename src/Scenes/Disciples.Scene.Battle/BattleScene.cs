using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;
using Disciples.Scene.Battle.Controllers;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle;

/// <summary>
/// Сцена битвы двух отрядов.
/// </summary>
internal class BattleScene : BaseScene, IBattleScene
{
    private readonly ITextProvider _textProvider;
    private readonly IUnitInfoProvider _unitInfoProvider;
    private readonly IInterfaceProvider _interfaceProvider;

    private readonly IBattleResourceProvider _battleResourceProvider;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly IBattleController _battleController;
    private readonly IBattleInterfaceController _battleInterfaceController;
    private readonly BattleContext _battleContext;
    private readonly BattleSoundController _battleSoundController;

    /// <summary>
    /// Создать объект типа <see cref="BattleScene" />.
    /// </summary>
    public BattleScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IDialogController dialogController,
        ITextProvider textProvider,
        IUnitInfoProvider unitInfoProvider,
        IInterfaceProvider interfaceProvider,
        IBattleResourceProvider battleResourceProvider,
        IBattleInterfaceProvider battleInterfaceProvider,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        IBattleController battleController,
        IBattleInterfaceController battleInterfaceController,
        BattleContext battleContext,
        BattleSoundController battleSoundController
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController)
    {
        _textProvider = textProvider;
        _unitInfoProvider = unitInfoProvider;
        _interfaceProvider = interfaceProvider;
        _battleResourceProvider = battleResourceProvider;
        _battleInterfaceProvider = battleInterfaceProvider;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _battleController = battleController;
        _battleInterfaceController = battleInterfaceController;
        _battleContext = battleContext;
        _battleSoundController = battleSoundController;
    }

    /// <inheritdoc />
    protected override bool IsProcessInputDeviceEvents => false;

    /// <inheritdoc />
    public void InitializeParameters(BattleSceneParameters parameters)
    {
        _battleContext.AttackingSquad = parameters.AttackingSquad;
        _battleContext.DefendingSquad = parameters.DefendingSquad;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        base.LoadInternal();

        _textProvider.Load();
        _unitInfoProvider.Load();
        _interfaceProvider.Load();

        _battleResourceProvider.Load();
        _battleInterfaceProvider.Load();
        _battleUnitResourceProvider.Load();

        _battleController.Load();
        _battleInterfaceController.Load();
        _battleSoundController.Load();
        _battleContext.Load();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        base.UnloadInternal();

        _battleSoundController.Unload();
    }

    /// <inheritdoc />
    protected override void BeforeSceneUpdate(UpdateSceneData data)
    {
        _battleContext.BeforeSceneUpdate(data);

        ProcessInputDeviceEvents(data.InputDeviceEvents);

        _battleInterfaceController.BeforeSceneUpdate();
        _battleController.BeforeSceneUpdate();
        _battleSoundController.BeforeSceneUpdate();
    }

    /// <inheritdoc />
    protected override void AfterSceneUpdate()
    {
        _battleContext.AfterSceneUpdate();
        _battleController.AfterSceneUpdate();
        _battleInterfaceController.AfterSceneUpdate();
        _battleSoundController.AfterSceneUpdate();
    }
}