using System;
using System.IO;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Models;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;
using ManagedBass;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <summary>
/// Контроллер музыки и звуков на основе <see cref="ManagedBass" />.
/// </summary>
public class BassSoundController : ISoundController
{
    private const string BACKGROUND_SOUNDS_FOLDER = "Resources/Music";

    /// <summary>
    /// Создать объект типа <see cref="BassSoundController" />.
    /// </summary>
    public BassSoundController()
    {
        if (!Bass.Init())
            throw new Exception("Невозможно инициализировать менеджер музыки");
    }

    /// <summary>
    /// Начать проигрывание музыки в фоне.
    /// </summary>
    public IPlayingSound PlayBackground(string fileName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BACKGROUND_SOUNDS_FOLDER, fileName);
        var soundHandle = Bass.CreateStream(filePath);
        if (soundHandle == 0 || !Bass.ChannelPlay(soundHandle))
            throw new Exception($"Не удалось проиграть {fileName}");

        return new BassPlayingSound(soundHandle);
    }

    /// <summary>
    /// Проиграть звук.
    /// </summary>
    public IPlayingSound PlaySound(RawSound sound)
    {
        var soundHandle = Bass.CreateStream(sound.Data, 0, sound.Data.Length, BassFlags.Default);
        if (soundHandle == 0 || !Bass.ChannelPlay(soundHandle))
            throw new Exception($"Не удалось проиграть звук");

        return new BassPlayingSound(soundHandle);
    }
}