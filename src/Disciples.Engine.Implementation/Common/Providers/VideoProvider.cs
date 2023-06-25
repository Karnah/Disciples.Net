using System.Collections.Generic;
using System.IO;
using Disciples.Engine.Common.Providers;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc />
internal class VideoProvider : IVideoProvider
{
    private const string VIDEO_PATH = "Resources\\Video";

    /// <inheritdoc />
    public IReadOnlyList<string> IntroVideoPaths { get; } = new[]
    {
        GetPath("sf.bik"),
        GetPath("intro.bik"),
    };

    /// <summary>
    /// Получить путь до видеоролика.
    /// </summary>
    private static string GetPath(string fileName)
    {
        return Path.Combine(VIDEO_PATH, fileName);
    }
}