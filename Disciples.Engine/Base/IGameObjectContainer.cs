using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using System.Collections.Generic;
using System;

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
    /// Добавить анимацию на сцену.
    /// </summary>
    /// <param name="frames">Кадры анимации.</param>
    /// <param name="x">Положение анимации, координата X.</param>
    /// <param name="y">Положение анимации, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться анимация.</param>
    /// <param name="repeat"><value>false</value>, если необходимо уничтожить объект после того, как анимация будет завершена.</param>
    AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true);

    /// <summary>
    /// Добавить кнопку на сцену.
    /// </summary>
    /// <param name="buttonStates">Изображения кнопки в зависимости от её состояния.</param>
    /// <param name="buttonPressedAction">Действие, которое будет выполняться на кнопке после нажатия.</param>
    /// <param name="x">Положение кнопки, координата X.</param>
    /// <param name="y">Положение кнопки, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
    /// <param name="hotkey">Горячая клавиша для кнопки.</param>
    ButtonObject AddButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null);

    /// <summary>
    /// Добавить кнопку на сцену, которая будет нажата до тех пор, пока на неё не нажмут еще раз.
    /// </summary>
    /// <param name="buttonStates">Изображения кнопки в зависимости от её состояния.</param>
    /// <param name="buttonPressedAction">Действие, которое будет выполняться на кнопке после нажатия.</param>
    /// <param name="x">Положение кнопки, координата X.</param>
    /// <param name="y">Положение кнопки, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
    /// <param name="hotkey">Горячая клавиша для кнопки.</param>
    ToggleButtonObject AddToggleButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null);

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