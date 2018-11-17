using System;
using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Engine.Common.Controllers;
using Engine.Common.Enums;

namespace Engine.Common.GameObjects
{
    /// <summary>
    /// Класс для "залипающей кнопки". То есть кнопка остаётся нажатой до тех пор, пока на неё не кликнут еще раз.
    /// </summary>
    public class ToggleButtonObject : ButtonObject
    {
        private bool _isChecked;

        public ToggleButtonObject(IMapVisual mapVisual, IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer)
            : base(mapVisual, buttonStates, buttonPressedAction, x, y, layer)
        {
            _isChecked = false;
        }


        public override void Disable()
        {
            _isChecked = false;

            base.Disable();
        }


        public override void OnSelected()
        {
            if (ButtonState == ButtonState.Pressed && _isChecked)
                return;

            base.OnSelected();
        }

        public override void OnUnselected()
        {
            if (ButtonState == ButtonState.Pressed && _isChecked)
                return;

            base.OnUnselected();
        }

        public override void OnReleased()
        {
            // Отлавливаем ситуацию, когда кликнули, убрали мышь, вернули на место.
            if (ButtonState != ButtonState.Pressed)
                return;

            if (_isChecked)
                ButtonState = ButtonState.Selected;
            _isChecked = !_isChecked;

            OnButtonClicked();
            UpdateButtonVisualObject();
        }
    }
}
