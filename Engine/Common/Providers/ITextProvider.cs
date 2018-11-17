namespace Engine.Common.Providers
{
    /// <summary>
    /// Поставщик текста.
    /// </summary>
    public interface ITextProvider
    {
        /// <summary>
        /// Получить текст по идентификатору.
        /// </summary>
        string GetText(string textId);
    }
}