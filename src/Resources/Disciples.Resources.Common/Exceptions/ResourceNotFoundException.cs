using System.Runtime.Serialization;

namespace Disciples.Resources.Common.Exceptions;

/// <summary>
/// Ошибка, что какой-то ресурс не был найден.
/// </summary>
public class ResourceNotFoundException : ResourceException
{
    /// <summary>
    /// Создать объект типа <see cref="ResourceNotFoundException" />.
    /// </summary>
    public ResourceNotFoundException()
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="ResourceNotFoundException" />.
    /// </summary>
    protected ResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="ResourceNotFoundException" />.
    /// </summary>
    public ResourceNotFoundException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="ResourceNotFoundException" />.
    /// </summary>
    public ResourceNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}