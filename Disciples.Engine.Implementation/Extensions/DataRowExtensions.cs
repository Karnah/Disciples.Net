using System;
using System.Data;

namespace Disciples.Engine.Implementation.Extensions;

/// <summary>
/// Методы расширения для работы со строкой таблицы.
/// </summary>
/// <remarks>
/// TODO Разобраться с null.
/// </remarks>
public static class DataRowExtensions
{
    /// <summary>
    /// Извлечь значение из строки и превратить объект указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип.</typeparam>
    /// <param name="dataRow">Строка, из которой необходимо извлечь данные.</param>
    /// <param name="columnName">Имя колонки в строке.</param>
    public static T GetClass<T>(this DataRow dataRow, string columnName)
        where T : class
    {
        var value = dataRow[columnName];
        if (value == DBNull.Value)
            return null;

        return (T) value;
    }

    /// <summary>
    /// Извлечь значение из строки и превратить объект указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип.</typeparam>
    /// <param name="dataRow">Строка, из которой необходимо извлечь данные.</param>
    /// <param name="columnName">Имя колонки в строке.</param>
    public static T? GetStruct<T>(this DataRow dataRow, string columnName)
        where T : struct
    {
        var value = dataRow[columnName];
        if (value == null || value == DBNull.Value)
            return null;

        if (value is T)
            return (T) value;

        if (value is IConvertible)
            return (T)Convert.ChangeType(value, typeof(T));

        return null;
    }
}