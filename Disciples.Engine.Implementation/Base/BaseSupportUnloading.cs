using System;
using Disciples.Engine.Base;

namespace Disciples.Engine.Implementation.Base
{
    /// <summary>
    /// Базовый класс для объектов, поддерживающих инициализацию и деинициализацию.
    /// </summary>
    public abstract class BaseSupportUnloading : ISupportUnloading
    {
        /// <inheritdoc />
        public bool IsLoaded { get; private set; }

        /// <inheritdoc />
        public abstract bool OneTimeLoading { get; }


        /// <inheritdoc />
        public void Unload()
        {
            Unload(UnloadInternal);
        }

        /// <summary>
        /// Деинициализировать объект.
        /// </summary>
        protected abstract void UnloadInternal();

        /// <summary>
        /// Инициализировать объект.
        /// </summary>
        /// <param name="loadAction">Метод для инициализации.</param>
        protected void Load(Action loadAction)
        {
            if (IsLoaded)
                return;

            loadAction?.Invoke();
            IsLoaded = true;
        }

        /// <summary>
        /// Деинициализировать объект.
        /// </summary>
        /// <param name="unloadAction">Метод для инициализации.</param>
        private void Unload(Action unloadAction)
        {
            if (!IsLoaded)
                return;

            unloadAction?.Invoke();
            IsLoaded = false;
        }
    }
}