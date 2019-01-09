namespace Disciples.Engine.Base
{
    /// <summary>
    /// Интерфейс для обозначения тех объектов, которые поддерживают деинициализацию.
    /// </summary>
    public interface ISupportUnloading
    {
        /// <summary>
        /// Был ли инициализирован объект.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Обозначение того, что объект инициализируется один раз и не деинициализируется при смене сцен.
        /// </summary>
        bool OneTimeLoading { get; }


        /// <summary>
        /// Очистить занимаемые объектом ресурсы.
        /// </summary>
        void Unload();
    }
}