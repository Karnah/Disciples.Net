namespace Disciples.Engine.Base
{
    /// <summary>
    /// Интерфейс для обозначения тех объектов, которые поддерживают инициализацию с одним параметром.
    /// </summary>
    public interface ISupportLoading<in TData> : ISupportUnloading
    {
        /// <summary>
        /// Инициализировать объект.
        /// </summary>
        void Load(TData data);
    }
}