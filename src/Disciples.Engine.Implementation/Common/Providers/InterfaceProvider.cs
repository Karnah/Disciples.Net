using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Disciples.Common.Models;
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
    public AnimationFrames GetAnimation(string animationName)
    {
        var frames = _interfaceImagesExtractor.GetAnimationFrames(animationName);
        if (frames == null)
            throw new ArgumentException($"Не найдена анимация {animationName}", nameof(animationName));

        return _bitmapFactory.ConvertToFrames(frames);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Для цветов используется схема BGRA.
    /// TODO Завести новый объект сцены, чтобы не выделять дополнительную память и не округлять?
    /// </remarks>
    public IBitmap GetColorBitmap(Color color, SizeD size)
    {
        var width = (int)Math.Round(size.Width);
        var height = (int)Math.Round(size.Height);
        var data = new byte[width * height * 4];
        var colorData = new[] { color.B, color.G, color.R, color.A };

        for (int i = 0; i < width * height; i++)
        {
            colorData.CopyTo(data, i * 4);
        }

        var rawBitmap = new RawBitmap
        {
            OriginalWidth = width,
            OriginalHeight = height,
            Bounds = new Rectangle(0, 0, width, height),
            Data = data
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
            ResourceSceneElementType.ToggleButton => GetToggleButtonSceneElement(resourceSceneElement),
            ResourceSceneElementType.TextListBox => GetTextListBoxSceneElement(resourceSceneElement),
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
            ButtonStates = GetButtonStates(button.ActiveStateImageName, button.HoverStateImageName, button.PressedStateImageName, button.DisabledStateImageName),
            ToolTip = GetElementText(button.ToolTipTextId),
            IsRepeat = button.IsRepeat,
            HotKeys = button.HotKeys.Select(k => (KeyboardButton)k).ToArray(),
        };
    }

    /// <summary>
    /// Получить Кнопка-переключатель из двух состояний.
    /// </summary>
    private ToggleButtonSceneElement GetToggleButtonSceneElement(ResourceSceneElement resourceSceneElement)
    {
        var toggleButton = (Disciples.Resources.Images.Models.ToggleButtonSceneElement)resourceSceneElement;
        return new ToggleButtonSceneElement
        {
            Name = toggleButton.Name,
            Position = toggleButton.Position,
            ButtonStates = GetButtonStates(toggleButton.ActiveStateImageName, toggleButton.HoverStateImageName, toggleButton.PressedStateImageName, toggleButton.DisabledStateImageName),
            CheckedButtonStates = GetButtonStates(toggleButton.CheckedActiveStateImageName, toggleButton.CheckedHoverStateImageName, toggleButton.CheckedPressedStateImageName, toggleButton.DisabledStateImageName),
            ToolTip = GetElementText(toggleButton.ToolTipTextId),
            HotKeys = toggleButton.HotKeys.Select(k => (KeyboardButton)k).ToArray(),
        };
    }

    /// <summary>
    /// Получить вид кнопки для каждого состояния.
    /// </summary>
    private ButtonStates? GetButtonStates(string? activeStateImageName, string? hoverStateImageName, string? pressedStateImageName, string? disabledStateImageName)
    {
        if (activeStateImageName == null ||
            hoverStateImageName == null ||
            pressedStateImageName == null ||
            disabledStateImageName == null)
        {
            return null;
        }

        return new ButtonStates(new Dictionary<SceneButtonState, IBitmap>
        {
            { SceneButtonState.Active, GetImage(activeStateImageName) },
            { SceneButtonState.Disabled, GetImage(disabledStateImageName) },
            { SceneButtonState.Hover, GetImage(hoverStateImageName) },
            { SceneButtonState.Pressed, GetImage(pressedStateImageName) }
        });
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
            // TODO Какая-то магия. Иногда для изображений вместо tooltip выводить просто имя изображения.
            // Например IMG_BIGFACESBG из DLG_BATTLE_A.
            // Возможно, одно и тоже имя может быть частью некоторых файлов, поэтому идёт уточнение.
            ToolTip = image.ToolTipTextId?.StartsWith("X") == true
                ? GetElementText(image.ToolTipTextId)
                : null,
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
    /// Получить список строк.
    /// </summary>
    private TextListBoxSceneElement GetTextListBoxSceneElement(ResourceSceneElement resourceSceneElement)
    {
        var textListBox = (Disciples.Resources.Images.Models.TextListBoxSceneElement)resourceSceneElement;
        return new TextListBoxSceneElement
        {
            Name = textListBox.Name,
            Position = textListBox.Position,
            ColumnCount = textListBox.ColumnCount,
            HorizontalSpacing = textListBox.HorizontalSpacing,
            VerticalSpacing = textListBox.VerticalSpacing,
            ScrollUpButtonName = textListBox.ScrollUpButtonName,
            ScrollDownButtonName = textListBox.ScrollDownButtonName,
            ScrollLeftButtonName = textListBox.ScrollLeftButtonName,
            ScrollRightButtonName = textListBox.ScrollRightButtonName,
            PageUpButtonName = textListBox.PageUpButtonName,
            PageDownButtonName = textListBox.PageDownButtonName,
            DoubleClickButtonName = textListBox.DoubleClickButtonName,
            SelectedTextStyle = GetElementTextStyle(textListBox.SelectedTextStyle),
            CommonTextStyle = GetElementTextStyle(textListBox.CommonTextStyle),
            SelectionImageName = GetElementImage(textListBox.SelectionImageName),
            UnselectedImageName = GetElementImage(textListBox.UnselectedImageName),
            BorderSize = textListBox.BorderSize,
            ToolTip = GetElementText(textListBox.ToolTipTextId),
            ShouldCreateBackgroundImage = textListBox.ShouldCreateBackgroundImage,
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