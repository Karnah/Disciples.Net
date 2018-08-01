using System.Collections.Generic;

using Engine.Components;

namespace Engine
{
    public class GameObject
    {
        public GameObject()
        {
        }

        public IReadOnlyCollection<IComponent> Components { get; set; }

        public bool IsDestroyed { get; private set; }


        public void OnInitialize()
        {
            foreach (var component in Components) {
                component.OnInitialize();
            }
        }

        public virtual void OnUpdate(long tickCount)
        {
            foreach (var component in Components) {
                component.OnUpdate(tickCount);
            }
        }

        public void Destroy()
        {
            foreach (var component in Components) {
                component.Destroy();
            }

            IsDestroyed = true;
        }
    }
}
