using System;
using Disciples.Engine.Common.Exceptions;
using Disciples.Engine.Common.GameObjects;

namespace Disciples.Engine.Common.Components
{
    /// <summary>
    /// Базовый класс для компонентов игровых объектов.
    /// </summary>
    public abstract class BaseComponent : IComponent
    {
        /// <summary>
        /// Создать объект типа <see cref="BaseComponent" />.
        /// </summary>
        protected BaseComponent(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        /// <summary>
        /// Игровой объект, который содержит данный компонент.
        /// </summary>
        public GameObject GameObject { get; }

        /// <inheritdoc />
        public virtual void Initialize()
        {
        }

        /// <inheritdoc />
        public virtual void Update(long tickCount)
        {
        }

        /// <inheritdoc />
        public virtual void Destroy()
        {
        }

        /// <summary>
        /// Получить компонент указанного типа.
        /// </summary>
        /// <typeparam name="TComponent">Тип искомого компонента.</typeparam>
        protected TComponent GetComponent<TComponent>()
            where TComponent : IComponent
        {
            return (TComponent)GetComponent(typeof(TComponent));
        }

        /// <summary>
        /// Получить компонент указанного типа.
        /// </summary>
        /// <param name="componentType">Тип искомого компонента.</param>
        protected object GetComponent(Type componentType)
        {
            foreach (var component in GameObject.Components)
            {
                if (component.GetType() == componentType)
                    return component;
            }

            throw new ComponentNotFoundException(componentType);
        }
    }
}