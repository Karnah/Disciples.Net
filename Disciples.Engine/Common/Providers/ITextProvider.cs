using Disciples.Engine.Base;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Поставщик текста.
/// </summary>
public interface ITextProvider : ISupportLoading
{
    /// <summary>
    /// Получить текст по идентификатору.
    /// </summary>
    string GetText(string textId);
}