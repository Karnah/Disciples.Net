﻿using System;

using Disciples.Engine.Common.Exceptions;
using Disciples.Engine.Common.GameObjects;

namespace Disciples.Engine.Common.Components
{
    public abstract class BaseComponent : IComponent
    {
        protected readonly GameObject GameObject;

        protected BaseComponent(GameObject gameObject)
        {
            GameObject = gameObject;
        }


        public virtual void OnInitialize()
        {
        }

        public virtual void OnUpdate(long tickCount)
        {
        }

        public virtual void Destroy()
        {
        }


        protected TComponent GetComponent<TComponent>()
            where TComponent : IComponent
        {
            return (TComponent)GetComponent(typeof(TComponent));
        }

        protected object GetComponent(Type componentType)
        {
            foreach (var component in GameObject.Components) {
                if (component.GetType() == componentType)
                    return component;
            }

            throw new ComponentNotFoundException(componentType);
        }
    }
}