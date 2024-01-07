using Disciples.Engine.Base;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Поставщик текста.
/// </summary>
public interface ITextProvider : ISupportLoading
{
    /// <summary>
    /// Получить текст по идентификатору.
    /// </summary>
    TextContainer GetText(string textId);
}