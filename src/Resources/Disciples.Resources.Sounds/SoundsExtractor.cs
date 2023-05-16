using Disciples.Resources.Common;
using Disciples.Resources.Common.Extensions;
using Disciples.Resources.Common.Models;
using Disciples.Resources.Sounds.Models;
using File = Disciples.Resources.Common.Models.File;

namespace Disciples.Resources.Sounds;

/// <summary>
/// Класс для извлечения аудио из ресурсов (.wdb).
/// </summary>
public class SoundsExtractor : BaseMqdbResourceExtractor
{
    /// <summary>
    /// Создать объект типа <see cref="SoundsExtractor" />.
    /// </summary>
    /// <param name="path">Путь до ресурса со звуками.</param>
    public SoundsExtractor(string path) : base(path)
    {
    }

    /// <inheritdoc />
    protected override int FilesRecordId => 1;

    /// <summary>
    /// Получит данные звукового файла.
    /// </summary>
    public RawSound? GetSound(string soundName)
    {
        var soundFile = TryGetFile(soundName);
        if (soundFile == null)
            return null;

        using (var stream = new FileStream(ResourceFilePath, FileMode.Open, FileAccess.Read))
        {
            var data = new byte[soundFile.Size];
            stream.Seek(soundFile.Offset, SeekOrigin.Begin);
            stream.Read(data, 0, data.Length);

            return new RawSound(data);
        }
    }

    /// <inheritdoc />
    protected override IReadOnlyList<File> LoadFiles(Stream stream, Record fileListRecord, IReadOnlyDictionary<int, Record> records)
    {
        // Пропускаем 8 байт, там какое-то магическое значение.
        stream.Skip(8);

        var filesCount = fileListRecord.Size / 24;
        var files = new List<File>(filesCount);
        for (int i = 0; i < filesCount; ++i)
        {
            var id = stream.ReadInt();
            var fileName = stream.ReadString(20);

            var record = records[id];
            var file = new File(id, fileName, record.Size, record.Offset);
            files.Add(file);
        }

        return files;
    }

    /// <inheritdoc />
    protected override void LoadInternal(Stream stream)
    {
    }
}