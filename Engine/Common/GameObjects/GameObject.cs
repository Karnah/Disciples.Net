using System.Collections.Generic;

using Engine.Common.Components;

namespace Engine.Common.GameObjects
{
    public class GameObject
    {
        public GameObject()
        {
            Components = new IComponent[0];
        }

        public GameObject(double x, double y) : this()
        {
            X = x;
            Y = y;
        }

        public GameObject((double X, double Y) position) : this(position.X, position.Y)
        {
        }


        public double X { get; protected set; }

        public double Y { get; protected set; }


        public IReadOnlyCollection<IComponent> Components { get; protected set; }

        public bool IsDestroyed { get; private set; }


        public virtual void OnInitialize()
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

        public virtual void Destroy()
        {
            foreach (var component in Components) {
                component.Destroy();
            }

            IsDestroyed = true;
        }
    }
}
