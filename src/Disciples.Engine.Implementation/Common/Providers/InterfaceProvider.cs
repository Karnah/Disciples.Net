using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Models;
using Disciples.Engine.Platform.Factories;
using ResourceSceneElementType = Disciples.Resources.Images.Enums.SceneElementType;
using ResourceSceneElement = Disciples.Resources.Images.Models.SceneElement;
using RawBitmap = Disciples.Resources.Images.Models.RawBitmap;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc cref="IInterfaceProvider" />
public class InterfaceProvider : BaseSupportLoading, IInterfaceProvider
{
    private readonly InterfaceImagesExtractor _interfaceImagesExtractor;
    private readonly IBitmapFactory _bitmapFactory;
    private readonly SceneInterfaceExtractor _sceneInterfaceExtractor;
    private readonly ITextProvider _textProvider;

    /// <inheritdoc />
    public InterfaceProvider(
        InterfaceImagesExtractor interfaceImagesExtractor,
        IBitmapFactory bitmapFactory,
        SceneInterfaceExtractor sceneInterfaceExtractor,
        ITextProvider textProvider)
    {
        _interfaceImagesExtractor = interfaceImagesExtractor;
        _bitmapFactory = bitmapFactory;
        _sceneInterfaceExtractor = sceneInterfaceExtractor;
        _textProvider = textProvider;
    }

    /// <inheritdoc />
    public SceneInterface GetSceneInterface(string name)
    {
        var resourceSceneInterface = _sceneInterfaceExtractor.GetSceneInterface(name);
        return new SceneInterface
        {
            Name = resourceSceneInterface.Name,
            Bounds = resourceSceneInterface.Bounds,
            Background = GetElementImage(resourceSceneInterface.BackgroundImageName),
            CursorType = GetCursorType(resourceSceneInterface.CursorImageName),
            Position = resourceSceneInterface.Position,
            Elements = resourceSceneInterface.Elements.Select(GetSceneElement).ToDictionary(e => e.Name, e => e)
        };
    }

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
            Bounds = new Rectangle(0, 0, 1, 1),
            Data = new[] { color.B, color.G, color.R, color.A }
        };
        return _bitmapFactory.FromRawToBitmap(rawBitmap);
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _interfaceImagesExtractor.Load();
        _sceneInterfaceExtractor.Load();
        _textProvider.Load();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Получить тип курсора.
    /// </summary>
    /// <remarks>
    /// TODO Реализовать.
    /// </remarks>
    private CursorType GetCursorType(string? cursorImageName)
    {
        return CursorType.Default;
    }

    /// <summary>
    /// Получить элемент сцены.
    /// </summary>
    private SceneElement GetSceneElement(ResourceSceneElement resourceSceneElement)
    {
        return resourceSceneElement.Type switch
        {
            ResourceSceneElementType.Image => GetImageSceneElement(resourceSceneElement),
            ResourceSceneElementType.Button => GetButtonSceneElement(resourceSceneElement),
            ResourceSceneElementType.TextBlock => GetTextBlockSceneElement(resourceSceneElement),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Получить кнопку.
    /// </summary>
    private ButtonSceneElement GetButtonSceneElement(ResourceSceneElement resourceSceneElement)
    {
        var button = (Disciples.Resources.Images.Models.ButtonSceneElement)resourceSceneElement;
        return new ButtonSceneElement
        {
            Name = button.Name,
            Position = button.Position,
            ButtonStates = GetButtonStates(button.ActiveStateImageName, button.SelectedStateImageName, button.PressedStateImageName, button.DisabledStateImageName),
            ToolTip = GetElementText(button.ToolTipTextId),
            IsRepeat = button.IsRepeat,
            HotKeys = button.HotKeys.Select(k => (KeyboardButton)k).ToArray(),
        };
    }

    /// <summary>
    /// Получить вид кнопки для каждого состояния.
    /// </summary>
    private IReadOnlyDictionary<SceneButtonState, IBitmap>? GetButtonStates(string? activeStateImageName, string? selectedStateImageName, string? pressedStateImageName, string? disabledStateImageName)
    {
        // TODO Есть невидимые кнопки, которые, тем не менее, имеют горячие клавиши.
        // Подумать, как их корректнее обрабатывать.
        if (string.IsNullOrEmpty(activeStateImageName)
            || string.IsNullOrEmpty(selectedStateImageName)
            || string.IsNullOrEmpty(pressedStateImageName)
            || string.IsNullOrEmpty(disabledStateImageName))
        {
            return null;
        }

        return new Dictionary<SceneButtonState, IBitmap>
        {
            { SceneButtonState.Active, GetImage(activeStateImageName) },
            { SceneButtonState.Disabled, GetImage(disabledStateImageName) },
            { SceneButtonState.Selected, GetImage(selectedStateImageName) },
            { SceneButtonState.Pressed, GetImage(pressedStateImageName) }
        };
    }

    /// <summary>
    /// Получить изображение.
    /// </summary>
    private SceneElement GetImageSceneElement(ResourceSceneElement resourceSceneElement)
    {
        var image = (Disciples.Resources.Images.Models.ImageSceneElement)resourceSceneElement;

        // Если изображение является анимацией, то обрабатываем как анимацию.
        var animationFrames = image.ImageName == null
            ? null
            : _interfaceImagesExtractor.GetAnimationFrames(image.ImageName);
        if (animationFrames != null)
        {
            return new AnimationSceneElement
            {
                Name = image.Name,
                Position = image.Position,
                Frames = _bitmapFactory.ConvertToFrames(animationFrames),
                ToolTip = GetElementText(image.ToolTipTextId)
            };
        }

        return new ImageSceneElement
        {
            Name = image.Name,
            Position = image.Position,
            ImageBitmap = GetElementImage(image.ImageName),
            ToolTip = GetElementText(image.ToolTipTextId),
        };
    }

    /// <summary>
    /// Получить текстовое поле.
    /// </summary>
    private TextBlockSceneElement GetTextBlockSceneElement(ResourceSceneElement resourceSceneElement)
    {
        var textBlock = (Disciples.Resources.Images.Models.TextBlockSceneElement)resourceSceneElement;
        return new TextBlockSceneElement
        {
            Name = textBlock.Name,
            Position = textBlock.Position,
            TextStyle = GetElementTextStyle(textBlock.TextStyle),
            Text = GetElementText(textBlock.TextId),
            ToolTip = GetElementText(textBlock.ToolTipTextId)
        };
    }

    /// <summary>
    /// Получить изображение элемента.
    /// </summary>
    private IBitmap? GetElementImage(string? imageName)
    {
        return imageName == null
            ? null
            : GetImage(imageName);
    }

    /// <summary>
    /// Получить текст элемента.
    /// </summary>
    private TextContainer? GetElementText(string? textId)
    {
        return textId == null
            ? null
            : _textProvider.GetText(textId);
    }

    /// <summary>
    /// Получить стиль текста.
    /// </summary>
    private static TextStyle? GetElementTextStyle(string? textStyle)
    {
        return textStyle == null
            ? null
            : new TextStyle(textStyle);
    }
}