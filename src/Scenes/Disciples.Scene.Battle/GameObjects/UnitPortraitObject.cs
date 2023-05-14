using System.Drawing;
using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.GameObjects;

/// <summary>
/// Потрет юнита с его состоянием.
/// </summary>
internal class UnitPortraitObject : GameObject
{
    private const int PANEL_SEPARATOR_LAYER_SHIFT = 1;
    private const int FOREGROUND_LAYER_SHIFT = 1;
    private const int TEXT_LAYER_SHIFT = 2;
    private const int EFFECTS_LAYER_SHIFT = 3;

    /// <summary>
    /// Идентификатор в ресурсах с текстом "Промах".
    /// </summary>
    private const string MISS_TEXT_ID = "X008TA0001";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Защита".
    /// </summary>
    private const string DEFEND_TEXT_ID = "X008TA0021";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Ждать".
    /// </summary>
    private const string WAIT_TEXT_ID = "X008TA0020";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Защита" (от какого-то типа атаки).
    /// </summary>
    private const string WARD_TEXT_ID = "X008TA0011";
    /// <summary>
    /// Идентификатор в ресурсах с текстом "Иммунитет".
    /// </summary>
    private const string IMMUNITY_TEXT_ID = "X008TA0002";

    private readonly ITextProvider _textProvider;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly bool _rightToLeft;

    private readonly IBitmap _unitFaceBitmap;

    /// <summary>
    /// Картинка с портретом юнита.
    /// </summary>
    private IImageSceneObject _unitPortrait = null!;
    /// <summary>
    /// Изображение, отображающий полученный юнитом урон (красный прямоугольник поверх портрета).
    /// </summary>
    private IImageSceneObject? _unitDamageForeground;
    /// <summary>
    /// Иконка умершего юнита.
    /// </summary>
    private IImageSceneObject? _deathIcon;
    /// <summary>
    /// Изображение, отображающий моментальный эффект.
    /// </summary>
    private IImageSceneObject? _instantaneousEffectImage;
    /// <summary>
    /// Изображение с текстом моментального эффекта.
    /// </summary>
    private ITextSceneObject? _instantaneousEffectText;
    /// <summary>
    /// Текст, отображающий текущее количество здоровья и максимальное.
    /// </summary>
    private ITextSceneObject _unitHitpoints = null!;
    /// <summary>
    /// Картинка-разделитель на панели для больших существ.
    /// </summary>
    private IImageSceneObject? _unitPanelSeparator;
    /// <summary>
    /// Иконка для юнитов, уровень которых больше базового на 5/10/15.
    /// </summary>
    private IImageSceneObject? _highLevelUnitIcon;
    /// <summary>
    /// Иконка, если юнит защитился.
    /// </summary>
    private IImageSceneObject? _defendIcon;
    /// <summary>
    /// Иконки эффектов, которые воздействуют на юнита.
    /// </summary>
    private readonly Dictionary<UnitAttackType, IImageSceneObject> _battleEffectsIcons;
    /// <summary>
    /// Передний фон эффектов, которые воздействуют на юнита.
    /// </summary>
    private readonly Dictionary<UnitAttackType, IImageSceneObject> _battleEffectsForegrounds;
    /// <summary>
    /// Количество ОЗ, которое было при предыдущей проверке.
    /// </summary>
    private int _lastUnitHitPoints;
    /// <summary>
    /// Количество уровней, которые было при предыдущей проверке.
    /// </summary>
    private int _lastLevelDiff;

    /// <summary>
    /// Создать объект класса <see cref="UnitPortraitObject" />.
    /// </summary>
    public UnitPortraitObject(
        ITextProvider textProvider,
        ISceneObjectContainer sceneObjectContainer,
        IBattleInterfaceProvider battleInterfaceProvider,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        Action<UnitPortraitObject> onUnitPortraitSelected,
        Action<UnitPortraitObject> onUnitPortraitRightMouseButtonClicked,
        Action<UnitPortraitObject> onUnitPortraitMouseLeftButtonPressed,
        Unit unit,
        bool rightToLeft,
        double x,
        double y) : base(x, y)
    {
        _textProvider = textProvider;
        _sceneObjectContainer = sceneObjectContainer;
        _battleInterfaceProvider = battleInterfaceProvider;
        _rightToLeft = rightToLeft;

        _unitFaceBitmap = battleUnitResourceProvider.GetUnitFace(unit.UnitType);

        Unit = unit;

        Width = _unitFaceBitmap.Width;
        Height = _unitFaceBitmap.Height;

        _battleEffectsIcons = new Dictionary<UnitAttackType, IImageSceneObject>();
        _battleEffectsForegrounds = new Dictionary<UnitAttackType, IImageSceneObject>();

        Components = new IComponent[]
        {
            new SelectionComponent(this, () => onUnitPortraitSelected.Invoke(this)),
            new MouseLeftButtonClickComponent(this, Array.Empty<KeyboardButton>(), onClickedAction: () => onUnitPortraitRightMouseButtonClicked.Invoke(this)),
            new MouseRightButtonClickComponent(this, () => onUnitPortraitMouseLeftButtonPressed.Invoke(this))
        };
    }

    /// <summary>
    /// Юнит.
    /// </summary>
    public Unit Unit { get; }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _unitPortrait = _sceneObjectContainer.AddImage(_unitFaceBitmap, X, Y, BattleLayers.INTERFACE_LAYER);
        _unitPortrait.IsReflected = _rightToLeft;

        _unitHitpoints = _sceneObjectContainer.AddText(string.Empty, 11, X, Y + Height + 3, BattleLayers.INTERFACE_LAYER, Width, isBold: true);
        // Если юнит большой, то необходимо "закрасить" область между двумя клетками на панели.
        if (!Unit.UnitType.IsSmall) {
            _unitPanelSeparator = _sceneObjectContainer.AddImage(
                _battleInterfaceProvider.PanelSeparator,
                X + (Width - _battleInterfaceProvider.PanelSeparator.Width) / 2 - 1,
                Y + Height - 1,
                BattleLayers.PANEL_LAYER + PANEL_SEPARATOR_LAYER_SHIFT);
        }

        UpdateUnitEffects();
    }

    public override void Update(long ticksCount)
    {
        base.Update(ticksCount);

        UpdateUnitEffects();
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        _sceneObjectContainer.RemoveSceneObject(_unitPortrait);
        _sceneObjectContainer.RemoveSceneObject(_unitHitpoints);

        RemoveSceneObject(ref _deathIcon);
        RemoveSceneObject(ref _instantaneousEffectImage);
        RemoveSceneObject(ref _instantaneousEffectText);
        RemoveSceneObject(ref _unitDamageForeground);
        RemoveSceneObject(ref _unitPanelSeparator);
        RemoveSceneObject(ref _highLevelUnitIcon);
        RemoveSceneObject(ref _defendIcon);

        foreach (var battleEffectsIcon in _battleEffectsIcons)
            _sceneObjectContainer.RemoveSceneObject(battleEffectsIcon.Value);

        foreach (var battleEffectsForeground in _battleEffectsForegrounds)
            _sceneObjectContainer.RemoveSceneObject(battleEffectsForeground.Value);

        base.Destroy();
    }

    /// <summary>
    /// Обработать событие, произошедшее с юнитом.
    /// </summary>
    public void ProcessBeginUnitPortraitEvent(BattleUnitPortraitEventData eventData)
    {
        if (_instantaneousEffectText != null || _instantaneousEffectImage != null)
        {
            // TODO Fatal в лог.
            RemoveSceneObject(ref _instantaneousEffectImage);
            RemoveSceneObject(ref _instantaneousEffectText);
        }

        switch (eventData.UnitActionType)
        {
            case UnitActionType.Damaged:
                _instantaneousEffectImage = AddColorImage(BattleColors.Damage);
                _instantaneousEffectText = AddText($"-{eventData.Power!.Value}");
                _unitHitpoints.Text = $"{Unit.HitPoints}/{Unit.MaxHitPoints}";
                break;

            case UnitActionType.Healed:
                _instantaneousEffectImage = AddColorImage(BattleColors.Heal);
                _instantaneousEffectText = AddText($"+{eventData.Power!.Value}");
                _unitHitpoints.Text = $"{Unit.HitPoints}/{Unit.MaxHitPoints}";
                break;

            case UnitActionType.Miss:
                _instantaneousEffectImage = AddColorImage(BattleColors.Miss);
                _instantaneousEffectText = AddText(_textProvider.GetText(MISS_TEXT_ID));
                break;

            case UnitActionType.Defend:
                _instantaneousEffectText = AddText(_textProvider.GetText(DEFEND_TEXT_ID));
                break;

            case UnitActionType.Waiting:
                _instantaneousEffectText = AddText(_textProvider.GetText(WAIT_TEXT_ID));
                break;

            case UnitActionType.Dying:
                break;

            case UnitActionType.UnderEffect:
            {
                var effectColor = GetEffectTypeColor(eventData.AttackType!.Value);
                if (effectColor != null)
                    _instantaneousEffectImage = AddColorImage(effectColor.Value, false);

                var underEffectText = _textProvider.GetText(GetEffectText(eventData.AttackType!.Value, false));
                _instantaneousEffectText = AddText(underEffectText);

                break;
            }

            case UnitActionType.TriggeredEffect:
            {
                var triggeredEffectText = _textProvider.GetText(GetEffectText(eventData.AttackType!.Value, eventData.EffectDuration!.IsCompleted));
                _instantaneousEffectText = AddText(eventData.Power == null
                    ? triggeredEffectText
                    : $"{triggeredEffectText} (-{eventData.Power})");

                break;
            }

            case UnitActionType.Ward:
                _instantaneousEffectText = AddText(_textProvider.GetText(WARD_TEXT_ID));
                break;

            case UnitActionType.Immunity:
                _instantaneousEffectText = AddText(_textProvider.GetText(IMMUNITY_TEXT_ID));
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Обработать завершение действия юнита.
    /// </summary>
    public void ProcessCompletedUnitPortraitEvent()
    {
        RemoveSceneObject(ref _instantaneousEffectImage);
        RemoveSceneObject(ref _instantaneousEffectText);

        // Сбрасываем количество ХП, чтобы обновить рамку.
        _lastUnitHitPoints = int.MaxValue;
        UpdateUnitEffects();
    }

    /// <summary>
    /// Обновить состояние юнита.
    /// </summary>
    private void UpdateUnitEffects()
    {
        var levelDiff = Unit.Level - Unit.UnitType.Level;
        if (levelDiff != _lastLevelDiff)
        {
            RemoveSceneObject(ref _highLevelUnitIcon);
            _highLevelUnitIcon = GetHighLevelUnitIcon(levelDiff);
            _lastLevelDiff = levelDiff;
        }

        // Если сейчас обрабатывается моментальный эффект, то рамку размещать не нужно.
        if (_instantaneousEffectImage != null || _instantaneousEffectText != null)
            return;

        ProcessBattleEffects();

        if (Unit.IsDead)
        {
            if (_deathIcon == null)
            {
                var deathScull = Unit.UnitType.IsSmall
                    ? _battleInterfaceProvider.DeathSkullSmall
                    : _battleInterfaceProvider.DeathSkullBig;
                _deathIcon = _sceneObjectContainer.AddImage(deathScull, X, Y, BattleLayers.INTERFACE_LAYER + FOREGROUND_LAYER_SHIFT);
                _unitHitpoints.Text = $"0/{Unit.MaxHitPoints}";

                RemoveSceneObject(ref _unitDamageForeground);
                RemoveBattleEffectsForegrounds();
            }
        }
        else if (_lastUnitHitPoints != Unit.HitPoints)
        {
            _lastUnitHitPoints = Unit.HitPoints;
            _unitHitpoints.Text = $"{_lastUnitHitPoints}/{Unit.MaxHitPoints}";

            RemoveSceneObject(ref _unitDamageForeground);

            var height = (1 - ((double)_lastUnitHitPoints / Unit.MaxHitPoints)) * Height;
            if (height > 0)
            {
                var width = Width;
                var x = _unitPortrait.X;
                var y = _unitPortrait.Y + (Height - height);

                _unitDamageForeground = _sceneObjectContainer.AddColorImage(BattleColors.Damage, width, height, x, y, BattleLayers.INTERFACE_LAYER + FOREGROUND_LAYER_SHIFT);
            }
        }
    }

    /// <summary>
    /// Обработать эффекты битвы - добавить новые и удалить старые.
    /// </summary>
    private void ProcessBattleEffects()
    {
        // Иконку "Защиты" располагаем по центру.
        if (Unit.Effects.IsDefended && _defendIcon == null)
        {
            var icon = _battleInterfaceProvider.UnitPortraitDefendIcon;
            _defendIcon = _sceneObjectContainer.AddImage(
                icon,
                X + (Width - icon.Width) / 2,
                Y + Height - icon.Height,
                BattleLayers.INTERFACE_LAYER + EFFECTS_LAYER_SHIFT);
        }
        else if (!Unit.Effects.IsDefended && _defendIcon != null)
        {
            RemoveSceneObject(ref _defendIcon);
        }

        var battleEffects = Unit.Effects.GetBattleEffects();

        // Удаляем иконки тех эффектов, действие которых закончилось.
        var expiredEffects = _battleEffectsIcons
            .Where(bei => battleEffects.All(be => be.AttackType != bei.Key))
            .ToList();
        foreach (var expiredEffect in expiredEffects)
        {
            if (_battleEffectsIcons.TryGetValue(expiredEffect.Key, out var effectIcon))
            {
                _sceneObjectContainer.RemoveSceneObject(effectIcon);
                _battleEffectsIcons.Remove(expiredEffect.Key);
            }

            if (_battleEffectsForegrounds.TryGetValue(expiredEffect.Key, out var effectForeground))
            {
                _sceneObjectContainer.RemoveSceneObject(effectForeground);
                _battleEffectsForegrounds.Remove(expiredEffect.Key);
            }
        }

        // Добавляем иконки новых эффектов.
        foreach (var battleEffect in battleEffects)
        {
            if (!_battleEffectsIcons.ContainsKey(battleEffect.AttackType))
            {
                if (_battleInterfaceProvider.UnitBattleEffectsIcon.TryGetValue(battleEffect.AttackType, out var icon))
                {
                    // Иконки остальных эффектов располагаются справа.
                    var iconsCount = _battleEffectsIcons.Keys
                        .GroupBy(be => be)
                        .Count();
                    _battleEffectsIcons.Add(battleEffect.AttackType, _sceneObjectContainer.AddImage(
                        icon,
                        X + Width - icon.Width,
                        Y + Height - icon.Height * (iconsCount + 1),
                        BattleLayers.INTERFACE_LAYER + EFFECTS_LAYER_SHIFT));
                }
            }

            // Добавляем фон, связанный с воздействием эффекта яда и заморозки.
            if (!_battleEffectsForegrounds.ContainsKey(battleEffect.AttackType))
            {
                var effectTypeColor = GetEffectTypeColor(battleEffect.AttackType);
                if (effectTypeColor != null)
                {
                    _battleEffectsForegrounds.Add(
                        battleEffect.AttackType,
                        AddColorImage(effectTypeColor.Value, false));
                }
            }
        }
    }

    /// <summary>
    /// Получить цвет эффекта.
    /// </summary>
    private static Color? GetEffectTypeColor(UnitAttackType unitEffectAttackType)
    {
        return unitEffectAttackType switch
        {
            UnitAttackType.Paralyze => BattleColors.Paralyze,
            UnitAttackType.Poison => BattleColors.Poison,
            UnitAttackType.Frostbite => BattleColors.Frostbite,
            UnitAttackType.Blister => BattleColors.Blister,
            _ => null
        };
    }

    /// <summary>
    /// Получить наименования эффекта, что воздействует на юнита.
    /// </summary>
    private static string GetEffectText(UnitAttackType attackType, bool isEffectCompleted)
    {
        return attackType switch
        {
            UnitAttackType.Paralyze => isEffectCompleted
                ? "X008TA0024"
                : "X005TA0789",
            UnitAttackType.Fear => "X005TA0794",
            UnitAttackType.BoostDamage => "X005TA0795",
            UnitAttackType.Petrify => isEffectCompleted
                ? "X008TA0025"
                : "X008TA0009",
            UnitAttackType.LowerDamage => "X005TA0796",
            UnitAttackType.LowerInitiative => "X005TA0797",
            UnitAttackType.Poison => "X005TA0798",
            UnitAttackType.Frostbite => "X005TA0799",
            UnitAttackType.Revive => "X005TA0800",
            UnitAttackType.Cure => "X005TA0793",
            UnitAttackType.DrainLevel => "X005TA0804",
            UnitAttackType.GiveAttack => "X005TA0805",
            UnitAttackType.Doppelganger => "X005TA0806",
            UnitAttackType.TransformSelf => "X005TA0807",
            UnitAttackType.TransformOther => "X005TA0808",
            UnitAttackType.Blister => "X160TA0012",
            UnitAttackType.BestowWards => "X160TA0014",
            UnitAttackType.Shatter => "X160TA0020",
            _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
        };
    }

    /// <summary>
    /// Добавить на портрет изображение указанного цвета.
    /// </summary>
    private IImageSceneObject AddColorImage(Color color, bool shouldRemoveDamageImage = true)
    {
        // Если мы добавляем изображение поверх портрета, то в некоторых случаях должны на время очистить изображение с % здоровья.
        if (shouldRemoveDamageImage)
        {
            RemoveSceneObject(ref _unitDamageForeground);
            RemoveBattleEffectsForegrounds();
        }

        return _sceneObjectContainer.AddColorImage(color, Width, Height, X, Y, BattleLayers.INTERFACE_LAYER + FOREGROUND_LAYER_SHIFT);
    }

    /// <summary>
    /// Добавить на портрет указанный текст.
    /// </summary>
    private ITextSceneObject AddText(string text)
    {
        return _sceneObjectContainer.AddText(text, 12, X - 3, Y + Height / 2 - 6, BattleLayers.INTERFACE_LAYER + TEXT_LAYER_SHIFT, Width, isBold: true, foregroundColor: GameColors.White);
    }

    /// <summary>
    /// Получить иконку соответствующую уровню юнита.
    /// </summary>
    private IImageSceneObject? GetHighLevelUnitIcon(int levelDiff)
    {
        if (levelDiff < 5)
            return null;

        var icon = levelDiff switch
        {
            >= 15 => _battleInterfaceProvider.RedLevelIcon,
            >= 10 => _battleInterfaceProvider.OrangeLevelIcon,
            _ => _battleInterfaceProvider.BlueLevelIcon
        };

        return _sceneObjectContainer.AddImage(
            icon,
            X + (Width - icon.Width) / 2,
            Y + icon.Height / 2,
            BattleLayers.INTERFACE_LAYER + EFFECTS_LAYER_SHIFT);
    }

    /// <summary>
    /// Очистить фоны эффектов битвы.
    /// </summary>
    private void RemoveBattleEffectsForegrounds()
    {
        foreach (var battleEffectsForeground in _battleEffectsForegrounds)
            _sceneObjectContainer.RemoveSceneObject(battleEffectsForeground.Value);

        _battleEffectsForegrounds.Clear();
    }

    /// <summary>
    /// Удалить визуальный объект со сцены и очистить ссылку.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    /// <param name="sceneObject">Объект, который необходимо удалить.</param>
    private void RemoveSceneObject<T>(ref T? sceneObject) where T : ISceneObject
    {
        _sceneObjectContainer.RemoveSceneObject(sceneObject);
        sceneObject = default(T);
    }
}