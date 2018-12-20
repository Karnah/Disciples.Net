using System;
using System.Collections.Generic;
using JetBrains.Annotations;

using Avalonia.Media;
using Avalonia.Media.Imaging;

using Engine.Battle.GameObjects;
using Engine.Common.Enums;
using Engine.Common.GameObjects;
using Engine.Common.Models;
using Engine.Common.VisualObjects;

namespace Engine.Common.Controllers
{
    /// <summary>
    /// Позволяет добавлять на сцену различные объекты.
    /// todo Разделить на VisualObject и GameObject?
    /// </summary>
    public interface IVisualSceneController
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
        /// Добавить анимацию на сцену.
        /// </summary>
        /// <param name="frames">Кадры анимации.</param>
        /// <param name="gameObject"></param>
        /// <param name="layerOffset"></param>
        /// <param name="repeat"><value>false</value>, если необходимо уничтожить объект после того, как анимация будет завершена.</param>
        AnimationObject AttachAnimation(IReadOnlyList<Frame> frames, GameObject gameObject, int layerOffset = 1, bool repeat = true);

        /// <summary>
        /// Добавить кнопку на сцену.
        /// </summary>
        /// <param name="buttonStates">Изображения кнопки в зависимости от её состояния.</param>
        /// <param name="buttonPressedAction">Действие, которое будет выполняться на кнопке после нажатия.</param>
        /// <param name="x">Положение кнопки, координата X.</param>
        /// <param name="y">Положение кнопки, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
        ButtonObject AddButton(IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer);

        /// <summary>
        /// Добавить кнопку на сцену, которая будет нажата до тех пор, пока на неё не нажмут еще раз.
        /// </summary>
        /// <param name="buttonStates">Изображения кнопки в зависимости от её состояния.</param>
        /// <param name="buttonPressedAction">Действие, которое будет выполняться на кнопке после нажатия.</param>
        /// <param name="x">Положение кнопки, координата X.</param>
        /// <param name="y">Положение кнопки, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться кнопка.</param>
        ToggleButtonObject AddToggleButton(IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer);

        /// <summary>
        /// Добавить статичное изображение на сцену.
        /// </summary>
        /// <param name="bitmap">Изображение.</param>
        /// <param name="x">Положение изображения, координата X.</param>
        /// <param name="y">Положение изображения, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться изображение.</param>
        ImageVisualObject AddImageVisual(Bitmap bitmap, double x, double y, int layer);

        /// <summary>
        /// Добавить статичное изображение указанных размеров на сцену.
        /// </summary>
        /// <param name="bitmap">Изображение.</param>
        /// <param name="width">Ширина изображения.</param>
        /// <param name="height">Высота изображения.</param>
        /// <param name="x">Положение изображения, координата X.</param>
        /// <param name="y">Положение изображения, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться изображение.</param>
        ImageVisualObject AddImageVisual(Bitmap bitmap, double width, double height, double x, double y, int layer);

        /// <summary>
        /// Добавить прямоугольник указанного цвета и размеров на сцену.
        /// </summary>
        /// <param name="color">Цвет изображения.</param>
        /// <param name="width">Ширина изображения.</param>
        /// <param name="height">Высота изображения.</param>
        /// <param name="x">Положение изображения, координата X.</param>
        /// <param name="y">Положение изображения, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться изображение.</param>
        ImageVisualObject AddColorImageVisual(GameColor color, double width, double height, double x, double y, int layer);

        /// <summary>
        /// Добавить текст на сцену.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="fontSize">Размер шрифта.</param>
        /// <param name="x">Положение тексте, координата X.</param>
        /// <param name="y">Положение текста, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться текст.</param>
        /// <param name="isBold">Использовать жирный шрифт.</param>
        TextVisualObject AddTextVisual(string text, double fontSize, double x, double y, int layer, bool isBold = false);

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
        TextVisualObject AddTextVisual(string text, double fontSize, double x, double y, int layer, double width,
            TextAlignment textAlignment = TextAlignment.Center, bool isBold = false, Color? foregroundColor = null);

        /// <summary>
        /// Добавить текст с информацией о юните на сцену.
        /// </summary>
        /// <param name="textGetter">Функция, с помощью которой можно получить текст.</param>
        /// <param name="fontSize">Размер шрифта.</param>
        /// <param name="x">Положение текста, координата X.</param>
        /// <param name="y">Положение текста, координата Y.</param>
        /// <param name="layer">Слой на котором будет отображаться текст.</param>
        /// <param name="isBold">Использовать жирный шрифт.</param>
        UnitInfoTextVisualObject AddUnitInfoTextVisualObject(Func<Unit, string> textGetter, double fontSize,
            int x, int y, int layer, bool isBold = false);



        /// <summary>
        /// Добавить юнита на сцену битвы.
        /// </summary>
        /// <param name="unit">Юнит.</param>
        /// <param name="isAttacker">Является ли юнит атакующим.</param>
        BattleUnit AddBattleUnit(Unit unit, bool isAttacker);

        /// <summary>
        /// Добавить портрет юнита на сцену.
        /// </summary>
        /// <param name="unit">Юнит, чей портрет необходимо добавить.</param>
        /// <param name="rightToLeft">Указатель того, что юнит смотрит справа налево.</param>
        /// <param name="x">Положение портрета, координата X.</param>
        /// <param name="y">Положение портрета, координата Y.</param>
        /// <returns></returns>
        UnitPortraitObject AddUnitPortrait(Unit unit, bool rightToLeft, double x, double y);

        /// <summary>
        /// Отобразить детальную информацию о юните.
        /// </summary>
        /// <param name="unit">Юнит, о котором необходимо вывести информацию.</param>
        /// <returns></returns>
        DetailUnitInfoObject ShowDetailUnitInfo(Unit unit);



        /// <summary>
        /// Удалить указанный объект со сцены.
        /// </summary>
        /// <param name="visualObject">Объект, который необходимо удалить.</param>
        void RemoveVisualObject([CanBeNull]VisualObject visualObject);
    }
}