using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Disciples.Engine.Base;
using Disciples.Engine.Common;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Models;
using Disciples.Engine.Settings;
using Disciples.Resources.Database.Sqlite;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <summary>
/// Класс для работы с сохранёнными играми пользователя.
/// </summary>
internal class SaveProvider : ISaveProvider
{
    /// <summary>
    /// Фильтр для поиска файлов сейвов.
    /// </summary>
    private const string SAVE_EXTENSION_FILTER = "*.json";

    private readonly IGameController _gameController;
    private readonly GameDataContextFactory _gameDataContextFactory;
    private readonly string _savesPath;

    /// <summary>
    /// Создать объект типа <see cref="SaveProvider" />.
    /// </summary>
    public SaveProvider(IGameController gameController, GameDataContextFactory gameDataContextFactory, GameSettings settings)
    {
        _gameController = gameController;
        _gameDataContextFactory = gameDataContextFactory;
        _savesPath = Path.Combine(Directory.GetCurrentDirectory(), settings.SavesFolder);
    }

    /// <inheritdoc />
    public IReadOnlyList<Save> GetSaves(MissionType? missionType = null)
    {
        var query = Directory
            .GetFiles(_savesPath, SAVE_EXTENSION_FILTER)
            .Where(f => !string.IsNullOrEmpty(f))
            .Select(f => new Save
            {
                Text = new TextContainer(Path.GetFileNameWithoutExtension(f)),
                Name = Path.GetFileNameWithoutExtension(f),
                Path = Path.Combine(_savesPath, f),
                GameContext = _gameController.LoadGame(Path.Combine(_savesPath, f))
            });

        if (missionType != null)
            query = query.Where(s => s.GameContext.MissionType == missionType);

        if (missionType is null or MissionType.Saga)
            query = query.Concat(CreateRandomSaves());

        return query.ToArray();
    }

    #region Создание сейва с случайными армиями

    record RandomSaveUnitType(
        string Id,
        int Level,
        int CalculatedLevel,
        UnitCategory UnitCategory,
        RaceType RaceType,
        UnitAttackReach MainAttackReach,
        int HitPoints,
        int LowLevelUpgradeHitPoints,
        int HighLevelUpgradeHitPoints,
        int UpgradeChangeLevel,
        bool IsSmall,
        bool CanUpgrade);

    /// <summary>
    /// Создать данные случайных битв.
    /// </summary>
    private IEnumerable<Save> CreateRandomSaves()
    {
        var allUnits = GetAllUnits();
        var raceTypes = Enum.GetValues<RaceType>();

        // Юниты могут быть максимально 5 уровня развития через строения.
        for (int battleLevel = 1; battleLevel <= 5; ++battleLevel)
        {
            yield return CreateRandomSave(allUnits, raceTypes, battleLevel);
        }
    }

    /// <summary>
    /// Создать данные случайной битвы.
    /// </summary>
    private static Save CreateRandomSave(IReadOnlyList<RandomSaveUnitType> allUnits, IReadOnlyList<RaceType> raceTypes, int battleLevel)
    {
        var randomBattleName = $"Random battle {battleLevel} lvl";

        var firstPlayerRace = raceTypes.GetRandomElement();
        var secondPlayerRace = raceTypes.GetRandomElement();

        return new Save
        {
            Text = new TextContainer(randomBattleName),
            Name = randomBattleName,
            Path = string.Empty,
            GameContext = new GameContext
            {
                SagaName = randomBattleName,
                SagaDescription = $"Create battle with random units {battleLevel} lvl",
                TurnNumber = 1,
                DifficultyLevel = DifficultyLevel.Average,
                Players = new []
                {
                    new Player
                    {
                        Id = 1,
                        IsComputer = false,
                        Race = firstPlayerRace,
                        Squads = new List<PlayerSquad> { GetRandomSquad(allUnits, firstPlayerRace, battleLevel) }
                    },
                    new Player
                    {
                        Id = 2,
                        IsComputer = true,
                        Race = secondPlayerRace,
                        Squads = new List<PlayerSquad> { GetRandomSquad(allUnits, secondPlayerRace, battleLevel) }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Получить список всех юнитов.
    /// </summary>
    private IReadOnlyList<RandomSaveUnitType> GetAllUnits()
    {
        using var context = _gameDataContextFactory.Create();
        var units = context
            .UnitTypes
            // При создании случайного отряда, занимаются все свободные места.
            // Поэтому исключаем юнитов с призывом, они будут практически бесполезны.
            .Where(u => u.MainAttack.AttackType != Disciples.Resources.Database.Sqlite.Enums.UnitAttackType.Summon)
            .Select(u => new
            {
                u.Id,
                u.Level,
                u.UnitCategory,
                u.Race.RaceType,
                u.MainAttack.Reach,
                u.HitPoints,
                LowLevelUpgradeHitPoints = u.LowLevelUpgrade.HitPoints,
                HighLevelUpgradeHitPoints = u.HighLevelUpgrade.HitPoints,
                u.UpgradeChangeLevel,
                u.IsSmall,
                RecruitCostGold = u.RecruitCost.Gold,
                u.PreviousUnitTypeId,
                u.LeaderBaseUnitTypeId
            })
            .ToDictionary(u => u.Id, u => u);

        var upgradeUnits = units
            .Values
            .Where(u => u.PreviousUnitTypeId != null)
            .Select(u => u.PreviousUnitTypeId!)
            .ToHashSet();

        return units
            .Values
            .Select(u =>
            {
                var leaderBaseUnitType = u.LeaderBaseUnitTypeId != null
                    ? units[u.LeaderBaseUnitTypeId]
                    : null;
                var calculatedLevel = IsLeader((UnitCategory)u.UnitCategory)
                    ? leaderBaseUnitType != null
                        ? GetCalculatedUnitTypeLevel(leaderBaseUnitType.Level, leaderBaseUnitType.RecruitCostGold)
                        : u.Level
                    : GetCalculatedUnitTypeLevel(u.Level, u.RecruitCostGold);

                return new RandomSaveUnitType(
                    u.Id,
                    u.Level,
                    calculatedLevel,
                    (UnitCategory)u.UnitCategory,
                    (RaceType) u.RaceType,
                    (UnitAttackReach)u.Reach,
                    u.HitPoints,
                    u.LowLevelUpgradeHitPoints,
                    u.HighLevelUpgradeHitPoints,
                    u.UpgradeChangeLevel,
                    u.IsSmall,
                    upgradeUnits.Contains(u.Id));
            })
            .ToArray();
    }

    /// <summary>
    /// Получить случайный отряд.
    /// </summary>
    private static PlayerSquad GetRandomSquad(IReadOnlyList<RandomSaveUnitType> allUnits, RaceType raceType, int battleLevel)
    {
        var units = allUnits
            .Where(u => u.RaceType == raceType &&
                        // Также берём и юнитов меньшего уровня, при условии что это их финальная форма.
                        (u.CalculatedLevel == battleLevel || (u.CalculatedLevel < battleLevel && !u.CanUpgrade)))
            .ToArray();
        var frontLineUnits = units
            .Where(u => IsFromLineUnit(u) && IsSoldier(u))
            .ToArray();
        var backLineUnits = units
            .Where(u => !IsFromLineUnit(u) && IsSoldier(u))
            .ToArray();

        var squadUnits = new List<SquadUnit>();

        // Заполняем центр лидером.
        var leaders = units
            .Where(IsLeader)
            .ToArray();
        var leader = leaders[RandomGenerator.Get(leaders.Length)];
        squadUnits.Add(CreateSquadUnit(leader, battleLevel, UnitSquadFlankPosition.Center));
        if (leader.IsSmall)
        {
            var leaderSecondUnits = IsFromLineUnit(leader)
                ? backLineUnits
                : frontLineUnits.Where(u => u.IsSmall).ToArray();
            var leaderSecondUnit = leaderSecondUnits[RandomGenerator.Get(leaderSecondUnits.Length)];
            squadUnits.Add(CreateSquadUnit(leaderSecondUnit, battleLevel, UnitSquadFlankPosition.Center));
        }

        // Верхняя линия.
        var topFrontUnit = frontLineUnits[RandomGenerator.Get(frontLineUnits.Length)];
        squadUnits.Add(CreateSquadUnit(topFrontUnit, battleLevel, UnitSquadFlankPosition.Top));
        if (topFrontUnit.IsSmall)
        {
            var topBackUnit = backLineUnits[RandomGenerator.Get(backLineUnits.Length)];
            squadUnits.Add(CreateSquadUnit(topBackUnit, battleLevel, UnitSquadFlankPosition.Top));
        }

        // Нижняя линия.
        var bottomFrontUnit = frontLineUnits[RandomGenerator.Get(frontLineUnits.Length)];
        squadUnits.Add(CreateSquadUnit(bottomFrontUnit, battleLevel, UnitSquadFlankPosition.Bottom));
        if (bottomFrontUnit.IsSmall)
        {
            var bottomBackUnit = backLineUnits[RandomGenerator.Get(backLineUnits.Length)];
            squadUnits.Add(CreateSquadUnit(bottomBackUnit, battleLevel, UnitSquadFlankPosition.Bottom));
        }

        return new PlayerSquad
        {
            Units = squadUnits
        };
    }

    /// <summary>
    /// Создать юнита в отряде.
    /// </summary>
    private static SquadUnit CreateSquadUnit(RandomSaveUnitType unitType, int battleLevel, UnitSquadFlankPosition flankPosition)
    {
        var levelDiff = GetUnitLevelDiff(unitType.CalculatedLevel, battleLevel);
        var unitLevel = unitType.Level + levelDiff;

        // Вычисляем максимальное количество здоровья в зависимости от уровня.
        var lowLevelDiff = Math.Min(levelDiff, unitType.UpgradeChangeLevel - unitType.Level);
        var highLevelDiff = Math.Max(0, levelDiff - lowLevelDiff);
        var maxHitPoints = unitType.HitPoints
                           + unitType.LowLevelUpgradeHitPoints * lowLevelDiff
                           + unitType.HighLevelUpgradeHitPoints * highLevelDiff;

        return new SquadUnit
        {
            Id = RandomGenerator.GetUnitId(),
            UnitTypeId = unitType.Id,
            SquadLinePosition = IsFromLineUnit(unitType)
                ? UnitSquadLinePosition.Front
                : UnitSquadLinePosition.Back,
            SquadFlankPosition = flankPosition,
            Level = unitLevel,
            Experience = 0,
            HitPoints = maxHitPoints
        };
    }

    /// <summary>
    /// Получить настоящий уровень юнита.
    /// </summary>
    /// <remarks>
    /// Если рассматривать юниты фракции нейтралов, то они все имеют 1 уровень.
    /// При этом по силе они могут быть как юнит 5-ого уровня (например, драконы).
    /// Поэтому пересчитываем уровень. Самый простой способ это сделать - ориентироваться на стоимость найма.
    /// </remarks>
    private static int GetCalculatedUnitTypeLevel(int unitLevel, int recruitCostGold)
    {
        if (unitLevel != 1)
            return unitLevel;

        return recruitCostGold switch
        {
            > 5000 => 6, // Это боссы, для случайных битв нет смысла их расставлять.
            > 2500 => 5,
            > 1500 => 4,
            > 750 => 3,
            > 200 => 2,
            _ => 1
        };
    }

    /// <summary>
    /// Получить значение усиления юнита из-за увеличенной сложности битвы.
    /// </summary>
    /// <remarks>
    /// Юнит 2-ого уровня примерно в два раза сильнее юнита 1-ого уровня.
    /// Чтобы уровнять немного разницу, низкоуровневые юниты усиляются сильнее.
    /// </remarks>
    private static int GetUnitLevelDiff(int unitLevel, int battleLevel)
    {
        var resultLevelDiff = 0;
        for (int i = unitLevel; i < battleLevel; i++)
        {
            // Если битва второго уровня, а юнит первого, то по итогу он получит 5-ый уровень.
            // На битве третьего уровня он получит 8ой. И так далее.
            resultLevelDiff += i switch
            {
                1 => 4,
                2 => 3,
                3 => 3,
                4 => 2,
                _ => 0
            };
        }

        return resultLevelDiff;
    }

    /// <summary>
    /// Проверить, что юнита необходимо располагать в первой линии.
    /// </summary>
    /// <returns>
    /// Считаем, что в первой линии нужно располагать только бойцов ближнего боя и больших юнитов.
    /// </returns>
    private static bool IsFromLineUnit(RandomSaveUnitType unitType)
    {
        return unitType.MainAttackReach == UnitAttackReach.Adjacent || !unitType.IsSmall;
    }

    /// <summary>
    /// Признак, что юнит - лидер.
    /// </summary>
    private static bool IsLeader(RandomSaveUnitType unitType)
    {
        return IsLeader(unitType.UnitCategory);
    }

    /// <summary>
    /// Признак, что юнит - лидер.
    /// </summary>
    private static bool IsLeader(UnitCategory unitCategory)
    {
        return unitCategory is UnitCategory.Leader or UnitCategory.NeutralLeader;
    }

    /// <summary>
    /// Признак, что юнит - солдат.
    /// </summary>
    private static bool IsSoldier(RandomSaveUnitType unitType)
    {
        return unitType.UnitCategory is UnitCategory.Soldier or UnitCategory.NeutralSoldier;
    }

    #endregion
}