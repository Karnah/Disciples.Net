using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;
using Disciples.Resources.Common;

namespace Disciples.Scene.LoadingSave;

/// <inheritdoc cref="ILoadingSaveScene" />
internal class LoadingSaveScene : BaseScene, ILoadingSaveScene
{
    private readonly IGameObjectContainer _gameObjectContainer;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IGameController _gameController;
    private readonly IUnitInfoProvider _unitInfoProvider;
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly ITextProvider _textProvider;
    private readonly IReadOnlyList<BaseResourceExtractor> _resourceExtractors;

    private string _savePath = null!;

    private AnimationObject _loadingAnimationObject = null!;
    private ITextSceneObject _loadingText = null!;

    /// <summary>
    /// Создать объект типа <see cref="LoadingSaveScene" />.
    /// </summary>
    public LoadingSaveScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IDialogController dialogController,
        IGameController gameController,
        IUnitInfoProvider unitInfoProvider,
        IInterfaceProvider interfaceProvider,
        ITextProvider textProvider,
        IReadOnlyList<BaseResourceExtractor> resourceExtractors
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController)
    {
        _gameObjectContainer = gameObjectContainer;
        _sceneObjectContainer = sceneObjectContainer;
        _gameController = gameController;
        _unitInfoProvider = unitInfoProvider;
        _interfaceProvider = interfaceProvider;
        _textProvider = textProvider;
        _resourceExtractors = resourceExtractors;
    }

    /// <inheritdoc />
    public override CursorState DefaultCursorState => CursorState.None;

    /// <inheritdoc />
    public void InitializeParameters(LoadingSaveSceneParameters parameters)
    {
        _savePath = parameters.SavePath;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        base.LoadInternal();

        var loadingAnimation = _interfaceProvider.GetAnimation("DLG_WAIT_HOURGLASS");
        var loadingAnimationWidth = loadingAnimation[0].Width;
        var loadingAnimationHeight = loadingAnimation[0].Height;
        _loadingAnimationObject = _gameObjectContainer.AddAnimation(
            loadingAnimation,
            (GameInfo.OriginalWidth - loadingAnimationWidth) / 2,
            (GameInfo.OriginalHeight - loadingAnimationHeight) / 2,
            1);


        _loadingText = _sceneObjectContainer.AddText(
            _textProvider.GetText("X003TA0004"),
            20,
            0,
            450,
            1,
            GameInfo.OriginalWidth,
            isBold: true,
            foregroundColor: GameColors.White);
    }

    /// <inheritdoc />
    protected override void AfterSceneLoadedInternal()
    {
        base.AfterSceneLoadedInternal();

        Task.Run(LoadSave);
    }

    /// <inheritdoc />
    protected override void BeforeSceneUpdate(UpdateSceneData data)
    {
    }

    /// <inheritdoc />
    protected override void AfterSceneUpdate()
    {
    }

    /// <summary>
    /// Загрузить игру из сейва.
    /// </summary>
    private void LoadSave()
    {
        LoadResources();

        var gameContext = _gameController.LoadGame(_savePath);

        // Следующая сцена будет сцена битвы.
        _gameController.ChangeScene<IBattleScene, BattleSceneParameters>(new BattleSceneParameters(
            CreateSquad(gameContext.Players[0], gameContext.Players[0].Squads[0]),
            CreateSquad(gameContext.Players[1], gameContext.Players[1].Squads[0])));
    }

    /// <summary>
    /// Загрузить необходимые ресурсы.
    /// </summary>
    private void LoadResources()
    {
        foreach (var resourceExtractor in _resourceExtractors)
        {
            resourceExtractor.Load();
        }

        _unitInfoProvider.Load();
    }

    /// <summary>
    /// Создать отряд.
    /// </summary>
    private Squad CreateSquad(Player player, PlayerSquad playerSquad)
    {
        var units = playerSquad
            .Units
            .Where(u => !u.IsDead)
            .Select(u => new Unit(u.Id.ToString(), _unitInfoProvider.GetUnitType(u.UnitTypeId), player, u.SquadLinePosition, u.SquadFlankPosition) { Level = u.Level, Experience = u.Experience, HitPoints = u.HitPoints})
            .ToArray();
        return new Squad(units);
    }
}