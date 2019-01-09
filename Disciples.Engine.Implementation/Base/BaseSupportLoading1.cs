using Disciples.Engine.Base;

namespace Disciples.Engine.Implementation.Base
{
    /// <summary>
    /// Базовый класс для объектов, которые поддерживают инициализацию с одним параметром.
    /// </summary>
    public abstract class BaseSupportLoading<TData> : BaseSupportUnloading, ISupportLoading<TData>
    {
        /// <inheritdoc />
        public void Load(TData data)
        {
            Load(() => LoadInternal(data));
        }

        protected abstract void LoadInternal(TData data);
    }
}