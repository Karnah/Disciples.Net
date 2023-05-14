using System.Drawing;
using Disciples.Engine.Base;
using Disciples.Engine.Common;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Implementation;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;
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
    /// Описание сейва.
    /// </summary>
    private const string SAVE_DESCRIPTION = "X005TA0665";

    private readonly LoadSagaGameObjectContainer _gameObjectContainer;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly LoadSagaInterfaceProvider _interfaceProvider;
    private readonly SaveProvider _saveProvider;
    private readonly GameController _gameController;
    private readonly ITextProvider _textProvider;

    private List<SagaSaveObject> _sagaSaves = null!;
    private int? _selectedSaveIndex;
    private IImageSceneObject? _saveSelection = null;
    private IReadOnlyList<IImageSceneObject> _saveRaces = Array.Empty<IImageSceneObject>();
    private ITextSceneObject? _saveHeaderDescription;
    private ITextSceneObject? _saveSagaName;
    private ITextSceneObject? _saveSagaDescription;

    private ButtonObject _goBackButton = null!;
    private ButtonObject _selectSaveButton = null!;
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
        ITextProvider textProvider)
    {
        _gameObjectContainer = gameObjectContainer;
        _sceneObjectContainer = sceneObjectContainer;
        _interfaceProvider = interfaceProvider;
        _saveProvider = saveProvider;
        _gameController = gameController;
        _textProvider = textProvider;
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
        _sceneObjectContainer.AddImage(_interfaceProvider.Background, 0, 0, 0);
        _gameObjectContainer.AddAnimation(
            _interfaceProvider.FireflyAnimation,
            74,
            321,
            1);

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


        _goBackButton = _gameObjectContainer.AddButton(
            _interfaceProvider.GoBackButton,
            () => { _gameController.Exit(); },
            382,
            553,
            1,
            KeyboardButton.Escape);
        _goBackButton.SetActive();
        _selectSaveButton = _gameObjectContainer.AddButton(
            _interfaceProvider.SelectSaveButton,
            () =>
            {
                var sagaSave = _sagaSaves[_selectedSaveIndex!.Value];
                LoadSave(sagaSave);
            },
            730,
            553,
            1,
            KeyboardButton.Enter);

        // TODO Эти кнопки должны быть типа RepeatButton.
        _saveUpButton = _gameObjectContainer.AddButton(
            _interfaceProvider.SaveUpButton,
            () =>
            {
                --_selectedSaveIndex;
                UpdateSelectedSave();
            },
            500,
            85,
            1,
            KeyboardButton.Up);
        _saveDownButton = _gameObjectContainer.AddButton(
            _interfaceProvider.SaveDownButton,
            () =>
            {
                ++_selectedSaveIndex;
                UpdateSelectedSave();
            },
            500,
            245,
            1,
            KeyboardButton.Down);


        UpdateSelectedSave();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
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
        _selectSaveButton.SetActive();

        if (_selectedSaveIndex > 0)
            _saveUpButton.SetActive();
        else
            _saveUpButton.SetDisabled();

        if (_selectedSaveIndex != _sagaSaves.Count - 1)
            _saveDownButton.SetActive();
        else
            _saveDownButton.SetDisabled();
    }

    /// <summary>
    /// Обновить список рас выбранного сейва.
    /// </summary>
    private void UpdateSelectedSaveRaces(Save save)
    {
        foreach (var saveRace in _saveRaces)
        {
            _sceneObjectContainer.RemoveSceneObject(saveRace);
        }

        var races = save
            .GameContext
            .Players
            .Select(p => p.Race)
            .Where(r => r != RaceType.Neutral)
            .OrderBy(r => r)
            .ToArray();
        var racesImages = new List<IImageSceneObject>(races.Length);
        for (int raceIndex = 0; raceIndex < races.Length; raceIndex++)
        {
            var raceImage = _sceneObjectContainer.AddImage(
                _interfaceProvider.Races[races[raceIndex]], 548 + 58 * raceIndex, 388, 2);
            racesImages.Add(raceImage);
        }

        _saveRaces = racesImages;
    }

    /// <summary>
    /// Обновить описание выбранного сейва.
    /// </summary>
    private void UpdateSelectedSaveDescription(Save save)
    {
        _sceneObjectContainer.RemoveSceneObject(_saveHeaderDescription);
        _sceneObjectContainer.RemoveSceneObject(_saveSagaName);
        _sceneObjectContainer.RemoveSceneObject(_saveSagaDescription);

        // TODO Нужно выравнивание по высоте.
        _saveHeaderDescription = _sceneObjectContainer.AddText(
            ReplacePlaceholders(_textProvider.GetText(SAVE_DESCRIPTION), save),
            12,
            425,
            350,
            2,
            107);
        _saveSagaName = _sceneObjectContainer.AddText(
            save.GameContext.SagaName,
            12,
            420,
            475,
            2,
            345,
            isBold: true);
        if (!string.IsNullOrEmpty(save.GameContext.SagaDescription))
        {
            _saveSagaDescription = _sceneObjectContainer.AddText(
                save.GameContext.SagaDescription,
                12,
                420,
                495,
                2,
                345);
        }
    }

    /// <summary>
    /// Заменить плейсхолдеры значениями из сейва.
    /// </summary>
    private string ReplacePlaceholders(string value, Save save)
    {
        // todo Модификаторы добавляются обычным цветом, а не зелёным/красном.
        // Пробовал сделать контрол, который будет принимать текст, содержащий теги разметки/размеры шрифтов/цвет шрифта,
        // Но он слишком медленно работал (StackPanel). Возможно, стоит дождаться TextBlock с поддержкой InlineUIContainer
        // https://github.com/AvaloniaUI/Avalonia/pull/1689.

        var gameContext = save.GameContext;
        return value
            .Replace("\\n", Environment.NewLine)
            .Replace("%TYPE%", GetSagaTypeText(gameContext.SagaType))
            .Replace("%TYPEDESC%", GetTypeDescription(gameContext))
            .Replace("%TURN%", gameContext.TurnNumber.ToString())
            .Replace("%DIFF%", GetDifficultyLevelTitle(gameContext.DifficultyLevel))
            ;
    }

    /// <summary>
    /// Получить текст для типа саги.
    /// </summary>
    private string GetSagaTypeText(SagaType type)
    {
        var textId = type switch
        {
            SagaType.Normal => "X005TA0721",
            SagaType.Hotseat => "X005TA0722",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return _textProvider.GetText(textId);
    }

    /// <summary>
    /// Получить текст для описания типа саги.
    /// </summary>
    private string GetTypeDescription(GameContext gameContext)
    {
        var race = gameContext.Players.FirstOrDefault(p => !p.IsComputer)?.Race;
        if (race == null)
            return string.Empty;

        var description = _textProvider.GetText("X005TA0719");
        return description
            .Replace("%RACE%", GetRaceTitle(race.Value));
    }

    /// <summary>
    /// Получить название расы.
    /// </summary>
    private string GetRaceTitle(RaceType race)
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
    private string GetDifficultyLevelTitle(DifficultyLevel difficultyLevel)
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
}