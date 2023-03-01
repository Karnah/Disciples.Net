using Disciples.Engine.Base;

namespace Disciples.Scene.Battle.Controllers
{
    /// <summary>
    /// Класс для мониторинга и управлением состояния битвы.
    /// </summary>
    public interface IBattleController : ISupportLoading
    {
        /// <summary>
        /// Обновить состояние объектов на сцене.
        /// </summary>
        public void BeforeSceneUpdate();

        /// <summary>
        /// Обновить состояние объектов на сцене.
        /// </summary>
        public void AfterSceneUpdate();
    }
}