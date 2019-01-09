using System.Collections.Generic;

using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;

using Action = System.Action;

namespace Disciples.Engine.Common.GameObjects
{
    /// <summary>
    /// Класс для кнопки.
    /// </summary>
    public class ButtonObject : GameObject
    {
        private readonly ISceneController _sceneController;
        private readonly IDictionary<SceneButtonState, IBitmap> _buttonStates;
        private readonly Action _buttonPressedAction;

        private IImageSceneObject _buttonVisualObject;

        public ButtonObject(
            ISceneController sceneController,
            IDictionary<SceneButtonState, IBitmap> buttonStates,
            Action buttonPressedAction,
            double x,
            double y,
            int layer,
            KeyboardButton? hotkey = null
            ) : base(x, y)
        {
            _sceneController = sceneController;
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
        public override void OnInitialize()
        {
            base.OnInitialize();

            _buttonVisualObject = _sceneController.AddImage(_buttonStates[ButtonState], X, Y, Layer);
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();
            
            _sceneController.RemoveSceneObject(_buttonVisualObject);
            _buttonVisualObject = null;
        }


        /// <summary>
        /// Сделать кнопку доступной для нажатия.
        /// </summary>
        public virtual void Activate()
        {
            ButtonState = SceneButtonState.Active;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Запретить нажатия на кнопку.
        /// </summary>
        public virtual void Disable()
        {
            ButtonState = SceneButtonState.Disabled;
            UpdateButtonVisualObject();
        }


        /// <summary>
        /// Обработка события наведения курсора на кнопку.
        /// </summary>
        public virtual void OnSelected()
        {
            if (ButtonState == SceneButtonState.Disabled)
                return;

            ButtonState = SceneButtonState.Selected;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Обработка события перемещения курсора с кнопки.
        /// </summary>
        public virtual void OnUnselected()
        {
            if (ButtonState == SceneButtonState.Disabled)
                return;

            ButtonState = SceneButtonState.Active;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Обработка события нажатия на кнопку.
        /// </summary>
        public virtual void OnPressed()
        {
            if (ButtonState == SceneButtonState.Disabled)
                return;

            ButtonState = SceneButtonState.Pressed;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Обработка события клика на кнопку (мышь отпустили).
        /// </summary>
        public virtual void OnReleased()
        {
            // Отлавливаем ситуацию, когда кликнули, убрали мышь, вернули на место.
            if (ButtonState != SceneButtonState.Pressed)
                return;

            OnButtonClicked();

            // Если после клика состояние кнопки не изменилось, то делаем её просто выделенной.
            if (ButtonState == SceneButtonState.Pressed)
                ButtonState = SceneButtonState.Selected;

            UpdateButtonVisualObject();
        }


        /// <summary>
        /// Обновить внешний вид кнопки на сцене.
        /// </summary>
        protected void UpdateButtonVisualObject()
        {
            _buttonVisualObject.Bitmap = _buttonStates[ButtonState];
        }

        /// <summary>
        /// Обработать событие нажатия на кнопку.
        /// </summary>
        public virtual void OnButtonClicked()
        {
            _buttonPressedAction?.Invoke();
        }
    }
}