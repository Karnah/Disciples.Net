using System.Drawing;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;
using ReactiveUI.Fody.Helpers;

namespace Disciples.WPF.SceneObjects;

/// <inheritdoc cref="ITextSceneObject" />
public class TextSceneObject : BaseSceneObject, ITextSceneObject
{
    /// <summary>
    /// Создать объект типа <see cref="TextSceneObject" />.
    /// </summary>
    public TextSceneObject(string text, double fontSize, int layer, bool isBold = false)
        : this(text, fontSize, layer, double.NaN, TextAlignment.Left, isBold)
    { }

    /// <summary>
    /// Создать объект типа <see cref="TextSceneObject" />.
    /// </summary>
    public TextSceneObject(string text,
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
        Foreground = foregroundColor ?? GameColors.Black;
    }


    /// <inheritdoc />
    [Reactive]
    public string Text { get; set; }

    /// <inheritdoc />
    public double FontSize { get; }

    /// <inheritdoc />
    public bool IsBold { get; }

    /// <inheritdoc />
    public TextAlignment TextAlignment { get; }

    /// <inheritdoc />
    public Color Foreground { get; }
}