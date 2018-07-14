using System;

namespace Engine.Exceptions
{
    public class ComponentNotFoundException : ApplicationException
    {
        public ComponentNotFoundException(Type componentType) : base($"Component type {componentType} not found")
        {
        }
    }
}
