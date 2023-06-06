using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;
using Disciples.Resources.Common;

namespace Disciples.Scene.LoadingSave;

/// <inheritdoc cref="ILoadingSaveScene" />
internal class LoadingSaveScene : BaseScene, ILoadingSaveScene
{
    private readonly IGameController _gameController;
    private readonly IUnitInfoProvider _unitInfoProvider;
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly ITextProvider _textProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;
    private readonly MenuSoundController _menuSoundController;
    private readonly IReadOnlyList<BaseMqdbResourceExtractor> _resourceExtractors;

    private string _savePath = null!;

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
        ISceneInterfaceController sceneInterfaceController,
        MenuSoundController menuSoundController,
        IReadOnlyList<BaseMqdbResourceExtractor> resourceExtractors
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController)
    {
        _gameController = gameController;
        _unitInfoProvider = unitInfoProvider;
        _interfaceProvider = interfaceProvider;
        _textProvider = textProvider;
        _sceneInterfaceController = sceneInterfaceController;
        _menuSoundController = menuSoundController;
        _resourceExtractors = resourceExtractors;
    }

    /// <inheritdoc />
    public override CursorType DefaultCursorType => CursorType.None;

    /// <inheritdoc />
    public void InitializeParameters(LoadingSaveSceneParameters parameters)
    {
        _savePath = parameters.SavePath;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        base.LoadInternal();

        // Музыка из меню больше не нужна, останавливаем её.
        _menuSoundController.Stop();

        var sceneInterface = _interfaceProvider.GetSceneInterface("DLG_WAIT");
        var gameObjects = _sceneInterfaceController.AddSceneGameObjects(sceneInterface, Layers.SceneLayers);
        var waitTextBlock = gameObjects.OfType<TextBlockObject>().First();
        waitTextBlock.Text = _textProvider.GetText("X003TA0004");
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
        return new Squad(player, units);
    }
}