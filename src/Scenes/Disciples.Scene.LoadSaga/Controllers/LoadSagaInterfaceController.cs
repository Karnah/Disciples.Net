using Disciples.Engine.Common;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;
using Disciples.Scene.LoadSaga.Constants;
using Disciples.Scene.LoadSaga.Models;
using Disciples.Scene.LoadSaga.Providers;

namespace Disciples.Scene.LoadSaga.Controllers;

/// <summary>
/// Контроллер сцены загрузки сейва сценария.
/// </summary>
internal class LoadSagaInterfaceController : BaseSupportLoading
{
    /// <summary>
    /// Информация о сейве.
    /// </summary>
    private const string SAVE_INFO = "X005TA0665";
    /// <summary>
    /// Имя и описание сейва.
    /// </summary>
    private const string SAVE_DESCRIPTION = "X005TA0664";

    private readonly LoadSagaInterfaceProvider _interfaceProvider;
    private readonly SaveProvider _saveProvider;
    private readonly GameController _gameController;
    private readonly ITextProvider _textProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;

    private TextListBoxObject _saveListBox = null!;
    private IReadOnlyList<ImageObject> _saveRaces = null!;
    private TextBlockObject _saveInfo = null!;
    private TextBlockObject _saveDescription = null!;

    private ButtonObject _goBackButton = null!;
    private ButtonObject _continueButton = null!;

    /// <summary>
    /// Создать объект типа <see cref="LoadSagaInterfaceController" />.
    /// </summary>
    public LoadSagaInterfaceController(
        LoadSagaInterfaceProvider interfaceProvider,
        SaveProvider saveProvider,
        GameController gameController,
        ITextProvider textProvider,
        ISceneInterfaceController sceneInterfaceController)
    {
        _interfaceProvider = interfaceProvider;
        _saveProvider = saveProvider;
        _gameController = gameController;
        _textProvider = textProvider;
        _sceneInterfaceController = sceneInterfaceController;
    }

    /// <summary>
    /// Обработать события перед обновлением сцены.
    /// </summary>
    public void BeforeSceneUpdate()
    {
    }

    /// <summary>
    /// Обработать завершение обновлении сцены.
    /// </summary>
    public void AfterSceneUpdate()
    {
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var sceneInterface = _interfaceProvider.SceneInterface;
        var gameObjects = _sceneInterfaceController.AddSceneGameObjects(sceneInterface, Layers.SceneLayers);

        _goBackButton = gameObjects.Get<ButtonObject>(LoadSagaElementNames.BACK_BUTTON);
        _goBackButton.ClickedAction = ExecuteBack;
        _goBackButton.SetActive();

        _continueButton = gameObjects.Get<ButtonObject>(LoadSagaElementNames.CONTINUE_BUTTON);
        _continueButton.ClickedAction = ExecuteContinue;

        _saveInfo = gameObjects.Get<TextBlockObject>(LoadSagaElementNames.SAVE_INFO_TEXT_BLOCK);
        _saveDescription = gameObjects.Get<TextBlockObject>(LoadSagaElementNames.SAVE_DESCRIPTION_TEXT_BLOCK);
        _saveRaces = gameObjects.Get<ImageObject>(i => i.Name?.StartsWith(LoadSagaElementNames.RACES_PATTERN_IMAGE) == true);

        _saveListBox = gameObjects.Get<TextListBoxObject>(LoadSagaElementNames.SAVES_TEXT_LIST_BOX);
        _saveListBox.ItemSelected = ExecuteSaveSelected;
        _saveListBox.SetItems(_saveProvider.GetSaves());
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Вернуться на предыдущую страницу (закрыть приложение).
    /// </summary>
    private void ExecuteBack()
    {
        _gameController.Exit();
    }

    /// <summary>
    /// Перейти на следующую страницу (загрузить сейв).
    /// </summary>
    private void ExecuteContinue()
    {
        LoadSave((Save)_saveListBox.SelectedItem!);
    }

    /// <summary>
    /// Обработать выбор сейва.
    /// </summary>
    private void ExecuteSaveSelected(TextListBoxItem? item)
    {
        var save = (Save?)item;
        if (save == null)
            return;

        UpdateSelectedSaveRaces(save);
        UpdateSelectedSaveDescription(save);
    }

    /// <summary>
    /// Загрузить сейв.
    /// </summary>
    private void LoadSave(Save save)
    {
        _gameController.ChangeScene<ILoadingSaveScene, LoadingSaveSceneParameters>(
            new LoadingSaveSceneParameters(save.Path));
    }

    /// <summary>
    /// Обновить список рас выбранного сейва.
    /// </summary>
    private void UpdateSelectedSaveRaces(Save save)
    {
        var races = save
            .GameContext
            .Players
            .Select(p => p.Race)
            .Where(r => r != RaceType.Neutral)
            .OrderBy(r => r)
            .ToArray();
        for (int raceIndex = 0; raceIndex < Math.Min(races.Length, _saveRaces.Count); raceIndex++)
        {
            _saveRaces[raceIndex].Bitmap = _interfaceProvider.Races[races[raceIndex]];
        }

        for (int raceIndex = races.Length; raceIndex < _saveRaces.Count; raceIndex++)
        {
            _saveRaces[raceIndex].Bitmap = null;
        }
    }

    /// <summary>
    /// Обновить описание выбранного сейва.
    /// </summary>
    private void UpdateSelectedSaveDescription(Save save)
    {
        var gameContext = save.GameContext;

        _saveInfo.Text = _textProvider
            .GetText(SAVE_INFO)
            .ReplacePlaceholders(new []
            {
                ("%TYPE%", GetGameTypeText(gameContext.GameType)),
                ("%TYPEDESC%", GetTypeDescription(gameContext)),
                ("%TURN%", new TextContainer(gameContext.TurnNumber.ToString())),
                ("%DIFF%", GetDifficultyLevelTitle(gameContext.DifficultyLevel)),
            });

        _saveDescription.Text = _textProvider
            .GetText(SAVE_DESCRIPTION)
            .ReplacePlaceholders(new []
            {
                ("%NAME%", new TextContainer(gameContext.SagaName)),
                ("%TYPE%", GetMissionTypeText(gameContext.MissionType)),
                ("%DESC%", new TextContainer(gameContext.SagaDescription ?? string.Empty)),
            });
    }

    /// <summary>
    /// Получить текст для типа игры.
    /// </summary>
    private TextContainer GetGameTypeText(GameType type)
    {
        var textId = type switch
        {
            GameType.SinglePlayer => "X005TA0721",
            GameType.Hotseat => "X005TA0722",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return _textProvider.GetText(textId);
    }

    /// <summary>
    /// Получить текст для описания типа саги.
    /// </summary>
    private TextContainer GetTypeDescription(GameContext gameContext)
    {
        var race = gameContext.Players.FirstOrDefault(p => !p.IsComputer)?.Race;
        if (race == null)
            return new TextContainer(string.Empty);

        var description = _textProvider.GetText("X005TA0719");
        return description
            .ReplacePlaceholders(new []{ ("%RACE%", GetRaceTitle(race.Value)) });
    }

    /// <summary>
    /// Получить название расы.
    /// </summary>
    private TextContainer GetRaceTitle(RaceType race)
    {
        var textId = race switch
        {
            RaceType.Human => "X000TG0000",
            RaceType.Undead => "X000TG0003",
            RaceType.Heretic => "X000TG0002",
            RaceType.Dwarf => "X000TG0001",
            RaceType.Elf => "X000TG0013",
            _ => throw new ArgumentOutOfRangeException(nameof(race), race, null)
        };

        return _textProvider.GetText(textId);
    }

    /// <summary>
    /// Получить наименование уровня сложности.
    /// </summary>
    private TextContainer GetDifficultyLevelTitle(DifficultyLevel difficultyLevel)
    {
        var textId = difficultyLevel switch
        {
            DifficultyLevel.Easy => "X010TA0002",
            DifficultyLevel.Average => "X010TA0003",
            DifficultyLevel.Hard => "X010TA0004",
            DifficultyLevel.VeryHard => "X010TA0005",
            _ => throw new ArgumentOutOfRangeException(nameof(difficultyLevel), difficultyLevel, null)
        };

        return _textProvider.GetText(textId);
    }

    /// <summary>
    /// Получить текст для типа миссии.
    /// </summary>
    private TextContainer GetMissionTypeText(MissionType type)
    {
        var textId = type switch
        {
            MissionType.Saga => "X005TA0633",
            MissionType.Quest => "X005TA0632",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return _textProvider.GetText(textId);
    }
}