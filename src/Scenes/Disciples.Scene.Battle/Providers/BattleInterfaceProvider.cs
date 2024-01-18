using Disciples.Engine;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Platform.Factories;

namespace Disciples.Scene.Battle.Providers;

/// <inheritdoc cref="IBattleInterfaceProvider" />
internal class BattleInterfaceProvider : BaseSupportLoading, IBattleInterfaceProvider
{
    private readonly IBitmapFactory _bitmapFactory;
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly IBattleResourceProvider _battleResourceProvider;
    private readonly UnitFaceImagesExtractor _unitFaceExtractor;

    /// <inheritdoc />
    public BattleInterfaceProvider(IBitmapFactory bitmapFactory,
        IInterfaceProvider interfaceProvider,
        IBattleResourceProvider battleResourceProvider,
        UnitFaceImagesExtractor unitFaceExtractor)
    {
        _bitmapFactory = bitmapFactory;
        _interfaceProvider = interfaceProvider;
        _battleResourceProvider = battleResourceProvider;
        _unitFaceExtractor = unitFaceExtractor;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        LoadBitmaps();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Инициализировать картинки.
    /// </summary>
    private void LoadBitmaps()
    {
        BattleInterface = _interfaceProvider.GetSceneInterface("DLG_BATTLE_A");
        UnitDetailInfoInterface = _interfaceProvider.GetSceneInterface("DLG_R_C_UNIT");

        Battleground = _battleResourceProvider.GetRandomBattleground();

        PanelSeparator = _interfaceProvider.GetImage("DLG_BATTLE_A_SPLITLRG");
        DeathSkullSmall = _bitmapFactory.FromRawBitmap(_unitFaceExtractor.GetImage("MASKDEADS"));
        DeathSkullBig = _bitmapFactory.FromRawBitmap(_unitFaceExtractor.GetImage("MASKDEADL"));

        BlueLevelIcon = _battleResourceProvider.GetBattleBitmap("FIHIGHLEVEL1");
        OrangeLevelIcon = _battleResourceProvider.GetBattleBitmap("FIHIGHLEVEL2");
        RedLevelIcon = _battleResourceProvider.GetBattleBitmap("FIHIGHLEVEL3");

        UnitPortraitDefendIcon = _battleResourceProvider.GetBattleBitmap("FIDEFENDING");
        // Вообще есть 4 иконки для побега: для большого и маленького юнита, для атакующего и защищающегося отряда.
        // Но они одинаковые, поэтому используем только одну.
        UnitPortraitRetreatedIcon = _battleResourceProvider.GetBattleBitmap("FIRETREATLA");
        UnitBattleEffectsIcon = new Dictionary<UnitAttackType, IBitmap>
        {
            { UnitAttackType.Paralyze, _battleResourceProvider.GetBattleBitmap("FIPARALYZE") },
            // Для BoostDamage есть 4 иконки (FIBOOST1/2/3/4), но между ними нет никакой разницы.
            { UnitAttackType.BoostDamage, _battleResourceProvider.GetBattleBitmap("FIBOOST1") },
            { UnitAttackType.Petrify, _battleResourceProvider.GetBattleBitmap("FIPETRIFY") },
            { UnitAttackType.Poison, _battleResourceProvider.GetBattleBitmap("FIPOISON") },
            { UnitAttackType.Frostbite, _battleResourceProvider.GetBattleBitmap("F1FROSTBITE") },
            { UnitAttackType.DrainLevel, _battleResourceProvider.GetBattleBitmap("F1DRAINLEVEL") },
            { UnitAttackType.TransformOther, _battleResourceProvider.GetBattleBitmap("FITRANSFORM") },
        };
    }

    /// <inheritdoc />
    public SceneInterface BattleInterface { get; private set; } = null!;

    /// <inheritdoc />
    public SceneInterface UnitDetailInfoInterface { get; private set; } = null!;

    /// <inheritdoc />
    public IReadOnlyList<IBitmap> Battleground { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap PanelSeparator { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap DeathSkullSmall { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap DeathSkullBig { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap BlueLevelIcon { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap OrangeLevelIcon { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap RedLevelIcon { get; private set; } = null!;


    /// <inheritdoc />
    public IBitmap UnitPortraitDefendIcon { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap UnitPortraitRetreatedIcon { get; private set; } = null!;

    /// <inheritdoc />
    public IReadOnlyDictionary<UnitAttackType, IBitmap> UnitBattleEffectsIcon { get; private set; } = null!;


    #region UnitPanelBorders

    /// <inheritdoc />
    public AnimationFrames GetUnitAttackBorder(bool sizeSmall)
    {
        return GetAttackBorder(sizeSmall ? BattleBorderSize.SmallUnit : BattleBorderSize.LargeUnit);
    }

    /// <inheritdoc />
    public AnimationFrames GetFieldAttackBorder()
    {
        return GetAttackBorder(BattleBorderSize.Field);
    }

    /// <summary>
    /// Извлечь из ресурсов рамку атаки.
    /// </summary>
    /// <param name="battleBorderSize">Размер рамки.</param>
    private AnimationFrames GetAttackBorder(BattleBorderSize battleBorderSize)
    {
        var suffix = GetBattleBorderTypeSuffix(battleBorderSize);

        // todo Выяснить в чем отличие от SELALLA
        return _battleResourceProvider.GetBattleAnimation($"ISEL{suffix}A");
    }


    /// <inheritdoc />
    public AnimationFrames GetUnitSelectionBorder(bool sizeSmall)
    {
        return GetSelectionBorder(sizeSmall ? BattleBorderSize.SmallUnit : BattleBorderSize.LargeUnit);
    }

    /// <summary>
    /// Извлечь из ресурсов рамку выделения текущего юнита.
    /// </summary>
    /// <param name="battleBorderSize">Размер рамки.</param>
    private AnimationFrames GetSelectionBorder(BattleBorderSize battleBorderSize)
    {
        if (battleBorderSize == BattleBorderSize.Field)
            throw new ArgumentException("Selection border can not use for field", nameof(battleBorderSize));

        var suffix = GetBattleBorderTypeSuffix(battleBorderSize);
        return _battleResourceProvider.GetBattleAnimation($"PLY{suffix}A");
    }


    /// <inheritdoc />
    public AnimationFrames GetUnitHealBorder(bool sizeSmall)
    {
        return GetHealBorder(sizeSmall ? BattleBorderSize.SmallUnit : BattleBorderSize.LargeUnit);
    }

    /// <inheritdoc />
    public AnimationFrames GetFieldHealBorder()
    {
        return GetHealBorder(BattleBorderSize.Field);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, SceneElement> GetUnitPlaceholders(string pattern)
    {
        var placeholders = new Dictionary<int, SceneElement>();
        for (int i = 1; i <= GameConstants.MAX_UNITS_IN_SQUAD; i++)
        {
            placeholders.Add(i, BattleInterface.Elements[$"{pattern}{i}"]);
        }

        return placeholders;
    }

    /// <summary>
    /// Извлечь из ресурсов рамку исцеления.
    /// </summary>
    /// <param name="battleBorderSize">Размер рамки.</param>
    private AnimationFrames GetHealBorder(BattleBorderSize battleBorderSize)
    {
        var suffix = GetBattleBorderTypeSuffix(battleBorderSize);
        return _battleResourceProvider.GetBattleAnimation($"HEA{suffix}A");
    }

    /// <summary>
    /// Получить часть названия файла по размеру рамки.
    /// </summary>
    private static string GetBattleBorderTypeSuffix(BattleBorderSize battleBorderSize)
    {
        switch (battleBorderSize)
        {
            case BattleBorderSize.SmallUnit:
                return "SMALL";
            case BattleBorderSize.LargeUnit:
                return "LARGE";
            case BattleBorderSize.Field:
                return "ALL";
            default:
                throw new ArgumentOutOfRangeException(nameof(battleBorderSize), battleBorderSize, null);
        }
    }

    /// <summary>
    /// Размер рамки на панели с юнитами.
    /// </summary>
    private enum BattleBorderSize
    {
        /// <summary>
        /// Рамка занимает одну клетку.
        /// </summary>
        SmallUnit,

        /// <summary>
        /// Рамка занимает две клетки.
        /// </summary>
        LargeUnit,

        /// <summary>
        /// Рамка занимает всю панель целиком.
        /// </summary>
        Field
    }

    #endregion
}