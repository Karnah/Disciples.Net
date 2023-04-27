using Disciples.Resources.Sounds.Helpers;
using Disciples.Resources.Sounds.Models;
using File = Disciples.Resources.Sounds.Models.File;

namespace Disciples.Resources.Sounds;

/// <summary>
/// Класс для извлечения звуков, которые соответствуют указанным юнитам/действиям.
/// </summary>
public class SoundsMappingExtractor
{
    /// <summary>
    /// Идентификатор записи в которой лежат имена файлов.
    /// </summary>
    private const int FILE_NAMES_RECORD_ID = 2;

    private readonly string _path;

    private IDictionary<int, Record> _records = null!;
    private IDictionary<int, File> _filesById = null!;
    private IDictionary<string, File> _filesByName = null!;

    /// <summary>
    /// Создать объект типа <see cref="SoundsMappingExtractor" />.
    /// </summary>
    /// <param name="path">Путь до ресурса со звуками.</param>
    public SoundsMappingExtractor(string path)
    {
        _path = path;

        Load();
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
            LoadIds(stream);
            LoadSoundInfo(stream);
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
            var header = stream.ReadString(4);
            if (stream.Position >= stream.Length - 1 || header != "MQRC")
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

        var filesCount = stream.ReadInt();
        _filesById = new Dictionary<int, File>();
        _filesByName = new Dictionary<string, File>();

        for (int i = 0; i < filesCount; ++i)
        {
            var fileName = stream.ReadString(256);
            var id = stream.ReadInt();

            var record = _records[id];
            var file = new File(id, fileName, record.Size, record.Offset);

            _filesById[id] = file;
            _filesByName[fileName] = file;
        }
    }

    /// <summary>
    /// Загрузить маппинги.
    /// </summary>
    private void LoadIds(Stream stream)
    {
        if (!_filesByName.TryGetValue("LSTIDS.DAT", out var file))
            return;

        stream.Seek(file.Offset, SeekOrigin.Begin);

        var idsCount = stream.ReadInt();
        var result = new int[idsCount];
        for (int i = 0; i < idsCount; i++)
        {
            result[i] = stream.ReadInt();
        }
    }

    /// <summary>
    /// Загрузить маппинги.
    /// </summary>
    /// <remarks>
    /// Как парсить Battle.wdt:
    /// Коротко: по аналогии с .ff файлами:KEKW: 
    /// Длинно:
    /// 1. прочитать заголовок (первые 24 байта)
    /// 2. прочитать следующие за заголовком 4 байта - смещение до таблицы содержания
    /// 3. прочитать таблицу содержания
    /// 4. найти по содержанию запись с id 2, так называемый "список имен"
    /// 5. в списке имен найти "LSTIDS.DAT" и рядом будет его id для таблицы содержания
    /// 6. найти данные LSTIDS.DAT через таблицу содержания, распарсить данные:
    /// первые 4 байта - кол-во ID юнитов
    /// затем массив ID юнитов в формате игры. "в формате игры" значит что они хранятся как 4-байтные числа, а не строки знакомые нам по таблицам из папки Globals. Как перевести ID из строки в формат игры или наоборот смотри здесь:
    /// https://github.com/VladimirMakeev/D2RSG/blob/master/ScenarioGenerator/src/rsgid.cpp
    /// 7. По аналогии найти в списке имен "SNDINFO.DAT", его id для таблицы содержимого и через нее сами данные, распарсить их:
    /// - первые 4 байта - кол-во записей, назовем их SndInfo
    /// - массив записей SndInfo, 192 байта каждая:
    /// struct SndInfo
    /// {
    /// u32 id; // ID юнита в формате игры
    /// char path1[16]; // путь к файлу .lst. Не уверен насчет длины, возможно 33
    /// char unknown[20];
    /// s32 data36; // Возможно кадр анимации когда начинать проигрывать звук
    /// s32 data40; // Возможно кадр когда закончить проигрывать звук
    /// char path2[33]; // путь к файлу .lst
    /// char padding[3]; // не используется
    /// s32 data80; // Когда начинать играть звук для файла из path2
    /// s32 data84; // Когда закончить играть звук для файла из path2
    /// char path3[33]; // путь к файлу .lst
    /// char path4[33]; // путь к файлу .lst
    /// char path5[33]; // путь к файлу .lst
    /// char padding;   // не используется
    /// };
    /// </remarks>
    private void LoadSoundInfo(Stream stream)
    {
        if (!_filesByName.TryGetValue("SNDINFO.DAT", out var file))
            return;

        stream.Seek(file.Offset, SeekOrigin.Begin);

        var idsCount = stream.ReadInt();
        for (int i = 0; i < idsCount; i++)
        {
            // 192.

            var id = stream.ReadInt();

            var attackListName = stream.ReadString(16);
            stream.Skip(20);
            var beginAttackSoundFrameIndex = stream.ReadInt();
            var endAttackSoundFrameIndex = stream.ReadInt();

            stream.Skip(3);
            var beginAttackHitSoundFrameIndex = stream.ReadInt();
            var endAttackHitSoundFrameIndex = stream.ReadInt();

            var getHitListName = stream.ReadString(33);
            var globalMapWalkListName = stream.ReadString(33);
            var listName = stream.ReadString(33);

            stream.Skip(1);
        }
    }
}