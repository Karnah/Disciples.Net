using ReactiveUI;

using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Avalonia.Common.SceneObjects
{
    /// <inheritdoc cref="ITextSceneObject" />
    public class TextSceneObject : BaseSceneObject, ITextSceneObject
    {
        private string _text;

        /// <inheritdoc />
        public TextSceneObject(string text, double fontSize, int layer, bool isBold = false)
            : this(text, fontSize, layer, double.NaN, TextAlignment.Left, isBold)
        { }

        /// <inheritdoc />
        public TextSceneObject(
            string text,
            double fontSize,
            int layer,
            double width,
            TextAlignment textAlignment = TextAlignment.Center,
            bool isBold = false,
            GameColor? foregroundColor = null) : base(layer)
        {
            Text = text;
            FontSize = fontSize;
            IsBold = isBold;
            Width = width;
            Height = double.NaN;
            TextAlignment = textAlignment;
            Foreground = foregroundColor ?? GameColor.Black;
        }


        /// <inheritdoc />
        public string Text {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        /// <inheritdoc />
        public double FontSize { get; }

        /// <inheritdoc />
        public bool IsBold { get; }

        /// <inheritdoc />
        public TextAlignment TextAlignment { get; }

        /// <inheritdoc />
        public GameColor Foreground { get; }
    }
}