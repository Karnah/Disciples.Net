namespace Disciples.Engine.Base
{
    /// <summary>
    /// Интерфейс для обозначения тех объектов, которые поддерживают инициализацию без параметров.
    /// </summary>
    public interface ISupportLoading : ISupportUnloading
    {
        /// <summary>
        /// Инициализировать объект.
        /// </summary>
        void Load();
    }
}