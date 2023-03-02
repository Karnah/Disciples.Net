using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Platform.Factories;
using Disciples.ResourceProvider;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc cref="IInterfaceProvider" />
public class InterfaceProvider : BaseSupportLoading, IInterfaceProvider
{
    private readonly IBitmapFactory _bitmapFactory;

    private ImagesExtractor _extractor;
    private Dictionary<GameColor, IBitmap> _gameColors;

    /// <inheritdoc />
    public InterfaceProvider(IBitmapFactory bitmapFactory)
    {
        _bitmapFactory = bitmapFactory;
    }


    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => true;


    /// <inheritdoc />
    public IBitmap GetImage(string imageName)
    {
        return _bitmapFactory.FromRawToBitmap(_extractor.GetImage(imageName));
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IBitmap> GetImageParts(string imageName)
    {
        var imageParts = _extractor.GetImageParts(imageName);

        return imageParts
            .Select(ip => new KeyValuePair<string, IBitmap>(ip.Key, _bitmapFactory.FromRawToBitmap(ip.Value)))
            .ToDictionary(ip => ip.Key, ip => ip.Value);
    }

    /// <inheritdoc />
    public IBitmap GetColorBitmap(GameColor color)
    {
        return _gameColors[color];
    }


    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\interf\\Interf.ff");

        InitGameColors();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        _extractor = null;
        _gameColors = null;
    }

    /// <summary>
    /// Инициализировать цвета приложения.
    /// </summary>
    private void InitGameColors()
    {
        var gameColors = new Dictionary<GameColor, IBitmap>();

        foreach (GameColor color in Enum.GetValues(typeof(GameColor))) {
            var colorFilePath = $"Resources/Colors/{color}.png";
            if (!File.Exists(colorFilePath)) {
                gameColors.Add(color, null);
                continue;
            }

            var bitmap = _bitmapFactory.FromFile(colorFilePath);
            gameColors.Add(color, bitmap);
        }

        _gameColors = gameColors;
    }

    // todo Так как наблюдаются проблемы со Skia, то генерировать во время выполнения так цвета - не вариант.
    // Используем вариант с загрузкой.
    //private void InitGameColors()
    //{
    //    var gameColors = new Dictionary<GameColor, IBitmap>();

    //    foreach (GameColor color in Enum.GetValues(typeof(GameColor))) {
    //        byte[] colorBytes = new byte[4];

    //        switch (color) {
    //            case GameColor.Red:
    //                colorBytes = new byte[] { 255, 0, 0, 128 };
    //                break;
    //            case GameColor.Gray:
    //                break;
    //            case GameColor.Green:
    //                break;
    //            case GameColor.Yellow:
    //                colorBytes = new byte[] { 255, 255, 0, 128 };
    //                break;
    //            case GameColor.Blue:
    //                colorBytes = new byte[] { 0, 0, 255, 128 };
    //                break;
    //            case GameColor.Black:
    //                colorBytes = new byte[] { 0, 0, 0, 255 };
    //                break;
    //            case GameColor.White:
    //                colorBytes = new byte[] { 255, 255, 255, 255 };
    //                break;
    //            default:
    //                throw new ArgumentOutOfRangeException();
    //        }

    //        var rawBitmap = new RawBitmap(0, 1, 0, 1, 1, 1, colorBytes);
    //        var bitmap = _bitmapFactory.FromRawToBitmap(rawBitmap);
    //        gameColors.Add(color, bitmap);

    //        //_bitmapFactory.SaveToFile(bitmap, $"Resources/Colors/{color}.png");
    //    }

    //    _gameColors = gameColors;
    //}
}