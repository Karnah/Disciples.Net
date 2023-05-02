using Disciples.Common.Models;
using Disciples.Resources.Common;
using Disciples.Resources.Common.Extensions;
using Disciples.Resources.Common.Models;
using Disciples.Resources.Sounds.Models;
using File = Disciples.Resources.Common.Models.File;

namespace Disciples.Resources.Sounds;

/// <summary>
/// Класс для извлечения звуков, которые соответствуют указанным юнитам/действиям.
/// </summary>
public class SoundsMappingExtractor : BaseResourceExtractor
{
    private IDictionary<string, UnitTypeSounds> _unitTypeSounds = null!;

    /// <summary>
    /// Создать объект типа <see cref="SoundsMappingExtractor" />.
    /// </summary>
    /// <param name="path">Путь до ресурса со звуками.</param>
    public SoundsMappingExtractor(string path) : base(path)
    {
    }

    /// <inheritdoc />
    protected override int FilesRecordId => 2;

    /// <summary>
    /// Получить информацию о звуках для указанного типа юнита.
    /// </summary>
    public UnitTypeSounds GetUnitTypeSounds(string unitTypeId)
    {
        return _unitTypeSounds[unitTypeId];
    }

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
        var soundInfoRecords = LoadSoundInfoRecords(stream);
        _unitTypeSounds = LoadUnitTypeSounds(stream, soundInfoRecords);
    }

    /// <summary>
    /// Загрузить маппинги.
    /// </summary>
    /// <remarks>
    /// В данном методе нет смысла, так как все идентификаторы содержатся в SNDINFO.DAT.
    /// </remarks>
    private IReadOnlyList<int> LoadIds(Stream stream)
    {
        var file = GetFile("LSTIDS.DAT");

        stream.Seek(file.Offset, SeekOrigin.Begin);

        var idsCount = stream.ReadInt();
        var result = new int[idsCount];
        for (int i = 0; i < idsCount; i++)
        {
            result[i] = stream.ReadInt();
        }

        return result;
    }

    /// <summary>
    /// Загрузить маппинги.
    /// </summary>
    private IReadOnlyList<SoundInfoRecord> LoadSoundInfoRecords(Stream stream)
    {
        var file = GetFile("SNDINFO.DAT");

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
        var file = TryGetFile(fileName);
        if (file == null)
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