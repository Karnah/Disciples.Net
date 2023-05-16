using Disciples.Resources.Images.Models;

namespace Disciples.Resources.Images.Parsers;

/// <summary>
/// Парсер для элементов интерфейса.
/// </summary>
internal abstract class BaseSceneElementParser
{
    /// <summary>
    /// Имя типа элемента в ресурсах, который обрабатывает данный парсер.
    /// </summary>
    public abstract string ElementTypeName { get; }

    /// <summary>
    /// Распарсить элемент из строки.
    /// </summary>
    public abstract SceneElement Parse(string line);
}