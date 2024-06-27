using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Enums;

namespace Disciples.Engine.Implementation.Extensions;

/// <summary>
/// Методы-расширения для обработки событий пользовательского ввода.
/// </summary>
internal static class InputDeviceEventExtensions
{
    /// <summary>
    /// Обработать событие мыши.
    /// </summary>
    public static void ProcessMouseEvent(InputDeviceActionType actionType, InputDeviceActionState actionState, GameObject? inputDeviceGameObject)
    {
        // Объект может быть уничтожен при обработке предыдущего события.
        if (inputDeviceGameObject is { IsDestroyed: false, IsDeactivated: false })
        {
            switch (actionType)
            {
                case InputDeviceActionType.Hover when actionState == InputDeviceActionState.Activated:
                    inputDeviceGameObject.SelectionComponent?.Hovered();
                    return;
                case InputDeviceActionType.Hover when actionState == InputDeviceActionState.Deactivated:
                    inputDeviceGameObject.SelectionComponent?.Unhovered();
                    return;

                case InputDeviceActionType.MouseLeft when actionState == InputDeviceActionState.Activated:
                    inputDeviceGameObject.MouseLeftButtonClickComponent?.Pressed();
                    return;
                case InputDeviceActionType.MouseLeft when actionState == InputDeviceActionState.Deactivated:
                    inputDeviceGameObject.MouseLeftButtonClickComponent?.Clicked();
                    return;

                case InputDeviceActionType.MouseRight when actionState == InputDeviceActionState.Activated:
                    inputDeviceGameObject.MouseRightButtonClickComponent?.Pressed();
                    return;
                case InputDeviceActionType.MouseRight when actionState == InputDeviceActionState.Deactivated:
                    inputDeviceGameObject.MouseRightButtonClickComponent?.Released();
                    return;
            }
        }
    }

    /// <summary>
    /// Обработать событие клавиатуры.
    /// </summary>
    public static void ProcessKeyboardEvent(InputDeviceActionState actionState, KeyboardButton keyboardButton, IReadOnlyCollection<GameObject> gameObjects)
    {
        if (actionState != InputDeviceActionState.Activated)
            return;

        // При нажатии на кнопку выбранный объект не имеет значения, поэтому просто перебираем все доступные объекты.
        // Обязательно материализуем коллекцию, так как gameObjects может меняться при обработке.
        var buttons = gameObjects
            .Where(go => go is { IsDestroyed: false, IsDeactivated: false } &&
                         go.MouseLeftButtonClickComponent?.HotKeys.Contains(keyboardButton) == true)
            .ToList();
        foreach (var gameObject in buttons)
            gameObject.MouseLeftButtonClickComponent!.PressedKeyboardButton(keyboardButton);
    }
}
