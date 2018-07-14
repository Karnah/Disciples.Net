using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ImageMagick;

using ResourceManager.Helpers;
using ResourceManager.Models;

using File = ResourceManager.Models.File;

namespace ResourceManager
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

        public byte[][] GetAnimationFrames(string name)
        {
            if (_animations.ContainsKey(name) == false)
                return null;

            var animation = _animations[name];
            return GetFrameData(_files[animation.FileId].Frames.ToArray());
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
            stream.Position = currentPos;

            file.Content = buffer;
        }


        private void LoadFrames()
        {
            var d = new SortedDictionary<string, List<Tuple<int, int>>>();
            using (var animStream = new MemoryStream(_files.First(f => f.Value.Name == "-ANIMS.OPT").Value.Content)) {
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
            using (var imagesStream = new MemoryStream(_files.First(f => f.Value.Name == "-IMAGES.OPT").Value.Content)) {
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

        private byte[][] GetFrameData(Frame[] frames)
        {
            using (var imagesStream = new MemoryStream(_files.First(f => f.Value.Name == "-IMAGES.OPT").Value.Content)) {
                var result = new List<byte[]>(frames.Length);

                byte[] mainPixels = null;
                int mainWidth = 0;
                int mainBitmapId = -1;

                foreach (var frame in frames.OrderBy(f => f.SeqNumber)) {
                    if (frame.Id != mainBitmapId) {
                        mainBitmapId = frame.Id;

                        var mainImage = new MagickImage(_files[mainBitmapId].Content);

                        var colorMap = new Dictionary<int, byte>();
                        var name = Path.GetFileNameWithoutExtension(_files[mainBitmapId].Name);
                        var fileType = GetFileType(name);
                        if (fileType == Aura) {
                            // Если файл содержит ауру, то создаём полупрозрачное изображение
                            // Прозрачности зависит от индекса цвета в палитре
                            for (int i = 0; i < 256; ++i) {
                                unchecked {
                                    var color = mainImage.GetColormap(i);
                                    var index = ((byte)color.R << 16) + ((byte)color.G << 8) + (byte)color.B;
                                    colorMap[index] = (byte)i;
                                }
                            }
                        }

                        mainPixels = mainImage.GetPixels().ToByteArray(PixelMapping.RGBA);
                        for (int i = 0; i < mainPixels.Length; i += 4) {
                            if (mainPixels[i] == 0 && mainPixels[i + 1] == 0 && mainPixels[i + 2] == 0) {
                                mainPixels[i + 3] = 0;
                            }
                            else if (mainPixels[i] == 255 && mainPixels[i + 1] == 0 && mainPixels[i + 2] == 255) {
                                mainPixels[i + 3] = 0;
                            }

                            // Если файл - аура, то определяем прозрачность по словарю
                            var index = (mainPixels[i] << 16) + (mainPixels[i + 1] << 8) + mainPixels[i + 2];
                            if (colorMap.ContainsKey(index)) {
                                mainPixels[i + 3] = colorMap[index];
                            }

                            // Если файл тень, то делаем его полупрозрачным
                            if (fileType == Shadow) {
                                if (mainPixels[i + 3] != 0) {
                                    mainPixels[i + 3] = 128;
                                }
                            }

                            if (mainPixels[i + 3] == 0) {
                                mainPixels[i] = 0;
                                mainPixels[i + 1] = 0;
                                mainPixels[i + 2] = 0;
                                continue;
                            }
                        }

                        mainWidth = mainImage.Width;
                    }

                    imagesStream.Seek(frame.Offset, SeekOrigin.Begin);

                    var piecesNumber = imagesStream.ReadInt();
                    int width = imagesStream.ReadInt(),
                        height = imagesStream.ReadInt();

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
                                    var posS = ((y2 + y) * mainWidth + (x2 + x)) << 2;
                                    var posT = ((y1 + y) * width + (x1 + x)) << 2;

                                    imageData[posT] = mainPixels[posS];
                                    imageData[posT + 1] = mainPixels[posS + 1];
                                    imageData[posT + 2] = mainPixels[posS + 2];
                                    imageData[posT + 3] = mainPixels[posS + 3];
                                }
                            }
                        }
                    }

                    result.Add(imageData);
                }

                return result.ToArray();
            }
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
            if (name.EndsWith("A2A00") || name.EndsWith("A2D00"))
                return 1;

            if (name.EndsWith("S1A00") || name.EndsWith("S1D00"))
                return 2;

            return 0;
        }
    }
}
