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
    public UpdateSceneData(long ticksCount, IReadOnlyList<InputDeviceEvent> inputDeviceEvents)
    {
        TicksCount = ticksCount;
        InputDeviceEvents = inputDeviceEvents;
    }

    /// <summary>
    /// Количество тиков, которое прошло с момента предыдущего обновления сцены.
    /// </summary>
    public long TicksCount { get; }

    /// <summary>
    /// События от устройств ввода (мыши или клавиатуры).
    /// </summary>
    public IReadOnlyList<InputDeviceEvent> InputDeviceEvents { get; }
}