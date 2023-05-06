using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Images.Models;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc cref="IInterfaceProvider" />
public class InterfaceProvider : BaseSupportLoading, IInterfaceProvider
{
    private readonly InterfaceImagesExtractor _interfaceImagesExtractor;
    private readonly IBitmapFactory _bitmapFactory;
    private readonly Dictionary<GameColor, IBitmap> _gameColors;

    /// <inheritdoc />
    public InterfaceProvider(InterfaceImagesExtractor interfaceImagesExtractor, IBitmapFactory bitmapFactory)
    {
        _interfaceImagesExtractor = interfaceImagesExtractor;
        _bitmapFactory = bitmapFactory;
        _gameColors = GetGameColors();
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => true;

    /// <inheritdoc />
    public IBitmap GetImage(string imageName)
    {
        return _bitmapFactory.FromRawToBitmap(_interfaceImagesExtractor.GetImage(imageName));
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IBitmap> GetImageParts(string imageName)
    {
        var imageParts = _interfaceImagesExtractor.GetImageParts(imageName);

        return imageParts
            .Select(ip => new KeyValuePair<string, IBitmap>(ip.Key, _bitmapFactory.FromRawToBitmap(ip.Value)))
            .ToDictionary(ip => ip.Key, ip => ip.Value);
    }

    /// <inheritdoc />
    public IReadOnlyList<Frame> GetAnimation(string animationName)
    {
        var frames = _interfaceImagesExtractor.GetAnimationFrames(animationName);
        if (frames == null)
            throw new ArgumentException($"Не найдена анимация {animationName}", nameof(animationName));

        return _bitmapFactory.ConvertToFrames(frames);
    }

    /// <inheritdoc />
    public IBitmap GetColorBitmap(GameColor color)
    {
        return _gameColors[color];
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _interfaceImagesExtractor.Load();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Получить картинки цветов.
    /// </summary>
    /// <remarks>
    /// Для цветов используется схема BGRA.
    /// </remarks>
    private Dictionary<GameColor, IBitmap> GetGameColors()
    {
        var gameColors = new Dictionary<GameColor, IBitmap>();

        foreach (GameColor color in Enum.GetValues(typeof(GameColor)))
        {
            var colorBytes = color switch
            {
                GameColor.Red => new byte[] { 0, 0, 255, 128 },
                GameColor.Gray => new byte[] { 0, 66, 66, 128 },
                GameColor.Green => new byte[] { 0, 33, 33, 128 },
                GameColor.Yellow => new byte[] { 0, 255, 255, 128 },
                GameColor.Blue => new byte[] { 255, 0, 0, 128 },
                GameColor.Black => new byte[] { 0, 0, 0, 255 },
                GameColor.White => new byte[] { 255, 255, 255, 255 },
                GameColor.Orange => new byte[] { 0, 102, 255, 128 },
                GameColor.Paralyze => new byte[] { 255, 255, 255, 64 },
                _ => throw new ArgumentOutOfRangeException()
            };

            var rawBitmap = new RawBitmap
            {
                OriginalWidth = 1,
                OriginalHeight = 1,
                Bounds = new Bounds(1, 1),
                Data = colorBytes
            };
            var bitmap = _bitmapFactory.FromRawToBitmap(rawBitmap);
            gameColors.Add(color, bitmap);
        }

        return gameColors;
    }
}