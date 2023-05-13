using Disciples.Engine;
using Disciples.Engine.Common.Enums;
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
        Battleground = _battleResourceProvider.GetRandomBattleground();

        var battleIcons = _interfaceProvider.GetImageParts("DLG_BATTLE_A.PNG");

        LeftPanel = battleIcons["DLG_BATTLE_A_LUNITGROUP"];
        RightPanel = battleIcons["DLG_BATTLE_A_RUNITGROUP"];
        BottomPanel = battleIcons["DLG_BATTLE_A_MAINCOMBATBG"];
        PanelSeparator = battleIcons["DLG_BATTLE_A_SPLITLRG"];
        DeathSkullSmall = _bitmapFactory.FromRawBitmap(_unitFaceExtractor.GetImage("MASKDEADS")).Bitmap;
        DeathSkullBig = _bitmapFactory.FromRawBitmap(_unitFaceExtractor.GetImage("MASKDEADL")).Bitmap;
        UnitInfoBackground = _interfaceProvider.GetImage("_PG0500IX");

        BlueLevelIcon = _battleResourceProvider.GetBattleFrame("FIHIGHLEVEL1").Bitmap;
        OrangeLevelIcon = _battleResourceProvider.GetBattleFrame("FIHIGHLEVEL2").Bitmap;
        RedLevelIcon = _battleResourceProvider.GetBattleFrame("FIHIGHLEVEL3").Bitmap;

        UnitPortraitDefendIcon = _battleResourceProvider.GetBattleFrame("FIDEFENDING").Bitmap;
        UnitBattleEffectsIcon = new Dictionary<UnitAttackType, IBitmap>
        {
            { UnitAttackType.Paralyze, _battleResourceProvider.GetBattleFrame("FIPARALYZE").Bitmap },
            { UnitAttackType.Petrify, _battleResourceProvider.GetBattleFrame("FIPETRIFY").Bitmap },
            { UnitAttackType.Poison, _battleResourceProvider.GetBattleFrame("FIPOISON").Bitmap },
            { UnitAttackType.Frostbite, _battleResourceProvider.GetBattleFrame("F1FROSTBITE").Bitmap },
            { UnitAttackType.DrainLevel, _battleResourceProvider.GetBattleFrame("F1DRAINLEVEL").Bitmap },
            { UnitAttackType.TransformOther, _battleResourceProvider.GetBattleFrame("FITRANSFORM").Bitmap },
        };

        ToggleRightButton = GetButtonBitmaps(battleIcons, "TOGGLERIGHT");
        DefendButton = GetButtonBitmaps(battleIcons, "DEFEND");
        RetreatButton = GetButtonBitmaps(battleIcons, "RETREAT");
        WaitButton = GetButtonBitmaps(battleIcons, "WAIT");
        InstantResolveButton = GetButtonBitmaps(battleIcons, "INSTANTRESOLVE");
        AutoBattleButton = GetButtonBitmaps(battleIcons, "AUTOB");
    }


    /// <inheritdoc />
    public IReadOnlyList<IBitmap> Battleground { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap LeftPanel { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap RightPanel { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap BottomPanel { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap PanelSeparator { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap DeathSkullSmall { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap DeathSkullBig { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap UnitInfoBackground { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap BlueLevelIcon { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap OrangeLevelIcon { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap RedLevelIcon { get; private set; } = null!;


    /// <inheritdoc />
    public IBitmap UnitPortraitDefendIcon { get; private set; } = null!;

    /// <inheritdoc />
    public IReadOnlyDictionary<UnitAttackType, IBitmap> UnitBattleEffectsIcon { get; private set; } = null!;


    #region Buttons

    /// <inheritdoc />
    public IReadOnlyDictionary<SceneButtonState, IBitmap> ToggleRightButton { get; private set; } = null!;

    /// <inheritdoc />
    public IReadOnlyDictionary<SceneButtonState, IBitmap> DefendButton { get; private set; } = null!;

    /// <inheritdoc />
    public IReadOnlyDictionary<SceneButtonState, IBitmap> RetreatButton { get; private set; } = null!;

    /// <inheritdoc />
    public IReadOnlyDictionary<SceneButtonState, IBitmap> WaitButton { get; private set; } = null!;

    /// <inheritdoc />
    public IReadOnlyDictionary<SceneButtonState, IBitmap> InstantResolveButton { get; private set; } = null!;

    /// <inheritdoc />
    public IReadOnlyDictionary<SceneButtonState, IBitmap> AutoBattleButton { get; private set; } = null!;


    /// <summary>
    /// Получить словарь с изображениями кнопки для каждого её состояния.
    /// </summary>
    /// <param name="battleIcons">Иконки битвы.</param>
    /// <param name="buttonName">Имя кнопки.</param>
    private static IReadOnlyDictionary<SceneButtonState, IBitmap> GetButtonBitmaps(IReadOnlyDictionary<string, IBitmap> battleIcons, string buttonName)
    {
        return new Dictionary<SceneButtonState, IBitmap>
        {
            { SceneButtonState.Disabled, battleIcons[$"DLG_BATTLE_A_{buttonName}_D"] },
            { SceneButtonState.Active, battleIcons[$"DLG_BATTLE_A_{buttonName}_N"] },
            { SceneButtonState.Selected, battleIcons[$"DLG_BATTLE_A_{buttonName}_H"] },
            { SceneButtonState.Pressed, battleIcons[$"DLG_BATTLE_A_{buttonName}_C"] }
        };
    }

    #endregion

    #region UnitPanelBorders

    /// <inheritdoc />
    public IReadOnlyList<Frame> GetUnitAttackBorder(bool sizeSmall)
    {
        return GetAttackBorder(sizeSmall ? BattleBorderSize.SmallUnit : BattleBorderSize.LargeUnit);
    }

    /// <inheritdoc />
    public IReadOnlyList<Frame> GetFieldAttackBorder()
    {
        return GetAttackBorder(BattleBorderSize.Field);
    }

    /// <summary>
    /// Извлечь из ресурсов рамку атаки.
    /// </summary>
    /// <param name="battleBorderSize">Размер рамки.</param>
    private IReadOnlyList<Frame> GetAttackBorder(BattleBorderSize battleBorderSize)
    {
        var suffix = GetBattleBorderTypeSuffix(battleBorderSize);

        // todo Выяснить в чем отличие от SELALLA
        return _battleResourceProvider.GetBattleAnimation($"ISEL{suffix}A");
    }


    /// <inheritdoc />
    public IReadOnlyList<Frame> GetUnitSelectionBorder(bool sizeSmall)
    {
        return GetSelectionBorder(sizeSmall ? BattleBorderSize.SmallUnit : BattleBorderSize.LargeUnit);
    }

    /// <summary>
    /// Извлечь из ресурсов рамку выделения текущего юнита.
    /// </summary>
    /// <param name="battleBorderSize">Размер рамки.</param>
    private IReadOnlyList<Frame> GetSelectionBorder(BattleBorderSize battleBorderSize)
    {
        if (battleBorderSize == BattleBorderSize.Field)
            throw new ArgumentException("Selection border can not use for field", nameof(battleBorderSize));

        var suffix = GetBattleBorderTypeSuffix(battleBorderSize);
        return _battleResourceProvider.GetBattleAnimation($"PLY{suffix}A");
    }


    /// <inheritdoc />
    public IReadOnlyList<Frame> GetUnitHealBorder(bool sizeSmall)
    {
        return GetHealBorder(sizeSmall ? BattleBorderSize.SmallUnit : BattleBorderSize.LargeUnit);
    }

    /// <inheritdoc />
    public IReadOnlyList<Frame> GetFieldHealBorder()
    {
        return GetHealBorder(BattleBorderSize.Field);
    }

    /// <summary>
    /// Извлечь из ресурсов рамку исцеления.
    /// </summary>
    /// <param name="battleBorderSize">Размер рамки.</param>
    private IReadOnlyList<Frame> GetHealBorder(BattleBorderSize battleBorderSize)
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