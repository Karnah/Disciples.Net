using System.Collections.Generic;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Провайдер для звуков.
/// </summary>
public interface ISoundProvider
{
    /// <summary>
    /// Музыка, которая воспроизводится в главном меню.
    /// </summary>
    string MenuSound { get; }

    /// <summary>
    /// Получить фоновые звуки битвы.
    /// </summary>
    IReadOnlyList<string> GetBackgroundBattleSounds();
}