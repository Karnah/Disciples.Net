using Disciples.Engine;
using Disciples.Engine.Common;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation;
using Disciples.Engine.Models;
using Disciples.Engine.Settings;
using Disciples.Resources.Database.Sqlite;
using Disciples.Scene.LoadSaga.Models;
using UnitAttackReach = Disciples.Resources.Database.Sqlite.Enums.UnitAttackReach;
using UnitCategory = Disciples.Resources.Database.Sqlite.Enums.UnitCategory;

namespace Disciples.Scene.LoadSaga.Providers;

/// <summary>
/// Класс для работы с сохранёнными играми пользователя.
/// </summary>
internal class SaveProvider
{
    /// <summary>
    /// Фильтр для поиска файлов сейвов.
    /// </summary>
    private const string SAVE_EXTENSION_FILTER = "*.json";

    private readonly GameController _gameController;
    private readonly GameDataContextFactory _gameDataContextFactory;
    private readonly string _savesPath;

    /// <summary>
    /// Создать объект типа <see cref="SaveProvider" />.
    /// </summary>
    public SaveProvider(GameController gameController, GameDataContextFactory gameDataContextFactory, GameSettings settings)
    {
        _gameController = gameController;
        _gameDataContextFactory = gameDataContextFactory;
        _savesPath = Path.Combine(Directory.GetCurrentDirectory(), settings.SavesFolder);
    }

    /// <summary>
    /// Получить список сейвов.
    /// </summary>
    public IReadOnlyList<Save> GetSaves()
    {
        return Directory
            .GetFiles(_savesPath, SAVE_EXTENSION_FILTER)
            .Where(f => !string.IsNullOrEmpty(f))
            .Select(f => new Save
            {
                Text = new TextContainer(Path.GetFileNameWithoutExtension(f)),
                Name = Path.GetFileNameWithoutExtension(f),
                Path = Path.Combine(_savesPath, f),
                GameContext = _gameController.LoadGame(Path.Combine(_savesPath, f))
            })
            .Concat(CreateRandomSaves())
            .ToArray();
    }

    #region Создание сейва с случайными армиями

    record RandomSaveUnitType(
        string Id,
        int Level,
        UnitCategory UnitCategory,
        UnitAttackReach MainAttackReach,
        int HitPoints,
        int LowLevelUpgradeHitPoints,
        int HighLevelUpgradeHitPoints,
        int UpgradeChangeLevel,
        bool IsSmall);

    /// <summary>
    /// Исключаем нейтралов, так как они слишком непредсказуемые для создания отряда.
    /// Дракон и гоблин-лучника оба 1ого уровня.
    /// </summary>
    private static readonly RaceType[] SupportedRaces = Enum
        .GetValues(typeof(RaceType))
        .Cast<RaceType>()
        .Where(r => r != RaceType.Neutral)
        .ToArray();

    /// <summary>
    /// Список типов атак, которые поддержаны.
    /// </summary>
    private static readonly UnitAttackType[] SupportedAttackTypes =
    {
        UnitAttackType.Damage,
        UnitAttackType.Paralyze,
        UnitAttackType.Heal,
        UnitAttackType.Fear,
        UnitAttackType.BoostDamage,
        UnitAttackType.Petrify,
        UnitAttackType.ReduceDamage,
        UnitAttackType.ReduceInitiative,
        UnitAttackType.Poison,
        UnitAttackType.Frostbite,
        UnitAttackType.GiveAdditionalAttack,
        UnitAttackType.Blister
    };

    /// <summary>
    /// Создать данные случайных битв.
    /// </summary>
    private IEnumerable<Save> CreateRandomSaves()
    {
        // Юниты могут быть максимально 5 уровня развития через строения.
        for (int battleLevel = 1; battleLevel <= 5; ++battleLevel)
        {
            yield return CreateRandomSave(battleLevel);
        }
    }

    /// <summary>
    /// Создать данные случайной битвы.
    /// </summary>
    private Save CreateRandomSave(int battleLevel)
    {
        string randomBattle = $"Random battle {battleLevel} lvl";

        var firstPlayerRace = GetRandomRaceType();
        var secondPlayerRace = GetRandomRaceType();

        using var context = _gameDataContextFactory.Create();

        return new Save
        {
            Text = new TextContainer(randomBattle),
            Name = randomBattle,
            Path = string.Empty,
            GameContext = new GameContext
            {
                SagaName = randomBattle,
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
                        Squads = new List<PlayerSquad> { GetRandomSquad(context, firstPlayerRace, battleLevel) }
                    },
                    new Player
                    {
                        Id = 2,
                        IsComputer = true,
                        Race = secondPlayerRace,
                        Squads = new List<PlayerSquad> { GetRandomSquad(context, secondPlayerRace, battleLevel) }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Получить случайную расу игрока.
    /// </summary>
    private static RaceType GetRandomRaceType()
    {
        return SupportedRaces[RandomGenerator.Get(SupportedRaces.Length)];
    }

    /// <summary>
    /// Получить случайный отряд.
    /// </summary>
    private static PlayerSquad GetRandomSquad(GameDataContext context, RaceType race, int battleLevel)
    {
        var units = context
            .UnitTypes
            .Where(u => (RaceType)u.Race.RaceType == race && u.Level <= battleLevel)
            .Where(u => SupportedAttackTypes.Contains((UnitAttackType)u.MainAttack.AttackType))
            .Select(u => new RandomSaveUnitType(u.Id, u.Level, u.UnitCategory, u.MainAttack.Reach, u.HitPoints, u.LowLevelUpgrade.HitPoints, u.HighLevelUpgrade.HitPoints, u.UpgradeChangeLevel, u.IsSmall))
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
        squadUnits.Add(CreateSquadUnit(1, leader, battleLevel, UnitSquadFlankPosition.Center));
        if (leader.IsSmall)
        {
            var leaderSecondUnits = IsFromLineUnit(leader)
                ? backLineUnits
                : frontLineUnits.Where(u => u.IsSmall).ToArray();
            var leaderSecondUnit = leaderSecondUnits[RandomGenerator.Get(leaderSecondUnits.Length)];
            squadUnits.Add(CreateSquadUnit(2, leaderSecondUnit, battleLevel, UnitSquadFlankPosition.Center));
        }

        // Верхняя линия.
        var topFrontUnit = frontLineUnits[RandomGenerator.Get(frontLineUnits.Length)];
        squadUnits.Add(CreateSquadUnit(3, topFrontUnit, battleLevel, UnitSquadFlankPosition.Top));
        if (topFrontUnit.IsSmall)
        {
            var topBackUnit = backLineUnits[RandomGenerator.Get(backLineUnits.Length)];
            squadUnits.Add(CreateSquadUnit(4, topBackUnit, battleLevel, UnitSquadFlankPosition.Top));
        }

        // Нижняя линия.
        var bottomFrontUnit = frontLineUnits[RandomGenerator.Get(frontLineUnits.Length)];
        squadUnits.Add(CreateSquadUnit(5, bottomFrontUnit, battleLevel, UnitSquadFlankPosition.Bottom));
        if (bottomFrontUnit.IsSmall)
        {
            var bottomBackUnit = backLineUnits[RandomGenerator.Get(backLineUnits.Length)];
            squadUnits.Add(CreateSquadUnit(6, bottomBackUnit, battleLevel, UnitSquadFlankPosition.Bottom));
        }

        return new PlayerSquad
        {
            Units = squadUnits
        };
    }

    /// <summary>
    /// Создать юнита в отряде.
    /// </summary>
    private static SquadUnit CreateSquadUnit(int id, RandomSaveUnitType unitType, int battleLevel, UnitSquadFlankPosition flankPosition)
    {
        // Юнит 2-ого уровня примерно в два раза сильнее юнита 1-ого уровня.
        // Чтобы уровнять немного разницу, низкоуровневые юниты усиляются сильнее.
        var levelDiff = (battleLevel - unitType.Level) * 4;
        var unitLevel = unitType.Level + levelDiff;

        // Вычисляем максимальное количество здоровья в зависимости от уровня.
        var lowLevelDiff = Math.Min(levelDiff, unitType.UpgradeChangeLevel - unitType.Level);
        var highLevelDiff = Math.Max(0, levelDiff - lowLevelDiff);
        var maxHitPoints = unitType.HitPoints
                           + unitType.LowLevelUpgradeHitPoints * lowLevelDiff
                           + unitType.HighLevelUpgradeHitPoints * highLevelDiff;

        return new SquadUnit
        {
            Id = id,
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
        return unitType.UnitCategory is UnitCategory.Leader or UnitCategory.NeutralLeader;
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