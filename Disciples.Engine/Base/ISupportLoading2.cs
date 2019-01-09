namespace Disciples.Engine.Base
{
    /// <summary>
    /// Интерфейс для обозначения тех объектов, которые поддерживают инициализацию с двумя параметрами.
    /// </summary>
    public interface ISupportLoading<in TData1, in TData2> : ISupportUnloading
    {
        /// <summary>
        /// Инициализировать объект.
        /// </summary>
        void Load(TData1 data1, TData2 data2);
    }
}