using System;

using Engine.Exceptions;

namespace Engine.Components
{
    public abstract class Component : IComponent
    {
        private readonly GameObject _gameObject;

        protected Component(GameObject gameObject)
        {
            _gameObject = gameObject;
        }


        public virtual void OnInitialize()
        {
        }

        public virtual void OnUpdate(long tickCount)
        {
        }


        protected TComponent GetComponent<TComponent>()
            where TComponent : IComponent
        {
            return (TComponent)GetComponent(typeof(TComponent));
        }

        protected object GetComponent(Type componentType)
        {
            foreach (var component in _gameObject.Components) {
                if (component.GetType() == componentType)
                    return component;
            }

            throw new ComponentNotFoundException(componentType);
        }
    }
}
