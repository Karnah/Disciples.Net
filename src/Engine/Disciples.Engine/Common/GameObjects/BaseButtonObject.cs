using System;
using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Базовый класс для кнопок.
/// </summary>
public abstract class BaseButtonObject : GameObject
{
    private readonly ISceneObjectContainer _sceneObjectContainer;

    private IImageSceneObject? _buttonVisualObject;

    /// <summary>
    /// Создать объект типа <see cref="BaseButtonObject" />.
    /// </summary>
    protected BaseButtonObject(
        ISceneObjectContainer sceneObjectContainer,
        SceneElement sceneElement,
        IReadOnlyList<KeyboardButton> hotKeys,
        int layer
        ) : base(sceneElement)
    {
        _sceneObjectContainer = sceneObjectContainer;

        ButtonState = SceneButtonState.Disabled;
        Layer = layer;

        Components = new IComponent[]
        {
            new SelectionComponent(this, OnHovered, OnUnhovered),
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

    /// <summary>
    /// Состояния кнопки.
    /// </summary>
    protected abstract ButtonStates? ButtonStates { get; }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        if (ButtonStates != null)
            _buttonVisualObject = _sceneObjectContainer.AddImage(ButtonStates[ButtonState], X, Y, Layer);
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

    /// <inheritdoc />
    protected override void OnHiddenChanged(bool isHidden)
    {
        base.OnHiddenChanged(isHidden);

        if (_buttonVisualObject != null)
            _buttonVisualObject.IsHidden = isHidden;

        if (isHidden)
            SetDisabled();
        else
            SetActive();
    }

    /// <summary>
    /// Обработка события наведения курсора на кнопку.
    /// </summary>
    protected virtual void OnHovered()
    {
        if (ButtonState == SceneButtonState.Disabled)
            return;

        SetState(SceneButtonState.Hover);
    }

    /// <summary>
    /// Обработка события перемещения курсора с кнопки.
    /// </summary>
    protected virtual void OnUnhovered()
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

        return SelectionComponent!.IsHover
            ? SceneButtonState.Hover
            : SceneButtonState.Active;
    }

    /// <summary>
    /// Установить состояние кнопки.
    /// </summary>
    protected virtual void SetState(SceneButtonState buttonState)
    {
        if (buttonState == SceneButtonState.Active && SelectionComponent!.IsHover)
            buttonState = SceneButtonState.Hover;

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
        if (ButtonStates == null)
            return;

        _buttonVisualObject!.Bitmap = ButtonStates[ButtonState];
    }
}