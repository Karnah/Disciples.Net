using Avalonia.Media;
using ReactiveUI;

namespace Engine.Common.VisualObjects
{
    /// <summary>
    /// Игровой объект, который отображает текст на сцене.
    /// </summary>
    public class TextVisualObject : VisualObject
    {
        private string _text;

        public TextVisualObject(string text, double fontSize, int layer, bool isBold = false)
            : this(text, fontSize, layer, double.NaN, TextAlignment.Left, isBold)
        {
        }

        public TextVisualObject(
            string text,
            double fontSize,
            int layer,
            double width,
            TextAlignment textAlignment = TextAlignment.Center,
            bool isBold = false,
            Color? foregroundColor = null) : base(layer)
        {
            Text = text;
            FontSize = fontSize;
            IsBold = isBold;
            Width = width;
            Height = double.NaN;
            TextAlignment = textAlignment;
            Foreground = new SolidColorBrush(foregroundColor ?? Colors.Black);
        }


        /// <summary>
        /// Текст, который необходимо отобразить.
        /// </summary>
        public string Text {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        /// <summary>
        /// Размер шрифта.
        /// </summary>
        public double FontSize { get; }

        /// <summary>
        /// Жирный ли шрифт.
        /// </summary>
        public bool IsBold { get; }

        /// <summary>
        /// Выравнивание текста по ширине.
        /// </summary>
        public TextAlignment TextAlignment { get; }

        /// <summary>
        /// Цвет текста.
        /// </summary>
        public Brush Foreground { get; }
    }
}