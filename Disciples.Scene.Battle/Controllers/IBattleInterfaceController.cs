using Disciples.Engine.Base;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers
{
    /// <summary>
    /// Контроллер, который взаимодействует и управляет интерфейсом во время битвы.
    /// </summary>
    public interface IBattleInterfaceController : ISupportLoading
    {
        /// <summary>
        /// Обработать события перед обновлением сцены.
        /// </summary>
        void BeforeSceneUpdate(BattleUpdateContext context);

        /// <summary>
        /// Обработать завершение обновлении сцены.
        /// </summary>
        void AfterSceneUpdate(BattleUpdateContext context);
    }
}