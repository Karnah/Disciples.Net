using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Engine.Common.Controllers;
using Engine.Common.Enums;
using Engine.Common.Models;

using Action = System.Action;

namespace Engine.Common.GameObjects
{
    /// <summary>
    /// Класс для кнопки
    /// </summary>
    public class ButtonObject : GameObject
    {
        private readonly IMapVisual _mapVisual;
        private readonly IDictionary<ButtonState, Bitmap> _buttonStates;
        private readonly Action _buttonPressedAction;

        private VisualObject _buttonVisualObject;

        public ButtonObject(IMapVisual mapVisual, IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer)
            : base(x, y)
        {
            _mapVisual = mapVisual;
            _buttonStates = buttonStates;
            _buttonPressedAction = buttonPressedAction;

            ButtonState = ButtonState.Disabled;
            var bitmap = _buttonStates[ButtonState];
            _buttonVisualObject = new VisualObject(this, layer) {
                Width = bitmap.PixelWidth,
                Height = bitmap.PixelHeight,
                X = X,
                Y = Y,
                Bitmap = bitmap
            };
        }


        /// <summary>
        /// Состояние кнопки
        /// </summary>
        public ButtonState ButtonState { get; protected set; }


        public override void OnInitialize()
        {
            base.OnInitialize();

            _mapVisual.AddVisual(_buttonVisualObject);
        }

        public override void Destroy()
        {
            base.Destroy();
            
            _mapVisual.RemoveVisual(_buttonVisualObject);
            _buttonVisualObject = null;
        }


        /// <summary>
        /// Сделать кнопку доступной для нажатия
        /// </summary>
        public virtual void Activate()
        {
            ButtonState = ButtonState.Active;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Запретить нажатия на кнопку
        /// </summary>
        public virtual void Disable()
        {
            ButtonState = ButtonState.Disabled;
            UpdateButtonVisualObject();
        }


        /// <summary>
        /// Обработка события наведения курсора на кнопку
        /// </summary>
        public virtual void OnSelected()
        {
            if (ButtonState == ButtonState.Disabled)
                return;

            ButtonState = ButtonState.Selected;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Обработка события перемещения курсора с кнопки
        /// </summary>
        public virtual void OnUnselected()
        {
            if (ButtonState == ButtonState.Disabled)
                return;

            ButtonState = ButtonState.Active;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Обработка события нажатия на кнопку
        /// </summary>
        public virtual void OnPressed()
        {
            if (ButtonState == ButtonState.Disabled)
                return;

            ButtonState = ButtonState.Pressed;
            UpdateButtonVisualObject();
        }

        /// <summary>
        /// Обработка события клика на кнопку (мышь отпустили)
        /// </summary>
        public virtual void OnReleased()
        {
            // Отлавливаем ситуацию, когда кликнули, убрали мышь, вернули на место
            if (ButtonState != ButtonState.Pressed)
                return;

            ButtonClicked();

            ButtonState = ButtonState.Selected;
            UpdateButtonVisualObject();
        }


        /// <summary>
        /// Обновить внешний вид кнопки на сцене
        /// </summary>
        protected void UpdateButtonVisualObject()
        {
            _buttonVisualObject.Bitmap = _buttonStates[ButtonState];
        }

        protected void ButtonClicked()
        {
            _buttonPressedAction?.Invoke();
        }
    }
}
