using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;

namespace Disciples.Engine.Common.Controllers;

/// <summary>
/// Контроллер музыки и звуков.
/// </summary>
public interface ISoundController
{
    /// <summary>
    /// Играть фоновую музыку.
    /// </summary>
    IPlayingSound PlayBackground(string name);

    /// <summary>
    /// Проиграть звук.
    /// </summary>
    IPlayingSound PlaySound(RawSound sound);
}