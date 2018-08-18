using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ImageMagick;

using ResourceProvider.Enums;
using ResourceProvider.Helpers;
using ResourceProvider.Models;

using File = ResourceProvider.Models.File;

namespace ResourceProvider
{
    public class ImagesExtractor
    {
        private readonly string _path;

        private IDictionary<int, Record> _records;
        private IDictionary<int, File> _filesById;
        private IDictionary<string, File> _filesByName;

        private IDictionary<string, MqImage> _mqImages;
        private IDictionary<string, MqAnimation> _mqAnimations;

        public ImagesExtractor(string path)
        {
            _path = path;

            Load();
        }

        // bug Невозможно получить информацию о некоторых файлах. В основном, связанных с эльфами.
        // Например, G000UU8029HHITA1A00
        // Ссылки на PNG нет, но в .ff файле какая-то информация есть
        public IReadOnlyCollection<RowImage> GetAnimationFrames(string name)
        {
            if (_mqAnimations?.ContainsKey(name) != true)
                return null;

            return GetAnimationFramesInternal(_mqAnimations[name]);
        }

        public RowImage GetImage(string name)
        {
            // Если информация об изображении есть в -IMAGES.OPT, значит необходимо будет собирать по частям
            if (_mqImages?.ContainsKey(name) == true) {
                var mqImage = _mqImages[name];
                var baseImage = PrepareImage(_filesById[mqImage.FileId]);

                return BuildImage(baseImage, mqImage);
            }

            // Иначе мы ищем файл с таким именем. Его можно просто отдать целиком, предварительно избавившись от прозрачности
            _filesByName.TryGetValue($"{name.ToUpper()}.PNG", out var imageFile);
            if (imageFile == null)
                return null;

            return PrepareImage(imageFile);
        }


        #region LoadData

        /// <summary>
        /// Извлечь все метаданные из файла-контейнера
        /// </summary>
        private void Load()
        {
            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read)) {
                var mqdb = stream.ReadString(4);
                if (mqdb != "MQDB")
                    throw new ArgumentException("Unknown format of file");

                LoadRecords(stream);
                LoadFilesList(stream);
            }

            var mqIndices = LoadMqIndices();
            if (mqIndices == null)
                return;

            // Конвертируем в другой словарь, чтобы после выкинуть все картинки, которые являются частями анимаций
            // Это позволит не хранить огромный словарь изображений
            var mqImages = LoadMqImages(mqIndices)
                .ToDictionary(mi => mi.Key, mi => new MqImageInfo(mi.Value));


            _mqAnimations = LoadMqAnimations(mqImages);
            _mqImages = mqImages
                .Where(mi => mi.Value.IsAnimationFrame == false)
                .ToDictionary(mi => mi.Key, mi => mi.Value.MqImage);
        }

        /// <summary>
        /// Загрузка информации о записях
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
        /// Загрузка информации о файлах
        /// </summary>
        private void LoadFilesList(Stream stream)
        {
            // Информация о списке файлов лежит в записи с идентификатором 2
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
        /// Загрузить индексы. Индекс позволяет определить в каком файле (идентификатор) находится изображение (по имени)
        /// </summary>
        private IDictionary<string, MqIndex> LoadMqIndices()
        {
            _filesByName.TryGetValue("-INDEX.OPT", out var indexFile);
            if (indexFile == null)
                return null;

            var mqIndices = new Dictionary<string, MqIndex>();
            using (var indicesStream = new FileStream(_path, FileMode.Open, FileAccess.Read)) {
                indicesStream.Seek(indexFile.Offset, SeekOrigin.Begin);

                var framesCount = indicesStream.ReadInt();
                for (int frameIndex = 0; frameIndex < framesCount; ++frameIndex) {
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
        /// Загрузить изображения. Изображения содержат информацию о том, как нужно разрезать базовую картинку, чтобы получить требуемую
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
        /// Загрузить анимации. Анимации хранят информацию о изображениях из которых состоят
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

                    // Имя анимации - это имя базового изображения для первого фрейма
                    // Фреймы могут иметь разные базовые изображения, но пока это работает
                    mqAnimations.TryAdd(safeFileName, mqAnimation);
                    break;
                }
            }

            return mqAnimations;
        }

        #endregion


        #region HelpMethods

        /// <summary>
        /// Получить кадры анимации
        /// </summary>
        /// <param name="animation">Информация об анимации</param>
        /// <returns>Коллекция кадров анимации</returns>
        private IReadOnlyCollection<RowImage> GetAnimationFramesInternal(MqAnimation animation)
        {
            var result = new List<RowImage>(animation.Frames.Count);
            // Обычно анимация "нарезается" из одного базового изображения,
            // Однака это не всегда. Поэтому необходимо иметь возможность кэшировать несколько изображений
            var baseImages = new Dictionary<int, RowImage>();

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
        /// Создать новое изображение из частей базового
        /// </summary>
        /// <param name="baseImage">Базовое изображение</param>
        /// <param name="mqImage">Информация о новом изображении</param>
        private static RowImage BuildImage(RowImage baseImage, MqImage mqImage)
        {
            int minRow = int.MaxValue, maxRow = int.MinValue;
            int minColumn = int.MaxValue, maxColumn = int.MinValue;
            var imageData = new byte[mqImage.Width * mqImage.Height * 4];

            foreach (var framePart in mqImage.ImagePieces) {
                for (int x = 0; x < framePart.Width; ++x) {
                    for (int y = 0; y < framePart.Height; ++y) {
                        unchecked {
                            var posS = ((framePart.DestY + y) * baseImage.Width + (framePart.DestX + x)) << 2;
                            var posT = ((framePart.SourceY + y) * mqImage.Width + (framePart.SourceX + x)) << 2;

                            imageData[posT] = baseImage.Data[posS];
                            imageData[posT + 1] = baseImage.Data[posS + 1];
                            imageData[posT + 2] = baseImage.Data[posS + 2];
                            imageData[posT + 3] = baseImage.Data[posS + 3];
                        }
                    }
                }

                minRow = Math.Min(minRow, framePart.SourceY);
                maxRow = Math.Max(maxRow, framePart.SourceY + framePart.Height);
                minColumn = Math.Min(minColumn, framePart.SourceX);
                maxColumn = Math.Max(maxColumn, framePart.SourceX + framePart.Width);
            }

            return new RowImage(minRow, maxRow, minColumn, maxColumn, mqImage.Width, mqImage.Height, imageData);
        }

        /// <summary>
        /// Извлечь изображение из файла и обработать его (заменить прозрачность и т.д.)
        /// </summary>
        /// <param name="file">Файл с изображением</param>
        /// <returns>Сырые данные, которые содержат картинку в массиве RGBA</returns>
        private RowImage PrepareImage(File file)
        {
            var fileContent = new byte[file.Size];
            using (var fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read)) {
                fileStream.Seek(file.Offset, SeekOrigin.Begin);
                fileStream.Read(fileContent, 0, file.Size);
            }

            var magickImage = new MagickImage(fileContent);

            var colorMap = new Dictionary<int, byte>();
            var safeName = Path.GetFileNameWithoutExtension(file.Name);
            var imageType = GetImageType(safeName);
            if (imageType == ImageType.Aura) {
                // Если файл содержит ауру, то создаём полупрозрачное изображение
                // Пока берём прозрачность равную индексу цвета в палитре
                // Но такое чувство, что есть более четкая зависимость
                for (int i = 0; i < 256; ++i) {
                    unchecked {
                        var color = magickImage.GetColormap(i);
                        var index = ((byte) color.R << 16) + ((byte) color.G << 8) + (byte) color.B;

                        colorMap[index] = (byte) i;
                    }
                }
            }

            var pixels = magickImage.GetPixels().ToByteArray(PixelMapping.RGBA);
            var transparentColor = magickImage.GetColormap(0);
            if (transparentColor == null) {
                transparentColor = MagickColor.FromRgb(255, 0, 255);
            }
            unchecked {
                for (int i = 0; i < pixels.Length; i += 4) {
                    if (pixels[i] == (byte)transparentColor.R &&
                        pixels[i + 1] == (byte)transparentColor.G &&
                        pixels[i + 2] == (byte)transparentColor.B) {
                        pixels[i + 3] = 0;
                    }

                    // Если файл - аура, то определяем прозрачность по словарю
                    var index = (pixels[i] << 16) + (pixels[i + 1] << 8) + pixels[i + 2];
                    if (colorMap.ContainsKey(index)) {
                        pixels[i + 3] = colorMap[index];
                    }

                    // Если файл тень, то делаем его полупрозрачным
                    if (imageType == ImageType.Shadow) {
                        if (pixels[i + 3] != 0) {
                            pixels[i + 3] = 128;
                        }
                    }

                    if (pixels[i + 3] == 0) {
                        pixels[i] = 0;
                        pixels[i + 1] = 0;
                        pixels[i + 2] = 0;
                        continue;
                    }
                }
            }


            return new RowImage(0, magickImage.Height - 1, 0, magickImage.Width - 1, magickImage.Width, magickImage.Height, pixels);
        }

        /// <summary>
        /// Получить тип изображения по его имени. Жуткий костыль, так как не знаю, где это находится в метаданных
        /// </summary>
        private static ImageType GetImageType(string name)
        {
            var safeName = name.EndsWith("_1")
                ? name.Substring(0, name.Length - 2)
                : name;

            // todo Ну это уже совсем никуда не годится, исправить
            if (safeName.EndsWith("A2A00") || safeName.EndsWith("A2D00") ||
                safeName.Substring(safeName.Length - 9, 4) == "HEFF" ||
                safeName.Substring(safeName.Length - 9, 4) == "TUCH" ||
                safeName.StartsWith("MRK") || safeName.StartsWith("DEATH"))
                return ImageType.Aura;

            if (safeName.EndsWith("S1A00") || safeName.EndsWith("S1D00"))
                return ImageType.Shadow;

            return ImageType.Simple;
        }


        #endregion
    }
}
