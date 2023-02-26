using System;

namespace Disciples.Engine.Common.Exceptions
{
    /// <summary>
    /// Исключение о том, что компонент игрового объекта не найден.
    /// </summary>
    public class ComponentNotFoundException : ApplicationException
    {
        /// <inheritdoc />
        public ComponentNotFoundException(Type componentType) : base($"Component type {componentType} not found")
        {
        }
    }
}