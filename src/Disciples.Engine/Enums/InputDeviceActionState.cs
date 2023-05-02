namespace Disciples.Engine.Enums;

/// <summary>
/// Состояние действия, которое выполняется с помощью устройства ввода.
/// </summary>
public enum InputDeviceActionState
{
    /// <summary>
    /// Действие началось.
    /// Была зажата кнопка или появилось выделение на объекте.
    /// </summary>
    Activated,

    /// <summary>
    /// Действие завершилось.
    /// Кнопка была отпущена или выделение снято с объекта.
    /// </summary>
    Deactivated
}