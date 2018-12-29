using Disciples.Avalonia.Common.SceneObjects;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Platform.Factories;

namespace Disciples.Avalonia.Platform.Factories
{
    /// <inheritdoc />
    public class SceneObjectFactory : ISceneObjectFactory
    {
        /// <inheritdoc />
        public IImageSceneObject CreateImageSceneObject(int layer)
        {
            return new ImageSceneObject(layer);
        }

        /// <inheritdoc />
        public ITextSceneObject CreateTextSceneObject(string text, double fontSize, int layer, bool isBold = false)
        {
            return new TextSceneObject(text, fontSize, layer, isBold);
        }

        /// <inheritdoc />
        public ITextSceneObject CreateTextSceneObject(string text,
            double fontSize,
            int layer,
            double width,
            TextAlignment textAlignment = TextAlignment.Center,
            bool isBold = false,
            GameColor? foregroundColor = null)
        {
            return new TextSceneObject(text, fontSize, layer, width, textAlignment, isBold, foregroundColor);
        }
    }
}