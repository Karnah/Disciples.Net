using Disciples.Resources.Sounds.Helpers;
using Disciples.Resources.Sounds.Models;
using File = Disciples.Resources.Sounds.Models.File;

namespace Disciples.Resources.Sounds;

/// <summary>
/// Класс для извлечения аудио из ресурсов.
/// </summary>
public class SoundsExtractor
{
    /// <summary>
    /// Идентификатор записи в которой лежат имена файлов.
    /// </summary>
    private const int FILE_NAMES_RECORD_ID = 1;

    private readonly string _path;

    private IDictionary<int, Record> _records = null!;
    private IDictionary<int, File> _filesById = null!;
    private IDictionary<string, File> _filesByName = null!;

    /// <summary>
    /// Создать объект типа <see cref="SoundsExtractor" />.
    /// </summary>
    /// <param name="path">Путь до ресурса со звуками.</param>
    public SoundsExtractor(string path)
    {
        _path = path;

        Load();
    }

    /// <summary>
    /// Получит данные звукового файла.
    /// </summary>
    public RawSound? GetSound(string soundName)
    {
        if (!_filesByName.TryGetValue(soundName, out var soundFile))
            return null;

        using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read))
        {
            var data = new byte[soundFile.Size];
            stream.Seek(soundFile.Offset, SeekOrigin.Begin);
            stream.Read(data, 0, data.Length);

            return new RawSound(data);
        }
    }

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
    }

    /// <summary>
    /// Загрузка информации о записях.
    /// </summary>
    private void LoadRecords(Stream stream)
    {
        _records = new SortedDictionary<int, Record>();
        stream.Seek(28, SeekOrigin.Begin);

        while (true)
        {
            var magic = stream.ReadString(4);
            if (stream.Position >= stream.Length - 1 || magic != "MQRC")
                break;

            stream.Skip(4);

            var id = stream.ReadInt();
            var size = stream.ReadInt();
            stream.Skip(3 * 4);
            var offset = stream.Position;
            var mqrc = new Record(id, size, offset);

            stream.Skip(mqrc.Size);

            _records[mqrc.Id] = mqrc;
        }

        if (_records.ContainsKey(FILE_NAMES_RECORD_ID) == false)
            throw new ArgumentException($"Unknown file format: ID000{FILE_NAMES_RECORD_ID} was not found");
    }

    /// <summary>
    /// Загрузка информации о файлах.
    /// </summary>
    private void LoadFilesList(Stream stream)
    {
        var fileNamesRecord = _records[FILE_NAMES_RECORD_ID];
        stream.Seek(fileNamesRecord.Offset, SeekOrigin.Begin);

        // Пропускаем 8 байт, там какое-то магическое значение.
        stream.Skip(8);

        _filesById = new Dictionary<int, File>();
        _filesByName = new Dictionary<string, File>();

        var filesCount = fileNamesRecord.Size / 24;
        for (int i = 0; i < filesCount; ++i)
        {
            var id = stream.ReadInt();
            var fileName = stream.ReadString(20);

            var record = _records[id];
            var file = new File(id, fileName, record.Size, record.Offset);

            _filesById[id] = file;
            _filesByName[fileName] = file;
        }
    }
}