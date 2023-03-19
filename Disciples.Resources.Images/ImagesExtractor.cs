using Disciples.Resources.Images.Enums;
using Disciples.Resources.Images.Helpers;
using Disciples.Resources.Images.Models;
using ImageMagick;
using File = Disciples.Resources.Images.Models.File;

namespace Disciples.Resources.Images;

/// <summary>
/// Класс для извлечения изображений из ресурсов.
/// </summary>
/// <remarks>
/// TODO Разобраться с null.
/// </remarks>
public class ImagesExtractor
{
    // Помимо "обычного" розового, существует еще такой, который также должен считать прозрачным.
    private readonly MagickColor _additionalTransparentColor = MagickColor.FromRgb(252, 2, 252);

    private readonly string _path;

    private IDictionary<int, Record> _records = null!;
    private IDictionary<int, File> _filesById = null!;
    private IDictionary<string, File> _filesByName = null!;

    private IDictionary<string, MqImage>? _mqImages;
    private IDictionary<string, MqAnimation>? _mqAnimations;


    /// <summary>
    /// Создать объект типа <see cref="ImagesExtractor" />.
    /// </summary>
    /// <param name="path">Путь до ресурса с изображениями.</param>
    public ImagesExtractor(string path)
    {
        _path = path;

        Load();
    }

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
        if (_filesByName?.ContainsKey(name) != true)
            return null;

        var result = new Dictionary<string, RawBitmap>();
        var baseFile = _filesByName[name];
        var baseImage = PrepareImage(baseFile);
        var parts = _mqImages.Select(i => i.Value).Where(i => i.FileId == baseFile.Id);
        foreach (var part in parts) {
            result.Add(part.Name, BuildImage(baseImage, part));
        }

        return result;
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
            var baseImage = PrepareImage(_filesById[mqImage.FileId]);

            return BuildImage(baseImage, mqImage);
        }

        // Иначе мы ищем файл с таким именем. Его можно просто отдать целиком, предварительно избавившись от прозрачности.
        if (_filesByName.TryGetValue($"{name.ToUpper()}.PNG", out var imageFile) == false)
            return null;

        return PrepareImage(imageFile);
    }

    /// <summary>
    /// Получить данные файла по его имени.
    /// </summary>
    public byte[] GetFileContent(string name)
    {
        if (_filesByName.TryGetValue($"{name.ToUpper()}.PNG", out var file) == false)
            throw new ArgumentException($"Файл {name} не найден");

        return GetFileContent(file);
    }

    /// <summary>
    /// Получить имена всех изображений в ресурсе.
    /// </summary>
    public IReadOnlyList<string> GetAllFilesNames()
    {
        return _filesByName.Keys.ToList();
    }


    #region LoadData

    /// <summary>
    /// Извлечь все метаданные из файла ресурсов.
    /// </summary>
    private void Load()
    {
        using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read))
        {
            var mqdb = stream.ReadString(4);
            if (mqdb != "MQDB")
                throw new ArgumentException("Unknown format of file");

            LoadRecords(stream);
            LoadFilesList(stream);
        }

        var mqIndexes = LoadMqIndexes();
        if (mqIndexes == null)
            return;

        // Конвертируем в другой словарь, чтобы после выкинуть все картинки, которые являются частями анимации.
        // Это позволит не хранить огромный словарь изображений.
        var mqImages = LoadMqImages(mqIndexes)
            .ToDictionary(mi => mi.Key, mi => new MqImageInfo(mi.Value));

        _mqAnimations = LoadMqAnimations(mqImages);
        _mqImages = mqImages
            .Where(mi => mi.Value.IsAnimationFrame == false)
            .ToDictionary(mi => mi.Key, mi => mi.Value.MqImage);
    }

    /// <summary>
    /// Загрузка информации о записях.
    /// </summary>
    private void LoadRecords(Stream stream)
    {
        _records = new SortedDictionary<int, Record>();
        stream.Seek(28, SeekOrigin.Begin);

        while (true) {
            var magic = stream.ReadString(4);
            if (stream.Position >= stream.Length - 1 || magic != "MQRC")
                break;

            stream.Seek(4, SeekOrigin.Current);

            var id = stream.ReadInt();
            var size = stream.ReadInt();
            stream.Seek(3 * 4, SeekOrigin.Current);
            var offset = stream.Position;
            var mqrc = new Record(id, size, offset);

            stream.Seek(mqrc.Size, SeekOrigin.Current);

            _records[mqrc.Id] = mqrc;
        }

        if (_records.ContainsKey(2) == false)
            throw new ArgumentException("Unknown file format: ID0002 was not found");
    }

    /// <summary>
    /// Загрузка информации о файлах.
    /// </summary>
    private void LoadFilesList(Stream stream)
    {
        // Информация о списке файлов лежит в записи с идентификатором 2.
        stream.Seek(_records[2].Offset, SeekOrigin.Begin);

        var filesCount = stream.ReadInt();
        _filesById = new SortedDictionary<int, File>();
        _filesByName = new Dictionary<string, File>();

        for (int i = 0; i < filesCount; ++i) {
            var fileName = stream.ReadString(256);
            var id = stream.ReadInt();

            var record = _records[id];
            var file = new File(id, fileName, record.Size, record.Offset);

            _filesById[id] = file;
            _filesByName[fileName] = file;
        }
    }


    /// <summary>
    /// Загрузить индексы. Индекс позволяет определить в каком файле (идентификатор) находится изображение (по имени).
    /// </summary>
    private IDictionary<string, MqIndex>? LoadMqIndexes()
    {
        _filesByName.TryGetValue("-INDEX.OPT", out var indexFile);
        if (indexFile == null)
            return null;

        var mqIndices = new Dictionary<string, MqIndex>();
        using (var indicesStream = new FileStream(_path, FileMode.Open, FileAccess.Read))
        {
            indicesStream.Seek(indexFile.Offset, SeekOrigin.Begin);

            var framesCount = indicesStream.ReadInt();
            for (int frameIndex = 0; frameIndex < framesCount; ++frameIndex)
            {
                var id = indicesStream.ReadInt();
                var name = indicesStream.ReadString();
                var unknownValue1 = indicesStream.ReadInt();
                var unknownValue2 = indicesStream.ReadInt();

                var mqIndex = new MqIndex(id, name, unknownValue1, unknownValue2);
                // todo Возможно дублирование. WTF?
                mqIndices.TryAdd(mqIndex.Name, mqIndex);
            }
        }

        return mqIndices;
    }

    /// <summary>
    /// Загрузить изображения. Изображения содержат информацию о том, как нужно разрезать базовую картинку, чтобы получить требуемую.
    /// </summary>
    private IDictionary<string, MqImage> LoadMqImages(IDictionary<string, MqIndex> mqIndices)
    {
        _filesByName.TryGetValue("-IMAGES.OPT", out var imagesFile);
        if (imagesFile == null)
            return null;

        var mqImages = new Dictionary<string, MqImage>();
        using (var imagesStream = new FileStream(_path, FileMode.Open, FileAccess.Read)) {
            imagesStream.Seek(imagesFile.Offset, SeekOrigin.Begin);

            var endOfFile = imagesFile.Offset + imagesFile.Size - 1;
            while (imagesStream.Position < endOfFile) {
                imagesStream.Seek(11 + 1024, SeekOrigin.Current);

                var fileFramesNumber = imagesStream.ReadInt();
                for (int frameIndex = 0; frameIndex < fileFramesNumber; ++frameIndex) {
                    var frameName = imagesStream.ReadString();
                    var piecesCount = imagesStream.ReadInt();
                    int width = imagesStream.ReadInt(),
                        height = imagesStream.ReadInt();

                    var pieces = new List<MqImagePiece>();
                    for (int pieceIndex = 0; pieceIndex < piecesCount; ++pieceIndex) {
                        int sourceX = imagesStream.ReadInt(),
                            sourceY = imagesStream.ReadInt(),
                            destX = imagesStream.ReadInt(),
                            destY = imagesStream.ReadInt(),
                            pieceWidth = imagesStream.ReadInt(),
                            pieceHeight = imagesStream.ReadInt();

                        var piece = new MqImagePiece(sourceX, sourceY, destX, destY, pieceWidth, pieceHeight);
                        pieces.Add(piece);
                    }

                    var fileId = mqIndices[frameName].Id;
                    var mqImage = new MqImage(frameName, fileId, width, height, pieces);
                    // todo Возможно дублирование. WTF?
                    mqImages.TryAdd(mqImage.Name, mqImage);
                }
            }
        }

        return mqImages;
    }

    /// <summary>
    /// Загрузить анимации. Анимации хранят информацию о изображениях из которых состоят.
    /// </summary>
    private IDictionary<string, MqAnimation> LoadMqAnimations(IDictionary<string, MqImageInfo> mqImages)
    {
        _filesByName.TryGetValue("-ANIMS.OPT", out var animsFile);
        if (animsFile == null)
            return null;

        var animationsInfo = new List<(int AnimationIndex, IReadOnlyCollection<MqImage> Frames)>();
        using (var animsStream = new FileStream(_path, FileMode.Open, FileAccess.Read)) {
            animsStream.Seek(animsFile.Offset, SeekOrigin.Begin);

            int animNumber = 0;
            var endOfFile = animsFile.Offset + animsFile.Size - 1;
            while (animsStream.Position < endOfFile) {
                var fileAnimNumber = animsStream.ReadInt();
                var frames = new List<MqImage>(fileAnimNumber);
                for (int animIndex = 0; animIndex < fileAnimNumber; ++animIndex) {
                    var animName = animsStream.ReadString();
                    var mqImageInfo = mqImages[animName];

                    mqImageInfo.IsAnimationFrame = true;
                    frames.Add(mqImageInfo.MqImage);
                }

                animationsInfo.Add((animNumber, frames));
                ++animNumber;
            }
        }

        var mqAnimations = new Dictionary<string, MqAnimation>();
        foreach (var animationInfo in animationsInfo) {
            foreach (var mqAnimationFrame in animationInfo.Frames) {
                var safeFileName = Path.GetFileNameWithoutExtension(_filesById[mqAnimationFrame.FileId].Name);
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

        foreach (var frame in animation.Frames) {
            var fileId = frame.FileId;
            if (baseImages.ContainsKey(frame.FileId) == false) {
                baseImages.Add(fileId, PrepareImage(_filesById[fileId]));
            }

            result.Add(BuildImage(baseImages[fileId], frame));
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
        int minRow = int.MaxValue, maxRow = int.MinValue;
        int minColumn = int.MaxValue, maxColumn = int.MinValue;
        var imageData = new byte[mqImage.Width * mqImage.Height * 4];

        foreach (var framePart in mqImage.ImagePieces) {
            var partWidth = framePart.Width << 2;
            for (int row = 0; row < framePart.Height; ++row) {
                var posS = ((framePart.DestY + row) * baseImage.Width + framePart.DestX) << 2;
                var posT = ((framePart.SourceY + row) * mqImage.Width + framePart.SourceX) << 2;

                Buffer.BlockCopy(baseImage.Data, posS, imageData, posT, partWidth);
            }

            minRow = Math.Min(minRow, framePart.SourceY);
            maxRow = Math.Max(maxRow, framePart.SourceY + framePart.Height);
            minColumn = Math.Min(minColumn, framePart.SourceX);
            maxColumn = Math.Max(maxColumn, framePart.SourceX + framePart.Width);
        }

        return new RawBitmap(minRow, maxRow, minColumn, maxColumn, mqImage.Width, mqImage.Height, imageData);
    }

    /// <summary>
    /// Извлечь изображение из файла и обработать его (заменить прозрачность и т.д.).
    /// </summary>
    /// <param name="file">Файл с изображением.</param>
    /// <returns>Сырые данные, которые содержат картинку в массиве BGRA.</returns>
    private RawBitmap PrepareImage(File file)
    {
        MagickImage magickImage;
        try {
            magickImage = new MagickImage(GetFileContent(file));
        }
        catch (MagickCoderErrorException) {
            // bug Какой-то странный баг. MagickImage не может сконвертить некоторые портреты.
            // Нужно получить решение.
            Console.WriteLine($"Corrupted image: {_path}\\{file.Name}");
            return null;
        }

        var colorMap = new Dictionary<int, byte>();
        var safeName = Path.GetFileNameWithoutExtension(file.Name);
        var imageType = GetImageType(safeName);
        if (imageType == ImageType.Aura) {
            // Если файл содержит ауру, то создаём полупрозрачное изображение.
            // Пока берём прозрачность равную индексу цвета в палитре,
            // Но такое чувство, что есть более четкая зависимость.
            for (int i = 0; i < 256; ++i) {
                unchecked {
                    var color = magickImage.GetColormapColor(i);
                    var index = ((byte) color.B << 16) + ((byte) color.G << 8) + (byte) color.R;

                    colorMap[index] = (byte) i;
                }
            }
        }

        var pixels = magickImage.GetPixels().ToByteArray("BGRA");
        var transparentColor = magickImage.GetColormapColor(0);
        if (transparentColor == null) {
            transparentColor = MagickColor.FromRgb(255, 0, 255);
        }

        unchecked {
            var tcBlue = (byte) transparentColor.B;
            var tcGreen = (byte) transparentColor.G;
            var tcRed = (byte) transparentColor.R;

            var atcBlue = (byte) _additionalTransparentColor.B;
            var atcGreen = (byte) _additionalTransparentColor.G;
            var atcRed = (byte) _additionalTransparentColor.R;

            for (int i = 0; i < pixels.Length; i += 4) {
                // Проверяем прозрачную область.
                if (pixels[i] == tcBlue && pixels[i + 1] == tcGreen && pixels[i + 2] == tcRed) {
                    pixels[i] = 0;
                    pixels[i + 1] = 0;
                    pixels[i + 2] = 0;
                    pixels[i + 3] = 0;

                    continue;
                }

                // Проверяем прозрачную область с помощью альтернативной кисти.
                // Так как иначе будут оставаться розовые пятна на правой панели с юнитами.
                if (pixels[i] == atcBlue && pixels[i + 1] == atcGreen && pixels[i + 2] == atcRed) {
                    pixels[i] = 0;
                    pixels[i + 1] = 0;
                    pixels[i + 2] = 0;
                    pixels[i + 3] = 0;

                    continue;
                }

                // Если файл - аура, то определяем прозрачность по словарю.
                if (imageType == ImageType.Aura) {
                    var index = (pixels[i] << 16) + (pixels[i + 1] << 8) + pixels[i + 2];
                    if (colorMap.TryGetValue(index, out var alphaChannel)) {
                        pixels[i + 3] = alphaChannel;
                    }

                    continue;
                }

                // Если файл тень, то делаем его полупрозрачным.
                if (imageType == ImageType.Shadow) {
                    if (pixels[i + 3] != 0) {
                        pixels[i + 3] = 128;
                    }

                    continue;
                }
            }
        }

        return new RawBitmap(0, magickImage.Height, 0, magickImage.Width, magickImage.Width, magickImage.Height, pixels);
    }

    /// <summary>
    /// Получить содержимое файла из ресурса.
    /// </summary>
    private byte[] GetFileContent(File file)
    {
        var fileContent = new byte[file.Size];
        using (var fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read)) {
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

        if (safeName.EndsWith("S1A00") || safeName.EndsWith("S1D00") || safeName.StartsWith("MASKDEAD"))
            return ImageType.Shadow;

        return ImageType.Simple;
    }


    #endregion
}