namespace Disciples.Engine.Common.Constants;

/// <summary>
/// Стили.
/// </summary>
public class Layers
{
    /// <summary>
    /// Слои для основного интерфейса сцены.
    /// </summary>
    public static Layers SceneLayers { get; } = new()
    {
        BackgroundLayer = 0,
        InterfacePanelLayer = 1000,
        InterfaceLayer = 1010,
        AboveAllLayer = 1999
    };

    /// <summary>
    /// Слои для диалогов.
    /// </summary>
    public static Layers DialogLayers { get; } = new()
    {
        BackgroundLayer = 2000,
        InterfacePanelLayer = 2010,
        InterfaceLayer = 2020,
        AboveAllLayer = 2999
    };

    /// <summary>
    /// Слой для расположения фона.
    /// </summary>
    public int BackgroundLayer { get; init; }

    /// <summary>
    /// Слой для расположения панелей интерфейса.
    /// </summary>
    public int InterfacePanelLayer { get; init; }

    /// <summary>
    /// Слой для расположения интерфейса (кнопки, текст, картинки и так далее).
    /// </summary>
    public int InterfaceLayer { get; init; }

    /// <summary>
    /// Слой, который перекрывает все элементы.
    /// </summary>
    public int AboveAllLayer { get; init; }
}