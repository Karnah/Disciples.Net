using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;
using Action = System.Action;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Класс для кнопки.
/// </summary>
public class ButtonObject : GameObject
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IReadOnlyDictionary<SceneButtonState, IBitmap>? _buttonStates;

    private IImageSceneObject? _buttonVisualObject;

    /// <summary>
    /// Создать объект типа <see cref="ButtonObject" />.
    /// </summary>
    public ButtonObject(
        ISceneObjectContainer sceneObjectContainer,
        ButtonSceneElement button,
        int layer
    ) : base(button)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _buttonStates = button.ButtonStates;

        ButtonState = SceneButtonState.Disabled;
        Layer = layer;

        Components = new IComponent[]
        {
            new SelectionComponent(this, OnSelected, OnUnselected),
            new MouseLeftButtonClickComponent(this, button.HotKeys, OnPressed, OnClicked)
        };
    }

    /// <summary>
    /// Создать объект типа <see cref="ButtonObject" />.
    /// </summary>
    public ButtonObject(
        ISceneObjectContainer sceneObjectContainer,
        IReadOnlyDictionary<SceneButtonState, IBitmap> buttonStates,
        Action buttonClickedAction,
        double x,
        double y,
        int layer,
        IReadOnlyList<KeyboardButton> hotKeys
        ) : base(x, y)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _buttonStates = buttonStates;

        ButtonState = SceneButtonState.Disabled;
        ClickedAction = buttonClickedAction;
        Layer = layer;

        var bitmap = _buttonStates[ButtonState];
        Width = bitmap.Width;
        Height = bitmap.Height;

        Components = new IComponent[]
        {
            new SelectionComponent(this, OnSelected, OnUnselected),
            new MouseLeftButtonClickComponent(this, hotKeys, OnPressed, OnClicked)
        };
    }

    /// <summary>
    /// Состояние кнопки.
    /// </summary>
    public SceneButtonState ButtonState { get; private set; }

    /// <summary>
    /// Обработчик клика на кнопку.
    /// </summary>
    public Action? ClickedAction { get; set; }

    /// <summary>
    /// Слой на котором располагается кнопка.
    /// </summary>
    public int Layer { get; }


    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        if (_buttonStates != null)
            _buttonVisualObject = _sceneObjectContainer.AddImage(_buttonStates[ButtonState], X, Y, Layer);
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneObjectContainer.RemoveSceneObject(_buttonVisualObject);
    }

    /// <summary>
    /// Сделать кнопку доступной для нажатия.
    /// </summary>
    public virtual void SetActive()
    {
        SetState(SceneButtonState.Active);
    }

    /// <summary>
    /// Запретить нажатия на кнопку.
    /// </summary>
    public virtual void SetDisabled()
    {
        SetState(SceneButtonState.Disabled);
    }

    /// <summary>
    /// Обработка события наведения курсора на кнопку.
    /// </summary>
    protected virtual void OnSelected()
    {
        if (ButtonState == SceneButtonState.Disabled)
            return;

        SetState(SceneButtonState.Selected);
    }

    /// <summary>
    /// Обработка события перемещения курсора с кнопки.
    /// </summary>
    protected virtual void OnUnselected()
    {
        if (ButtonState == SceneButtonState.Disabled)
            return;

        SetState(SceneButtonState.Active);
    }

    /// <summary>
    /// Обработка события нажатия на кнопку.
    /// </summary>
    protected virtual void OnPressed()
    {
        if (ButtonState == SceneButtonState.Disabled)
            return;

        SetState(SceneButtonState.Pressed);
    }

    /// <summary>
    /// Обработка события клика на кнопку (мышь отпустили).
    /// </summary>
    protected virtual void OnClicked()
    {
        if (ButtonState == SceneButtonState.Disabled)
            return;

        var newButtonState = ProcessClickInternal();
        SetState(newButtonState);
    }

    /// <summary>
    /// Обработать событие нажатия на кнопку.
    /// </summary>
    protected virtual SceneButtonState ProcessClickInternal()
    {
        ClickedAction?.Invoke();

        // Во время клика могли изменить состояние кнопки.
        if (ButtonState == SceneButtonState.Disabled)
            return SceneButtonState.Disabled;

        return SelectionComponent!.IsSelected
            ? SceneButtonState.Selected
            : SceneButtonState.Active;
    }

    /// <summary>
    /// Установить состояние кнопки.
    /// </summary>
    protected virtual void SetState(SceneButtonState buttonState)
    {
        if (buttonState == SceneButtonState.Active && SelectionComponent!.IsSelected)
            buttonState = SceneButtonState.Selected;

        if (ButtonState == buttonState)
            return;

        ButtonState = buttonState;
        UpdateButtonVisualObject();
    }

    /// <summary>
    /// Обновить внешний вид кнопки на сцене.
    /// </summary>
    protected void UpdateButtonVisualObject()
    {
        if (_buttonStates == null)
            return;

        _buttonVisualObject!.Bitmap = _buttonStates[ButtonState];
    }
}