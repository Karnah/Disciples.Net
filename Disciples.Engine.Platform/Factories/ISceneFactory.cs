using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Platform.Factories
{
    /// <summary>
    /// Фабрика для сцены и объектов на ней.
    /// </summary>
    public interface ISceneFactory
    {
        /// <summary>
        /// Создать пустой контейнер для объектов сцены.
        /// </summary>
        ISceneContainer CreateSceneContainer();


        /// <summary>
        /// Создать пустое изображение, которое отображается на сцене.
        /// </summary>
        /// <param name="layer">Слой, на котором изображение будет отображаться.</param>
        IImageSceneObject CreateImageSceneObject(int layer);

        /// <summary>
        /// Создать текст, который будет отображаться на сцене.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="fontSize">Размер шрифта.</param>
        /// <param name="layer">Слой, на котором будет отображаться текст.</param>
        /// <param name="isBold">Использовать жирный шрифт.</param>
        ITextSceneObject CreateTextSceneObject(string text, double fontSize, int layer, bool isBold = false);

        /// <summary>
        /// Создать текст, который будет отображаться на сцене.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="fontSize">Размер шрифта.</param>
        /// <param name="layer">Слой, на котором будет отображаться текст.</param>
        /// <param name="width">Ширина пространства, занимаемого текстом.</param>
        /// <param name="textAlignment">Выравнивание текста по ширине.</param>
        /// <param name="isBold">Использовать жирный шрифт.</param>
        /// <param name="foregroundColor">Цвет шрифта.</param>
        ITextSceneObject CreateTextSceneObject(string text,
            double fontSize,
            int layer,
            double width,
            TextAlignment textAlignment = TextAlignment.Center,
            bool isBold = false,
            GameColor? foregroundColor = null);
    }
}