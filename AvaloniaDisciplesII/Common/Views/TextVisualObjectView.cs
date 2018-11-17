using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

using Engine.Common.Models;

namespace AvaloniaDisciplesII.Common.Views
{
    /// <summary>
    /// Контрол для отображения текста на сцене.
    /// </summary>
    public class TextVisualObjectView : TemplatedControl
    {
        /// <summary>
        /// Объект текста.
        /// </summary>
        public static readonly DirectProperty<TextVisualObjectView, TextVisualObject> TextVisualObjectProperty =
            AvaloniaProperty.RegisterDirect<TextVisualObjectView, TextVisualObject>(
                nameof(TextVisualObject),
                uc => uc.TextVisualObject,
                (uc, vo) => uc.TextVisualObject = vo,
                defaultBindingMode: BindingMode.TwoWay);

        private TextVisualObject _textVisualObject;

        /// <summary>
        /// Объект текста.
        /// </summary>
        public TextVisualObject TextVisualObject {
            get => _textVisualObject;
            set => SetAndRaise(TextVisualObjectProperty, ref _textVisualObject, value);
        }
    }
}
