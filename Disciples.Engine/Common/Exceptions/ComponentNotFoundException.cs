using System;

namespace Disciples.Engine.Common.Exceptions
{
    public class ComponentNotFoundException : ApplicationException
    {
        public ComponentNotFoundException(Type componentType) : base($"Component type {componentType} not found")
        {
        }
    }
}
