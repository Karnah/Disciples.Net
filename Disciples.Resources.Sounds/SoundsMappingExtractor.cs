using Disciples.Common.Models;
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
    private IDictionary<string, UnitTypeSounds> _unitTypeSounds = null!;

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
    /// Получить информацию о звуках для указанного типа юнита.
    /// </summary>
    public UnitTypeSounds GetUnitTypeSounds(string unitTypeId)
    {
        return _unitTypeSounds[unitTypeId];
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

            var soundInfoRecords = LoadSoundInfoRecords(stream);

            _unitTypeSounds = LoadUnitTypeSounds(stream, soundInfoRecords);
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
    /// <remarks>
    /// В данном методе нет смысла, так как все идентификаторы содержатся в SNDINFO.DAT.
    /// </remarks>
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
    private IReadOnlyList<SoundInfoRecord> LoadSoundInfoRecords(Stream stream)
    {
        if (!_filesByName.TryGetValue("SNDINFO.DAT", out var file))
            return Array.Empty<SoundInfoRecord>();

        stream.Seek(file.Offset, SeekOrigin.Begin);

        var recordsCount = stream.ReadInt();
        var soundInfoRecords = new List<SoundInfoRecord>(recordsCount);

        for (int i = 0; i < recordsCount; i++)
        {
            var unitTypeId = stream.ReadInt();

            var attackListName = stream.ReadString(33).ToUpperInvariant();
            stream.Skip(3);
            var beginAttackSoundFrameIndex = stream.ReadInt();
            var endAttackSoundFrameIndex = stream.ReadInt();

            var hitTargetFirstListName = stream.ReadString(33).ToUpperInvariant();
            stream.Skip(3);
            var beginAttackHitSoundFrameIndex = stream.ReadInt();
            var endAttackHitSoundFrameIndex = stream.ReadInt();

            var hitTargetSecondListName = stream.ReadString(33).ToUpperInvariant();
            var damagedHitListName = stream.ReadString(33).ToUpperInvariant();
            var globalMapWalkListName = stream.ReadString(33).ToUpperInvariant();

            stream.Skip(1);

            soundInfoRecords.Add(new SoundInfoRecord
            {
                UnitTypeId = unitTypeId,
                AttackListName = attackListName,
                BeginAttackSoundFrameIndex = beginAttackSoundFrameIndex,
                EndAttackSoundFrameIndex = endAttackSoundFrameIndex,
                HitTargetListName = string.IsNullOrEmpty(hitTargetFirstListName) ? hitTargetSecondListName : hitTargetFirstListName,
                BeginAttackHitSoundFrameIndex = beginAttackHitSoundFrameIndex,
                EndAttackHitSoundFrameIndex = endAttackHitSoundFrameIndex,
                DamagedListName = damagedHitListName,
                GlobalMapWalkListName = globalMapWalkListName
            });
        }

        return soundInfoRecords;
    }

    /// <summary>
    /// Загрузить список звуков для каждого типа юнита.
    /// </summary>
    private IDictionary<string, UnitTypeSounds> LoadUnitTypeSounds(Stream stream, IReadOnlyList<SoundInfoRecord> soundInfoRecords)
    {
        var soundLists = new Dictionary<string, IReadOnlyList<string>>();

        IReadOnlyList<string> GetSoundList(string? soundListName)
        {
            if (string.IsNullOrEmpty(soundListName))
                return Array.Empty<string>();

            if (!soundLists.TryGetValue(soundListName, out var soundList))
            {
                soundList = LoadSoundList(soundListName, stream);
                soundLists.Add(soundListName, soundList);
            }

            return soundList;
        }

        return soundInfoRecords
            .Select(record => new UnitTypeSounds
            {
                UnitTypeId = new ObjectId(record.UnitTypeId).GetStringId(),
                AttackSounds = GetSoundList(record.AttackListName),
                BeginAttackSoundFrameIndex = record.BeginAttackSoundFrameIndex,
                EndAttackSoundFrameIndex = record.EndAttackSoundFrameIndex,
                HitTargetSounds = GetSoundList(record.HitTargetListName),
                BeginAttackHitSoundFrameIndex = record.BeginAttackHitSoundFrameIndex,
                EndAttackHitSoundFrameIndex = record.EndAttackHitSoundFrameIndex,
                DamagedSounds = GetSoundList(record.DamagedListName),
                GlobalMapWalkSounds = GetSoundList(record.GlobalMapWalkListName)
            })
            .ToDictionary(uts => uts.UnitTypeId, uts => uts);
    }

    /// <summary>
    /// Загрузить файл, содержащий список звуков.
    /// </summary>
    private IReadOnlyList<string> LoadSoundList(string fileName, Stream stream)
    {
        if (!_filesByName.TryGetValue(fileName, out var file))
        {
            // По какой-то причине есть не все файлы.
            return Array.Empty<string>();
        }

        stream.Seek(file.Offset, SeekOrigin.Begin);

        var soundFileName = stream.ReadString();

        stream.Skip(4);

        var sounds = new List<string>();

        while (true)
        {
            var soundName = stream.ReadString();
            if (string.IsNullOrEmpty(soundName))
                break;

            sounds.Add(soundName.ToUpperInvariant());
        }

        return sounds;
    }
}