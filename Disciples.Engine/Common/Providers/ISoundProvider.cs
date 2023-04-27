using System.Collections.Generic;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Провайдер для звуков.
/// </summary>
public interface ISoundProvider
{
    /// <summary>
    /// Получить фоновые звуки битвы.
    /// </summary>
    IReadOnlyList<string> GetBackgroundBattleSounds();
}