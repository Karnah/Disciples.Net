using System;
using System.Collections.Generic;
using Disciples.Engine.Common.Components;

namespace Disciples.Engine.Common.GameObjects
{
    /// <summary>
    /// Объект, который располагается на сцене.
    /// </summary>
    public abstract class GameObject
    {
        protected GameObject()
        {
            Components = Array.Empty<IComponent>();
        }

        protected GameObject(double x, double y) : this()
        {
            X = x;
            Y = y;
        }

        protected GameObject((double X, double Y) position) : this(position.X, position.Y)
        {
        }


        /// <summary>
        /// Положение объекта на сцене, координата X.
        /// </summary>
        /// <remarks>Отсчёт идёт справа налево.</remarks>
        public double X { get; protected set; }

        /// <summary>
        /// Положение объекта на сцене, координата Y.
        /// </summary>
        /// <remarks>Отсчёт идет сверху вниз.</remarks>
        public double Y { get; protected set; }

        /// <summary>
        /// Ширина объекта на сцене.
        /// </summary>
        public double Width { get; protected set; }

        /// <summary>
        /// Высота объекта на сцене.
        /// </summary>
        public double Height { get; protected set; }

        /// <summary>
        /// Возможно ли наведение/выбор объекта курсором.
        /// </summary>
        public abstract bool IsInteractive { get; }


        /// <summary>
        /// Компоненты из которых состоит объект.
        /// </summary>
        public IReadOnlyCollection<IComponent> Components { get; protected set; }

        /// <summary>
        /// Был ли объект инициализирован.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Был ли объект удалён со сцены.
        /// </summary>
        public bool IsDestroyed { get; private set; }


        /// <summary>
        /// Инициализировать игровой объект.
        /// </summary>
        public virtual void Initialize()
        {
            if (IsInitialized)
                throw new InvalidOperationException("Game object already initialized");

            foreach (var component in Components) {
                component.Initialize();
            }

            IsInitialized = true;
        }

        /// <summary>
        /// Обработать событие обновление объекта.
        /// </summary>
        /// <param name="ticksCount">Количество тиков, которое прошло со времени предыдущего обновления.</param>
        public virtual void Update(long ticksCount)
        {
            foreach (var component in Components) {
                component.Update(ticksCount);
            }
        }

        /// <summary>
        /// Уничтожить объект.
        /// </summary>
        public virtual void Destroy()
        {
            if (IsDestroyed)
                throw new ObjectDisposedException("Game object already destroyed");

            foreach (var component in Components) {
                component.Destroy();
            }

            IsDestroyed = true;
        }
    }
}
