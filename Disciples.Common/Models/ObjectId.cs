namespace Disciples.Common.Models;

/// <summary>
/// Уникальный идентификатор объекта.
/// </summary>
/// <remarks>
/// TODO Идентификатор объекта где-то хранится в виде строки, где-то в виде числа.
/// Нужно перевести всё на единый стиль. Лучше, если это будет именно число.
/// Больше информации про идентификатор:
/// https://github.com/VladimirMakeev/D2RSG/blob/master/ScenarioGenerator/src/rsgid.cpp
/// </remarks>
public class ObjectId
{
    private readonly int _id;

    /// <summary>
    /// Создать объект типа <see cref="ObjectId" />.
    /// </summary>
    public ObjectId(int id)
    {
        _id = id;
    }

    /// <summary>
    /// Получить текстовый идентификатор объекта.
    /// </summary>
    public string GetStringId()
    {
        var category = "G";
        var categoryIndex = "000";
        var type = "UU";
        var typeIndex = _id & 0xffff;

        return $"{category}{categoryIndex}{type}{typeIndex:X4}";
    }
}