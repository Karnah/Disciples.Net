namespace Engine.Common.Components
{
    /// <summary>
    /// Представляет базовый интерфейс для всех компонентов игровых объектов
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        void OnInitialize();

        /// <summary>
        /// Обновить состояние компонента
        /// </summary>
        /// <param name="tickCount">Количество милисекунд, которое прошло с предыдущего вызова</param>
        void OnUpdate(long tickCount);

        /// <summary>
        /// Очистить ресурсы, используемые компонентом
        /// </summary>
        void Destroy();
    }
}