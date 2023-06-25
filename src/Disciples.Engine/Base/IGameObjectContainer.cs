using System.Collections.Generic;
using System.IO;
using Disciples.Common.Models;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Base;

/// <summary>
/// Контейнер для игровых объектов.
/// </summary>
public interface IGameObjectContainer
{
    /// <summary>
    /// Список игровых объектов.
    /// </summary>
    IReadOnlyCollection<GameObject> GameObjects { get; }

    /// <summary>
    /// Добавить текстовый блок.
    /// </summary>
    TextBlockObject AddTextBlock(TextBlockSceneElement textBlock, int layer);

    /// <summary>
    /// Добавить изображение на сцену.
    /// </summary>
    ImageObject AddImage(ImageSceneElement image, int layer);

    /// <summary>
    /// Добавить анимацию на сцену.
    /// </summary>
    /// <param name="frames">Кадры анимации.</param>
    /// <param name="x">Положение анимации, координата X.</param>
    /// <param name="y">Положение анимации, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться анимация.</param>
    /// <param name="repeat"><value>false</value>, если необходимо уничтожить объект после того, как анимация будет завершена.</param>
    AnimationObject AddAnimation(AnimationFrames frames, double x, double y, int layer, bool repeat = true);

    /// <summary>
    /// Добавить анимацию на сцену.
    /// </summary>
    /// <param name="animation">Данные анимации.</param>
    /// <param name="layer">Слой на котором будет отображаться анимация.</param>
    /// <param name="repeat"><value>false</value>, если необходимо уничтожить объект после того, как анимация будет завершена.</param>
    AnimationObject AddAnimation(AnimationSceneElement animation, int layer, bool repeat = true);

    /// <summary>
    /// Добавить кнопку на сцену.
    /// </summary>
    /// <param name="button">Информация о кнопке.</param>
    /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
    ButtonObject AddButton(ButtonSceneElement button, int layer);

    /// <summary>
    /// Добавить кнопку-переключатель из двух состояний на сцену.
    /// </summary>
    /// <param name="toggleButton">Информация о кнопке.</param>
    /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
    ToggleButtonObject AddToggleButton(ToggleButtonSceneElement toggleButton, int layer);

    /// <summary>
    /// Добавить список строк на сцену.
    /// </summary>
    /// <param name="textListBox">Информация о списке строк.</param>
    /// <param name="layer">Слой на котором будет отображаться список.</param>
    TextListBoxObject AddTextListBox(TextListBoxSceneElement textListBox, int layer);

    /// <summary>
    /// Добавить видео на сцену.
    /// </summary>
    /// <param name="videoStream">Видеопоток.</param>
    /// <param name="bounds">Границы.</param>
    /// <param name="layer">Слой.</param>
    /// <param name="canSkip">Можно ли пропустить видеоролик.</param>
    VideoGameObject AddVideo(Stream videoStream, RectangleD bounds, int layer, bool canSkip = true);

    /// <summary>
    /// Добавить игровой объект.
    /// </summary>
    TGameObject AddObject<TGameObject>(TGameObject gameObject)
        where TGameObject : GameObject;

    /// <summary>
    /// Обновить состояние игровых объектов.
    /// </summary>
    void UpdateGameObjects(long ticksCount);
}