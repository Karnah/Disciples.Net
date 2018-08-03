using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ImageMagick;

using ResourceProvider.Helpers;
using ResourceProvider.Models;

using File = ResourceProvider.Models.File;

namespace ResourceProvider
{
    public class ImagesExtractor
    {
        private readonly string _path;

        private SortedDictionary<int, Record> _records;
        private SortedDictionary<int, File> _files;
        private SortedDictionary<string, List<Frame>> _frames;

        private SortedDictionary<string, Animation> _animations;

        public ImagesExtractor(string path)
        {
            _path = path;

            _animations = new SortedDictionary<string, Animation>();

            Extract();
        }

        // bug Невозможно получить информацию о некоторых файлах. В основном, связанных с эльфами.
        // Например, G000UU8029HHITA1A00
        // Ссылки на PNG нет, но в .ff файле какая-то информация есть
        public IReadOnlyCollection<Image> GetAnimationFrames(string name)
        {
            if (_animations.ContainsKey(name) == false)
                return null;

            var animation = _animations[name];
            return GetFrameData(_files[animation.FileId].Frames);
        }

        public byte[] GetFileContent(string fileName)
        {
            var file = _files.FirstOrDefault(f => string.Equals(f.Value.Name, fileName, StringComparison.CurrentCultureIgnoreCase));
            return file.Value?.Content;
        }

        public Image GetImage(string name)
        {
            var file = _files.FirstOrDefault(f => string.Equals(f.Value.Name, $"{name}.PNG", StringComparison.CurrentCultureIgnoreCase)).Value;
            if (file == null)
                return null;

            return GetImage(file.Name, file.Content);
        }


        private void Extract()
        {
            using (var stream = System.IO.File.OpenRead(_path)) {
                var mqdb = stream.ReadString(4);
                if (mqdb != "MQDB")
                    throw new ArgumentException("Unknown format of file");

                LoadRecords(stream);
                LoadFilesList(stream);
            }

            LoadFrames();
            LoadImages();
        }

        /// <summary>
        /// Загрузка информации о записях
        /// </summary>
        /// <param name="stream"></param>
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
        /// <param name="stream"></param>
        private void LoadFilesList(Stream stream)
        {
            // Информация о списке файлов лежит в записи с идентификатором 2
            stream.Seek(_records[2].Offset, SeekOrigin.Begin);

            var filesCount = stream.ReadInt();
            _files = new SortedDictionary<int, File>();
            _animations = new SortedDictionary<string, Animation>();

            for (int i = 0; i < filesCount; ++i) {
                var fileName = stream.ReadString(256);
                var id = stream.ReadInt();

                var record = _records[id];
                var file = new File(id, record.Size, record.Offset, fileName);
                SaveFile(stream, ref file);
                _files[id] = file;

                var safeFileName = Path.GetFileNameWithoutExtension(fileName);
                _animations.Add(safeFileName, new Animation(safeFileName, file.Id));
            }
        }


        private static void SaveFile(Stream stream, ref File file)
        {
            var currentPos = stream.Position;
            var buffer = new byte[file.Size];
            stream.Seek(file.Offset, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(currentPos, SeekOrigin.Begin);

            file.Content = buffer;
        }


        private void LoadFrames()
        {
            var animsFile = _files.FirstOrDefault(f => f.Value.Name == "-ANIMS.OPT").Value;
            if (animsFile == null)
                return;

            var d = new SortedDictionary<string, List<Tuple<int, int>>>();
            using (var animStream = new MemoryStream(animsFile.Content)) {
                int animNumber = 0;
                while (true) {
                    var fileAnimNumber = animStream.ReadInt();
                    if (animStream.Position >= animStream.Length - 1)
                        break;

                    for (int animIndex = 0; animIndex < fileAnimNumber; ++animIndex) {
                        var animName = animStream.ReadString();
                        if (d.ContainsKey(animName) == false) {
                            d.Add(animName, new List<Tuple<int, int>>());
                        }

                        d[animName].Add(new Tuple<int, int>(animNumber, animIndex));
                    }

                    ++animNumber;
                }
            }

            using (var framesStream = new MemoryStream(_files.First(f => f.Value.Name == "-INDEX.OPT").Value.Content)) {
                var framesCount = framesStream.ReadInt();
                _frames = new SortedDictionary<string, List<Frame>>();

                for (int frameIndex = 0; frameIndex < framesCount; ++frameIndex) {
                    var id = framesStream.ReadInt();
                    var name = framesStream.ReadString();

                    framesStream.Seek(8, SeekOrigin.Current);
                    if (d.ContainsKey(name)) {
                        if (_frames.ContainsKey(name) == false)
                            _frames[name] = new List<Frame>();

                        foreach (var animInfo in d[name]) {
                            var frame = new Frame(id, animInfo.Item1, animInfo.Item2, name);
                            _frames[name].Add(frame);
                            _files[frame.Id].Frames.Add(frame);
                        }
                    }
                }
            }
        }

        private void LoadImages()
        {
            var imagesFile = _files.FirstOrDefault(f => f.Value.Name == "-IMAGES.OPT").Value;
            if (imagesFile == null)
                return;

            using (var imagesStream = new MemoryStream(imagesFile.Content)) {
                for (int i = 0; i < _frames.Count; ++i) {
                    imagesStream.Seek(11 + 1024, SeekOrigin.Current);

                    var fileFramesNumber = imagesStream.ReadInt();
                    for (int frameIndex = 0; frameIndex < fileFramesNumber; ++frameIndex) {
                        var frameName = imagesStream.ReadString();

                        if (_frames.ContainsKey(frameName)) {
                            var frames = _frames[frameName];
                            foreach (var frame in frames) {
                                frame.Offset = imagesStream.Position;
                            }
                        }

                        var piecesNumber = imagesStream.ReadInt();
                        imagesStream.Seek(2 * 4, SeekOrigin.Current); // ширина и высота
                        imagesStream.Seek(piecesNumber * 6 * 4, SeekOrigin.Current); // смещение частей
                    }
                }
            }
        }

        private IReadOnlyCollection<Image> GetFrameData(IReadOnlyCollection<Frame> frames)
        {
            using (var imagesStream = new MemoryStream(_files.First(f => f.Value.Name == "-IMAGES.OPT").Value.Content)) {
                var result = new List<Image>(frames.Count);

                int mainBitmapId = -1;
                Image mainImage = null;

                foreach (var frame in frames.OrderBy(f => f.SeqNumber)) {
                    if (frame.Id != mainBitmapId) {
                        mainBitmapId = frame.Id;
                        mainImage = GetImage(_files[mainBitmapId].Name, _files[mainBitmapId].Content);
                    }

                    imagesStream.Seek(frame.Offset, SeekOrigin.Begin);

                    var piecesNumber = imagesStream.ReadInt();
                    int width = imagesStream.ReadInt(),
                        height = imagesStream.ReadInt();

                    int minRow = int.MaxValue, maxRow = int.MinValue;
                    int minColumn = int.MaxValue, maxColumn = int.MinValue;
                    var imageData = new byte[width * height * 4];
                    for (int pieceIndex = 0; pieceIndex < piecesNumber; ++pieceIndex) {
                        int x1 = imagesStream.ReadInt(),
                            y1 = imagesStream.ReadInt(),
                            x2 = imagesStream.ReadInt(),
                            y2 = imagesStream.ReadInt(),
                            dX = imagesStream.ReadInt(),
                            dY = imagesStream.ReadInt();

                        for (int x = 0; x < dX; ++x) {
                            for (int y = 0; y < dY; ++y) {
                                unchecked {
                                    var posS = ((y2 + y) * mainImage.Width + (x2 + x)) << 2;
                                    var posT = ((y1 + y) * width + (x1 + x)) << 2;

                                    imageData[posT] = mainImage.Data[posS];
                                    imageData[posT + 1] = mainImage.Data[posS + 1];
                                    imageData[posT + 2] = mainImage.Data[posS + 2];
                                    imageData[posT + 3] = mainImage.Data[posS + 3];
                                }
                            }
                        }

                        minRow = Math.Min(minRow, y1);
                        maxRow = Math.Max(maxRow, y1 + dY);
                        minColumn = Math.Min(minColumn, x1);
                        maxColumn = Math.Max(maxColumn, x1 + dX);
                    }

                    result.Add(new Image(minRow, maxRow, minColumn, maxColumn, width, height, imageData));
                }

                return result;
            }
        }

        private static Image GetImage(string name, byte[] content)
        {
            var magicImage = new MagickImage(content);

            var colorMap = new Dictionary<int, byte>();
            var safeName = Path.GetFileNameWithoutExtension(name);
            var fileType = GetFileType(safeName);
            if (fileType == Aura) {
                // Если файл содержит ауру, то создаём полупрозрачное изображение
                // Пока берём прозрачность равную индексу цвета в палитре
                // Но такое чувство, что есть более четкая зависимость
                for (int i = 0; i < 256; ++i) {
                    unchecked {
                        var color = magicImage.GetColormap(i);
                        var index = ((byte) color.R << 16) + ((byte) color.G << 8) + (byte) color.B;

                        colorMap[index] = (byte) i;
                    }
                }
            }

            var pixels = magicImage.GetPixels().ToByteArray(PixelMapping.RGBA);
            var transparentColor = magicImage.GetColormap(0);
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
                    if (fileType == Shadow) {
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


            return new Image(0, magicImage.Height, 0, magicImage.Width, magicImage.Width, magicImage.Height, pixels);
        }


        // todo to Enum
        private const byte Unit = 0;
        private const byte Aura = 1;
        private const byte Shadow = 2;

        /// <summary>
        /// Возвращает тип объекта по его имени
        /// </summary>
        /// <param name="name"></param>
        /// <returns>0, если это юнит,
        /// 1, если аура,
        /// 2, если тень</returns>
        private static byte GetFileType(string name)
        {
            // todo Ну это уже совсем никуда не годится, исправить
            if (name.EndsWith("A2A00") || name.EndsWith("A2D00") ||
                name.Substring(name.Length - 9, 4) == "HEFF" ||
                name.Substring(name.Length - 9, 4) == "TUCH" ||
                name.StartsWith("MRK") || name.StartsWith("DEATH"))
                return 1;

            if (name.EndsWith("S1A00") || name.EndsWith("S1D00"))
                return 2;

            return 0;
        }
    }
}
