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
    IReadOnlyList<string> MenuSounds { get; }

    /// <summary>
    /// Фоновые звуки битвы.
    /// </summary>
    IReadOnlyList<string> BattleSounds { get; }
}