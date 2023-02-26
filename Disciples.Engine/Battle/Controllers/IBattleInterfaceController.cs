using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Models;

namespace Disciples.Engine.Battle.Controllers
{
    /// <summary>
    /// Контроллер, который взаимодействует и управляет интерфейсом во время битвы.
    /// </summary>
    public interface IBattleInterfaceController : ISupportLoading
    {
        /// <summary>
        /// Обработать события от устройств ввода.
        /// </summary>
        void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> events);
    }
}