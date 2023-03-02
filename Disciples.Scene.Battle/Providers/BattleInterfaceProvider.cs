using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Platform.Factories;

namespace Disciples.Scene.Battle.Providers;

/// <inheritdoc cref="IBattleInterfaceProvider" />
public class BattleInterfaceProvider : BaseSupportLoading, IBattleInterfaceProvider
{
    private readonly IBitmapFactory _bitmapFactory;
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly IBattleResourceProvider _battleResourceProvider;

    /// <inheritdoc />
    public BattleInterfaceProvider(IBitmapFactory bitmapFactory, IInterfaceProvider interfaceProvider, IBattleResourceProvider battleResourceProvider)
    {
        _bitmapFactory = bitmapFactory;
        _interfaceProvider = interfaceProvider;
        _battleResourceProvider = battleResourceProvider;
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => false;

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        LoadBitmaps();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        Battleground = null;
        RightPanel = null;
        BottomPanel = null;
        PanelSeparator = null;
        DeathSkull = null;
        UnitInfoBackground = null;

        UnitBattleEffectsIcon = null;

        ToggleRightButton = null;
        DefendButton = null;
        RetreatButton = null;
        WaitButton = null;
        InstantResolveButton = null;
        AutoBattleButton = null;
    }

    /// <summary>
    /// Инициализировать картинки.
    /// </summary>
    private void LoadBitmaps()
    {
        Battleground = _battleResourceProvider.GetRandomBattleground();

        var battleIcons = _interfaceProvider.GetImageParts("DLG_BATTLE_A.PNG");

        RightPanel = battleIcons["DLG_BATTLE_A_RUNITGROUP"];
        BottomPanel = battleIcons["DLG_BATTLE_A_MAINCOMBATBG"];
        PanelSeparator = battleIcons["DLG_BATTLE_A_SPLITLRG"];
        // todo Не смог найти эту картинку в ресурсах игры. Скачал другую где-то на просторах интернета.
        DeathSkull = _bitmapFactory.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "Resources\\Common\\Skull.png"));
        UnitInfoBackground = _interfaceProvider.GetImage("_PG0500IX");

        UnitBattleEffectsIcon = new Dictionary<UnitBattleEffectType, IBitmap> {
            { UnitBattleEffectType.Defend, _battleResourceProvider.GetBattleFrame("FIDEFENDING").Bitmap },
            { UnitBattleEffectType.Poison, _battleResourceProvider.GetBattleFrame("FIPOISON").Bitmap },
            { UnitBattleEffectType.Frostbite, _battleResourceProvider.GetBattleFrame("F1FROSTBITE").Bitmap },
            //{ UnitBattleEffectType.Blister, _battleResourceProvider.GetBattleFrame("FIDEFENDING").Bitmap },
        };

        ToggleRightButton = GetButtonBitmaps(battleIcons, "TOGGLERIGHT");
        DefendButton = GetButtonBitmaps(battleIcons, "DEFEND");
        RetreatButton = GetButtonBitmaps(battleIcons, "RETREAT");
        WaitButton = GetButtonBitmaps(battleIcons, "WAIT");
        InstantResolveButton = GetButtonBitmaps(battleIcons, "INSTANTRESOLVE");
        AutoBattleButton = GetButtonBitmaps(battleIcons, "AUTOB");
    }


    /// <inheritdoc />
    public IReadOnlyList<IBitmap> Battleground { get; private set; }

    /// <inheritdoc />
    public IBitmap RightPanel { get; private set; }

    /// <inheritdoc />
    public IBitmap BottomPanel { get; private set; }

    /// <inheritdoc />
    public IBitmap PanelSeparator { get; private set; }

    /// <inheritdoc />
    public IBitmap DeathSkull { get; private set; }

    /// <inheritdoc />
    public IBitmap UnitInfoBackground { get; private set; }


    /// <inheritdoc />
    public IDictionary<UnitBattleEffectType, IBitmap> UnitBattleEffectsIcon { get; private set; }


    #region Buttons

    /// <inheritdoc />
    public IDictionary<SceneButtonState, IBitmap> ToggleRightButton { get; private set; }

    /// <inheritdoc />
    public IDictionary<SceneButtonState, IBitmap> DefendButton { get; private set; }

    /// <inheritdoc />
    public IDictionary<SceneButtonState, IBitmap> RetreatButton { get; private set; }

    /// <inheritdoc />
    public IDictionary<SceneButtonState, IBitmap> WaitButton { get; private set; }

    /// <inheritdoc />
    public IDictionary<SceneButtonState, IBitmap> InstantResolveButton { get; private set; }

    /// <inheritdoc />
    public IDictionary<SceneButtonState, IBitmap> AutoBattleButton { get; private set; }


    /// <summary>
    /// Получить словарь с изображениями кнопки для каждого её состояния.
    /// </summary>
    /// <param name="battleIcons">Иконки битвы.</param>
    /// <param name="buttonName">Имя кнопки.</param>
    private static IDictionary<SceneButtonState, IBitmap> GetButtonBitmaps(IReadOnlyDictionary<string, IBitmap> battleIcons, string buttonName)
    {
        return new Dictionary<SceneButtonState, IBitmap> {
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
        switch (battleBorderSize) {
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