using System.Collections.Generic;
using Disciples.Engine.Common.Providers;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc />
/// TODO Файлы можно доставать поиском по маске. Переделать через инициализацию.
internal class SoundProvider : ISoundProvider
{
    /// <inheritdoc />
    public IReadOnlyList<string> GetBackgroundBattleSounds()
    {
        return new[]
        {
            "battle01.wav",
            "battle02.wav",
            "battle03.wav",
            "battle04.wav",
        };
    }
}