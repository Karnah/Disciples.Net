using System.Collections.Generic;

using Disciples.Engine.Base;
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
    private readonly IDictionary<SceneButtonState, IBitmap> _buttonStates;
    private readonly Action _buttonPressedAction;

    private IImageSceneObject _buttonVisualObject = null!;

    /// <summary>
    /// Создать объект типа <see cref="ButtonObject" />.
    /// </summary>
    public ButtonObject(
        ISceneObjectContainer sceneObjectContainer,
        IDictionary<SceneButtonState, IBitmap> buttonStates,
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
        Hotkey = hotkey;
        Layer = layer;

        var bitmap = _buttonStates[ButtonState];
        Width = bitmap.Width;
        Height = bitmap.Height;
    }


    /// <summary>
    /// Состояние кнопки.
    /// </summary>
    public SceneButtonState ButtonState { get; protected set; }

    /// <summary>
    /// Горячая клавиша для кнопки.
    /// </summary>
    public KeyboardButton? Hotkey { get; }

    /// <summary>
    /// Слой на котором располагается кнопка.
    /// </summary>
    public int Layer { get; }

    /// <inheritdoc />
    public override bool IsInteractive => true;


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
        if (ButtonState == SceneButtonState.Active)
            return;

        ButtonState = SceneButtonState.Active;
        UpdateButtonVisualObject();
    }

    /// <summary>
    /// Запретить нажатия на кнопку.
    /// </summary>
    public virtual void SetDisabled()
    {
        if (ButtonState == SceneButtonState.Disabled)
            return;

        ButtonState = SceneButtonState.Disabled;
        UpdateButtonVisualObject();
    }

    /// <summary>
    /// Обработка события наведения курсора на кнопку.
    /// </summary>
    public virtual void SetSelected()
    {
        if (ButtonState == SceneButtonState.Disabled)
            return;

        ButtonState = SceneButtonState.Selected;
        UpdateButtonVisualObject();
    }

    /// <summary>
    /// Обработка события перемещения курсора с кнопки.
    /// </summary>
    public virtual void SetUnselected()
    {
        if (ButtonState == SceneButtonState.Disabled)
            return;

        ButtonState = SceneButtonState.Active;
        UpdateButtonVisualObject();
    }

    /// <summary>
    /// Обработка события нажатия на кнопку.
    /// </summary>
    public virtual void Press()
    {
        if (ButtonState == SceneButtonState.Disabled)
            return;

        ButtonState = SceneButtonState.Pressed;
        UpdateButtonVisualObject();
    }

    /// <summary>
    /// Обработка события клика на кнопку (мышь отпустили).
    /// </summary>
    public virtual void Release()
    {
        // Отлавливаем ситуацию, когда кликнули, убрали мышь, вернули на место.
        if (ButtonState != SceneButtonState.Pressed)
            return;

        Click();

        // Если после клика состояние кнопки не изменилось, то делаем её просто выделенной.
        if (ButtonState == SceneButtonState.Pressed)
            ButtonState = SceneButtonState.Selected;

        UpdateButtonVisualObject();
    }

    /// <summary>
    /// Обработать событие нажатия на кнопку.
    /// </summary>
    public virtual void Click()
    {
        _buttonPressedAction.Invoke();
    }

    /// <summary>
    /// Обновить внешний вид кнопки на сцене.
    /// </summary>
    protected void UpdateButtonVisualObject()
    {
        _buttonVisualObject.Bitmap = _buttonStates[ButtonState];
    }
}