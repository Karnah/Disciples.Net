namespace Disciples.Engine.Base
{
    /// <summary>
    /// Менеджер для журнала.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Добавить сообщение в журнал.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        void Log(string message);
    }
}