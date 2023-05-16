using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Common.Extensions;
using Disciples.Resources.Common.Models;
using File = Disciples.Resources.Common.Models.File;

namespace Disciples.Resources.Common;

/// <summary>
/// Базовый класс для извлечения ресурсов, которые запакованы в MQDB.
/// </summary>
public abstract class BaseMqdbResourceExtractor : BaseResourceExtractor
{
    /// <summary>
    /// Заголовок, с которого должен начинаться файл ресурса.
    /// </summary>
    private const string RESOURCE_HEADER = "MQDB";
    /// <summary>
    /// Заголовок, с которого начинается <see cref="Record" />.
    /// </summary>
    private const string RECORD_HEADER = "MQRC";

    private IReadOnlyDictionary<int, Record> _records = null!;
    private IReadOnlyDictionary<int, File> _filesById = null!;
    private IReadOnlyDictionary<string, File> _filesByName = null!;

    /// <summary>
    /// Создать объект типа <see cref="BaseMqdbResourceExtractor" />.
    /// </summary>
    protected BaseMqdbResourceExtractor(string resourceFilePath) : base(resourceFilePath)
    {
    }

    /// <summary>
    /// Список всех имён файлов.
    /// </summary>
    protected IReadOnlyList<string> FileNames { get; private set; } = null!;

    /// <summary>
    /// Идентификатор <see cref="Record" />, где хранится список файлов.
    /// </summary>
    protected abstract int FilesRecordId { get; }

    /// <inheritdoc />
    protected sealed override void LoadInternal()
    {
        using var stream = new FileStream(ResourceFilePath, FileMode.Open, FileAccess.Read);
        var header = stream.ReadString(RESOURCE_HEADER.Length);
        if (header != RESOURCE_HEADER)
            throw new ResourceException($"Неизвестный тип файла. Файл ресурсов должен начинаться с {RESOURCE_HEADER}");

        _records = LoadRecords(stream);
        if (!_records.TryGetValue(FilesRecordId, out var fileListRecord))
            throw new ResourceNotFoundException($"В ресурсе не найдена запись со списком файлов с идентификатором {FilesRecordId}");

        // Сразу переходим к файлу и читаем его.
        stream.Seek(fileListRecord.Offset, SeekOrigin.Begin);
        var files = LoadFiles(stream, fileListRecord, _records);
        _filesById = files.ToDictionary(f => f.Id, f => f);
        _filesByName = files.ToDictionary(f => f.Name, f => f);
        FileNames = files.Select(f => f.Name).ToArray();

        LoadInternal(stream);
    }

    /// <summary>
    /// Загрузить список файлов.
    /// </summary>
    protected abstract IReadOnlyList<File> LoadFiles(Stream stream, Record fileListRecord, IReadOnlyDictionary<int, Record> records);

    /// <summary>
    /// Загрузить дополнительные ресурсы.
    /// </summary>
    protected abstract void LoadInternal(Stream stream);

    /// <summary>
    /// Получить файл по его идентификатору.
    /// </summary>
    protected File GetFile(int id)
    {
        return TryGetFile(id) ?? throw new ResourceNotFoundException($"Не найден файл с идентификатором {id}");
    }

    /// <summary>
    /// Получить файл по его идентификатору.
    /// </summary>
    protected File? TryGetFile(int id)
    {
        if (_filesById.TryGetValue(id, out var file))
            return file;

        return null;
    }

    /// <summary>
    /// Получить файл по его имени.
    /// </summary>
    protected File GetFile(string fileName)
    {
        return TryGetFile(fileName) ?? throw new ResourceNotFoundException($"Не найден файл с именем {fileName}");
    }

    /// <summary>
    /// Получить файл по его имени.
    /// </summary>
    protected File? TryGetFile(string fileName)
    {
        if (_filesByName.TryGetValue(fileName, out var file))
            return file;

        return null;
    }

    /// <summary>
    /// Загрузка информации о записях.
    /// </summary>
    private static IReadOnlyDictionary<int, Record> LoadRecords(Stream stream)
    {
        var records = new Dictionary<int, Record>();
        stream.Seek(28, SeekOrigin.Begin);

        while (true)
        {
            var header = stream.ReadString(RECORD_HEADER.Length);
            if (stream.Position >= stream.Length - 1 || header != RECORD_HEADER)
                break;

            stream.Skip(4);

            var id = stream.ReadInt();
            var size = stream.ReadInt();
            var realFileSize = stream.ReadInt();
            var isNotDeleted = stream.ReadInt();
            var recordMagic = stream.ReadInt();

            var offset = stream.Position;
            var mqrc = new Record(id, size, offset);

            stream.Skip(realFileSize);

            records[mqrc.Id] = mqrc;
        }

        return records;
    }
}