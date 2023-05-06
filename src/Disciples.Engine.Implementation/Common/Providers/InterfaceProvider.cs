using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

    /// <inheritdoc />
    public InterfaceProvider(InterfaceImagesExtractor interfaceImagesExtractor, IBitmapFactory bitmapFactory)
    {
        _interfaceImagesExtractor = interfaceImagesExtractor;
        _bitmapFactory = bitmapFactory;
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
    /// <remarks>
    /// Для цветов используется схема BGRA.
    /// </remarks>
    public IBitmap GetColorBitmap(Color color)
    {
        var rawBitmap = new RawBitmap
        {
            OriginalWidth = 1,
            OriginalHeight = 1,
            Bounds = new Bounds(1, 1),
            Data = new[] { color.B, color.G, color.R, color.A }
        };
        return _bitmapFactory.FromRawToBitmap(rawBitmap);
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
}