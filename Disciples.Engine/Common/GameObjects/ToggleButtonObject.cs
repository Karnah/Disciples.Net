using System;
using System.Collections.Generic;

using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.GameObjects
{
    /// <summary>
    /// Класс для "залипающей кнопки". То есть кнопка остаётся нажатой до тех пор, пока на неё не кликнут еще раз.
    /// </summary>
    public class ToggleButtonObject : ButtonObject
    {
        private bool _isChecked;

        public ToggleButtonObject(
            IVisualSceneController visualSceneController,
            IDictionary<SceneButtonState, IBitmap> buttonStates,
            Action buttonPressedAction,
            double x,
            double y,
            int layer,
            KeyboardButton? hotkey = null)
            : base(visualSceneController, buttonStates, buttonPressedAction, x, y, layer, hotkey)
        {
            _isChecked = false;
        }


        /// <inheritdoc />
        public override void Disable()
        {
            _isChecked = false;

            base.Disable();
        }


        /// <inheritdoc />
        public override void OnSelected()
        {
            if (ButtonState == SceneButtonState.Pressed && _isChecked)
                return;

            base.OnSelected();
        }

        /// <inheritdoc />
        public override void OnUnselected()
        {
            if (ButtonState == SceneButtonState.Pressed && _isChecked)
                return;

            base.OnUnselected();
        }

        /// <inheritdoc />
        public override void OnReleased()
        {
            // Отлавливаем ситуацию, когда кликнули, убрали мышь, вернули на место.
            if (ButtonState != SceneButtonState.Pressed)
                return;

            if (_isChecked)
                ButtonState = SceneButtonState.Selected;

            OnButtonClicked();
            UpdateButtonVisualObject();
        }


        /// <inheritdoc />
        public override void OnButtonClicked()
        {
            SetState(!_isChecked);

            base.OnButtonClicked();
        }

        /// <summary>
        /// Установить новое состояние кнопки.
        /// </summary>
        public void SetState(bool isChecked)
        {
            if (ButtonState == SceneButtonState.Disabled || _isChecked == isChecked)
                return;

            _isChecked = isChecked;
            ButtonState = _isChecked
                ? SceneButtonState.Pressed
                : SceneButtonState.Active;
            UpdateButtonVisualObject();
        }
    }
}