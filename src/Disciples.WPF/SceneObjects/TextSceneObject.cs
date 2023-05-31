using ReactiveUI.Fody.Helpers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;

namespace Disciples.WPF.SceneObjects;

/// <inheritdoc cref="ITextSceneObject" />
public class TextSceneObject : BaseSceneObject, ITextSceneObject
{
    /// <summary>
    /// Создать объект типа <see cref="TextSceneObject" />.
    /// </summary>
    public TextSceneObject(TextContainer? text, TextStyle textStyle, double width, double height, double x, double y, int layer) : base(layer)
    {
        Text = text;
        TextStyle = textStyle;
        Width = width;
        Height = height;
        X = x;
        Y = y;
    }

    /// <inheritdoc />
    [Reactive]
    public TextContainer? Text { get; set; }

    /// <inheritdoc />
    [Reactive]
    public TextStyle TextStyle { get; set; }
}