namespace Disciples.Engine.Common.Controllers;

/// <summary>
/// Контроллер музыки и звуков.
/// </summary>
public interface IAudioController
{
    /// <summary>
    /// Играть зацикленную фоновую музыку.
    /// </summary>
    void PlayBackground(string name);

    /// <summary>
    /// Проиграть звук.
    /// </summary>
    void PlaySound(string name);
}