using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Enums;

namespace Disciples.Engine.Models;

/// <summary>
/// Событие от устройств ввода (мыши или клавиатуры).
/// </summary>
public class InputDeviceEvent
{
    /// <summary>
    /// Создать объект класса <see cref="InputDeviceEvent" />.
    /// </summary>
    public InputDeviceEvent(InputDeviceActionType actionType, InputDeviceActionState actionState, GameObject? gameObject)
    {
        ActionType = actionType;
        ActionState = actionState;
        GameObject = gameObject;
    }

    /// <summary>
    /// Действие, которое было выполнено.
    /// </summary>
    public InputDeviceActionType ActionType { get; }

    /// <summary>
    /// Состояние действия.
    /// </summary>
    public InputDeviceActionState ActionState { get; }

    /// <summary>
    /// Объект над которым было совершено действие.
    /// </summary>
    public GameObject? GameObject { get; }
}