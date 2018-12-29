using System.Collections.Generic;

using Disciples.Engine.Common.Controllers;
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
        private readonly IVisualSceneController _visualSceneController;
        private readonly IDictionary<ButtonState, IBitmap> _buttonStates;
        private readonly Action _buttonPressedAction;

        private IImageSceneObject _buttonVisualObject;

        public ButtonObject(
            IVisualSceneController visualSceneController,
            IDictionary<ButtonState, IBitmap> buttonStates,
            Action buttonPressedAction,
            double x,
            double y,
            int layer,
            KeyboardButton? hotkey = null
            ) : base(x, y)
        {
            _visualSceneController = visualSceneController;
            _buttonStates = buttonStates;
            _buttonPressedAction = buttonPressedAction;

            ButtonState = ButtonState.Disabled;
            Hotkey = hotkey;
            Layer = layer;

            var bitmap = _buttonStates[ButtonState];
            Width = bitmap.Width;
            Height = bitmap.Height;
        }


        /// <summary>
        /// Состояние кнопки.
        /// </summary>
        public ButtonState ButtonState { get; protected set; }

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

            _buttonVisualObject = _visualSceneController.AddImage(_buttonStates[ButtonState], X, Y, Layer);
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();
            
            _visualSceneController.RemoveSceneObject(_buttonVisualObject);
            _buttonVisualObject = null;
        }


        /// <summary>
        /// Сделать кнопку доступной для нажатия.
        /// </summary>
        public virtual void Activate()
        {
            ButtonState = ButtonState.Active;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Запретить нажатия на кнопку.
        /// </summary>
        public virtual void Disable()
        {
            ButtonState = ButtonState.Disabled;
            UpdateButtonVisualObject();
        }


        /// <summary>
        /// Обработка события наведения курсора на кнопку.
        /// </summary>
        public virtual void OnSelected()
        {
            if (ButtonState == ButtonState.Disabled)
                return;

            ButtonState = ButtonState.Selected;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Обработка события перемещения курсора с кнопки.
        /// </summary>
        public virtual void OnUnselected()
        {
            if (ButtonState == ButtonState.Disabled)
                return;

            ButtonState = ButtonState.Active;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Обработка события нажатия на кнопку.
        /// </summary>
        public virtual void OnPressed()
        {
            if (ButtonState == ButtonState.Disabled)
                return;

            ButtonState = ButtonState.Pressed;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Обработка события клика на кнопку (мышь отпустили).
        /// </summary>
        public virtual void OnReleased()
        {
            // Отлавливаем ситуацию, когда кликнули, убрали мышь, вернули на место.
            if (ButtonState != ButtonState.Pressed)
                return;

            OnButtonClicked();

            // Если после клика состояние кнопки не изменилось, то делаем её просто выделенной.
            if (ButtonState == ButtonState.Pressed)
                ButtonState = ButtonState.Selected;

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