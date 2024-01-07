namespace Disciples.Engine.Common.Components;

/// <summary>
/// Представляет базовый интерфейс для всех компонентов игровых объектов
/// </summary>
public interface IComponent
{
    /// <summary>
    /// Инициализация компонента
    /// </summary>
    void Initialize();

    /// <summary>
    /// Обновить состояние компонента
    /// </summary>
    /// <param name="tickCount">Количество миллисекунд, которое прошло с предыдущего вызова</param>
    void Update(long tickCount);

    /// <summary>
    /// Очистить ресурсы, используемые компонентом
    /// </summary>
    void Destroy();
}