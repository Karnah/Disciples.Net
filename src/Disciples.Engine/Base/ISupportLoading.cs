namespace Disciples.Engine.Base;

/// <summary>
/// Интерфейс для обозначения тех объектов, которые поддерживают загрузку и очистку ресурсов.
/// </summary>
public interface ISupportLoading
{
    /// <summary>
    /// Был ли инициализирован объект.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Инициализировать объект.
    /// </summary>
    void Load();

    /// <summary>
    /// Очистить занимаемые объектом ресурсы.
    /// </summary>
    void Unload();
}