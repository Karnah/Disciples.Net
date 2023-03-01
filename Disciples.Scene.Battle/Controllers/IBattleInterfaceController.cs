using Disciples.Engine.Base;

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
        void BeforeSceneUpdate();

        /// <summary>
        /// Обработать завершение обновлении сцены.
        /// </summary>
        void AfterSceneUpdate();
    }
}