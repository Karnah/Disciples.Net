using Disciples.Common.Models;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;
using ReactiveUI.Fody.Helpers;

namespace Disciples.Avalonia.SceneObjects;

/// <inheritdoc cref="ITextSceneObject" />
public class TextSceneObject : BaseSceneObject, ITextSceneObject
{
    /// <summary>
    /// Создать объект типа <see cref="TextSceneObject" />.
    /// </summary>
    public TextSceneObject(TextContainer? text, TextStyle textStyle, RectangleD bounds, int layer) : base(bounds, layer)
    {
        Text = text;
        TextStyle = textStyle;
    }

    /// <inheritdoc />
    [Reactive]
    public TextContainer? Text { get; set; }

    /// <inheritdoc />
    [Reactive]
    public TextStyle TextStyle { get; set; }
}