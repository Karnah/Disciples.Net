using System.Collections.Generic;

namespace Disciples.Engine.Models;

/// <summary>
/// Данные для обновления сцены.
/// </summary>
public class UpdateSceneData
{
    /// <summary>
    /// Создать объект типа <see cref="UpdateSceneData" />.
    /// </summary>
    public UpdateSceneData(long ticksCount, Point mousePosition, IReadOnlyList<InputDeviceEvent> inputDeviceEvents)
    {
        TicksCount = ticksCount;
        MousePosition = mousePosition;
        InputDeviceEvents = inputDeviceEvents;
    }

    /// <summary>
    /// Количество тиков, которое прошло с момента предыдущего обновления сцены.
    /// </summary>
    public long TicksCount { get; }

    /// <summary>
    /// Позиция курсора.
    /// </summary>
    public Point MousePosition { get; }

    /// <summary>
    /// События от устройств ввода (мыши или клавиатуры).
    /// </summary>
    public IReadOnlyList<InputDeviceEvent> InputDeviceEvents { get; }
}