using System;
using Newtonsoft.Json;

namespace Disciples.Engine.Implementation.Extensions;

/// <summary>
/// Методы для работы с сериализацией и десериализацией json.
/// </summary>
internal static class JsonExtensions
{
    /// <summary>
    /// Сериализовать объект в json.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    /// <param name="obj">Объект для сериализации.</param>
    public static string SerializeToJson<T>(this T obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    /// <summary>
    /// Десериализовать объект из json.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    /// <param name="json">Json.</param>
    /// <returns>Объект.</returns>
    /// <exception cref="Exception">Если не удалось десериализовать объект.</exception>
    public static T DeserializeFromJson<T>(this string json)
    {
        var result = JsonConvert.DeserializeObject<T>(json);
        if (result == null)
            throw new Exception($"Не удалось десериализовать: {json}");

        return result;
    }
}