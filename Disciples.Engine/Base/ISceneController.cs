using System;
using System.Collections.Generic;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Base
{
    /// <summary>
    /// Базовый интерфейс для контроллера сцены.
    /// </summary>
    public interface ISceneController
    {
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
        /// Добавить пустое изображение на сцену.
        /// </summary>
        /// <param name="layer">Слой на котором будет отображаться изображение.</param>
        IImageSceneObject AddImage(int layer);

        /// <summary>
        /// Добавить статичное изображение на сцену.
        /// </summary>
        /// <param name="bitmap">Изображение.</param>
        /// <param name="x">Положение изображения, координата X.</param>
        /// <param name="y">Положение изображения, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться изображение.</param>
        IImageSceneObject AddImage(IBitmap bitmap, double x, double y, int layer);

        /// <summary>
        /// Добавить статичное изображение указанных размеров на сцену.
        /// </summary>
        /// <param name="bitmap">Изображение.</param>
        /// <param name="width">Ширина изображения.</param>
        /// <param name="height">Высота изображения.</param>
        /// <param name="x">Положение изображения, координата X.</param>
        /// <param name="y">Положение изображения, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться изображение.</param>
        IImageSceneObject AddImage(IBitmap bitmap, double width, double height, double x, double y, int layer);

        /// <summary>
        /// Добавить прямоугольник указанного цвета и размеров на сцену.
        /// </summary>
        /// <param name="color">Цвет изображения.</param>
        /// <param name="width">Ширина изображения.</param>
        /// <param name="height">Высота изображения.</param>
        /// <param name="x">Положение изображения, координата X.</param>
        /// <param name="y">Положение изображения, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться изображение.</param>
        IImageSceneObject AddColorImage(GameColor color, double width, double height, double x, double y, int layer);

        /// <summary>
        /// Добавить текст на сцену.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="fontSize">Размер шрифта.</param>
        /// <param name="x">Положение тексте, координата X.</param>
        /// <param name="y">Положение текста, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться текст.</param>
        /// <param name="isBold">Использовать жирный шрифт.</param>
        ITextSceneObject AddText(string text, double fontSize, double x, double y, int layer, bool isBold = false);

        /// <summary>
        /// Добавить текст на сцену.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="fontSize">Размер шрифта.</param>
        /// <param name="x">Положение тексте, координата X.</param>
        /// <param name="y">Положение текста, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться текст.</param>
        /// <param name="width">Ширина текста.</param>
        /// <param name="textAlignment">Выравнивание текста.</param>
        /// <param name="isBold">Использовать жирный шрифт.</param>
        /// <param name="foregroundColor">Цвет текста.</param>
        ITextSceneObject AddText(string text, double fontSize, double x, double y, int layer, double width,
            TextAlignment textAlignment = TextAlignment.Center, bool isBold = false, GameColor? foregroundColor = null);


        /// <summary>
        /// Удалить указанный объект со сцены.
        /// </summary>
        /// <param name="sceneObject">Объект, который необходимо удалить.</param>
        void RemoveSceneObject(ISceneObject? sceneObject);
    }
}