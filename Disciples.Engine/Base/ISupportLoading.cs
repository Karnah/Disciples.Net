namespace Disciples.Engine.Base
{
    /// <summary>
    /// Интерфейс для обозначения тех объектов, которые поддерживают загрузку и очистку ресурсов.
    /// </summary>
    public interface ISupportLoading
    {
        /// <summary>
        /// Был ли инициализирован объект.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Обозначение того, что объект инициализируется один раз и не деинициализируется при смене сцен.
        /// </summary>
        bool IsSharedBetweenScenes { get; }


        /// <summary>
        /// Инициализировать объект.
        /// </summary>
        void Load();

        /// <summary>
        /// Очистить занимаемые объектом ресурсы.
        /// </summary>
        void Unload();
    }
}