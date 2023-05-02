using System.Runtime.Serialization;

namespace Disciples.Resources.Common.Exceptions;

/// <summary>
/// Ошибка ресурса.
/// </summary>
public class ResourceException : Exception
{
    /// <summary>
    /// Создать объект типа <see cref="ResourceException" />.
    /// </summary>
    public ResourceException()
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="ResourceException" />.
    /// </summary>
    protected ResourceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="ResourceException" />.
    /// </summary>
    public ResourceException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="ResourceException" />.
    /// </summary>
    public ResourceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}