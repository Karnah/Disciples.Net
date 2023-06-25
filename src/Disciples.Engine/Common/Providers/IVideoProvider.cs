using System.Collections.Generic;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Провайдер для видео-файлов.
/// </summary>
public interface IVideoProvider
{
    /// <summary>
    /// Видеоролики при загрузке игры.
    /// </summary>
    IReadOnlyList<string> IntroVideoPaths { get; }
}