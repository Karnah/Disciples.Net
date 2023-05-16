using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Объект текстового поля.
/// </summary>
public class TextBlockObject : GameObject
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly TextBlockSceneElement _textBlock;
    private readonly int _layer;

    private ITextSceneObject _textSceneObject = null!;

    /// <summary>
    /// Создать объект типа <see cref="TextBlockObject" />.
    /// </summary>
    public TextBlockObject(
        ISceneObjectContainer sceneObjectContainer,
        TextBlockSceneElement textBlock,
        int layer) : base(textBlock)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _textBlock = textBlock;
        _layer = layer;
    }

    /// <summary>
    /// Текст.
    /// </summary>
    public string? Text
    {
        get => _textSceneObject.Text;
        set => _textSceneObject.Text = value;
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _textSceneObject = _sceneObjectContainer.AddText(_textBlock.Text, 12, X, Y, _layer, Width);
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneObjectContainer.RemoveSceneObject(_textSceneObject);
    }
}