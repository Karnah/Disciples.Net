using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Models;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <summary>
/// Заглушка для контроллера звуков.
/// </summary>
internal class NullSoundController : ISoundController
{
    /// <inheritdoc />
    public IPlayingSound PlayBackground(string name)
    {
        return new NullPlayingSound();
    }

    /// <inheritdoc />
    public IPlayingSound PlaySound(RawSound sound)
    {
        return new NullPlayingSound();
    }
}