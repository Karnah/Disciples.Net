using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;

namespace Disciples.Scene.LoadSaga.Providers;

/// <summary>
/// Провайдер для интерфейса сцены.
/// </summary>
internal class LoadSagaInterfaceProvider : BaseSupportLoading
{
    private readonly IInterfaceProvider _interfaceProvider;

    /// <summary>
    /// Создать объект типа <see cref="LoadSagaInterfaceProvider" />.
    /// </summary>
    public LoadSagaInterfaceProvider(IInterfaceProvider interfaceProvider)
    {
        _interfaceProvider = interfaceProvider;
    }

    /// <summary>
    /// Фон сцены.
    /// </summary>
    public IBitmap Background { get; private set; } = null!;

    /// <summary>
    /// Анимация светлячков на фоне сцены.
    /// </summary>
    public IReadOnlyList<Frame> FireflyAnimation { get; private set; } = null!;

    /// <summary>
    /// Кнопка для возврата назад.
    /// </summary>
    public IDictionary<SceneButtonState, IBitmap> GoBackButton { get; private set; } = null!;

    /// <summary>
    /// Кнопка для выбора файла сохранения.
    /// </summary>
    public IDictionary<SceneButtonState, IBitmap> SelectSaveButton { get; private set; } = null!;

    /// <summary>
    /// Кнопка для перемещения наверх.
    /// </summary>
    public IDictionary<SceneButtonState, IBitmap> SaveUpButton { get; private set; } = null!;

    /// <summary>
    /// Кнопка для перемещения вниз.
    /// </summary>
    public IDictionary<SceneButtonState, IBitmap> SaveDownButton { get; private set; } = null!;

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var images = _interfaceProvider.GetImageParts("DLG_LOAD.PNG");
        Background = images["DLG_LOAD_BG1TRANS"];
        GoBackButton = GetButtonBitmaps(images, "GCAN01B");
        SelectSaveButton = GetButtonBitmaps(images, "GOK01B");

        // В файле DLG_LOAD эти кнопки заданые некорректно. Поэтому вытаскиваем их отдельно.
        SaveUpButton = GetButtonBitmaps("_MENU_ARROW_UP");
        SaveDownButton = GetButtonBitmaps("_MENU_ARROW_DOWN");

        FireflyAnimation = _interfaceProvider.GetAnimation("_FIREFLY");
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Получить словарь с изображениями кнопки для каждого её состояния.
    /// </summary>
    /// <param name="images">Изображения.</param>
    /// <param name="buttonName">Имя кнопки.</param>
    private static IDictionary<SceneButtonState, IBitmap> GetButtonBitmaps(IReadOnlyDictionary<string, IBitmap> images, string buttonName)
    {
        return new Dictionary<SceneButtonState, IBitmap>
        {
            { SceneButtonState.Disabled, images[$"DLG_LOAD_{buttonName}D"] },
            { SceneButtonState.Active, images[$"DLG_LOAD_{buttonName}N"] },
            { SceneButtonState.Selected, images[$"DLG_LOAD_{buttonName}H"] },
            { SceneButtonState.Pressed, images[$"DLG_LOAD_{buttonName}C"] }
        };
    }

    /// <summary>
    /// Получить словарь с изображениями кнопки для каждого её состояния.
    /// </summary>
    /// <param name="buttonName">Имя кнопки.</param>
    private IDictionary<SceneButtonState, IBitmap> GetButtonBitmaps(string buttonName)
    {
        return new Dictionary<SceneButtonState, IBitmap>
        {
            { SceneButtonState.Disabled, _interfaceProvider.GetImage($"{buttonName}_D") },
            { SceneButtonState.Active, _interfaceProvider.GetImage($"{buttonName}_N") },
            { SceneButtonState.Selected, _interfaceProvider.GetImage($"{buttonName}_H") },
            { SceneButtonState.Pressed, _interfaceProvider.GetImage($"{buttonName}_C") }
        };
    }
}