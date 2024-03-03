using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Base;
using Disciples.Engine.Common;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <summary>
/// Базовый контроллер для сцен загрузки сейва.
/// </summary>
public abstract class BaseLoadSaveSceneInterfaceController : BaseSupportLoading
{
    /// <summary>
    /// Информация о сейве.
    /// </summary>
    private const string SAVE_INFO = "X005TA0665";
    /// <summary>
    /// Имя и описание сейва.
    /// </summary>
    private const string SAVE_DESCRIPTION = "X005TA0664";

    private readonly ISaveProvider _saveProvider;
    private readonly IGameController _gameController;
    private readonly ITextProvider _textProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;
    private readonly IRaceProvider _raceProvider;
    private readonly IInterfaceProvider _interfaceProvider;

    private TextListBoxObject _saveListBox = null!;
    private IReadOnlyList<ImageObject> _saveRaces = null!;
    private TextBlockObject _saveInfo = null!;
    private TextBlockObject _saveDescription = null!;

    private ButtonObject _goBackButton = null!;
    private ButtonObject _continueButton = null!;

    /// <summary>
    /// Создать объект типа <see cref="BaseLoadSaveSceneInterfaceController" />.
    /// </summary>
    protected BaseLoadSaveSceneInterfaceController(
        ISaveProvider saveProvider,
        IGameController gameController,
        ITextProvider textProvider,
        ISceneInterfaceController sceneInterfaceController,
        IRaceProvider raceProvider,
        IInterfaceProvider interfaceProvider)
    {
        _saveProvider = saveProvider;
        _gameController = gameController;
        _textProvider = textProvider;
        _sceneInterfaceController = sceneInterfaceController;
        _raceProvider = raceProvider;
        _interfaceProvider = interfaceProvider;
    }

    /// <summary>
    /// Имя для интерфейса сцены.
    /// </summary>
    protected abstract string SceneInterfaceName { get; }

    /// <summary>
    /// Сейвы какого типа миссий загружать.
    /// </summary>
    protected abstract MissionType MissionType { get; }

    /// <summary>
    /// Список сейвов.
    /// </summary>
    protected virtual string SavesTextListBoxName => "TLBOX_GAME_SLOT";

    /// <summary>
    /// Кнопка "Назад".
    /// </summary>
    protected virtual string BackButtonName => "BTN_BACK";

    /// <summary>
    /// Кнопка для загрузки выбранного сейва.
    /// </summary>
    protected virtual string ContinueButtonName => "BTN_LOAD";

    /// <summary>
    /// Текст с информацией о сейве.
    /// </summary>
    protected virtual string SaveInfoTextBlockName => "TXT_INFO";

    /// <summary>
    /// Текст с описанием сейва.
    /// </summary>
    protected virtual string SaveDescriptionTextBlockName => "TXT_DESC";

    /// <summary>
    /// Шаблон для изображений рас.
    /// </summary>
    protected virtual string RacesPatternImageName => "IMG_RACE_";

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
        var sceneInterface = _interfaceProvider.GetSceneInterface(SceneInterfaceName);
        var gameObjects = _sceneInterfaceController.AddSceneGameObjects(sceneInterface, Layers.SceneLayers);

        _goBackButton = gameObjects.GetButton(BackButtonName, ExecuteBack);
        _goBackButton.SetActive();

        _continueButton = gameObjects.GetButton(ContinueButtonName, ExecuteContinue);

        _saveInfo = gameObjects.Get<TextBlockObject>(SaveInfoTextBlockName);
        _saveDescription = gameObjects.Get<TextBlockObject>(SaveDescriptionTextBlockName);
        _saveRaces = gameObjects.Get<ImageObject>(i => i.Name?.StartsWith(RacesPatternImageName) == true);

        _saveListBox = gameObjects.Get<TextListBoxObject>(SavesTextListBoxName);
        _saveListBox.ItemSelected = ExecuteSaveSelected;
        _saveListBox.SetItems(_saveProvider.GetSaves(MissionType));
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Вернуться на страницу главного меню.
    /// </summary>
    private void ExecuteBack()
    {
        _gameController.ChangeScene<IMainMenuScene, SceneParameters>(SceneParameters.Empty);
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
            new LoadingSaveSceneParameters(save.GameContext));
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
            _saveRaces[raceIndex].Bitmap = _raceProvider.GetRaceImage(races[raceIndex]);
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
        var raceType = gameContext.Players.FirstOrDefault(p => !p.IsComputer)?.Race;
        if (raceType == null)
            return new TextContainer(string.Empty);

        var race = _raceProvider.GetRace(raceType.Value);

        var description = _textProvider.GetText("X005TA0719");
        return description
            .ReplacePlaceholders(new []{ ("%RACE%", new TextContainer(race.Name)) });
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