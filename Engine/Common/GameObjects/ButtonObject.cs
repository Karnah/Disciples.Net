using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Engine.Common.Controllers;
using Engine.Common.Enums;
using Engine.Common.VisualObjects;
using Action = System.Action;

namespace Engine.Common.GameObjects
{
    /// <summary>
    /// Класс для кнопки.
    /// </summary>
    public class ButtonObject : GameObject
    {
        private readonly IMapVisual _mapVisual;
        private readonly IDictionary<ButtonState, Bitmap> _buttonStates;
        private readonly Action _buttonPressedAction;

        private ImageVisualObject _buttonVisualObject;

        public ButtonObject(
            IMapVisual mapVisual,
            IDictionary<ButtonState, Bitmap> buttonStates,
            Action buttonPressedAction,
            double x,
            double y,
            int layer,
            KeyboardButton? hotkey = null)
            : base(x, y)
        {
            _mapVisual = mapVisual;
            _buttonStates = buttonStates;
            _buttonPressedAction = buttonPressedAction;

            ButtonState = ButtonState.Disabled;
            Hotkey = hotkey;

            var bitmap = _buttonStates[ButtonState];
            Width = bitmap.PixelSize.Width;
            Height = bitmap.PixelSize.Height;

            _buttonVisualObject = new ImageVisualObject(layer) {
                Width = bitmap.PixelSize.Width,
                Height = bitmap.PixelSize.Height,
                X = X,
                Y = Y,
                Bitmap = bitmap
            };
        }


        /// <summary>
        /// Состояние кнопки.
        /// </summary>
        public ButtonState ButtonState { get; protected set; }

        /// <summary>
        /// Горячая клавиша для кнопки.
        /// </summary>
        public KeyboardButton? Hotkey { get; }

        /// <inheritdoc />
        public override bool IsInteractive => true;


        /// <inheritdoc />
        public override void OnInitialize()
        {
            base.OnInitialize();

            _mapVisual.AddVisual(_buttonVisualObject);
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();
            
            _mapVisual.RemoveVisual(_buttonVisualObject);
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