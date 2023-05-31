using System;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Текстовый элемент списка.
/// </summary>
internal class TextListBoxItemObject : GameObject
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly TextStyle? _selectedTextStyle;
    private readonly TextStyle? _commonTextStyle;
    private readonly int _layer;

    private bool _isSelected;
    private ITextSceneObject _textSceneObject = null!;

    /// <summary>
    /// Создать объект типа <see cref="TextListBoxItemObject" />.
    /// </summary>
    public TextListBoxItemObject(
        ISceneObjectContainer sceneObjectContainer,
        TextStyle? selectedTextStyle,
        TextStyle? commonTextStyle,
        TextListBoxItem item,
        Action<TextListBoxItemObject> onItemPressed,
        Action<TextListBoxItemObject> onItemMouseLeftButtonDoubleClicked,
        RectangleD position,
        int layer
        ) : base(position)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _selectedTextStyle = selectedTextStyle;
        _commonTextStyle = commonTextStyle;
        _layer = layer;

        Item = item;

        Components = new IComponent[]
        {
            new SelectionComponent(this),
            new MouseLeftButtonClickComponent(this, Array.Empty<KeyboardButton>(),
                () => onItemPressed.Invoke(this),
                onDoubleClickedAction: () => onItemMouseLeftButtonDoubleClicked.Invoke(this))
        };
    }

    /// <summary>
    /// Элемент списка.
    /// </summary>
    public TextListBoxItem Item { get; }

    /// <summary>
    /// Признак, что объект выделен.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value )
                return;

            _isSelected = value;
            _textSceneObject.TextStyle = (value
                ? _selectedTextStyle
                : _commonTextStyle) ?? new TextStyle();
        }
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _textSceneObject = _sceneObjectContainer.AddText(Item.Text, _commonTextStyle, Width, Height, X, Y, _layer);
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneObjectContainer.RemoveSceneObject(_textSceneObject);
    }
}