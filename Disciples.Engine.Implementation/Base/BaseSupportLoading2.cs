using Disciples.Engine.Base;

namespace Disciples.Engine.Implementation.Base
{
    /// <summary>
    /// Базовый класс для объектов, которые поддерживают инициализацию с двумя параметрами.
    /// </summary>
    public abstract class BaseSupportLoading<TData1, TData2> : BaseSupportUnloading, ISupportLoading<TData1, TData2>
    {
        /// <inheritdoc />
        public void Load(TData1 data1, TData2 data2)
        {
            Load(() => LoadInternal(data1, data2));
        }

        protected abstract void LoadInternal(TData1 data1, TData2 data);
    }
}