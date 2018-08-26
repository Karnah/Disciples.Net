using System;
using System.Collections.Generic;

using Avalonia.Media.Imaging;
using Engine.Battle.GameObjects;
using Engine.Common.Enums;
using Engine.Common.GameObjects;
using Engine.Common.Models;

namespace Engine.Common.Controllers
{
    /// <summary>
    /// Позволяет добавлять на сцену различные объекты
    /// </summary>
    public interface IVisualSceneController
    {
        /// <summary>
        /// Добавить анимацию на сцену
        /// </summary>
        /// <param name="frames">Кадры анимации</param>
        /// <param name="x">Положение анимации, координата X</param>
        /// <param name="y">Положение анимации, координата Y</param>
        /// <param name="layer">Слой на котором будет отображаться анимация</param>
        /// <param name="repeat"><value>false</value>, если необходимо уничтожить объект после того, как анимация будет завершена</param>
        AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true);

        /// <summary>
        /// Добавить кнопку на сцену
        /// </summary>
        /// <param name="buttonStates">Изображения кнопки в зависимости от её состояния</param>
        /// <param name="buttonPressedAction">Действие, которое будет выполняться на кнопке после нажатия</param>
        /// <param name="x">Положение кнопки, координата X</param>
        /// <param name="y">Положение кнопки, координата Y</param>
        /// <param name="layer">Слой на котором будет отображаться кнопка</param>
        ButtonObject AddButton(IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer);

        /// <summary>
        /// Добавить кнопку на сцену, которая будет нажата до тех пор, пока на неё не нажмут еще раз
        /// </summary>
        /// <param name="buttonStates">Изображения кнопки в зависимости от её состояния</param>
        /// <param name="buttonPressedAction">Действие, которое будет выполняться на кнопке после нажатия</param>
        /// <param name="x">Положение кнопки, координата X</param>
        /// <param name="y">Положение кнопки, координата Y</param>
        /// <param name="layer">Слой на котором будет отображаться кнопка</param>
        ToggleButtonObject AddToggleButton(IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer);

        /// <summary>
        /// Добавить статичное изображение на сцену
        /// </summary>
        /// <param name="bitmap">Изображение</param>
        /// <param name="x">Положение изображения, координата X</param>
        /// <param name="y">Положение изображения, координата Y</param>
        /// <param name="layer">Слой на котором будет отображаться изображение</param>
        VisualObject AddVisual(Bitmap bitmap, int x, int y, int layer);

        /// <summary>
        /// Добавить статичное изображение указанных размеров на сцену
        /// </summary>
        /// <param name="bitmap">Изображение</param>
        /// <param name="width">Ширина изображения</param>
        /// <param name="height">Высота изображения</param>
        /// <param name="x">Положение изображения, координата X</param>
        /// <param name="y">Положение изображения, координата Y</param>
        /// <param name="layer">Слой на котором будет отображаться изображение</param>
        VisualObject AddVisual(Bitmap bitmap, int width, int height, int x, int y, int layer);


        /// <summary>
        /// Добавить юнита на сцену битвы
        /// </summary>
        /// <param name="unit">Юнит</param>
        /// <param name="isAttacker">Является ли юнит атакующим</param>
        BattleUnit AddBattleUnit(Unit unit, bool isAttacker);
    }
}
