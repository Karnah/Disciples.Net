using System.Collections.Generic;

using Inftastructure.Components;

namespace Inftastructure
{
    public class GameObject
    {
        public GameObject()
        {
        }

        public IReadOnlyCollection<IComponent> Components { get; set; }


        public void OnInitialize()
        {
            foreach (var component in Components) {
                component.OnInitialize();
            }
        }
    }
}
