using System;
using System.Collections.Generic;
using Disciples.Engine.Common.Enums;
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
    AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true);

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
    /// <param name="buttonStates">Изображения кнопки в зависимости от её состояния.</param>
    /// <param name="buttonPressedAction">Действие, которое будет выполняться на кнопке после нажатия.</param>
    /// <param name="x">Положение кнопки, координата X.</param>
    /// <param name="y">Положение кнопки, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
    /// <param name="hotkey">Горячая клавиша для кнопки.</param>
    ButtonObject AddButton(IReadOnlyDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null);

    /// <summary>
    /// Добавить кнопку на сцену.
    /// </summary>
    /// <param name="buttonStates">Изображения кнопки в зависимости от её состояния.</param>
    /// <param name="buttonPressedAction">Действие, которое будет выполняться на кнопке после нажатия.</param>
    /// <param name="x">Положение кнопки, координата X.</param>
    /// <param name="y">Положение кнопки, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
    /// <param name="hotKeys">Горячие клавиши для кнопки.</param>
    ButtonObject AddButton(IReadOnlyDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, IReadOnlyList<KeyboardButton> hotKeys);

    /// <summary>
    /// Добавить кнопку на сцену.
    /// </summary>
    /// <param name="button">Информация о кнопке.</param>
    /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
    ButtonObject AddButton(ButtonSceneElement button, int layer);

    /// <summary>
    /// Добавить кнопку на сцену, которая будет нажата до тех пор, пока на неё не нажмут еще раз.
    /// </summary>
    /// <param name="buttonStates">Изображения кнопки в зависимости от её состояния.</param>
    /// <param name="buttonPressedAction">Действие, которое будет выполняться на кнопке после нажатия.</param>
    /// <param name="x">Положение кнопки, координата X.</param>
    /// <param name="y">Положение кнопки, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
    /// <param name="hotkey">Горячая клавиша для кнопки.</param>
    ToggleButtonObject AddToggleButton(IReadOnlyDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null);

    /// <summary>
    /// Добавить список строк на сцену.
    /// </summary>
    /// <param name="textListBox">Информация о списке строк.</param>
    /// <param name="layer">Слой на котором будет отображаться список.</param>
    TextListBoxObject AddTextListBox(TextListBoxSceneElement textListBox, int layer);

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