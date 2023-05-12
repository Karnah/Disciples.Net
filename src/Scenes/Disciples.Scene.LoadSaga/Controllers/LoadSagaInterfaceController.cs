using System.Drawing;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;
using Disciples.Scene.LoadSaga.Models;
using Disciples.Scene.LoadSaga.Providers;

namespace Disciples.Scene.LoadSaga.Controllers;

/// <summary>
/// Контроллер сцены загрузки сейва сценария.
/// </summary>
internal class LoadSagaInterfaceController : BaseSupportLoading
{
    private readonly IGameObjectContainer _gameObjectContainer;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly LoadSagaInterfaceProvider _interfaceProvider;
    private readonly SaveProvider _saveProvider;
    private readonly IGameController _gameController;

    private IReadOnlyList<Save> _saves = null!;
    private List<ITextSceneObject> _saveObjects = null!;
    private int? _selectedSaveIndex;
    private IImageSceneObject? _saveSelection = null;

    private ButtonObject _goBackButton = null!;
    private ButtonObject _selectSaveButton = null!;
    private ButtonObject _saveUpButton = null!;
    private ButtonObject _saveDownButton = null!;

    /// <summary>
    /// Создать объект типа <see cref="LoadSagaInterfaceController" />.
    /// </summary>
    public LoadSagaInterfaceController(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        LoadSagaInterfaceProvider interfaceProvider,
        SaveProvider saveProvider,
        IGameController gameController)
    {
        _gameObjectContainer = gameObjectContainer;
        _sceneObjectContainer = sceneObjectContainer;
        _interfaceProvider = interfaceProvider;
        _saveProvider = saveProvider;
        _gameController = gameController;
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
        _sceneObjectContainer.AddImage(_interfaceProvider.Background, 0, 0, 0);
        _gameObjectContainer.AddAnimation(
            _interfaceProvider.FireflyAnimation,
            74,
            321,
            1);

        _saves = _saveProvider.GetSaves();
        _saveObjects = new List<ITextSceneObject>(_saves.Count);
        if (_saves.Count > 0)
        {
            _selectedSaveIndex = 0;

            var saveIndex = 0;
            foreach (var save in _saves)
            {
                var saveObject = _sceneObjectContainer.AddText(
                    save.Name,
                    12,
                    550,
                    22 + saveIndex * 18,
                    2,
                    300,
                    TextAlignment.Left,
                    false,
                    foregroundColor: GameColors.Black);
                _saveObjects.Add(saveObject);

                saveIndex++;
            }
        }

        _goBackButton = _gameObjectContainer.AddButton(
            _interfaceProvider.GoBackButton,
            () => { },
            382,
            553,
            1,
            KeyboardButton.Escape);
        _selectSaveButton = _gameObjectContainer.AddButton(
            _interfaceProvider.SelectSaveButton,
            () =>
            {
                var save = _saves[_selectedSaveIndex!.Value];
                _gameController.ChangeScene<ILoadingSaveScene, LoadingSaveSceneParameters>(
                    new LoadingSaveSceneParameters(save.Path));
            },
            730,
            553,
            1,
            KeyboardButton.Enter);

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
    /// Обновить выбранный сейв-файл.
    /// </summary>
    private void UpdateSelectedSave()
    {
        if (_selectedSaveIndex == null)
            return;

        if (_saveSelection == null)
        {
            _saveSelection = _sceneObjectContainer.AddColorImage(
                Color.White,
                215,
                18,
                550,
                23 + _selectedSaveIndex.Value * 18,
                1);
        }
        else
        {
            _saveSelection.Y = 23 + _selectedSaveIndex.Value * 18;
        }

        _selectSaveButton.SetActive();

        if (_selectedSaveIndex > 0)
            _saveUpButton.SetActive();
        else
            _saveUpButton.SetDisabled();

        if (_selectedSaveIndex != _saveObjects.Count - 1)
            _saveDownButton.SetActive();
        else
            _saveDownButton.SetDisabled();
    }
}