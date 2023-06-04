using System.Drawing;
using Disciples.Resources.Common;
using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Common.Extensions;
using Disciples.Resources.Common.Models;
using Disciples.Resources.Images.Enums;
using Disciples.Resources.Images.Models;
using ImageMagick;
using SubstreamSharp;
using File = Disciples.Resources.Common.Models.File;

namespace Disciples.Resources.Images;

/// <summary>
/// Класс для извлечения изображений из ресурсов (.ff).
/// </summary>
/// <remarks>
/// TODO Разобраться с null.
/// </remarks>
public class ImagesExtractor : BaseMqdbResourceExtractor
{
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
    // bug Невозможно получить информацию о некоторых файлах. В основном, связанных с эльфами.
    // Например, G000UU8029HHITA1A00.
    // Ссылки на PNG нет, но в .ff файле какая-то информация есть.
    public IReadOnlyCollection<RawBitmap>? GetAnimationFrames(string name)
    {
        if (_mqAnimations?.ContainsKey(name) != true)
            return null;

        return GetAnimationFramesInternal(_mqAnimations[name]);
    }

    /// <summary>
    /// Признак, что изображение является частью базового.
    /// </summary>
    public string? GetBaseImageName(string name)
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

        var baseImage = PrepareImage(file);
        return images.ToDictionary(
            image => image.Name,
            image => BuildImage(baseImage, image));
    }

    /// <summary>
    /// Получить изображение по его имени.
    /// </summary>
    public RawBitmap GetImage(string name)
    {
        // Если информация об изображении есть в -IMAGES.OPT, значит необходимо будет собирать по частям.
        if (_mqImages?.ContainsKey(name) == true)
        {
            var mqImage = _mqImages[name];
            var baseImage = PrepareImage(GetFile(mqImage.FileId));

            return BuildImage(baseImage, mqImage);
        }

        // Иначе мы ищем файл с таким именем. Его можно просто отдать целиком, предварительно избавившись от прозрачности.
        var imageFile = TryGetFile($"{name.ToUpper()}.PNG");
        if (imageFile == null)
            return null;

        return PrepareImage(imageFile);
    }

    /// <summary>
    /// Получить данные файла по его имени.
    /// </summary>
    public byte[] GetFileContent(string name)
    {
        return GetFileContent(GetFile($"{name.ToUpper()}.PNG"));
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

        // Конвертируем в другой словарь, чтобы после выкинуть все картинки, которые являются частями анимации.
        // Это позволит не хранить огромный словарь изображений.
        var mqImages = LoadMqImages(stream, mqIndexes)
            ?.ToDictionary(mi => mi.Key, mi => new MqImageInfo(mi.Value));
        if (mqImages == null)
            return;

        _mqAnimations = LoadMqAnimations(stream, mqImages);
        _mqImages = mqImages
            .Where(mi => mi.Value.IsAnimationFrame == false)
            .ToDictionary(mi => mi.Key, mi => mi.Value.MqImage);
        _baseMqImages = _mqImages
            .Values
            .GroupBy(i => i.FileId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<MqImage>)g.ToArray());
    }

    /// <summary>
    /// Загрузить индексы. Индекс позволяет определить в каком файле (идентификатор) находится изображение (по имени).
    /// </summary>
    private IDictionary<string, MqIndex>? LoadMqIndexes(Stream stream)
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
            var unknownValue1 = stream.ReadInt();
            var unknownValue2 = stream.ReadInt();

            var mqIndex = new MqIndex(id, name, unknownValue1, unknownValue2);
            // todo Возможно дублирование. WTF?
            mqIndices.TryAdd(mqIndex.Name, mqIndex);
        }

        return mqIndices;
    }

    /// <summary>
    /// Загрузить изображения. Изображения содержат информацию о том, как нужно разрезать базовую картинку, чтобы получить требуемую.
    /// </summary>
    private IDictionary<string, MqImage>? LoadMqImages(Stream stream, IDictionary<string, MqIndex> mqIndices)
    {
        var imagesFile = TryGetFile("-IMAGES.OPT");
        if (imagesFile == null)
            return null;

        var mqImages = new Dictionary<string, MqImage>();
        stream.Seek(imagesFile.Offset, SeekOrigin.Begin);

        var endOfFile = imagesFile.Offset + imagesFile.Size - 1;
        while (stream.Position < endOfFile)
        {
            stream.Seek(11 + 1024, SeekOrigin.Current);

            var fileFramesNumber = stream.ReadInt();
            for (int frameIndex = 0; frameIndex < fileFramesNumber; ++frameIndex)
            {
                var frameName = stream.ReadString();
                var piecesCount = stream.ReadInt();
                int width = stream.ReadInt(),
                    height = stream.ReadInt();

                var pieces = new List<MqImagePiece>();
                for (int pieceIndex = 0; pieceIndex < piecesCount; ++pieceIndex)
                {
                    int sourceX = stream.ReadInt(),
                        sourceY = stream.ReadInt(),
                        destX = stream.ReadInt(),
                        destY = stream.ReadInt(),
                        pieceWidth = stream.ReadInt(),
                        pieceHeight = stream.ReadInt();

                    var piece = new MqImagePiece(sourceX, sourceY, destX, destY, pieceWidth, pieceHeight);
                    pieces.Add(piece);
                }

                var fileId = mqIndices[frameName].Id;
                var mqImage = new MqImage(frameName, fileId, width, height, pieces);
                // todo Возможно дублирование. WTF?
                mqImages.TryAdd(mqImage.Name, mqImage);
            }
        }

        return mqImages;
    }

    /// <summary>
    /// Загрузить анимации. Анимации хранят информацию о изображениях из которых состоят.
    /// </summary>
    private IReadOnlyDictionary<string, MqAnimation>? LoadMqAnimations(Stream stream, IDictionary<string, MqImageInfo> mqImages)
    {
        var animsFile = TryGetFile("-ANIMS.OPT");
        if (animsFile == null)
            return null;

        var animationsInfo = new List<(int AnimationIndex, IReadOnlyCollection<MqImage> Frames)>();
        stream.Seek(animsFile.Offset, SeekOrigin.Begin);

        int animNumber = 0;
        var endOfFile = animsFile.Offset + animsFile.Size - 1;
        while (stream.Position < endOfFile)
        {
            var fileAnimNumber = stream.ReadInt();
            var frames = new List<MqImage>(fileAnimNumber);
            for (int animIndex = 0; animIndex < fileAnimNumber; ++animIndex)
            {
                var animName = stream.ReadString();
                var mqImageInfo = mqImages[animName];

                mqImageInfo.IsAnimationFrame = true;
                frames.Add(mqImageInfo.MqImage);
            }

            animationsInfo.Add((animNumber, frames));
            ++animNumber;
        }

        var mqAnimations = new Dictionary<string, MqAnimation>();
        foreach (var animationInfo in animationsInfo)
        {
            foreach (var mqAnimationFrame in animationInfo.Frames)
            {
                var safeFileName = Path.GetFileNameWithoutExtension(GetFile(mqAnimationFrame.FileId).Name);
                var mqAnimation = new MqAnimation(animationInfo.AnimationIndex, safeFileName, animationInfo.Frames);

                // Имя анимации - это имя базового изображения для первого фрейма.
                // Фреймы могут иметь разные базовые изображения, но пока это работает.
                mqAnimations.TryAdd(safeFileName, mqAnimation);
                break;
            }
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
                baseImages.Add(fileId, PrepareImage(GetFile(fileId)));

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
        var imagePieces = mqImage.ImagePieces;
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

        foreach (var framePart in mqImage.ImagePieces)
        {
            var partWidth = framePart.Width << 2;
            for (int row = 0; row < framePart.Height; ++row)
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
    /// <returns>Сырые данные, которые содержат картинку в массиве BGRA.</returns>
    private RawBitmap PrepareImage(File file)
    {
        MagickImage magickImage;
        try
        {
            using (var fileStream = new FileStream(ResourceFilePath, FileMode.Open, FileAccess.Read))
            using (var streamReader = new Substream(fileStream, file.Offset, file.Size))
            {
                magickImage = new MagickImage(streamReader, MagickFormat.Png);
            }
        }
        catch (MagickCoderErrorException)
        {
            // bug Какой-то странный баг. MagickImage не может сконвертить некоторые портреты.
            // Нужно получить решение.
            Console.WriteLine($"Corrupted image: {ResourceFilePath}\\{file.Name}");
            return null;
        }

        var colorMap = new Dictionary<int, byte>();
        var safeName = Path.GetFileNameWithoutExtension(file.Name);
        var imageType = GetImageType(safeName);
        if (imageType == ImageType.Aura)
        {
            // Если файл содержит ауру, то создаём полупрозрачное изображение.
            // Пока берём прозрачность равную индексу цвета в палитре,
            // Но такое чувство, что есть более четкая зависимость.
            for (int i = 0; i < 256; ++i)
            {
                unchecked
                {
                    var color = magickImage.GetColormapColor(i)!;
                    var index = GetColor(color.B, color.G, color.R);

                    colorMap[index] = (byte)i;
                }
            }
        }

        var pixels = magickImage.GetPixels().ToByteArray("BGRA")!;
        var zeroColor = magickImage.GetColormapColor(0);
        var mainTransparentColor = zeroColor == null
            ? GetColor(255, 0, 255)
            : GetColor(zeroColor.B, zeroColor.G, zeroColor.R);

        unchecked
        {
            for (int i = 0; i < pixels.Length; i += 4)
            {
                var color = GetColor(pixels[i], pixels[i + 1], pixels[i + 2]);

                // Проверяем прозрачную область.
                // Вторая проверка нужна потому, что по какой-то причине есть еще несколько дополнительных прозрачных цветов.
                if (color == mainTransparentColor || (pixels[i] > 248 && pixels[i + 1] < 4 && pixels[i + 2] > 251))
                {
                    pixels[i] = 0;
                    pixels[i + 1] = 0;
                    pixels[i + 2] = 0;
                    pixels[i + 3] = 0;

                    continue;
                }

                // Если файл - аура, то определяем прозрачность по словарю.
                if (imageType == ImageType.Aura)
                {
                    if (colorMap.TryGetValue(color, out var alphaChannel))
                        pixels[i + 3] = alphaChannel;

                    continue;
                }

                // Если файл тень, то делаем его полупрозрачным.
                if (imageType == ImageType.Shadow)
                {
                    if (pixels[i + 3] != 0)
                        pixels[i + 3] = 128;

                    continue;
                }
            }
        }

        return new RawBitmap
        {
            OriginalWidth = magickImage.Width,
            OriginalHeight = magickImage.Height,
            Bounds = new Rectangle(0, 0, magickImage.Width, magickImage.Height),
            Data = pixels
        };
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

    /// <summary>
    /// Получить тип изображения по его имени. Жуткий костыль, так как не знаю, где это находится в метаданных.
    /// </summary>
    private static ImageType GetImageType(string name)
    {
        var safeName = name.EndsWith("_1")
            ? name.Substring(0, name.Length - 2)
            : name;

        // todo Ну это уже совсем никуда не годится, исправить.
        if (safeName.EndsWith("A2A00") || safeName.EndsWith("A2D00") ||
            (safeName.Length > 8 && (safeName.Substring(safeName.Length - 9, 4) == "HEFF" ||
                                     safeName.Substring(safeName.Length - 9, 4) == "TUCH")) ||
            safeName.StartsWith("MRK") || safeName.StartsWith("DEATH"))
            return ImageType.Aura;

        if (safeName.StartsWith("POISONANIM") ||
            safeName.StartsWith("FROSTBITEANIM") ||
            safeName.StartsWith("BLISTERANIM"))
        {
            return ImageType.Aura;
        }

        if (safeName.EndsWith("S1A00") || safeName.EndsWith("S1D00") || safeName.StartsWith("MASKDEAD"))
            return ImageType.Shadow;

        return ImageType.Simple;
    }


    #endregion
}