using System.Drawing;
using Disciples.Engine.Base;
using Disciples.Engine.Common;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;
using Disciples.Scene.LoadSaga.Constants;
using Disciples.Scene.LoadSaga.GameObjects;
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

    private readonly LoadSagaGameObjectContainer _gameObjectContainer;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly LoadSagaInterfaceProvider _interfaceProvider;
    private readonly SaveProvider _saveProvider;
    private readonly GameController _gameController;
    private readonly ITextProvider _textProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;

    private List<SagaSaveObject> _sagaSaves = null!;
    private int? _selectedSaveIndex;
    private IImageSceneObject? _saveSelection;
    private IReadOnlyList<ImageObject> _saveRaces = null!;
    private TextBlockObject _saveInfo = null!;
    private TextBlockObject _saveDescription = null!;

    private ButtonObject _goBackButton = null!;
    private ButtonObject _continueButton = null!;
    private ButtonObject _saveUpButton = null!;
    private ButtonObject _saveDownButton = null!;

    /// <summary>
    /// Создать объект типа <see cref="LoadSagaInterfaceController" />.
    /// </summary>
    public LoadSagaInterfaceController(
        LoadSagaGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        LoadSagaInterfaceProvider interfaceProvider,
        SaveProvider saveProvider,
        GameController gameController,
        ITextProvider textProvider,
        ISceneInterfaceController sceneInterfaceController)
    {
        _gameObjectContainer = gameObjectContainer;
        _sceneObjectContainer = sceneObjectContainer;
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

    /// <summary>
    /// Обработать нажатие на файл сейва.
    /// </summary>
    public void OnSaveLeftMouseButtonPressed(SagaSaveObject sagaSave)
    {
        _selectedSaveIndex = _sagaSaves.IndexOf(sagaSave);
        UpdateSelectedSave();
    }

    /// <summary>
    /// Обработать двойной клик на файле с сейвом.
    /// </summary>
    public void OnSaveLeftMouseButtonDoubleClicked(SagaSaveObject sagaSave)
    {
        LoadSave(sagaSave);
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var sceneInterface = _interfaceProvider.SceneInterface;
        var gameObjects = _sceneInterfaceController.AddSceneGameObjects(sceneInterface, Layers.SceneLayers);

        var saves = _saveProvider.GetSaves();
        var sagaSaves = new List<SagaSaveObject>(saves.Count);
        for (int saveIndex = 0; saveIndex < saves.Count; saveIndex++)
        {
            var saveObject = _gameObjectContainer.AddSave(
                saves[saveIndex],
                550,
                22 + saveIndex * 18);
            sagaSaves.Add(saveObject);
        }

        _sagaSaves = sagaSaves;

        if (_sagaSaves.Count > 0)
            _selectedSaveIndex = 0;

        _goBackButton = gameObjects.Get<ButtonObject>(LoadSagaElementNames.BACK_BUTTON);
        _goBackButton.ClickedAction = ExecuteBack;
        _goBackButton.SetActive();

        _continueButton = gameObjects.Get<ButtonObject>(LoadSagaElementNames.CONTINUE_BUTTON);
        _continueButton.ClickedAction = ExecuteContinue;

        _saveUpButton = gameObjects.Get<ButtonObject>(LoadSagaElementNames.SAVE_UP_BUTTON);
        _saveUpButton.ClickedAction = ExecuteSaveUp;

        _saveDownButton = gameObjects.Get<ButtonObject>(LoadSagaElementNames.SAVE_DOWN_BUTTON);
        _saveDownButton.ClickedAction = ExecuteSaveDown;

        _saveInfo = gameObjects.Get<TextBlockObject>(LoadSagaElementNames.SAVE_INFO_TEXT_BLOCK);
        _saveDescription = gameObjects.Get<TextBlockObject>(LoadSagaElementNames.SAVE_DESCRIPTION_TEXT_BLOCK);
        _saveRaces = gameObjects.Get<ImageObject>(i => i.Name?.StartsWith(LoadSagaElementNames.RACES_PATTERN_IMAGE) == true);

        UpdateSelectedSave();
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
    /// Выбрать сейв выше.
    /// </summary>
    private void ExecuteSaveUp()
    {
        --_selectedSaveIndex;
        UpdateSelectedSave();
    }

    /// <summary>
    /// Выбрать сейв ниже.
    /// </summary>
    private void ExecuteSaveDown()
    {
        ++_selectedSaveIndex;
        UpdateSelectedSave();
    }

    /// <summary>
    /// Перейти на следующую страницу (загрузить сейв).
    /// </summary>
    private void ExecuteContinue()
    {
        var sagaSave = _sagaSaves[_selectedSaveIndex!.Value];
        LoadSave(sagaSave);
    }

    /// <summary>
    /// Загрузить сейв.
    /// </summary>
    private void LoadSave(SagaSaveObject sagaSave)
    {
        _gameController.ChangeScene<ILoadingSaveScene, LoadingSaveSceneParameters>(
            new LoadingSaveSceneParameters(sagaSave.Save.Path));
    }

    /// <summary>
    /// Обновить выбранный сейв-файл.
    /// </summary>
    private void UpdateSelectedSave()
    {
        if (_selectedSaveIndex == null)
            return;

        UpdateSaveSelection();
        UpdateButtonsState();

        var save = _sagaSaves[_selectedSaveIndex!.Value].Save;
        UpdateSelectedSaveRaces(save);
        UpdateSelectedSaveDescription(save);
    }

    /// <summary>
    /// Обновить выделение сейва.
    /// </summary>
    private void UpdateSaveSelection()
    {
        if (_saveSelection == null)
        {
            _saveSelection = _sceneObjectContainer.AddColorImage(
                Color.White,
                215,
                18,
                550,
                23 + _selectedSaveIndex!.Value * 18,
                1);
        }
        else
        {
            _saveSelection.Y = 23 + _selectedSaveIndex!.Value * 18;
        }
    }

    /// <summary>
    /// Обновить состояние кнопок.
    /// </summary>
    private void UpdateButtonsState()
    {
        _continueButton.SetActive();

        if (_selectedSaveIndex > 0)
            _saveUpButton.SetActive();
        else
            _saveUpButton.SetDisabled();

        if (_selectedSaveIndex < _sagaSaves.Count - 1)
            _saveDownButton.SetActive();
        else
            _saveDownButton.SetDisabled();
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