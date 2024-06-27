using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Disciples.Engine.Common.Providers;
using Disciples.Resources.Common.Exceptions;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc />
internal class SoundProvider : ISoundProvider
{
    private const string SOUNDS_FOLDER = "Resources//Music";

    /// <summary>
    /// Создать объект типа <see cref="SoundProvider" />.
    /// </summary>
    public SoundProvider()
    {
        var sounds = Directory
            .GetFiles(SOUNDS_FOLDER, "*.wav")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Select(f => f!)
            .ToList();
        MenuSounds = GetSounds(sounds, @"^menutrck\d*\.wav$", "Menu music haven't found");
        BattleSounds = GetSounds(sounds, @"^battle\d*\.wav$", "Battle music haven't found");
    }

    /// <inheritdoc />
    public IReadOnlyList<string> MenuSounds { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> BattleSounds { get; }

    /// <summary>
    /// Получить список музыки.
    /// </summary>
    private static IReadOnlyList<string> GetSounds(IReadOnlyList<string> allSounds, string regex, string notFoundErrorMessage)
    {
        var sounds = allSounds
            .Where(s => Regex.IsMatch(s, regex))
            .ToList();
        if (sounds.Count == 0)
            throw new ResourceException(notFoundErrorMessage);

        return sounds;
    }
}