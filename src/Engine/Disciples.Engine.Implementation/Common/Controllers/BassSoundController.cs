using System;
using System.IO;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Models;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;
using ManagedBass;
using Microsoft.Extensions.Logging;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <summary>
/// Контроллер музыки и звуков на основе <see cref="ManagedBass" />.
/// </summary>
public class BassSoundController : ISoundController
{
    private const string BACKGROUND_SOUNDS_FOLDER = "Resources/Music";

    private readonly ILogger<BassSoundController> _logger;
    private readonly bool _isInitialized;

    /// <summary>
    /// Создать объект типа <see cref="BassSoundController" />.
    /// </summary>
    public BassSoundController(ILogger<BassSoundController> logger)
    {
        _logger = logger;

        if (!Bass.Init())
        {
            _logger.LogError("Bass was not initialized");
            return;
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Начать проигрывание музыки в фоне.
    /// </summary>
    public IPlayingSound PlayBackground(string fileName)
    {
        if (!_isInitialized)
            return new NullPlayingSound();

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), BACKGROUND_SOUNDS_FOLDER, fileName);
        var soundHandle = Bass.CreateStream(filePath);

        try
        {
            if (soundHandle == 0 || !Bass.ChannelPlay(soundHandle))
                throw new Exception($"Cannot play sound {fileName}");

            return new BassPlayingSound(soundHandle);
        }
        catch (Exception e)
        {
            if (soundHandle != 0)
                Bass.MusicFree(soundHandle);

            _logger.LogError(e, "Cannot play sound");
            return new NullPlayingSound();
        }
    }

    /// <summary>
    /// Проиграть звук.
    /// </summary>
    public IPlayingSound PlaySound(RawSound sound)
    {
        if (!_isInitialized)
            return new NullPlayingSound();

        var soundHandle = Bass.CreateStream(sound.Data, 0, sound.Data.Length, BassFlags.Default);

        try
        {
            if (soundHandle == 0 || !Bass.ChannelPlay(soundHandle))
                throw new Exception("Cannot play sound");

            return new BassPlayingSound(soundHandle);
        }
        catch (Exception e)
        {
            if (soundHandle != 0)
                Bass.MusicFree(soundHandle);

            _logger.LogError(e, "Cannot play sound");
            return new NullPlayingSound();
        }
    }
}