using Disciples.Engine.Base;

namespace Disciples.Engine.Implementation.Base
{
    /// <summary>
    /// Базовый класс для объектов, которые поддерживают инициализацию без параметров.
    /// </summary>
    public abstract class BaseSupportLoading : BaseSupportUnloading, ISupportLoading
    {
        /// <inheritdoc />
        public void Load()
        {
            Load(LoadInternal);
        }

        protected abstract void LoadInternal();
    }
}