using System.Buffers;
using System.Drawing;
using Disciples.Resources.Common;
using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Common.Extensions;
using Disciples.Resources.Common.Models;
using Disciples.Resources.Images.Enums;
using Disciples.Resources.Images.Models;
using SkiaSharp;
using SubstreamSharp;
using File = Disciples.Resources.Common.Models.File;

namespace Disciples.Resources.Images;

/// <summary>
/// Класс для извлечения изображений из ресурсов (.ff).
/// </summary>
public class ImagesExtractor : BaseMqdbResourceExtractor
{
    /// <summary>
    /// Анимации имеют данный идентификатор фрейма.
    /// </summary>
    private const int ANIMATION_MQ_INDEX_ID = -1;

    private IReadOnlyDictionary<string, MqImage>? _mqImages;
    private IReadOnlyDictionary<string, MqAnimation>? _mqAnimations;
    private IReadOnlyDictionary<int, IReadOnlyList<MqImage>>? _baseMqImages;

    /// <summary>
    /// Создать объект типа <see cref="ImagesExtractor" />.
    /// </summary>
    /// <param name="path">Путь до ресурса с изображениями.</param>
    public ImagesExtractor(string path) : base(path)
    {
    }

    /// <inheritdoc />
    protected override int FilesRecordId => 2;

    /// <summary>
    /// Получить кадры анимации по её имени.
    /// </summary>
    public IReadOnlyCollection<RawBitmap>? TryGetAnimationFrames(string name)
    {
        if (_mqAnimations?.ContainsKey(name) != true)
            return null;

        return GetAnimationFramesInternal(_mqAnimations[name]);
    }

    /// <summary>
    /// Получить имя базового изображения для указанного.
    /// </summary>
    public string? TryGetBaseImageName(string name)
    {
        if (_mqImages?.TryGetValue(name, out var image) == true)
            return GetFile(image.FileId).Name;

        return null;
    }

    /// <summary>
    /// Получить все изображения, которые создаются из указанного базового.
    /// </summary>
    /// <param name="name">Имя базового изображения.</param>
    /// <returns>Список всех изображений-частей.</returns>
    /// <remarks>
    /// Элементы интерфейса располагаются на одной картинке.
    /// Данный метод позволяет получить их за один проход.
    /// </remarks>
    public IDictionary<string, RawBitmap> GetImageParts(string name)
    {
        var file = TryGetFile(name);
        if (file == null)
            throw new ResourceNotFoundException($"Не найдено изображение с именем {name}");

        if (_baseMqImages == null)
            throw new ResourceException("Ресурс не поддерживает части изображений");

        if (!_baseMqImages.TryGetValue(file.Id, out var images))
            throw new ResourceException($"Изображение {name} не состоит из частей");

        // Метаданные берём из первого изображения, так как они должны быть одинаковы в рамках базового.
        var metadata = images[0].Metadata;
        var baseImage = PrepareImage(file, metadata);
        return images.ToDictionary(
            image => image.Name,
            image => BuildImage(baseImage, image));
    }

    /// <summary>
    /// Получить изображение по его имени.
    /// </summary>
    public RawBitmap GetImage(string name, ImageProcessingAlgorithm imageProcessingAlgorithm = ImageProcessingAlgorithm.Default)
    {
        // Если информация об изображении есть в -IMAGES.OPT, значит необходимо будет собирать по частям.
        if (_mqImages?.TryGetValue(name, out var mqImage) == true)
        {
            var baseImage = PrepareImage(GetFile(mqImage.FileId), mqImage.Metadata);
            return BuildImage(baseImage, mqImage);
        }

        // Иначе мы ищем файл с таким именем. Его можно просто отдать целиком, предварительно избавившись от прозрачности.
        var imageFile = TryGetFile($"{name.ToUpper()}.PNG");
        if (imageFile == null)
            throw new ResourceNotFoundException($"Не найдено изображение с именем {name}");

        return PrepareImage(imageFile, null, imageProcessingAlgorithm);
    }

    /// <summary>
    /// Получить данные файла по его имени.
    /// </summary>
    public byte[] GetFileContent(string name)
    {
        return GetFileContent(GetFile(name.ToUpperInvariant()));
    }

    /// <summary>
    /// Получить имена всех изображений в ресурсе.
    /// </summary>
    public IReadOnlyList<string> GetAllFilesNames()
    {
        return FileNames;
    }

    #region LoadData

    /// <inheritdoc />
    protected override IReadOnlyList<File> LoadFiles(Stream stream, Record fileListRecord, IReadOnlyDictionary<int, Record> records)
    {
        var filesCount = stream.ReadInt();
        var files = new List<File>(filesCount);

        for (int i = 0; i < filesCount; ++i)
        {
            var fileName = stream.ReadString(256);
            var id = stream.ReadInt();

            var record = records[id];
            var file = new File(id, fileName, record.Size, record.Offset);
            files.Add(file);
        }

        return files;
    }

    /// <inheritdoc />
    protected override void LoadInternal(Stream stream)
    {
        var mqIndexes = LoadMqIndexes(stream);
        if (mqIndexes == null)
            return;

        var mqImages = LoadMqImages(stream, mqIndexes);
        if (mqImages == null)
            return;

        _mqAnimations = LoadMqAnimations(stream, mqIndexes, mqImages, out var animationFrameNames);

        // Все изображения делятся на две категории: обычные изображения и кадры анимации.
        // Ко вторым нужен доступ только через _mqAnimations, поэтому удаляем их из итогово словаря для оптимизации.
        _mqImages = animationFrameNames?.Count > 0
            ? mqImages
                .Values
                .Where(im => !animationFrameNames.Contains(im.Name))
                .ToDictionary(im => im.Name, im => im)
            : mqImages;
        _baseMqImages = _mqImages
            .Values
            .GroupBy(i => i.FileId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<MqImage>)g.ToArray());
    }

    /// <summary>
    /// Загрузить индексы. Индекс позволяет определить в каком файле (идентификатор) находится изображение (по имени).
    /// </summary>
    private IReadOnlyDictionary<string, MqIndex>? LoadMqIndexes(Stream stream)
    {
        var indexFile = TryGetFile("-INDEX.OPT");
        if (indexFile == null)
            return null;

        var mqIndices = new Dictionary<string, MqIndex>();
        stream.Seek(indexFile.Offset, SeekOrigin.Begin);

        var framesCount = stream.ReadInt();
        for (int frameIndex = 0; frameIndex < framesCount; ++frameIndex)
        {
            var id = stream.ReadInt();
            var name = stream.ReadString();
            var relatedOffset = stream.ReadInt();
            var size = stream.ReadInt();

            var mqIndex = new MqIndex(id, name, relatedOffset, size);
            // todo Возможно дублирование. WTF?
            mqIndices.TryAdd(mqIndex.Name, mqIndex);
        }

        return mqIndices;
    }

    /// <summary>
    /// Загрузить изображения.
    /// Изображения содержат информацию о том, как нужно разрезать базовую картинку, чтобы получить требуемую.
    /// </summary>
    private IReadOnlyDictionary<string, MqImage>? LoadMqImages(Stream stream, IReadOnlyDictionary<string, MqIndex> mqIndexes)
    {
        var imagesFile = TryGetFile("-IMAGES.OPT");
        if (imagesFile == null)
            return null;

        stream.Seek(imagesFile.Offset, SeekOrigin.Begin);

        const int paletteLength = 1024;
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(paletteLength);
        var paletteBuffer = memoryOwner.Memory[..paletteLength].Span;

        var mqImages = new Dictionary<string, MqImage>();
        var endOfFile = imagesFile.Offset + imagesFile.Size - 1;
        while (stream.Position < endOfFile)
        {
            // Читаем метаданные изображения.
            var transparentColorIndex = stream.ReadByte();
            var opacityAlgorithm = stream.ReadShort();
            var sizeX = stream.ReadInt();
            var sizeY = stream.ReadInt();

            // Палитра изображения.
            stream.Read(paletteBuffer);
            var palette = new List<Color>(paletteBuffer.Length / 4);
            for (int i = 0; i < paletteBuffer.Length; i += 4)
                palette.Add(Color.FromArgb(paletteBuffer[i + 3], paletteBuffer[i], paletteBuffer[i + 1], paletteBuffer[i + 2]));

            var metadata = new MqImageMetadata(transparentColorIndex, opacityAlgorithm, sizeX, sizeY, palette);

            // Читаем изображения.
            var framesCount = stream.ReadInt();
            for (int frameIndex = 0; frameIndex < framesCount; ++frameIndex)
            {
                var frameName = stream.ReadString();
                var piecesCount = stream.ReadInt();
                int width = stream.ReadInt(),
                    height = stream.ReadInt();

                // Кадр собирается из отдельных кусочков базового изображения.
                var pieces = new List<MqImagePart>();
                for (int pieceIndex = 0; pieceIndex < piecesCount; ++pieceIndex)
                {
                    int sourceX = stream.ReadInt(),
                        sourceY = stream.ReadInt(),
                        destX = stream.ReadInt(),
                        destY = stream.ReadInt(),
                        pieceWidth = stream.ReadInt(),
                        pieceHeight = stream.ReadInt();

                    var piece = new MqImagePart(sourceX, sourceY, destX, destY, pieceWidth, pieceHeight);
                    pieces.Add(piece);
                }

                var fileId = mqIndexes[frameName].Id;
                var mqImage = new MqImage(frameName, fileId, width, height, metadata, pieces);

                // Изображения могут встречаться несколько раз, но метаданные у них должны быть одинаковые.
                mqImages.TryAdd(mqImage.Name, mqImage);
            }
        }

        return mqImages;
    }

    /// <summary>
    /// Загрузить анимации. Анимации хранят информацию об изображениях из которых состоят.
    /// </summary>
    private IReadOnlyDictionary<string, MqAnimation>? LoadMqAnimations(
        Stream stream,
        IReadOnlyDictionary<string, MqIndex> mqIndexes,
        IReadOnlyDictionary<string, MqImage> mqImages,
        out HashSet<string>? animationFrameNames)
    {
        var animationsFile = TryGetFile("-ANIMS.OPT");
        if (animationsFile == null)
        {
            animationFrameNames = null;
            return null;
        }

        animationFrameNames = new HashSet<string>();
        var animationsInfo = new List<(int AnimationIndex, IReadOnlyCollection<MqImage> Frames)>();
        stream.Seek(animationsFile.Offset, SeekOrigin.Begin);

        int animNumber = 0;
        var endOfFile = animationsFile.Offset + animationsFile.Size - 1;
        while (stream.Position < endOfFile)
        {
            var framesCount = stream.ReadInt();
            var frames = new List<MqImage>(framesCount);
            for (int animIndex = 0; animIndex < framesCount; ++animIndex)
            {
                var frameName = stream.ReadString();
                var mqImage = mqImages[frameName];
                frames.Add(mqImage);
                animationFrameNames.Add(frameName);
            }

            animationsInfo.Add((animNumber, frames));
            ++animNumber;
        }

        // Анимации идут в одинаковом порядке в -INDEX.OPT и -ANIMS.OPT,
        // Поэтому имена извлекаем таким образом.
        var animationNames = mqIndexes
            .Values
            .Where(ind => ind.Id == ANIMATION_MQ_INDEX_ID)
            .Select(ind => ind.Name)
            .ToArray();
        var mqAnimations = new Dictionary<string, MqAnimation>();
        foreach (var (animationIndex, frames) in animationsInfo)
        {
            var animationName = animationNames[animationIndex];
            var mqAnimation = new MqAnimation(animationIndex, animationName, frames);
            mqAnimations.Add(animationName, mqAnimation);
        }

        return mqAnimations;
    }

    #endregion

    #region HelpMethods

    /// <summary>
    /// Получить кадры анимации.
    /// </summary>
    /// <param name="animation">Информация об анимации.</param>
    /// <returns>Коллекция кадров анимации.</returns>
    private IReadOnlyCollection<RawBitmap> GetAnimationFramesInternal(MqAnimation animation)
    {
        var result = new List<RawBitmap>(animation.Frames.Count);

        // Обычно анимация "нарезается" из одного базового изображения,
        // Однако это не всегда. Поэтому необходимо иметь возможность кэшировать несколько изображений.
        var baseImages = new Dictionary<int, RawBitmap>();

        // Нарезаем все кадры так, чтобы у них были одинаковые размеры.
        // Так удобнее работать.
        var animationBounds = GetAnimationBounds(animation.Frames);

        foreach (var frame in animation.Frames)
        {
            var fileId = frame.FileId;
            if (!baseImages.ContainsKey(frame.FileId))
                baseImages.Add(fileId, PrepareImage(GetFile(fileId), frame.Metadata));

            result.Add(BuildImage(baseImages[fileId], frame, animationBounds));
        }

        return result;
    }

    /// <summary>
    /// Создать новое изображение из частей базового.
    /// </summary>
    /// <param name="baseImage">Базовое изображение.</param>
    /// <param name="mqImage">Информация о новом изображении.</param>
    private static RawBitmap BuildImage(RawBitmap baseImage, MqImage mqImage)
    {
        var bounds = GetImageBounds(mqImage);
        return BuildImage(baseImage, mqImage, bounds);
    }

    /// <summary>
    /// Получить общие границы для анимации.
    /// </summary>
    private static Rectangle GetAnimationBounds(IReadOnlyCollection<MqImage> images)
    {
        if (images.Count == 0)
            return Rectangle.Empty;

        var bounds = GetImageBounds(images.First());
        if (images.Count == 1)
            return bounds;

        return images
            .Skip(1)
            .Aggregate(bounds, (current, framePart) => Rectangle.Union(current, GetImageBounds(framePart)));
    }

    /// <summary>
    /// Получить заполненную область изображения.
    /// </summary>
    private static Rectangle GetImageBounds(MqImage mqImage)
    {
        var imagePieces = mqImage.ImageParts;
        if (imagePieces.Count == 0)
            return Rectangle.Empty;

        var bounds = new Rectangle(imagePieces[0].SourceX, imagePieces[0].SourceY, imagePieces[0].Width, imagePieces[0].Height);
        if (imagePieces.Count == 1)
            return bounds;

        return imagePieces
            .Skip(1)
            .Aggregate(bounds, (current, imagePiece) => Rectangle.Union(current, new Rectangle(imagePiece.SourceX, imagePiece.SourceY, imagePiece.Width, imagePiece.Height)));
    }

    /// <summary>
    /// Создать новое изображение из частей базового.
    /// </summary>
    /// <param name="baseImage">Базовое изображение.</param>
    /// <param name="mqImage">Информация о новом изображении.</param>
    /// <param name="bounds">Границы изображения.</param>
    private static RawBitmap BuildImage(RawBitmap baseImage, MqImage mqImage, Rectangle bounds)
    {
        var imageHeight = bounds.Height;
        var imageWidth = bounds.Width;
        var imageData = new byte[imageHeight * imageWidth * 4];

        foreach (var framePart in mqImage.ImageParts)
        {
            // Баг ресурсов Disciples.
            // В некоторых случаях фрейм может иметь размеры больше, чем его базовое изображение (пример, G000UU0049HMOVA1A00).
            if (baseImage.OriginalWidth < framePart.DestX)
                continue;

            var frameWidth = Math.Min(framePart.Width, Math.Max(baseImage.OriginalWidth - framePart.DestX, 0));
            var frameHeight = Math.Min(framePart.Height, Math.Max(baseImage.OriginalHeight - framePart.DestY, 0));

            var partWidth = frameWidth << 2;
            for (int row = 0; row < frameHeight; ++row)
            {
                var sourcePosition = ((framePart.DestY + row) * baseImage.OriginalWidth + framePart.DestX) << 2;
                var destinationPosition = ((framePart.SourceY + row - bounds.Y) * imageWidth + framePart.SourceX - bounds.X) << 2;

                Buffer.BlockCopy(baseImage.Data, sourcePosition, imageData, destinationPosition, partWidth);
            }
        }

        return new RawBitmap
        {
            OriginalWidth = mqImage.Width,
            OriginalHeight = mqImage.Height,
            Bounds = bounds,
            Data = imageData
        };
    }

    /// <summary>
    /// Извлечь изображение из файла и обработать его (заменить прозрачность и т.д.).
    /// </summary>
    /// <param name="file">Файл с изображением.</param>
    /// <param name="imageMetadata">Метаданные изображения.</param>
    /// <param name="imageProcessingAlgorithm">Алгоритм обработки изображения.</param>
    /// <returns>Сырые данные, которые содержат картинку в массиве BGRA.</returns>
    private RawBitmap PrepareImage(File file, MqImageMetadata? imageMetadata, ImageProcessingAlgorithm imageProcessingAlgorithm = ImageProcessingAlgorithm.Default)
    {
        using var fileStream = new FileStream(ResourceFilePath, FileMode.Open, FileAccess.Read);
        using var streamReader = new Substream(fileStream, file.Offset, file.Size);
        using var image = SKBitmap.Decode(streamReader);

        var pixels = image.Bytes;
        var alphaColors = imageMetadata == null
                          ? GetDefaultAlphaColors()
                          : GetAlphaColors(imageMetadata.TransparentColorIndex, imageMetadata.OpacityAlgorithm, imageMetadata.Palette);

        unchecked
        {
            for (int i = 0; i < pixels.Length; i += 4)
            {
                var color = GetColor(pixels[i], pixels[i + 1], pixels[i + 2]);
                var opacity = alphaColors.GetValueOrDefault(color, byte.MaxValue);
                pixels[i + 3] = opacity;

                // BUG в Avalonia: если не выставить пиксели в 0, то у изображения появляется фиолетовая рамка.
                if (opacity == 0)
                {
                    pixels[i] = 0;
                    pixels[i + 1] = 0;
                    pixels[i + 2] = 0;
                }
                else if (imageProcessingAlgorithm == ImageProcessingAlgorithm.Shadow)
                {
                    pixels[i + 3] = 127;
                }
            }
        }

        return new RawBitmap
        {
            OriginalWidth = image.Width,
            OriginalHeight = image.Height,
            Bounds = new Rectangle(0, 0, image.Width, image.Height),
            Data = pixels
        };
    }

    /// <summary>
    /// Получить альфа-каналы для всех цветов.
    /// </summary>
    private static IReadOnlyDictionary<int, byte> GetAlphaColors(int transparentColorIndex, int opacityAlgorithm, IReadOnlyList<Color> palette)
    {
        var alphaColors = new Dictionary<int, byte>();

        switch (opacityAlgorithm)
        {
            // Если значение 255 и меньше, то это и есть значение прозрачности.
            case <= byte.MaxValue:
                foreach (var color in palette)
                    alphaColors[GetColor(color.B, color.G, color.R)] = (byte)opacityAlgorithm;
                break;

            // Значение для различных аур. Уровень прозрачности - это номер цвета в палитре.
            case 300:
                for (int colorIndex = 0; colorIndex < palette.Count; colorIndex++)
                {
                    var color = palette[colorIndex];
                    alphaColors[GetColor(color.B, color.G, color.R)] = (byte)colorIndex;
                }
                break;
        }

        var transparentColor = palette[transparentColorIndex];
        alphaColors[GetColor(transparentColor.B, transparentColor.G, transparentColor.R)] = 0;

        var defaultAlphaColors = GetDefaultAlphaColors();
        foreach (var defaultAlphaColor in defaultAlphaColors)
            alphaColors[defaultAlphaColor.Key] = defaultAlphaColor.Value;

        return alphaColors;
    }

    /// <summary>
    /// Получить альфа-канал для цветов по умолчанию.
    /// </summary>
    private static IReadOnlyDictionary<int, byte> GetDefaultAlphaColors()
    {
        var alphaColors = new Dictionary<int, byte>();

        // Особенность изображений Disciples. Прозрачной является не только кисть 255/0/255, но и цвета близкие к ней.
        // TODO. Выбор цветов может быть сложнее.
        // См. https://bitbucket.org/NevendaarTools/toolsqt/src/55b89969c79f74dbbac90a63854256f90d52f0e1/ResourceModel/GameResource.cpp#lines-178:191
        for (int blue = 249; blue <= byte.MaxValue; blue++)
        for (int green = 0; green <= 3 ; green++)
        for (int red = 252; red <= byte.MaxValue; red++)
            alphaColors[GetColor((byte)blue, (byte)green, (byte)red)] = 0;

        return alphaColors;
    }

    /// <summary>
    /// Получить цвет пикселя.
    /// </summary>
    private static int GetColor(uint blue, uint green, uint red)
    {
        return ((byte)blue << 16)
               | ((byte)green << 8)
               | (byte)red;
    }

    /// <summary>
    /// Получить содержимое файла из ресурса.
    /// </summary>
    private byte[] GetFileContent(File file)
    {
        var fileContent = new byte[file.Size];
        using (var fileStream = new FileStream(ResourceFilePath, FileMode.Open, FileAccess.Read))
        {
            fileStream.Seek(file.Offset, SeekOrigin.Begin);
            fileStream.Read(fileContent, 0, file.Size);
        }

        return fileContent;
    }

    #endregion
}