using System;
using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;
using Action = System.Action;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Класс для кнопки.
/// </summary>
public class ButtonObject : GameObject
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IReadOnlyDictionary<SceneButtonState, IBitmap> _buttonStates;
    private readonly Action _buttonPressedAction;

    private IImageSceneObject _buttonVisualObject = null!;

    /// <summary>
    /// Создать объект типа <see cref="ButtonObject" />.
    /// </summary>
    public ButtonObject(
        ISceneObjectContainer sceneObjectContainer,
        IReadOnlyDictionary<SceneButtonState, IBitmap> buttonStates,
        Action buttonPressedAction,
        double x,
        double y,
        int layer,
        KeyboardButton? hotkey = null
    ) : base(x, y)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _buttonStates = buttonStates;
        _buttonPressedAction = buttonPressedAction;

        ButtonState = SceneButtonState.Disabled;
        Layer = layer;

        var bitmap = _buttonStates[ButtonState];
        Width = bitmap.Width;
        Height = bitmap.Height;

        var hotKeys = hotkey == null
            ? Array.Empty<KeyboardButton>()
            : new[] { hotkey.Value };
        Components = new IComponent[]
        {
            new SelectionComponent(this, OnSelected, OnUnselected),
            new MouseLeftButtonClickComponent(this, hotKeys, OnPressed, OnClicked)
        };
    }

    /// <summary>
    /// Состояние кнопки.
    /// </summary>
    public SceneButtonState ButtonState { get; protected set; }

    /// <summary>
    /// Слой на котором располагается кнопка.
    /// </summary>
    public int Layer { get; }


    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

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

        ProcessClickInternal();
        UpdateButtonVisualObject();
    }

    /// <summary>
    /// Обработать событие нажатия на кнопку.
    /// </summary>
    protected virtual void ProcessClickInternal()
    {
        _buttonPressedAction.Invoke();
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
        _buttonVisualObject.Bitmap = _buttonStates[ButtonState];
    }
}